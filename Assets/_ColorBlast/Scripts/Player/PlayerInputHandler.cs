using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ColorBlast.Player
{
    public class PlayerInputHandler : IDisposable
    {
        private readonly PlayerInputActions playerInputActions;

        public event Action<Vector2> OnTap;

        public PlayerInputHandler()
        {
            playerInputActions = new PlayerInputActions();
            playerInputActions.Player.Tap.performed += HandleTap;

            playerInputActions.Enable();
        }

        private void HandleTap(InputAction.CallbackContext context)
        {
            Vector2 position = playerInputActions.Player.PointerPosition.ReadValue<Vector2>();
            TriggerOnTap(position);
        }

        public void Dispose()
        {
            playerInputActions.Player.Tap.performed -= HandleTap;
            playerInputActions.Disable();
            playerInputActions?.Dispose();
        }

        private void TriggerOnTap(Vector2 position)
        {
            OnTap?.Invoke(position);
        }
    }
}