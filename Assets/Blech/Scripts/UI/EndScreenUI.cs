using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Blech.World;

namespace Blech.UI
{
    public class EndScreenUI : MonoBehaviour
    {
        [SerializeField] public CanvasGroup group;
        [SerializeField] public TMP_Text timeLabel;
        [SerializeField] public TMP_Text fallsLabel;
        [SerializeField] public TMP_Text maxFallLabel;
        [SerializeField] public Button runAgainButton;
        [SerializeField] public Button mainMenuButton;
        [SerializeField] public ParticleSystem confetti;
        [SerializeField] public string mainMenuSceneName = "MainMenu";
        [SerializeField] public RunClock runClock;

        private void Awake()
        {
            if (group != null) { group.alpha = 0f; group.blocksRaycasts = false; group.interactable = false; }
        }

        private void Start()
        {
            foreach (var f in FindObjectsByType<FinishTrigger>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                f.OnRouteComplete += Show;
            if (runAgainButton != null) runAgainButton.onClick.AddListener(RunAgain);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ToMainMenu);
        }

        public void Show()
        {
            if (runClock != null) runClock.ticking = false;
            float t = RunStatsTracker.ElapsedSeconds;
            int m = Mathf.FloorToInt(t / 60f);
            float s = t - m * 60;
            if (timeLabel != null)    timeLabel.text    = $"Time: {m:00}:{s:00.0}";
            if (fallsLabel != null)   fallsLabel.text   = $"Falls: {RunStatsTracker.FallCount}";
            if (maxFallLabel != null) maxFallLabel.text = $"Most dramatic fall: {RunStatsTracker.MaxFallHeight:0.0}m";
            if (confetti != null) confetti.Play();
            if (group != null) { group.alpha = 1f; group.blocksRaycasts = true; group.interactable = true; }
        }

        private void RunAgain()
        {
            RunStatsTracker.ResetForTests();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void ToMainMenu()
        {
            RunStatsTracker.ResetForTests();
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
