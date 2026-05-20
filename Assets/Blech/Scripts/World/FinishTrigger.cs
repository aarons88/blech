using System;
using UnityEngine;
using Blech.Player;

namespace Blech.World
{
    [RequireComponent(typeof(Collider))]
    public class FinishTrigger : MonoBehaviour
    {
        public event Action OnRouteComplete;
        private bool _fired;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerRespawn _)) return;
            RaiseFromTest();
        }

        public void RaiseFromTest()
        {
            if (_fired) return;
            _fired = true;
            OnRouteComplete?.Invoke();
            Blech.Audio.SfxPlayer.Play(Blech.Audio.SfxId.FinishFanfare);
        }
    }
}
