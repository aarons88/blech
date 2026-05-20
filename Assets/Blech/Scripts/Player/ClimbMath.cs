using UnityEngine;

namespace Blech.Player
{
    public static class ClimbMath
    {
        public static Vector3 ProjectInputOnWall(Vector2 input, Vector3 wallNormal, float speed)
        {
            Vector3 right = Vector3.Cross(Vector3.up, wallNormal).normalized;
            Vector3 wallUp = Vector3.Cross(wallNormal, right).normalized;
            Vector3 v = right * input.x + wallUp * input.y;
            return v.sqrMagnitude > 0.0001f ? v.normalized * speed : Vector3.zero;
        }

        public static Vector3 JumpOffWallVelocity(Vector3 wallNormal, float awayForce, float upForce)
            => wallNormal * awayForce + Vector3.up * upForce;
    }
}
