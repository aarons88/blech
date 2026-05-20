using UnityEditor;
using UnityEngine;
using Blech.Player;

namespace Blech.Editor
{
    public static class PlayerPrefabBuilder
    {
        const string PrefabPath = "Assets/Blech/Prefabs/Player/Bean.prefab";
        const string BeanStatsPath = "Assets/Blech/ScriptableObjects/Characters/Bean.asset";
        const string BodyMatPath = "Assets/Blech/Art/Materials/M_BeanBody.mat";
        const string EyeMatPath = "Assets/Blech/Art/Materials/M_BeanEyes.mat";
        const string PupilMatPath = "Assets/Blech/Art/Materials/M_BeanPupil.mat";
        const string InputActionsPath = "Assets/Blech/Scripts/Player/PlayerInputActions.inputactions";

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

            var input = root.AddComponent<PlayerInput>();
            var actionsAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(InputActionsPath);
            if (actionsAsset != null)
            {
                var so = new SerializedObject(input);
                so.FindProperty("actions").objectReferenceValue = actionsAsset;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            var mover = root.AddComponent<PlayerMovementController>();
            var climber = root.AddComponent<PlayerClimbingController>();
            root.AddComponent<PlayerStamina>();
            root.AddComponent<PlayerRespawn>();
            var visual = root.AddComponent<PlayerCharacterVisual>();

            var stats = AssetDatabase.LoadAssetAtPath<PlayerCharacterStats>(BeanStatsPath);
            SerializedObjectAssign(mover, "stats", stats);
            SerializedObjectAssign(climber, "stats", stats);

            // Body: try Quaternius food egg/coconut, else fallback to stretched sphere primitive.
            var bodyMesh = TryLoadFoodMesh("Egg_Whole") ?? TryLoadFoodMesh("Coconut") ?? TryLoadFoodMesh("Avocado");
            GameObject body;
            if (bodyMesh != null)
            {
                body = Object.Instantiate(bodyMesh);
                body.name = "Body";
                body.transform.SetParent(root.transform, false);
                body.transform.localPosition = new Vector3(0, 0.5f, 0);
                body.transform.localScale = Vector3.one * 1.5f;
                foreach (var c in body.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
                foreach (var mr in body.GetComponentsInChildren<MeshRenderer>()) mr.sharedMaterial = bodyMat;
            }
            else
            {
                body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                body.name = "Body";
                body.transform.SetParent(root.transform, false);
                body.transform.localPosition = new Vector3(0, 0.5f, 0);
                body.transform.localScale = new Vector3(0.6f, 0.45f, 0.6f);
                Object.DestroyImmediate(body.GetComponent<Collider>());
                body.GetComponent<MeshRenderer>().sharedMaterial = bodyMat;
            }

            var limbs = new Transform[4];
            limbs[0] = MakeLimb("Arm_L", root.transform, new Vector3(-0.25f, 0.55f, 0.1f), new Vector3(0, 0, 90), bodyMat);
            limbs[1] = MakeLimb("Arm_R", root.transform, new Vector3( 0.25f, 0.55f, 0.1f), new Vector3(0, 0, -90), bodyMat);
            limbs[2] = MakeLimb("Leg_L", root.transform, new Vector3(-0.15f, 0.18f, 0f), Vector3.zero, bodyMat);
            limbs[3] = MakeLimb("Leg_R", root.transform, new Vector3( 0.15f, 0.18f, 0f), Vector3.zero, bodyMat);

            MakeEye("Eye_L", body.transform, new Vector3(-0.15f, 0.2f, 0.45f), eyeMat, pupilMat);
            MakeEye("Eye_R", body.transform, new Vector3( 0.15f, 0.2f, 0.45f), eyeMat, pupilMat);

            SerializedObjectAssign(visual, "body", body.transform);
            SerializedObjectAssignArray(visual, "limbs", limbs);

            AssetFactory.SavePrefab(root, PrefabPath);
            AssetDatabase.SaveAssets();
        }

        static GameObject TryLoadFoodMesh(string keyword)
        {
            string[] guids = AssetDatabase.FindAssets($"t:Model {keyword}", new[] { "Assets/Blech/_ThirdParty" });
            foreach (var g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                if (path.ToLowerInvariant().Contains(keyword.ToLowerInvariant()))
                    return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            return null;
        }

        static Transform MakeLimb(string name, Transform parent, Vector3 pos, Vector3 euler, Material mat)
        {
            var limb = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            limb.name = name;
            limb.transform.SetParent(parent, false);
            limb.transform.localPosition = pos;
            limb.transform.localEulerAngles = euler;
            limb.transform.localScale = new Vector3(0.1f, 0.15f, 0.1f);
            Object.DestroyImmediate(limb.GetComponent<Collider>());
            limb.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return limb.transform;
        }

        static void MakeEye(string name, Transform parent, Vector3 pos, Material eyeMat, Material pupilMat)
        {
            var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = name;
            eye.transform.SetParent(parent, false);
            eye.transform.localPosition = pos;
            eye.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            Object.DestroyImmediate(eye.GetComponent<Collider>());
            eye.GetComponent<MeshRenderer>().sharedMaterial = eyeMat;

            var pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pupil.name = "Pupil";
            pupil.transform.SetParent(eye.transform, false);
            pupil.transform.localPosition = new Vector3(0, 0, 0.3f);
            pupil.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
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
