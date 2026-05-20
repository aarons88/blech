using NUnit.Framework;
using UnityEngine;
using Blech.Player;

namespace Blech.Tests
{
    public class PlayerCharacterStatsTests
    {
        [Test]
        public void DefaultStats_HaveSensibleValues()
        {
            var stats = ScriptableObject.CreateInstance<PlayerCharacterStats>();
            Assert.AreEqual("Bean", stats.displayName);
            Assert.Greater(stats.moveSpeed, 0f);
            Assert.Greater(stats.jumpForce, 0f);
            Assert.Greater(stats.maxStamina, 0f);
            Assert.Greater(stats.staminaDrainPerSecond, 0f);
            Assert.Greater(stats.staminaRegenPerSecond, 0f);
            Assert.GreaterOrEqual(stats.gripStrength, 0f);
        }
    }
}
