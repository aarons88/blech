using UnityEditor;
using UnityEngine;
using Blech.World;

namespace Blech.Editor
{
    public static class HazardPrefabBuilder
    {
        const string Dir = "Assets/Blech/Prefabs/Hazards";

        public static void BuildAll()
        {
            AssetFactory.EnsureFolder(Dir);
            BuildAcidPool();
            BuildAcidGeyser();
            BuildWindHazard();
            AssetDatabase.SaveAssets();
        }

        static void BuildAcidPool()
        {
            var root = new GameObject("AcidPool");

            var surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
            surface.name = "Surface";
            surface.transform.SetParent(root.transform, false);
            surface.transform.localScale = new Vector3(10, 0.2f, 10);
            Object.DestroyImmediate(surface.GetComponent<BoxCollider>());
            var acidMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Blech/Art/Materials/M_Acid_Surface.mat");
            if (acidMat != null) surface.GetComponent<MeshRenderer>().sharedMaterial = acidMat;

            var killGo = new GameObject("KillVolume");
            killGo.transform.SetParent(root.transform, false);
            var kc = killGo.AddComponent<BoxCollider>();
            kc.isTrigger = true;
            kc.size = new Vector3(10, 1, 10);
            var kv = killGo.AddComponent<KillVolume>();
            kv.cause = KillCause.Acid;

            var fxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Blech/Prefabs/FX/FX_Bubbles.prefab");
            if (fxPrefab != null)
            {
                var fxInstance = (GameObject)PrefabUtility.InstantiatePrefab(fxPrefab, root.transform);
                fxInstance.name = "Bubbles";
                fxInstance.transform.localPosition = new Vector3(0, 0.2f, 0);
                var visual = root.AddComponent<AcidHazardVisual>();
                var so = new SerializedObject(visual);
                so.FindProperty("ambientBubbles").objectReferenceValue = fxInstance.GetComponent<ParticleSystem>();
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            AssetFactory.SavePrefab(root, $"{Dir}/AcidPool.prefab");
        }

        static void BuildAcidGeyser()
        {
            var root = new GameObject("AcidGeyser");
            var marker = root.AddComponent<BoxCollider>();
            marker.isTrigger = true;
            marker.size = new Vector3(0.5f, 0.5f, 0.5f);
            var geyser = root.AddComponent<AcidGeyser>();

            var kill = new GameObject("KillVolume");
            kill.transform.SetParent(root.transform, false);
            var bc = kill.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(1, 6, 1);
            bc.center = new Vector3(0, 3, 0);
            kill.AddComponent<KillVolume>().cause = KillCause.Acid;
            kill.SetActive(false);

            var splashFxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Blech/Prefabs/FX/FX_AcidSplash.prefab");
            GameObject splash = null;
            if (splashFxPrefab != null)
            {
                splash = (GameObject)PrefabUtility.InstantiatePrefab(splashFxPrefab, root.transform);
                splash.name = "Splash";
            }

            var so = new SerializedObject(geyser);
            so.FindProperty("killVolumeRoot").objectReferenceValue = kill;
            if (splash != null) so.FindProperty("splashVfx").objectReferenceValue = splash.GetComponent<ParticleSystem>();
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetFactory.SavePrefab(root, $"{Dir}/AcidGeyser.prefab");
        }

        static void BuildWindHazard()
        {
            var root = new GameObject("WindHazard");
            var box = root.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(5, 5, 5);
            var wind = root.AddComponent<WindHazard>();

            var streaksPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Blech/Prefabs/FX/FX_WindStreaks.prefab");
            GameObject streaks = null;
            if (streaksPrefab != null)
            {
                streaks = (GameObject)PrefabUtility.InstantiatePrefab(streaksPrefab, root.transform);
                streaks.name = "Streaks";
            }

            var so = new SerializedObject(wind);
            if (streaks != null) so.FindProperty("streaksVfx").objectReferenceValue = streaks.GetComponent<ParticleSystem>();
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetFactory.SavePrefab(root, $"{Dir}/WindHazard.prefab");
        }
    }
}
