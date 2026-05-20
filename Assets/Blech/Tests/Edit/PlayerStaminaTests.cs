using NUnit.Framework;
using UnityEngine;
using Blech.Player;

namespace Blech.Tests
{
    public class PlayerStaminaTests
    {
        private PlayerCharacterStats MakeStats()
        {
            var s = ScriptableObject.CreateInstance<PlayerCharacterStats>();
            s.maxStamina = 100f; s.staminaDrainPerSecond = 10f; s.staminaRegenPerSecond = 20f;
            return s;
        }

        [Test]
        public void Initial_IsMax()
        {
            var go = new GameObject(); var p = go.AddComponent<PlayerStamina>(); p.Configure(MakeStats());
            Assert.AreEqual(100f, p.Current); Object.DestroyImmediate(go);
        }

        [Test]
        public void Tick_DrainsWhenSpending()
        {
            var go = new GameObject(); var p = go.AddComponent<PlayerStamina>(); p.Configure(MakeStats());
            p.Tick(1f, true, 1f); Assert.AreEqual(90f, p.Current, 0.001f); Object.DestroyImmediate(go);
        }

        [Test]
        public void Tick_DrainScalesBySlipMultiplier()
        {
            var go = new GameObject(); var p = go.AddComponent<PlayerStamina>(); p.Configure(MakeStats());
            p.Tick(1f, true, 2f); Assert.AreEqual(80f, p.Current, 0.001f); Object.DestroyImmediate(go);
        }

        [Test]
        public void Tick_RegensCappedAtMax()
        {
            var go = new GameObject(); var p = go.AddComponent<PlayerStamina>(); p.Configure(MakeStats());
            p.Tick(1f, true, 1f); p.Tick(1f, false, 1f);
            Assert.AreEqual(100f, p.Current, 0.001f); Object.DestroyImmediate(go);
        }

        [Test]
        public void Tick_FiresOnZero()
        {
            var go = new GameObject(); var p = go.AddComponent<PlayerStamina>(); p.Configure(MakeStats());
            bool fired = false; p.OnStaminaZero += () => fired = true;
            p.Tick(100f, true, 1f);
            Assert.IsTrue(fired); Assert.AreEqual(0f, p.Current); Object.DestroyImmediate(go);
        }
    }
}
