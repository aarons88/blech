using System;
using UnityEngine;

namespace Blech.Player
{
    public class PlayerStamina : MonoBehaviour
    {
        private PlayerCharacterStats _stats;
        private float _current;
        private bool _wasZero;

        public float Current => _current;
        public float Normalized => _stats == null ? 0f : _current / _stats.maxStamina;

        public event Action<float> OnStaminaChanged;
        public event Action OnStaminaZero;

        public void Configure(PlayerCharacterStats stats)
        {
            _stats = stats;
            _current = stats.maxStamina;
            _wasZero = false;
            OnStaminaChanged?.Invoke(_current);
        }

        public void Tick(float dt, bool spending, float slipMultiplier)
        {
            if (_stats == null) return;
            _current += spending
                ? -_stats.staminaDrainPerSecond * slipMultiplier * dt
                :  _stats.staminaRegenPerSecond * dt;
            _current = Mathf.Clamp(_current, 0f, _stats.maxStamina);
            OnStaminaChanged?.Invoke(_current);

            if (_current <= 0f && !_wasZero) { _wasZero = true; OnStaminaZero?.Invoke(); }
            else if (_current > 0f) _wasZero = false;
        }
    }
}
