using UnityEngine;

namespace Blech.World
{
    [RequireComponent(typeof(Collider))]
    public class AcidGeyser : MonoBehaviour
    {
        public enum Phase { Idle, Warning, Erupt, Cooldown }

        public float idleDuration = 3f;
        public float warningDuration = 1f;
        public float eruptDuration = 1f;
        public float cooldownDuration = 2f;

        [SerializeField] private ParticleSystem splashVfx;
        [SerializeField] private GameObject killVolumeRoot;

        public Phase CurrentPhase { get; private set; } = Phase.Idle;
        private float _t;

        private void Reset() { var c = GetComponent<Collider>(); if (c != null) c.isTrigger = true; }

        private void Update() => TickForTest(Time.deltaTime);

        public void TickForTest(float dt)
        {
            _t += dt;
            switch (CurrentPhase)
            {
                case Phase.Idle:     if (_t >= idleDuration)     Enter(Phase.Warning); break;
                case Phase.Warning:  if (_t >= warningDuration)  Enter(Phase.Erupt);   break;
                case Phase.Erupt:    if (_t >= eruptDuration)    Enter(Phase.Cooldown); break;
                case Phase.Cooldown: if (_t >= cooldownDuration) Enter(Phase.Idle);    break;
            }
        }

        private void Enter(Phase next)
        {
            CurrentPhase = next;
            _t = 0f;
            if (next == Phase.Erupt)
            {
                if (splashVfx != null) splashVfx.Play();
                if (killVolumeRoot != null) killVolumeRoot.SetActive(true);
                Blech.Audio.SfxPlayer.Play(Blech.Audio.SfxId.AcidSplash);
            }
            else if (killVolumeRoot != null)
            {
                killVolumeRoot.SetActive(false);
            }
        }
    }
}
