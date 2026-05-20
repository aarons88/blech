using NUnit.Framework;
using UnityEngine;
using Blech.Player;

namespace Blech.Tests
{
    public class VisualMathTests
    {
        [Test]
        public void Bob_AtZero_IsZero()
            => Assert.AreEqual(0f, VisualMath.IdleBob(0f, 0.1f, 2f), 0.0001f);

        [Test]
        public void Bob_AtQuarterPeriod_IsAmplitude()
            => Assert.AreEqual(0.1f, VisualMath.IdleBob(0.25f / 2f, 0.1f, 2f), 0.001f);

        [Test]
        public void Squash_HighVelocity_StretchesX_SquashesY()
        {
            var s = VisualMath.WalkSquash(4f, 4f, 0.1f);
            Assert.Greater(s.x, 1f); Assert.Less(s.y, 1f);
        }
    }
}
