using ColorBlast.Blocks;
using ColorBlast.Grid;
using UnityEngine;

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

        public void Initialize(IGridInteraction gridInteraction)
        {
            this.gridInteraction = gridInteraction;

            playerInputHandler = GetComponent<PlayerInputHandler>();
            playerInputHandler.Initialize(this);
        }

        public void HandleTap(Vector2 position)
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