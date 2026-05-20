using NUnit.Framework;
using UnityEngine;
using Blech.World;

namespace Blech.Tests
{
    public class ClimbableSurfaceTests
    {
        [Test]
        public void Grip_DefaultsToOne()
        {
            var go = new GameObject();
            var c = go.AddComponent<ClimbableSurface>();
            Assert.AreEqual(1f, c.grip);
            Object.DestroyImmediate(go);
        }
    }
}
