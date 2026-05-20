using NUnit.Framework;
using UnityEngine;
using Blech.Player;

namespace Blech.Tests
{
    public class MovementMathTests
    {
        [Test]
        public void HorizontalVelocity_ForwardInput_AlignsCameraForward()
        {
            var v = MovementMath.HorizontalVelocity(new Vector2(0, 1), Vector3.forward, 5f);
            Assert.AreEqual(0f, v.x, 0.001f);
            Assert.AreEqual(5f, v.z, 0.001f);
        }

        [Test]
        public void HorizontalVelocity_NoInput_IsZero()
        {
            Assert.AreEqual(Vector3.zero, MovementMath.HorizontalVelocity(Vector2.zero, Vector3.forward, 5f));
        }

        [Test]
        public void ApplyGravity_AccumulatesNegativeY()
        {
            Assert.AreEqual(-10f, MovementMath.ApplyGravity(0f, -20f, 0.5f, false), 0.001f);
        }

        [Test]
        public void ApplyGravity_Grounded_ReturnsSmallStickForce()
        {
            Assert.AreEqual(-2f, MovementMath.ApplyGravity(-30f, -20f, 1f, true), 0.001f);
        }
    }
}
