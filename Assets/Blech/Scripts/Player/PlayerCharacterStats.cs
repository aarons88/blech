using UnityEngine;

namespace Blech.Player
{
    [CreateAssetMenu(menuName = "Blech/Player Character Stats", fileName = "NewCharacterStats")]
    public class PlayerCharacterStats : ScriptableObject
    {
        [Header("Identity")]
        public string displayName = "Bean";

        [Header("Movement")]
        public float moveSpeed = 4.5f;
        public float jumpForce = 7f;
        public float gravity = -20f;

        [Header("Stamina")]
        public float maxStamina = 100f;
        public float staminaDrainPerSecond = 12f;
        public float staminaRegenPerSecond = 30f;

        [Header("Climbing")]
        public float climbSpeed = 2.5f;
        public float gripStrength = 5f;
        public float slipResistance = 1f;
    }
}
