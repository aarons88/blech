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

            // Map each SfxId to (pack-name-keyword, clip-name-keyword). FindClip searches recursively
            // under Assets/Blech/_ThirdParty/ matching BOTH keywords (case-insensitive).
            var entries = new List<SfxEntry>
            {
                MakeEntry(SfxId.Footstep,      FindClip("RPG", "footstep")),
                MakeEntry(SfxId.Jump,          FindClip("Voiceover", "jump")),
                MakeEntry(SfxId.WallGrab,      FindClip("Impact", "wood")),
                MakeEntry(SfxId.Slip,          FindClip("Impact", "soft")),
                MakeEntry(SfxId.FallYell,      FindClip("Voiceover", "hurt")),
                MakeEntry(SfxId.MucusSquelch,  FindClip("Impact", "wet")),
                MakeEntry(SfxId.AcidBubble,    FindClip("Sci-Fi", "bubble") ?? FindClip("Sci-Fi", "laser")),
                MakeEntry(SfxId.AcidSplash,    FindClip("Impact", "wet")),
                MakeEntry(SfxId.WindWarning,   FindClip("Sci-Fi", "wind") ?? FindClip("Sci-Fi", "whoosh")),
                MakeEntry(SfxId.Checkpoint,    FindClip("Digital", "powerup") ?? FindClip("UI", "click")),
                MakeEntry(SfxId.FinishFanfare, FindClip("Music Jingles", "") ?? FindClip("Music", "fanfare")),
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

        // Search whole _ThirdParty/ tree for an AudioClip whose path contains both keywords.
        static AudioClip FindClip(string packKeyword, string clipKeyword)
        {
            const string root = "Assets/Blech/_ThirdParty";
            if (!AssetDatabase.IsValidFolder(root)) return null;
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { root });
            packKeyword = (packKeyword ?? "").ToLowerInvariant();
            clipKeyword = (clipKeyword ?? "").ToLowerInvariant();

            AudioClip packFallback = null;
            foreach (var g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g).ToLowerInvariant();
                bool packMatch = packKeyword.Length == 0 || path.Contains(packKeyword);
                bool clipMatch = clipKeyword.Length == 0 || path.Contains(clipKeyword);
                if (packMatch && clipMatch)
                    return AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(g));
                if (packMatch && packFallback == null)
                    packFallback = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(g));
            }
            return packFallback;
        }
    }
}
