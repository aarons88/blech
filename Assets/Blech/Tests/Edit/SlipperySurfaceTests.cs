using NUnit.Framework;
using UnityEngine;
using Blech.World;

namespace Blech.Tests
{
    public class SlipperySurfaceTests
    {
        [Test]
        public void Defaults()
        {
            var go = new GameObject();
            var s = go.AddComponent<SlipperySurface>();
            Assert.AreEqual(2f, s.slipMultiplier);
            Assert.AreEqual(1f, s.slideRate);
            Object.DestroyImmediate(go);
        }
    }
}
