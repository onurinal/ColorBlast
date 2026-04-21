using UnityEngine;
using ColorBlast.Features;

namespace ColorBlast.Input
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private LayerMask blockLayer;

        private PlayerInputHandler playerInputHandler;
        private Camera mainCamera;

        public void Initialize()
        {
            playerInputHandler = new PlayerInputHandler();
            playerInputHandler.OnTap += HandleTap;
        }

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void OnDestroy()
        {
            if (playerInputHandler != null)
            {
                playerInputHandler.OnTap -= HandleTap;
                playerInputHandler.Dispose();
            }
        }

        private void HandleTap(Vector2 position)
        {
            var worldPosition = mainCamera.ScreenToWorldPoint(position);
            worldPosition.z = 0f;

            var hit = Physics2D.OverlapPoint(worldPosition, blockLayer);

            if (hit == null)
            {
                return;
            }

            var interactable = hit.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }
}