using UnityEditor;
using UnityEngine;
using Blech.Audio;

namespace Blech.Editor
{
    public static class MusicPrefabBuilder
    {
        public static void BuildAll()
        {
            AssetFactory.EnsureFolder("Assets/Blech/Prefabs");

            var root = new GameObject("MusicPlayer");
            var src = root.AddComponent<AudioSource>();
            src.loop = true;
            src.playOnAwake = true;
            src.spatialBlend = 0f;
            src.volume = 0.35f;

            var clip = FindMusicLoop();
            if (clip != null)
            {
                src.clip = clip;
                Debug.Log($"[Blech] Music track: {AssetDatabase.GetAssetPath(clip)}");
            }
            else
            {
                Debug.LogWarning("[Blech] No music loop found in _ThirdParty — MusicPlayer will be silent");
            }

            var player = root.AddComponent<MusicPlayer>();
            var so = new SerializedObject(player);
            so.FindProperty("source").objectReferenceValue = src;
            if (clip != null) so.FindProperty("clip").objectReferenceValue = clip;
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetFactory.SavePrefab(root, "Assets/Blech/Prefabs/MusicPlayer.prefab");
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Pick a track from Kenney's Music Loops folder. Prefer alphabetically-first
        /// for determinism; deliberately skip anything whose name screams "intro/sting"
        /// to avoid one-shot stingers being looped.
        /// </summary>
        static AudioClip FindMusicLoop()
        {
            const string root = "Assets/Blech/_ThirdParty";
            if (!AssetDatabase.IsValidFolder(root)) return null;

            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { root });
            string bestPath = null;
            foreach (var g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g).Replace('\\', '/');
                string lower = path.ToLowerInvariant();
                if (!lower.Contains("/music loops/")) continue;
                // Skip "Idents" subfolder (those are short jingles, not loop tracks)
                if (lower.Contains("/idents/")) continue;
                string stem = System.IO.Path.GetFileNameWithoutExtension(lower);
                if (stem.Contains("sting") || stem.Contains("intro") || stem.Contains("ident")) continue;

                if (bestPath == null || string.CompareOrdinal(path, bestPath) < 0)
                    bestPath = path;
            }
            return bestPath == null ? null : AssetDatabase.LoadAssetAtPath<AudioClip>(bestPath);
        }
    }
}
