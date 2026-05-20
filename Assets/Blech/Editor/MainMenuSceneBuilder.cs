using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Blech.UI;

namespace Blech.Editor
{
    public static class MainMenuSceneBuilder
    {
        const string ScenePath = "Assets/Blech/Scenes/MainMenu.unity";

        public static void Build()
        {
            AssetFactory.EnsureFolder("Assets/Blech/Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            foreach (var go in scene.GetRootGameObjects())
                if (go.name == "Main Camera" || go.name == "Directional Light") Object.DestroyImmediate(go);

            var camObj = new GameObject("MainCamera", typeof(Camera), typeof(AudioListener));
            camObj.tag = "MainCamera";
            var cam = camObj.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.95f, 0.6f, 0.7f);

            // Canvas
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Title
            var title = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            title.transform.SetParent(canvasGo.transform, false);
            var titleRt = title.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 1); titleRt.anchorMax = new Vector2(0.5f, 1);
            titleRt.anchoredPosition = new Vector2(0, -200); titleRt.sizeDelta = new Vector2(900, 200);
            var t = title.GetComponent<TextMeshProUGUI>();
            t.text = "Blech"; t.fontSize = 180; t.alignment = TextAlignmentOptions.Center; t.color = Color.white;

            var menuObj = new GameObject("Menu");
            var menu = menuObj.AddComponent<MainMenu>();

            var play = MakeButton(canvasGo.transform, "PlayButton", new Vector2(0, -50), "Play");
            UnityEventTools.AddPersistentListener(play.onClick, menu.Play);

            var quit = MakeButton(canvasGo.transform, "QuitButton", new Vector2(0, -130), "Quit");
            UnityEventTools.AddPersistentListener(quit.onClick, menu.Quit);

            // EventSystem
            new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));

            // SfxPlayer (carries over via DontDestroyOnLoad)
            var sfx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Blech/Prefabs/SfxPlayer.prefab");
            if (sfx != null) PrefabUtility.InstantiatePrefab(sfx);

            EditorSceneManager.SaveScene(scene, ScenePath);

            // Build settings
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Blech/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Blech/Scenes/MVP_VerticalSlice.unity", true),
            };
        }

        static Button MakeButton(Transform parent, string name, Vector2 pos, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(260, 70);
            go.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero; labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero; labelRt.offsetMax = Vector2.zero;
            var t = labelGo.GetComponent<TextMeshProUGUI>();
            t.text = label; t.alignment = TextAlignmentOptions.Center; t.fontSize = 36; t.color = Color.white;

            return go.GetComponent<Button>();
        }
    }
}
