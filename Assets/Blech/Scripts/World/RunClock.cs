using UnityEngine;

namespace Blech.World
{
    public class RunClock : MonoBehaviour
    {
        public bool ticking = true;
        private void Update() { if (ticking) RunStatsTracker.TickTime(Time.deltaTime); }
    }
}
