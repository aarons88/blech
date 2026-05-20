using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Blech.Player
{
    /// <summary>
    /// Input actions are defined directly in code rather than loaded from an .inputactions
    /// asset. This avoids serialization-binding fragility — the prefab cannot ship with a
    /// missing or stale asset reference.
    /// </summary>
    public class PlayerInput : MonoBehaviour
    {
        public event Action JumpPressed;
        public event Action RespawnPressed;
        public Vector2 Move { get; private set; }
        public bool ClimbHeld { get; private set; }

        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _climbAction;
        private InputAction _respawnAction;

        private void Awake()
        {
            _moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Gamepad>/leftStick/up")
                .With("Down", "<Gamepad>/leftStick/down")
                .With("Left", "<Gamepad>/leftStick/left")
                .With("Right", "<Gamepad>/leftStick/right");

            _jumpAction = new InputAction("Jump", InputActionType.Button);
            _jumpAction.AddBinding("<Keyboard>/space");
            _jumpAction.AddBinding("<Gamepad>/buttonSouth");
            _jumpAction.performed += OnJumpPerformed;

            _climbAction = new InputAction("Climb", InputActionType.Button);
            _climbAction.AddBinding("<Keyboard>/leftShift");
            _climbAction.AddBinding("<Gamepad>/buttonEast");

            _respawnAction = new InputAction("Respawn", InputActionType.Button);
            _respawnAction.AddBinding("<Keyboard>/r");
            _respawnAction.AddBinding("<Gamepad>/start");
            _respawnAction.performed += OnRespawnPerformed;
        }

        private void OnEnable()
        {
            _moveAction?.Enable();
            _jumpAction?.Enable();
            _climbAction?.Enable();
            _respawnAction?.Enable();
        }

        private void OnDisable()
        {
            _moveAction?.Disable();
            _jumpAction?.Disable();
            _climbAction?.Disable();
            _respawnAction?.Disable();
        }

        private void OnDestroy()
        {
            if (_jumpAction != null) _jumpAction.performed -= OnJumpPerformed;
            if (_respawnAction != null) _respawnAction.performed -= OnRespawnPerformed;
            _moveAction?.Dispose();
            _jumpAction?.Dispose();
            _climbAction?.Dispose();
            _respawnAction?.Dispose();
        }

        private void Update()
        {
            Move = _moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
            ClimbHeld = _climbAction?.IsPressed() ?? false;
        }

        private void OnJumpPerformed(InputAction.CallbackContext ctx) => JumpPressed?.Invoke();
        private void OnRespawnPerformed(InputAction.CallbackContext ctx) => RespawnPressed?.Invoke();
    }
}
