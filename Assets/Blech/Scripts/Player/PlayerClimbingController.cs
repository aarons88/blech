using UnityEngine;
using Blech.World;

namespace Blech.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(PlayerMovementController))]
    [RequireComponent(typeof(PlayerStamina))]
    public class PlayerClimbingController : MonoBehaviour
    {
        [SerializeField] private PlayerCharacterStats stats;
        [SerializeField] private float castOriginHeight = 0.5f;
        [SerializeField] private float castRadius = 0.3f;
        [SerializeField] private float castDistance = 0.5f;
        [SerializeField] private float jumpAwayForce = 4f;
        [SerializeField] private float jumpUpForce = 5f;
        [SerializeField] private LayerMask wallMask = ~0;

        private CharacterController _cc;
        private PlayerInput _input;
        private PlayerMovementController _movement;
        private PlayerStamina _stamina;
        private bool _climbing;
        private Vector3 _wallNormal;
        private float _slipMultiplier = 1f;
        private float _slideRate;
        private bool _previouslySlippery;

        public bool IsClimbing => _climbing;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInput>();
            _movement = GetComponent<PlayerMovementController>();
            _stamina = GetComponent<PlayerStamina>();
            if (stats != null) _stamina.Configure(stats);
            if (_stamina != null) _stamina.OnStaminaZero += ExitClimb;
            if (_input != null) _input.JumpPressed += TryJumpOff;
        }

        private void OnDestroy()
        {
            if (_stamina != null) _stamina.OnStaminaZero -= ExitClimb;
            if (_input != null) _input.JumpPressed -= TryJumpOff;
        }

        private void Update()
        {
            if (stats == null || _input == null) return;

            bool wallHit = TryDetectWall(out RaycastHit hit);
            bool isClimbable = wallHit && hit.collider.TryGetComponent(out ClimbableSurface _);

            if (!_climbing)
            {
                _stamina.Tick(Time.deltaTime, false, 1f);
                if (_input.ClimbHeld && isClimbable) EnterClimb(hit);
                return;
            }

            if (!_input.ClimbHeld || !isClimbable) { ExitClimb(); return; }

            _wallNormal = hit.normal;
            UpdateSlip(hit);

            Vector3 v = ClimbMath.ProjectInputOnWall(_input.Move, _wallNormal, stats.climbSpeed) + Vector3.down * _slideRate;
            _cc.Move(v * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(-_wallNormal);
            _stamina.Tick(Time.deltaTime, true, _slipMultiplier);
        }

        private bool TryDetectWall(out RaycastHit hit)
        {
            Vector3 origin = transform.position + Vector3.up * castOriginHeight;
            return Physics.SphereCast(origin, castRadius, transform.forward, out hit, castDistance, wallMask, QueryTriggerInteraction.Ignore);
        }

        private void EnterClimb(RaycastHit hit)
        {
            _climbing = true;
            _wallNormal = hit.normal;
            UpdateSlip(hit);
            _movement.enabled = false;
            Blech.Audio.SfxPlayer.Play(Blech.Audio.SfxId.WallGrab);
        }

        private void ExitClimb()
        {
            if (!_climbing) return;
            _climbing = false;
            _movement.enabled = true;
            _slipMultiplier = 1f;
            _slideRate = 0f;
            _previouslySlippery = false;
        }

        private void TryJumpOff()
        {
            if (!_climbing) return;
            Vector3 v = ClimbMath.JumpOffWallVelocity(_wallNormal, jumpAwayForce, jumpUpForce);
            ExitClimb();
            _movement.AddExternalVelocity(new Vector3(v.x, 0, v.z), 0.3f);
            _movement.SetVerticalVelocity(v.y);
        }

        private void UpdateSlip(RaycastHit hit)
        {
            bool slippery = hit.collider.TryGetComponent(out SlipperySurface slip);
            if (slippery)
            {
                _slipMultiplier = slip.slipMultiplier;
                _slideRate = slip.slideRate;
                if (!_previouslySlippery)
                {
                    Blech.Audio.SfxPlayer.Play(Blech.Audio.SfxId.MucusSquelch);
                    Blech.Audio.SfxPlayer.Play(Blech.Audio.SfxId.Slip);
                }
            }
            else
            {
                _slipMultiplier = 1f;
                _slideRate = 0f;
            }
            _previouslySlippery = slippery;
        }

        public void ApplyExternalImpulse(Vector3 impulse)
        {
            if (!_climbing || stats == null) return;
            if (impulse.magnitude > stats.gripStrength) ExitClimb();
        }
    }
}
