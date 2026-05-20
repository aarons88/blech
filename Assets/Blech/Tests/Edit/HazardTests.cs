using NUnit.Framework;
using UnityEngine;
using Blech.World;

namespace Blech.Tests
{
    public class HazardTests
    {
        [Test]
        public void AcidGeyser_CyclesPhases()
        {
            var go = new GameObject();
            go.AddComponent<BoxCollider>().isTrigger = true;
            var g = go.AddComponent<AcidGeyser>();
            g.idleDuration = 1f; g.warningDuration = 1f; g.eruptDuration = 1f; g.cooldownDuration = 1f;

            Assert.AreEqual(AcidGeyser.Phase.Idle, g.CurrentPhase);
            g.TickForTest(1f); Assert.AreEqual(AcidGeyser.Phase.Warning, g.CurrentPhase);
            g.TickForTest(1f); Assert.AreEqual(AcidGeyser.Phase.Erupt, g.CurrentPhase);
            g.TickForTest(1f); Assert.AreEqual(AcidGeyser.Phase.Cooldown, g.CurrentPhase);
            g.TickForTest(1f); Assert.AreEqual(AcidGeyser.Phase.Idle, g.CurrentPhase);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void WindHazard_CyclesPhases()
        {
            var go = new GameObject();
            go.AddComponent<BoxCollider>().isTrigger = true;
            var w = go.AddComponent<WindHazard>();
            w.idleDuration = 1f; w.warningDuration = 1f; w.gustDuration = 1f; w.cooldownDuration = 1f;

            w.TickForTest(1f); Assert.AreEqual(WindHazard.Phase.Warning, w.CurrentPhase);
            w.TickForTest(1f); Assert.AreEqual(WindHazard.Phase.Gust, w.CurrentPhase);
            w.TickForTest(1f); Assert.AreEqual(WindHazard.Phase.Cooldown, w.CurrentPhase);
            w.TickForTest(1f); Assert.AreEqual(WindHazard.Phase.Idle, w.CurrentPhase);
            Object.DestroyImmediate(go);
        }
    }
}
