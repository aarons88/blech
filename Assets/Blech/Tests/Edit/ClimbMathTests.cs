using NUnit.Framework;
using UnityEngine;
using Blech.Player;

namespace Blech.Tests
{
    public class ClimbMathTests
    {
        [Test]
        public void ProjectInputOnWall_VerticalInput_GivesUpward()
        {
            var v = ClimbMath.ProjectInputOnWall(new Vector2(0, 1), Vector3.back, 2f);
            Assert.AreEqual(2f, v.y, 0.001f);
            Assert.AreEqual(0f, v.x, 0.001f);
            Assert.AreEqual(0f, v.z, 0.001f);
        }

        [Test]
        public void ProjectInputOnWall_HorizontalInput_GivesSideways()
        {
            var v = ClimbMath.ProjectInputOnWall(new Vector2(1, 0), Vector3.back, 2f);
            Assert.AreEqual(-2f, v.x, 0.001f);
            Assert.AreEqual(0f, v.y, 0.001f);
        }

        [Test]
        public void JumpOffWall_AddsAwayAndUp()
        {
            var v = ClimbMath.JumpOffWallVelocity(Vector3.back, 5f, 3f);
            Assert.AreEqual(0f, v.x, 0.001f);
            Assert.AreEqual(3f, v.y, 0.001f);
            Assert.AreEqual(-5f, v.z, 0.001f);
        }
    }
}
