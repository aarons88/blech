using System.Collections;
using TMPro;
using UnityEngine;
using Blech.World;

namespace Blech.UI
{
    public class CheckpointToastUI : MonoBehaviour
    {
        [SerializeField] public TMP_Text label;
        [SerializeField] public CanvasGroup group;
        [SerializeField] public float showSeconds = 2f;
        [SerializeField] public float fadeSeconds = 0.4f;

        private void Awake() { if (group != null) group.alpha = 0f; }

        private void Start()
        {
            foreach (var cp in FindObjectsByType<Checkpoint>(FindObjectsSortMode.None))
                cp.OnRegistered += cp2 => Show($"{cp2.displayName}!");
            foreach (var f in FindObjectsByType<FinishTrigger>(FindObjectsSortMode.None))
                f.OnRouteComplete += () => Show("PEAK reached!");
        }

        public void Show(string text)
        {
            if (label == null || group == null) return;
            label.text = text;
            StopAllCoroutines();
            StartCoroutine(Routine());
        }

        private IEnumerator Routine()
        {
            float t = 0f;
            while (t < fadeSeconds) { t += Time.deltaTime; group.alpha = t / fadeSeconds; yield return null; }
            group.alpha = 1f;
            yield return new WaitForSeconds(showSeconds);
            t = 0f;
            while (t < fadeSeconds) { t += Time.deltaTime; group.alpha = 1f - (t / fadeSeconds); yield return null; }
            group.alpha = 0f;
        }
    }
}
