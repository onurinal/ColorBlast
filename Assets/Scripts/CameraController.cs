using ColorBlast.Blocks;
using ColorBlast.Grid;
using UnityEngine;

namespace ColorBlast
{
    public class CameraController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float padding = 1f;

        private Camera mainCamera;
        private GridSpawner gridSpawner;
        private BlockProperties blockProperties;
        private int rowCount, columnCount;

        public void Initialize(int row, int col, GridSpawner gridSpawner, BlockProperties blockProperties)
        {
            rowCount = row;
            columnCount = col;
            this.gridSpawner = gridSpawner;
            this.blockProperties = blockProperties;

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            UpdateCameraPosition();
            UpdateCameraOrthographicSize();
        }

        private Vector3 GetCenterPosition()
        {
            var leftBottomBlockPosition = gridSpawner.GetBlockWorldPosition(0, 0);
            var leftTopBlockPosition = gridSpawner.GetBlockWorldPosition(0, columnCount - 1);

            var centerPositionOfVertical = (leftBottomBlockPosition + leftTopBlockPosition) / 2;

            var rightBottomBlockPosition = gridSpawner.GetBlockWorldPosition(rowCount - 1, 0);

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
        }
    }
}