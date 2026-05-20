using UnityEngine;

namespace Blech.Player
{
    public static class MovementMath
    {
        public static Vector3 HorizontalVelocity(Vector2 input, Vector3 cameraForward, float speed)
        {
            if (input.sqrMagnitude < 0.0001f) return Vector3.zero;
            Vector3 forward = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            Vector3 right = new Vector3(forward.z, 0, -forward.x);
            Vector3 dir = (forward * input.y + right * input.x).normalized;
            return dir * speed;
        }

        public static float ApplyGravity(float currentY, float gravity, float dt, bool isGrounded)
        {
            if (isGrounded) return -2f;
            return currentY + gravity * dt;
        }
    }
}
