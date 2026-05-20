using System.IO;
using UnityEditor;
using UnityEngine;

namespace Blech.Editor
{
    public static class AssetFactory
    {
        public static T CreateScriptableObjectAsset<T>(string path) where T : ScriptableObject
        {
            EnsureFolder(Path.GetDirectoryName(path));
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        public static Material CreateOrUpdateMaterial(string path, Shader shader)
        {
            EnsureFolder(Path.GetDirectoryName(path));
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                if (shader != null) existing.shader = shader;
                return existing;
            }
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            var mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        public static GameObject SavePrefab(GameObject sourceInScene, string path)
        {
            EnsureFolder(Path.GetDirectoryName(path));
            var prefab = PrefabUtility.SaveAsPrefabAsset(sourceInScene, path);
            Object.DestroyImmediate(sourceInScene);
            return prefab;
        }

        public static void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) || AssetDatabase.IsValidFolder(folderPath)) return;
            string parent = Path.GetDirectoryName(folderPath).Replace('\\', '/');
            string name = Path.GetFileName(folderPath);
            EnsureFolder(parent);
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder(parent, name);
        }
    }
}
