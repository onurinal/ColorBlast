using ColorBlast.Blocks;
using ColorBlast.Grid;
using ColorBlast.Manager;
using UnityEngine;

namespace ColorBlast.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private LayerMask blockLayer;

        private GridChecker gridChecker;
        private PlayerInputHandler playerInputHandler;
        private Camera mainCamera;

        public void Initialize(GridChecker gridChecker)
        {
            this.gridChecker = gridChecker;

            playerInputHandler = GetComponent<PlayerInputHandler>();
            playerInputHandler.Initialize(this);

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }


        public void HandleTap(Vector2 position)
        {
            var worldPosition = mainCamera.ScreenToWorldPoint(position);
            worldPosition.z = 0f;

            var hit = Physics2D.OverlapPoint(worldPosition, blockLayer);

            if (hit != null)
            {
                var selectedBlock = hit.GetComponentInParent<Block>();
                var groups = gridChecker.GetGroup(selectedBlock.GridX, selectedBlock.GridY);

                if (groups.Count >= 2)
                {
                    foreach (var block in groups)
                    {
                        block.Destroy();
                    }
                }
            }
        }
    }
}