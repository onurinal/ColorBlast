using ColorBlast.Blocks;
using ColorBlast.Grid;
using ColorBlast.Level;
using UnityEngine;

namespace ColorBlast.Manager
{
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Configuration")]
        [SerializeField] private BlockProperties blockProperties;

        [Header("References")]
        [SerializeField] private CameraController cameraController;

        private GridSpawner gridSpawner;
        private GridChecker gridChecker;
        private LevelProperties levelProperties;

        private Block[,] blockGrid;
        private Vector2 blockSize;

        public GridChecker GridChecker => gridChecker;

        public void Initialize(LevelProperties levelProperties)
        {
            this.levelProperties = levelProperties;

            gridSpawner = new GridSpawner();
            gridSpawner.Initialize(blockProperties, levelProperties);

            CacheBlockSize();
            CreateGrid();

            gridChecker = new GridChecker();
            gridChecker.Initialize(blockGrid, levelProperties);

            cameraController.Initialize(levelProperties.RowCount, levelProperties.ColumnCount, this, blockProperties);
        }

        private void CacheBlockSize()
        {
            blockSize = blockProperties.GetBlockSpriteBoundSize();
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
    }
}