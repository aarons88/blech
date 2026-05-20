using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Blech.UI;

namespace Blech.Editor
{
    public static class HudCanvasBuilder
    {
        public static GameObject BuildHud()
        {
            var canvasGo = new GameObject("HUD",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            BuildStaminaBar(canvasGo.transform);
            BuildCheckpointToast(canvasGo.transform);
            BuildTimerLabel(canvasGo.transform);
            BuildEndScreen(canvasGo.transform);

            var watcher = new GameObject("EscapeWatcher");
            watcher.AddComponent<EscapeToMainMenu>();

            return canvasGo;
        }

        static void BuildStaminaBar(Transform parent)
        {
            var bar = new GameObject("StaminaBar",
                typeof(RectTransform), typeof(CanvasGroup));
            bar.transform.SetParent(parent, false);
            var rt = bar.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0); rt.anchorMax = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 40); rt.sizeDelta = new Vector2(400, 24);

            var bg = new GameObject("Bg", typeof(Image));
            bg.transform.SetParent(bar.transform, false);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);

            var fillGo = new GameObject("Fill", typeof(Image));
            fillGo.transform.SetParent(bar.transform, false);
            var fillRt = fillGo.GetComponent<RectTransform>();
            fillRt.anchorMin = new Vector2(0, 0); fillRt.anchorMax = new Vector2(1, 1);
            fillRt.offsetMin = new Vector2(4, 4); fillRt.offsetMax = new Vector2(-4, -4);
            var fillImg = fillGo.GetComponent<Image>();
            fillImg.color = Color.green;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;

            var ui = bar.AddComponent<StaminaUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("fill").objectReferenceValue = fillImg;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildCheckpointToast(Transform parent)
        {
            var go = new GameObject("CheckpointToast", typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1); rt.anchorMax = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -80); rt.sizeDelta = new Vector2(600, 80);

            var labelGo = new GameObject("Label", typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero; labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero; labelRt.offsetMax = Vector2.zero;
            var label = labelGo.GetComponent<TextMeshProUGUI>();
            label.text = "Blechmark!";
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 48; label.color = Color.white;

            var toast = go.AddComponent<CheckpointToastUI>();
            var so = new SerializedObject(toast);
            so.FindProperty("label").objectReferenceValue = label;
            so.FindProperty("group").objectReferenceValue = go.GetComponent<CanvasGroup>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildTimerLabel(Transform parent)
        {
            var go = new GameObject("RunTimer", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1); rt.anchorMax = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-100, -40); rt.sizeDelta = new Vector2(180, 40);

            var labelGo = new GameObject("Label", typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero; labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero; labelRt.offsetMax = Vector2.zero;
            var label = labelGo.GetComponent<TextMeshProUGUI>();
            label.text = "00:00.0";
            label.alignment = TextAlignmentOptions.Right;
            label.fontSize = 32; label.color = Color.white;

            var ui = go.AddComponent<RunTimerUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("label").objectReferenceValue = label;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildEndScreen(Transform parent)
        {
            var go = new GameObject("EndScreen", typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var dim = new GameObject("Dim", typeof(Image));
            dim.transform.SetParent(go.transform, false);
            var dimRt = dim.GetComponent<RectTransform>();
            dimRt.anchorMin = Vector2.zero; dimRt.anchorMax = Vector2.one;
            dimRt.offsetMin = Vector2.zero; dimRt.offsetMax = Vector2.zero;
            dim.GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);

            var time = MakeText(go.transform, "TimeLabel", new Vector2(0, 80), "Time: 00:00.0", 48);
            var falls = MakeText(go.transform, "FallsLabel", new Vector2(0, 20), "Falls: 0", 36);
            var maxFall = MakeText(go.transform, "MaxFallLabel", new Vector2(0, -30), "Most dramatic fall: 0m", 32);

            var runAgain = MakeButton(go.transform, "RunAgain", new Vector2(-120, -130), "Run Again");
            var menu = MakeButton(go.transform, "MainMenu", new Vector2( 120, -130), "Main Menu");

            var ui = go.AddComponent<EndScreenUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("group").objectReferenceValue = go.GetComponent<CanvasGroup>();
            so.FindProperty("timeLabel").objectReferenceValue = time;
            so.FindProperty("fallsLabel").objectReferenceValue = falls;
            so.FindProperty("maxFallLabel").objectReferenceValue = maxFall;
            so.FindProperty("runAgainButton").objectReferenceValue = runAgain;
            so.FindProperty("mainMenuButton").objectReferenceValue = menu;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static TextMeshProUGUI MakeText(Transform parent, string name, Vector2 pos, string text, int size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(800, 60);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.text = text; t.alignment = TextAlignmentOptions.Center; t.fontSize = size; t.color = Color.white;
            return t;
        }

        static Button MakeButton(Transform parent, string name, Vector2 pos, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(200, 60);
            go.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            var labelGo = new GameObject("Label", typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero; labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero; labelRt.offsetMax = Vector2.zero;
            var t = labelGo.GetComponent<TextMeshProUGUI>();
            t.text = label; t.alignment = TextAlignmentOptions.Center; t.fontSize = 28; t.color = Color.white;

            return go.GetComponent<Button>();
        }
    }
}
