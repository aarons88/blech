using System;
using UnityEngine;
using Blech.World;

namespace Blech.Player
{
    public class PlayerRespawn : MonoBehaviour
    {
        public event Action<KillCause> OnKill;
        private CharacterController _cc;
        private PlayerInput _input;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInput>();
            if (_input != null) _input.RespawnPressed += OnRespawnPressed;
        }

        private void OnDestroy()
        {
            if (_input != null) _input.RespawnPressed -= OnRespawnPressed;
        }

        private void OnRespawnPressed() => Kill(KillCause.OutOfBounds);

        public virtual void Kill(KillCause cause)
        {
            OnKill?.Invoke(cause);
            RunStatsTracker.RecordKill(cause);
            Blech.Audio.SfxPlayer.Play(Blech.Audio.SfxId.FallYell);

            var spawn = CheckpointManager.CurrentSpawn;
            if (spawn == null) return;
            if (_cc != null) _cc.enabled = false;
            transform.position = spawn.position;
            transform.rotation = spawn.rotation;
            if (_cc != null) _cc.enabled = true;
        }
    }
}
