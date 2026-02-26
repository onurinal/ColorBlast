using UnityEngine;
using ColorBlast.Gameplay;

namespace ColorBlast.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private LayerMask blockLayer;

        private PlayerInputHandler playerInputHandler;
        private Camera mainCamera;
        private IGridInteraction gridInteraction;

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

        public void Initialize(IGridInteraction gridInteraction)
        {
            playerInputHandler = new PlayerInputHandler();
            playerInputHandler.OnTap += HandleTap;

            this.gridInteraction = gridInteraction;
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

            var selectedBlock = hit.GetComponentInParent<Block>();

            if (selectedBlock == null)
            {
                return;
            }

            if (!selectedBlock.CanBeInteract())
            {
                return;
            }

            gridInteraction.OnBlockClicked(selectedBlock);
        }
    }
}