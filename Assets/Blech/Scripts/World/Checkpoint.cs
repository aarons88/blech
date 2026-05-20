using System;
using UnityEngine;
using Blech.Player;

namespace Blech.World
{
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        public string displayName = "Blechmark";
        public event Action<Checkpoint> OnRegistered;
        private bool _registered;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerRespawn _)) return;
            RegisterFromTest();
        }

        public void RegisterFromTest()
        {
            if (_registered) return;
            _registered = true;
            CheckpointManager.SetSpawn(transform);
            OnRegistered?.Invoke(this);
            Blech.Audio.SfxPlayer.Play(Blech.Audio.SfxId.Checkpoint);
        }
    }
}
