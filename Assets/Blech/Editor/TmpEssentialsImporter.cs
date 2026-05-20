using System.IO;
using UnityEditor;
using UnityEngine;

namespace Blech.Editor
{
    /// <summary>
    /// Auto-imports the TextMeshPro Essential Resources (default font, shaders, settings)
    /// the first time the project is opened. Without these, TMP renders text invisibly.
    /// </summary>
    [InitializeOnLoad]
    public static class TmpEssentialsImporter
    {
        const string MarkerFolder = "Assets/TextMesh Pro";

        static TmpEssentialsImporter()
        {
            // Defer until Unity is past initial load
            EditorApplication.delayCall += EnsureImported;
        }

        [MenuItem("Blech/Build/TMP Essentials")]
        public static void ImportNow()
        {
            EnsureImported(forceReimport: true);
        }

        static void EnsureImported() => EnsureImported(false);

        static void EnsureImported(bool forceReimport)
        {
            if (!forceReimport && AssetDatabase.IsValidFolder(MarkerFolder))
                return;

            string packagePath = FindTmpEssentialsPackage();
            if (string.IsNullOrEmpty(packagePath))
            {
                Debug.LogWarning("[Blech] TMP Essential Resources.unitypackage not found in any package cache. " +
                                 "Open the project in Unity and use Window > TextMeshPro > Import TMP Essential Resources.");
                return;
            }

            Debug.Log($"[Blech] Importing TMP Essential Resources from {packagePath}");
            AssetDatabase.ImportPackage(packagePath, interactive: false);
        }

        static string FindTmpEssentialsPackage()
        {
            string[] roots = { "Library/PackageCache", "Packages" };
            foreach (var root in roots)
            {
                if (!Directory.Exists(root)) continue;
                try
                {
                    var matches = Directory.GetFiles(root, "TMP Essential Resources.unitypackage", SearchOption.AllDirectories);
                    if (matches.Length > 0) return matches[0];
                }
                catch { /* ignore unreadable subtrees */ }
            }
            return null;
        }
    }
}
