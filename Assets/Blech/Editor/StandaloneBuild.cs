using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Blech.Editor
{
    public static class StandaloneBuild
    {
        static readonly string[] Scenes =
        {
            "Assets/Blech/Scenes/MainMenu.unity",
            "Assets/Blech/Scenes/MVP_VerticalSlice.unity"
        };

        public static void BuildMac()
        {
            var opts = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = "Builds/Blech_Mac/Blech.app",
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(opts);
            Debug.Log($"[Blech] Mac build: {report.summary.result}");
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        public static void BuildWindows()
        {
            var opts = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = "Builds/Blech_Windows/Blech.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(opts);
            Debug.Log($"[Blech] Windows build: {report.summary.result}");
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}
