using UnityEditor;
using UnityEngine;
using Blech.Player;

namespace Blech.Editor
{
    /// <summary>
    /// Builds the Bean player prefab.
    /// Body: one Quaternius food mesh (exact filename match), or a stretched sphere fallback.
    /// No limbs — the procedural-capsule limb approach made the character look like a
    /// pile of hot dogs. Two eyes on the front of the body for cuteness.
    /// </summary>
    public static class PlayerPrefabBuilder
    {
        const string PrefabPath = "Assets/Blech/Prefabs/Player/Bean.prefab";
        const string BeanStatsPath = "Assets/Blech/ScriptableObjects/Characters/Bean.asset";
        const string BodyMatPath = "Assets/Blech/Art/Materials/M_BeanBody.mat";
        const string EyeMatPath = "Assets/Blech/Art/Materials/M_BeanEyes.mat";
        const string PupilMatPath = "Assets/Blech/Art/Materials/M_BeanPupil.mat";

        public static void BuildAll()
        {
            AssetFactory.EnsureFolder("Assets/Blech/Prefabs/Player");

            var bodyMat = AssetFactory.CreateOrUpdateMaterial(BodyMatPath, Shader.Find("Universal Render Pipeline/Lit"));
            bodyMat.SetColor("_BaseColor", new Color(0.835f, 0.647f, 0.455f));
            bodyMat.SetFloat("_Smoothness", 0.6f);
            EditorUtility.SetDirty(bodyMat);

            var eyeMat = AssetFactory.CreateOrUpdateMaterial(EyeMatPath, Shader.Find("Universal Render Pipeline/Lit"));
            eyeMat.SetColor("_BaseColor", Color.white);
            eyeMat.SetFloat("_Smoothness", 0.9f);
            EditorUtility.SetDirty(eyeMat);

            var pupilMat = AssetFactory.CreateOrUpdateMaterial(PupilMatPath, Shader.Find("Universal Render Pipeline/Unlit"));
            pupilMat.SetColor("_BaseColor", Color.black);
            EditorUtility.SetDirty(pupilMat);

            var root = new GameObject("Bean");
            var cc = root.AddComponent<CharacterController>();
            cc.height = 1.0f; cc.radius = 0.3f; cc.stepOffset = 0.2f; cc.center = new Vector3(0, 0.5f, 0);

            root.AddComponent<PlayerInput>();
            var mover = root.AddComponent<PlayerMovementController>();
            var climber = root.AddComponent<PlayerClimbingController>();
            root.AddComponent<PlayerStamina>();
            root.AddComponent<PlayerRespawn>();
            var visual = root.AddComponent<PlayerCharacterVisual>();

            var stats = AssetDatabase.LoadAssetAtPath<PlayerCharacterStats>(BeanStatsPath);
            SerializedObjectAssign(mover, "stats", stats);
            SerializedObjectAssign(climber, "stats", stats);

            GameObject body = BuildBody(root.transform, bodyMat);
            MakeEye("Eye_L", body.transform, new Vector3(-0.18f, 0.25f, 0.5f), eyeMat, pupilMat);
            MakeEye("Eye_R", body.transform, new Vector3( 0.18f, 0.25f, 0.5f), eyeMat, pupilMat);

            SerializedObjectAssign(visual, "body", body.transform);
            SerializedObjectAssignArray(visual, "limbs", new Transform[0]);

            AssetFactory.SavePrefab(root, PrefabPath);
            AssetDatabase.SaveAssets();
        }

        static GameObject BuildBody(Transform parent, Material mat)
        {
            // Prefer egg (bean-shaped), then avocado (round-ish), then coconut (round).
            var bodyMesh = TryLoadMeshExact("Egg_Whole")
                        ?? TryLoadMeshExact("Avocado")
                        ?? TryLoadMeshExact("Coconut");

            GameObject body;
            if (bodyMesh != null)
            {
                body = (GameObject)PrefabUtility.InstantiatePrefab(bodyMesh);
                body.name = "Body";
                body.transform.SetParent(parent, false);
                body.transform.localPosition = new Vector3(0, 0.1f, 0);
                body.transform.localScale = Vector3.one * 1.4f;
                foreach (var c in body.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
                foreach (var mr in body.GetComponentsInChildren<MeshRenderer>()) mr.sharedMaterial = mat;
            }
            else
            {
                body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                body.name = "Body";
                body.transform.SetParent(parent, false);
                body.transform.localPosition = new Vector3(0, 0.5f, 0);
                body.transform.localScale = new Vector3(0.55f, 0.7f, 0.65f);
                Object.DestroyImmediate(body.GetComponent<Collider>());
                body.GetComponent<MeshRenderer>().sharedMaterial = mat;
            }
            return body;
        }

        /// <summary>
        /// Loads an FBX from _ThirdParty whose filename (without extension) exactly matches
        /// the given name. The old fuzzy contains-match could return a multi-item FBX (e.g.
        /// a sausage assortment) and turn the player into "five hot dogs."
        /// </summary>
        static GameObject TryLoadMeshExact(string exactName)
        {
            string[] guids = AssetDatabase.FindAssets($"t:Model {exactName}", new[] { "Assets/Blech/_ThirdParty" });
            foreach (var g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                if (string.Equals(fileName, exactName, System.StringComparison.OrdinalIgnoreCase))
                    return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            return null;
        }

        static void MakeEye(string name, Transform parent, Vector3 pos, Material eyeMat, Material pupilMat)
        {
            var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = name;
            eye.transform.SetParent(parent, false);
            eye.transform.localPosition = pos;
            eye.transform.localScale = new Vector3(0.16f, 0.16f, 0.16f);
            Object.DestroyImmediate(eye.GetComponent<Collider>());
            eye.GetComponent<MeshRenderer>().sharedMaterial = eyeMat;

            var pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pupil.name = "Pupil";
            pupil.transform.SetParent(eye.transform, false);
            pupil.transform.localPosition = new Vector3(0, 0, 0.3f);
            pupil.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
            Object.DestroyImmediate(pupil.GetComponent<Collider>());
            pupil.GetComponent<MeshRenderer>().sharedMaterial = pupilMat;
        }

        static void SerializedObjectAssign(Object target, string fieldName, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop != null) prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SerializedObjectAssignArray(Object target, string fieldName, Transform[] values)
        {
            var so = new SerializedObject(target);
            var arr = so.FindProperty(fieldName);
            if (arr == null) return;
            arr.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                arr.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
