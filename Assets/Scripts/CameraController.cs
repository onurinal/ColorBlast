using ColorBlast.Blocks;
using ColorBlast.Manager;
using UnityEngine;

namespace ColorBlast
{
    public class CameraController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float padding = 1f;

        private Camera mainCamera;
        private GridManager gridManager;
        private BlockProperties blockProperties;
        private int rowCount, columnCount;

        public void Initialize(int row, int col, GridManager gridManager, BlockProperties blockProperties)
        {
            rowCount = row;
            columnCount = col;
            this.gridManager = gridManager;
            this.blockProperties = blockProperties;

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            UpdateCameraPosition();

            // For more rows and columns or after localScale change, it will scale camera size
            UpdateCameraOrthographicSize();
        }

        private Vector3 GetCenterPosition()
        {
            var leftBottomBlockPosition = gridManager.GetCellWorldPosition(0, 0);
            var leftTopBlockPosition = gridManager.GetCellWorldPosition(0, columnCount - 1);

            var centerPositionOfVertical = (leftBottomBlockPosition + leftTopBlockPosition) / 2;

            var rightBottomBlockPosition = gridManager.GetCellWorldPosition(rowCount - 1, 0);

            var centerPositionOfHorizontal = (leftBottomBlockPosition + rightBottomBlockPosition) / 2;

            return new Vector3(centerPositionOfHorizontal.x, centerPositionOfVertical.y, transform.position.z);
        }

        private void UpdateCameraPosition()
        {
            transform.position = GetCenterPosition();
        }

        private void UpdateCameraOrthographicSize()
        {
            var gridWidth = rowCount * blockProperties.GetBlockSpriteBoundSize().x;
            var gridHeight = columnCount * blockProperties.GetBlockSpriteBoundSize().y;

            var minWidthSize = (gridWidth + (padding * 2f)) / 2f / mainCamera.aspect;
            var minHeightSize = (gridHeight + (padding * 2f)) / 2f;

            mainCamera.orthographicSize = Mathf.Max(minWidthSize, minHeightSize);
            if (mainCamera.orthographicSize < 11)
            {
                mainCamera.orthographicSize = 10;
            }
        }
    }
}