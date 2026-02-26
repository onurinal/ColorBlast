using UnityEngine;
using UnityEngine.InputSystem;

namespace ColorBlast.Player
{
    public class PlayerInputHandler : MonoBehaviour
    {
        private PlayerInputActions playerInputActions;
        private PlayerController playerController;

        public void Initialize(PlayerController playerController)
        {
            this.playerController = playerController;
        }

        private void OnEnable()
        {
            if (playerInputActions == null)
            {
                playerInputActions = new PlayerInputActions();
            }

            playerInputActions.Player.Tap.performed += HandleTap;
            playerInputActions.Enable();
        }

        private void OnDisable()
        {
            if (playerInputActions == null)
            {
                return;
            }

            playerInputActions.Player.Tap.performed -= HandleTap;
            playerInputActions.Disable();
        }

        private void HandleTap(InputAction.CallbackContext context)
        {
            var position = Pointer.current.position.ReadValue();

            if (playerController != null)
            {
                playerController.HandleTap(position);
            }
        }
    }
}