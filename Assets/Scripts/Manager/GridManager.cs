using ColorBlast.Blocks;
using ColorBlast.Grid;
using ColorBlast.Level;
using UnityEngine;

namespace ColorBlast.Manager
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private LevelProperties levelProperties;
        [SerializeField] private BlockProperties blockProperties;
        [SerializeField] private CameraController cameraController;
        [SerializeField] private Transform blocksParent;

        private GridSpawner gridSpawner;
        private GridChecker gridChecker;

        private Block[,] blockGrid;
        private Vector2 blockSize;

        public void Initialize()
        {
            gridSpawner = new GridSpawner();
            gridSpawner.Initialize(blockProperties, levelProperties, blocksParent);

            CacheBlockSize();
            CreateGrid();

            gridChecker = new GridChecker();
            gridChecker.Initialize(blockGrid, levelProperties);

            cameraController.Initialize(levelProperties.RowCount, levelProperties.ColumnCount, this, blockProperties);
        }

        private void CreateGrid()
        {
            blockGrid = new Block[levelProperties.RowCount, levelProperties.ColumnCount];

            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                for (int col = 0; col < levelProperties.ColumnCount; col++)
                {
                    var blockPosition = CalculateBlockWorldPosition(row, col);
                    blockGrid[row, col] = gridSpawner.CreateBlock(row, col, blockPosition);
                }
            }
        }

        private Vector2 CalculateBlockWorldPosition(int row, int col)
        {
            return new Vector2(row * (blockSize.x + blockProperties.SpacingX), col * (blockSize.y + blockProperties.SpacingY));
        }

        public Vector2 GetBlockWorldPosition(int row, int col)
        {
            if (blockGrid[row, col] != null)
            {
                return new Vector2(blockGrid[row, col].transform.position.x, blockGrid[row, col].transform.position.y);
            }

            Debug.LogError($"Block_{row}_{col} not found");
            return Vector2.zero;
        }

        private void CacheBlockSize()
        {
            blockSize = blockProperties.GetBlockSpriteBoundSize();
        }
    }
}