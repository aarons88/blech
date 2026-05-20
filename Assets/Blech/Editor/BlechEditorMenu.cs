using UnityEditor;
using UnityEngine;

namespace Blech.Editor
{
    public static class BlechEditorMenu
    {
        public const string MenuRoot = "Blech/Build/";

        [MenuItem(MenuRoot + "All %#&b")]
        public static void BuildAll()
        {
            Debug.Log("[Blech] BuildAll start");
            BuildCharacterStats();
            BuildShadersAndMaterials();
            BuildParticles();
            BuildPlayerPrefab();
            BuildHazardPrefabs();
            BuildSfxPlayerPrefab();
            BuildMusicPlayerPrefab();
            BuildMainMenuScene();
            BuildVerticalSliceScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Blech] BuildAll done");
        }

        [MenuItem(MenuRoot + "Character Stats")]
        public static void BuildCharacterStats() => CharacterStatsBuilder.BuildAll();

        [MenuItem(MenuRoot + "Shaders and Materials")]
        public static void BuildShadersAndMaterials() => ShaderBuilder.BuildAll();

        [MenuItem(MenuRoot + "Particles")]
        public static void BuildParticles() => ParticleBuilder.BuildAll();

        [MenuItem(MenuRoot + "Player Prefab")]
        public static void BuildPlayerPrefab() => PlayerPrefabBuilder.BuildAll();

        [MenuItem(MenuRoot + "Hazard Prefabs")]
        public static void BuildHazardPrefabs() => HazardPrefabBuilder.BuildAll();

        [MenuItem(MenuRoot + "Sfx Player Prefab")]
        public static void BuildSfxPlayerPrefab() => SfxPrefabBuilder.BuildAll();

        [MenuItem(MenuRoot + "Music Player Prefab")]
        public static void BuildMusicPlayerPrefab() => MusicPrefabBuilder.BuildAll();

        [MenuItem(MenuRoot + "Main Menu Scene")]
        public static void BuildMainMenuScene() => MainMenuSceneBuilder.Build();

        [MenuItem(MenuRoot + "Vertical Slice Scene")]
        public static void BuildVerticalSliceScene() => VerticalSliceSceneBuilder.Build();
    }
}
