using UnityEngine;

namespace Blech.World
{
    public static class CheckpointManager
    {
        public static Transform CurrentSpawn { get; private set; }
        public static void SetSpawn(Transform t) => CurrentSpawn = t;
        public static void ResetForTests() => CurrentSpawn = null;
    }
}
