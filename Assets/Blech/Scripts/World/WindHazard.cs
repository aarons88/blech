using System.Collections.Generic;
using UnityEngine;
using Blech.Player;

namespace Blech.World
{
    [RequireComponent(typeof(Collider))]
    public class WindHazard : MonoBehaviour
    {
        public enum Phase { Idle, Warning, Gust, Cooldown }

        public float idleDuration = 4f;
        public float warningDuration = 1.5f;
        public float gustDuration = 1.5f;
        public float cooldownDuration = 1f;
        public Vector3 gustDirection = Vector3.up;
        public float gustStrength = 6f;

        [SerializeField] private ParticleSystem streaksVfx;

        public Phase CurrentPhase { get; private set; } = Phase.Idle;
        private float _t;
        private readonly HashSet<PlayerMovementController> _players = new();
        private readonly HashSet<PlayerClimbingController> _climbers = new();

        private void Reset() { var c = GetComponent<Collider>(); if (c != null) c.isTrigger = true; }

        private void Update() => TickForTest(Time.deltaTime);

        public void TickForTest(float dt)
        {
            _t += dt;
            switch (CurrentPhase)
            {
                case Phase.Idle:     if (_t >= idleDuration)     Enter(Phase.Warning); break;
                case Phase.Warning:  if (_t >= warningDuration)  Enter(Phase.Gust);    break;
                case Phase.Gust:     ApplyGust(); if (_t >= gustDuration) Enter(Phase.Cooldown); break;
                case Phase.Cooldown: if (_t >= cooldownDuration) Enter(Phase.Idle);    break;
            }
        }

        private void Enter(Phase next)
        {
            CurrentPhase = next;
            _t = 0f;
            if (next == Phase.Warning)
            {
                // Only fire the warning if a player is actually inside the trigger.
                // Otherwise distant wind hazards beep at you across the whole level.
                if (_players.Count > 0)
                    Blech.Audio.SfxPlayer.Play(Blech.Audio.SfxId.WindWarning);
            }
            else if (next == Phase.Gust && streaksVfx != null)
            {
                streaksVfx.Play();
            }
            else if (next == Phase.Idle && streaksVfx != null)
            {
                streaksVfx.Stop();
            }
        }

        private void ApplyGust()
        {
            Vector3 v = gustDirection.normalized * gustStrength;
            foreach (var p in _players) if (p != null) p.AddExternalVelocity(v, Time.deltaTime * 2f);
            foreach (var c in _climbers) if (c != null) c.ApplyExternalImpulse(v);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerMovementController m)) _players.Add(m);
            if (other.TryGetComponent(out PlayerClimbingController c)) _climbers.Add(c);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out PlayerMovementController m)) _players.Remove(m);
            if (other.TryGetComponent(out PlayerClimbingController c)) _climbers.Remove(c);
        }
    }
}
