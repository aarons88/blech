using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Blech.Audio;

namespace Blech.Editor
{
    public static class SfxPrefabBuilder
    {
        public static void BuildAll()
        {
            AssetFactory.EnsureFolder("Assets/Blech/Prefabs");

            var root = new GameObject("SfxPlayer");
            var src = root.AddComponent<AudioSource>();
            src.spatialBlend = 0f; src.playOnAwake = false;

            var player = root.AddComponent<SfxPlayer>();

            // Each entry is (SfxId, Kenney subfolder anchor, filename keyword).
            // Subfolder anchor is matched as "/<keyword>/" in the asset path (case-insensitive).
            // Filename keyword is matched as substring in the file stem.
            // If no match, the entry is silent rather than wrong-sounding.
            var entries = new List<SfxEntry>
            {
                MakeEntry(SfxId.Footstep,      FindClip("rpg audio", "footstep")),
                MakeEntry(SfxId.Jump,          FindClip("voiceover pack", "jump")
                                            ?? FindClip("voiceover pack", "yah")
                                            ?? FindClip("voiceover pack", "ha")),
                MakeEntry(SfxId.WallGrab,      FindClip("impact sounds", "wood")
                                            ?? FindClip("impact sounds", "thump")),
                MakeEntry(SfxId.Slip,          FindClip("impact sounds", "soft")
                                            ?? FindClip("foley", "slide")),
                MakeEntry(SfxId.FallYell,      FindClip("voiceover pack", "ouch")
                                            ?? FindClip("voiceover pack", "hurt")
                                            ?? FindClip("voiceover pack", "uh")),
                MakeEntry(SfxId.MucusSquelch,  FindClip("impact sounds", "wet")
                                            ?? FindClip("foley", "wet")),
                MakeEntry(SfxId.AcidBubble,    FindClip("sci-fi sounds", "bubble")
                                            ?? FindClip("foley", "bubble")),
                MakeEntry(SfxId.AcidSplash,    FindClip("impact sounds", "splash")
                                            ?? FindClip("impact sounds", "wet")),
                MakeEntry(SfxId.WindWarning,   FindClip("sci-fi sounds", "wind")
                                            ?? FindClip("sci-fi sounds", "whoosh")),
                MakeEntry(SfxId.Checkpoint,    FindClip("digital audio", "powerup")
                                            ?? FindClip("digital audio", "pickup")
                                            ?? FindClip("ui audio", "click")),
                MakeEntry(SfxId.FinishFanfare, FindClip("music jingles", "")),
            };

            var so = new SerializedObject(player);
            so.FindProperty("source").objectReferenceValue = src;
            var arr = so.FindProperty("entries");
            arr.arraySize = entries.Count;
            for (int i = 0; i < entries.Count; i++)
            {
                var el = arr.GetArrayElementAtIndex(i);
                el.FindPropertyRelative("id").enumValueIndex = (int)entries[i].id;
                el.FindPropertyRelative("clip").objectReferenceValue = entries[i].clip;
                el.FindPropertyRelative("volume").floatValue = entries[i].volume;
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetFactory.SavePrefab(root, "Assets/Blech/Prefabs/SfxPlayer.prefab");
            AssetDatabase.SaveAssets();
        }

        static SfxEntry MakeEntry(SfxId id, AudioClip clip)
            => new SfxEntry { id = id, clip = clip, volume = 1f };

        /// <summary>
        /// Find an AudioClip whose asset path is inside the specified Kenney subfolder
        /// AND whose filename contains the keyword. Returns null on miss (silent is better
        /// than wrong). Skips files with "error" in the name unless explicitly requested.
        /// </summary>
        static AudioClip FindClip(string subfolderAnchor, string fileKeyword)
        {
            const string root = "Assets/Blech/_ThirdParty";
            if (!AssetDatabase.IsValidFolder(root)) return null;

            string subKey = (subfolderAnchor ?? "").ToLowerInvariant();
            string fileKey = (fileKeyword ?? "").ToLowerInvariant();
            bool blockErrors = !fileKey.Contains("error");

            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { root });
            foreach (var g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                string lowerPath = path.Replace('\\', '/').ToLowerInvariant();

                if (subKey.Length > 0 && !lowerPath.Contains("/" + subKey + "/")) continue;

                string fileStem = System.IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                if (blockErrors && fileStem.Contains("error")) continue;
                if (fileKey.Length > 0 && !fileStem.Contains(fileKey)) continue;

                return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }
            return null;
        }
    }
}
