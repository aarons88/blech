using UnityEngine;
using UnityEngine.UI;
using Blech.Player;

namespace Blech.UI
{
    public class StaminaUI : MonoBehaviour
    {
        [SerializeField] public Image fill;
        [SerializeField] public Color highColor = Color.green;
        [SerializeField] public Color lowColor = Color.red;
        [SerializeField] public float lowThreshold = 0.2f;
        [SerializeField] public float pulseSpeed = 4f;

        private PlayerStamina _stamina;

        private void Start()
        {
            _stamina = FindFirstObjectByType<PlayerStamina>();
            if (_stamina != null) { _stamina.OnStaminaChanged += OnChanged; OnChanged(_stamina.Current); }
        }

        private void OnDestroy()
        {
            if (_stamina != null) _stamina.OnStaminaChanged -= OnChanged;
        }

        private void OnChanged(float value)
        {
            if (fill == null) return;
            float n = _stamina != null ? _stamina.Normalized : 0f;
            fill.fillAmount = n;
            Color c = Color.Lerp(lowColor, highColor, Mathf.Clamp01(n / lowThreshold));
            if (n < lowThreshold) c *= (0.6f + 0.4f * Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed)));
            fill.color = c;
        }
    }
}
