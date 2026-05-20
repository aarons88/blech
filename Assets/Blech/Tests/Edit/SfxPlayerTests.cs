using NUnit.Framework;
using Blech.Audio;

namespace Blech.Tests
{
    public class SfxPlayerTests
    {
        [Test]
        public void SfxId_HasAllRequiredEvents()
        {
            string[] expected = {
                "Footstep", "Jump", "WallGrab", "Slip", "FallYell",
                "MucusSquelch", "AcidBubble", "AcidSplash", "WindWarning",
                "Checkpoint", "FinishFanfare"
            };
            foreach (var name in expected)
                Assert.IsTrue(System.Enum.IsDefined(typeof(SfxId), name), $"Missing {name}");
        }

        [Test]
        public void Play_WithoutInstance_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => SfxPlayer.Play(SfxId.Jump));
        }
    }
}
