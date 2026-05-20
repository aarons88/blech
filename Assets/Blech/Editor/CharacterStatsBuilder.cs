using UnityEditor;
using Blech.Player;

namespace Blech.Editor
{
    public static class CharacterStatsBuilder
    {
        private const string Dir = "Assets/Blech/ScriptableObjects/Characters";

        public static void BuildAll()
        {
            Build("Bean", s =>
            {
                s.displayName = "Bean";
                s.moveSpeed = 4.5f; s.jumpForce = 7f; s.maxStamina = 100f;
                s.staminaDrainPerSecond = 12f; s.staminaRegenPerSecond = 30f;
                s.climbSpeed = 2.5f; s.gripStrength = 5f; s.slipResistance = 1f;
            });
            Build("Pea", s =>
            {
                s.displayName = "Pea";
                s.moveSpeed = 5.2f; s.jumpForce = 7.5f; s.maxStamina = 80f;
                s.staminaDrainPerSecond = 14f; s.staminaRegenPerSecond = 32f;
                s.climbSpeed = 3f; s.gripStrength = 4f; s.slipResistance = 0.9f;
            });
            Build("CarrotChunk", s =>
            {
                s.displayName = "Carrot Chunk";
                s.moveSpeed = 3.8f; s.jumpForce = 6.5f; s.maxStamina = 110f;
                s.staminaDrainPerSecond = 10f; s.staminaRegenPerSecond = 28f;
                s.climbSpeed = 2.2f; s.gripStrength = 7f; s.slipResistance = 1.2f;
            });
            AssetDatabase.SaveAssets();
        }

        private static void Build(string name, System.Action<PlayerCharacterStats> configure)
        {
            string path = $"{Dir}/{name}.asset";
            var asset = AssetFactory.CreateScriptableObjectAsset<PlayerCharacterStats>(path);
            configure(asset);
            EditorUtility.SetDirty(asset);
        }
    }
}
