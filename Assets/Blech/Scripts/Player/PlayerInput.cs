using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Blech.Player
{
    public class PlayerInput : MonoBehaviour
    {
        public event Action JumpPressed;
        public event Action RespawnPressed;
        public Vector2 Move { get; private set; }
        public bool ClimbHeld { get; private set; }

        [SerializeField] private InputActionAsset actions;

        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _climbAction;
        private InputAction _respawnAction;

        private void Awake()
        {
            if (actions == null)
            {
                Debug.LogError("[PlayerInput] No InputActionAsset assigned");
                enabled = false;
                return;
            }
            var map = actions.FindActionMap("Player", true);
            _moveAction = map.FindAction("Move", true);
            _jumpAction = map.FindAction("Jump", true);
            _climbAction = map.FindAction("Climb", true);
            _respawnAction = map.FindAction("Respawn", true);

            _jumpAction.performed += OnJumpPerformed;
            _respawnAction.performed += OnRespawnPerformed;
        }

        private void OnDestroy()
        {
            if (_jumpAction != null) _jumpAction.performed -= OnJumpPerformed;
            if (_respawnAction != null) _respawnAction.performed -= OnRespawnPerformed;
        }

        private void OnEnable() { actions?.Enable(); }
        private void OnDisable() { actions?.Disable(); }

        private void Update()
        {
            if (_moveAction == null) return;
            Move = _moveAction.ReadValue<Vector2>();
            ClimbHeld = _climbAction.IsPressed();
        }

        private void OnJumpPerformed(InputAction.CallbackContext ctx) => JumpPressed?.Invoke();
        private void OnRespawnPerformed(InputAction.CallbackContext ctx) => RespawnPressed?.Invoke();
    }
}
