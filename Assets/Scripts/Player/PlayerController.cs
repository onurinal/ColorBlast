using ColorBlast.Blocks;
using ColorBlast.Manager;
using UnityEngine;

namespace ColorBlast.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private LayerMask blockLayer;

        private PlayerInputHandler playerInputHandler;
        private Camera mainCamera;
        private GridManager gridManager;

        public void Initialize(GridManager gridManager)
        {
            this.gridManager = gridManager;

            playerInputHandler = GetComponent<PlayerInputHandler>();
            playerInputHandler.Initialize(this);

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        public void HandleTap(Vector2 position)
        {
            if (gridManager.IsBusy)
            {
                return;
            }

            var worldPosition = mainCamera.ScreenToWorldPoint(position);
            worldPosition.z = 0f;

            var hit = Physics2D.OverlapPoint(worldPosition, blockLayer);

            if (hit != null)
            {
                var selectedBlock = hit.GetComponentInParent<Block>();
                if (selectedBlock != null)
                {
                    gridManager.OnBlockClicked(selectedBlock);
                }
            }
        }
    }
}