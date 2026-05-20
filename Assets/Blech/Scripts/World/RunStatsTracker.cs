using System.Collections.Generic;

namespace Blech.World
{
    public static class RunStatsTracker
    {
        public static int FallCount { get; private set; }
        public static float MaxFallHeight { get; private set; }
        public static float ElapsedSeconds { get; private set; }
        private static readonly Dictionary<KillCause, int> _causes = new();

        public static int CountForCause(KillCause c) => _causes.TryGetValue(c, out int n) ? n : 0;

        public static void RecordKill(KillCause cause)
        {
            FallCount++;
            _causes[cause] = CountForCause(cause) + 1;
        }

        public static void RecordFallHeight(float h) { if (h > MaxFallHeight) MaxFallHeight = h; }
        public static void TickTime(float dt) => ElapsedSeconds += dt;

        public static void ResetForTests()
        {
            FallCount = 0; MaxFallHeight = 0; ElapsedSeconds = 0; _causes.Clear();
        }
    }
}
