using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Blech.Player
{
    /// <summary>
    /// Reads input by polling Keyboard/Gamepad directly each frame. This is intentionally
    /// simpler than the InputActions API — fewer moving parts, no asset binding to break,
    /// trivial to debug by checking the actual Keyboard.current device state.
    /// </summary>
    public class PlayerInput : MonoBehaviour
    {
        public event Action JumpPressed;
        public event Action RespawnPressed;
        public Vector2 Move { get; private set; }
        public bool ClimbHeld { get; private set; }

        private void Update()
        {
            var kb = Keyboard.current;
            var pad = Gamepad.current;

            Vector2 move = Vector2.zero;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    move.y += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  move.y -= 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  move.x -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) move.x += 1f;
            }
            if (pad != null)
            {
                Vector2 stick = pad.leftStick.ReadValue();
                if (stick.sqrMagnitude > move.sqrMagnitude) move = stick;
            }
            Move = move.sqrMagnitude > 1f ? move.normalized : move;

            bool climb = false;
            if (kb != null) climb |= kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
            if (pad != null) climb |= pad.buttonEast.isPressed;
            ClimbHeld = climb;

            if (kb != null && kb.spaceKey.wasPressedThisFrame) JumpPressed?.Invoke();
            if (pad != null && pad.buttonSouth.wasPressedThisFrame) JumpPressed?.Invoke();

            if (kb != null && kb.rKey.wasPressedThisFrame) RespawnPressed?.Invoke();
            if (pad != null && pad.startButton.wasPressedThisFrame) RespawnPressed?.Invoke();
        }
    }
}
