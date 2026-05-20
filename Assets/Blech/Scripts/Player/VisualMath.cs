using UnityEngine;

namespace Blech.Player
{
    public static class VisualMath
    {
        public static float IdleBob(float time, float amp, float freq)
            => Mathf.Sin(time * freq * Mathf.PI * 2f) * amp;

        public static Vector3 WalkSquash(float velocityMag, float walkSpeed, float amount)
        {
            float t = Mathf.Clamp01(velocityMag / Mathf.Max(walkSpeed, 0.0001f));
            return new Vector3(1f + amount * t, 1f - amount * t, 1f + amount * t);
        }
    }
}
