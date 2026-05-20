using TMPro;
using UnityEngine;
using Blech.World;

namespace Blech.UI
{
    public class RunTimerUI : MonoBehaviour
    {
        [SerializeField] public TMP_Text label;

        private void Update()
        {
            if (label == null) return;
            float t = RunStatsTracker.ElapsedSeconds;
            int m = Mathf.FloorToInt(t / 60f);
            float s = t - m * 60;
            label.text = $"{m:00}:{s:00.0}";
        }
    }
}
