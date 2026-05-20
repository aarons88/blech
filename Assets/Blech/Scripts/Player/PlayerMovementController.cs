using UnityEngine;

namespace Blech.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerMovementController : MonoBehaviour
    {
        [SerializeField] private PlayerCharacterStats stats;
        [SerializeField] private Transform cameraTransform;

        private CharacterController _controller;
        private PlayerInput _input;
        private float _verticalVelocity;
        private Vector3 _externalVelocity;
        private float _externalVelocityUntil;
        private float _lastGroundedTime = -10f;
        private const float CoyoteTime = 0.2f;

        public bool IsGrounded => _controller != null && _controller.isGrounded;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInput>();
            if (_input != null) _input.JumpPressed += OnJump;
        }

        private void OnDestroy()
        {
            if (_input != null) _input.JumpPressed -= OnJump;
        }

        private void Update()
        {
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
            if (cameraTransform == null || stats == null || _input == null) return;

            Vector3 horizontal = MovementMath.HorizontalVelocity(_input.Move, cameraTransform.forward, stats.moveSpeed);
            _verticalVelocity = MovementMath.ApplyGravity(_verticalVelocity, stats.gravity, Time.deltaTime, _controller.isGrounded);

            if (_controller.isGrounded) _lastGroundedTime = Time.time;

            Vector3 external = Time.time < _externalVelocityUntil ? _externalVelocity : Vector3.zero;
            _controller.Move((horizontal + Vector3.up * _verticalVelocity + external) * Time.deltaTime);

            if (horizontal.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(horizontal), 12f * Time.deltaTime);
        }

        private void OnJump()
        {
            if (stats == null)
            {
                Debug.LogWarning("[Bean] Jump: stats is null — prefab wasn't wired correctly");
                return;
            }
            if (_controller == null) return;

            bool canJump = _controller.isGrounded || (Time.time - _lastGroundedTime <= CoyoteTime);
            if (!canJump)
            {
                // Comment out if too chatty; useful for diagnosing jump-not-working.
                // Debug.Log($"[Bean] Jump blocked (not grounded, last grounded {Time.time - _lastGroundedTime:F2}s ago)");
                return;
            }

            _verticalVelocity = stats.jumpForce;
            _lastGroundedTime = -10f; // consume the coyote window
            Blech.Audio.SfxPlayer.Play(Blech.Audio.SfxId.Jump);
        }

        public void AddExternalVelocity(Vector3 velocity, float duration)
        {
            _externalVelocity = velocity;
            _externalVelocityUntil = Time.time + duration;
        }

        public void SetVerticalVelocity(float y) => _verticalVelocity = y;
    }
}
