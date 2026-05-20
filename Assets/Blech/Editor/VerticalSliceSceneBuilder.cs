using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Blech.World;

namespace Blech.Editor
{
    public static class VerticalSliceSceneBuilder
    {
        const string ScenePath = "Assets/Blech/Scenes/MVP_VerticalSlice.unity";

        public static void Build()
        {
            AssetFactory.EnsureFolder("Assets/Blech/Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.name == "Main Camera") Object.DestroyImmediate(go);
                else if (go.name == "Directional Light")
                {
                    var l = go.GetComponent<Light>();
                    l.color = new Color(1f, 0.9f, 0.85f);
                    l.intensity = 1.2f;
                }
            }

            new GameObject("RunClock").AddComponent<RunClock>();

            // Player
            var beanPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Blech/Prefabs/Player/Bean.prefab");
            GameObject bean = null;
            if (beanPrefab != null)
            {
                bean = (GameObject)PrefabUtility.InstantiatePrefab(beanPrefab);
                bean.transform.position = new Vector3(0, 1f, 1f);
            }
            else
            {
                Debug.LogWarning("[Blech] Bean prefab not found; player will be absent from scene");
            }

            // Cinemachine 3 camera setup
            var camHost = new GameObject("MainCamera");
            camHost.tag = "MainCamera";
            var cam = camHost.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.02f, 0.06f);
            camHost.AddComponent<AudioListener>();
            camHost.AddComponent<Unity.Cinemachine.CinemachineBrain>();

            if (bean != null)
            {
                var vcamGo = new GameObject("CinemachineCamera");
                var cmCam = vcamGo.AddComponent<Unity.Cinemachine.CinemachineCamera>();
                cmCam.Follow = bean.transform;
                cmCam.LookAt = bean.transform;
                vcamGo.AddComponent<Unity.Cinemachine.CinemachineOrbitalFollow>();
                vcamGo.AddComponent<Unity.Cinemachine.CinemachineRotationComposer>();
            }

            // Biomes
            var worldRoot = new GameObject("World");
            BiomeBuilder.BuildIntestine(worldRoot.transform);
            BiomeBuilder.BuildStomach(worldRoot.transform);
            BiomeBuilder.BuildThroat(worldRoot.transform);
            BiomeBuilder.BuildMouth(worldRoot.transform);

            // HUD canvas
            HudCanvasBuilder.BuildHud();

            // EventSystem for UI input
            var es = new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));

            // SfxPlayer (DontDestroyOnLoad)
            var sfx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Blech/Prefabs/SfxPlayer.prefab");
            if (sfx != null) PrefabUtility.InstantiatePrefab(sfx);

            // Floor kill volume
            var killFloor = new GameObject("KillFloor");
            var kfBc = killFloor.AddComponent<BoxCollider>();
            kfBc.isTrigger = true;
            kfBc.size = new Vector3(200, 1, 200);
            killFloor.transform.position = new Vector3(0, -20, 0);
            killFloor.AddComponent<KillVolume>().cause = KillCause.Pit;

            EditorSceneManager.SaveScene(scene, ScenePath);
        }
    }
}
