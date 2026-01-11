using ColorBlast.Blocks;
using ColorBlast.Level;
using UnityEngine;

namespace ColorBlast.Grid
{
    public class GridSpawner : MonoBehaviour
    {
        [SerializeField] private Transform blockParent;
        private Block[,] blockGrid;
        private BlockProperties blockProperties;
        private LevelProperties levelProperties;
        private GridChecker gridChecker;

        private Vector2 blockSize;

        public void Initialize(BlockProperties blockProperties, LevelProperties levelProperties, GridChecker gridChecker)
        {
            this.blockProperties = blockProperties;
            this.levelProperties = levelProperties;
            this.gridChecker = gridChecker;


            CacheBlockSize();
            CreateGrid();

            gridChecker.Initialize(blockGrid, levelProperties);
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
                    blockGrid[row, col] = CreateBlock(row, col, blockPosition);
                }
            }
        }

        private Block CreateBlock(int row, int col, Vector2 position)
        {
            var newBlock = Instantiate(blockProperties.BlockPrefab, position, Quaternion.identity, blockParent);
            newBlock.name = $"Block_{row}_{col}";
            newBlock.Initialize(row, col, GetRandomColor());
            return newBlock;
        }

        private BlockColorType GetRandomColor()
        {
            var colorSize = levelProperties.ColorCount;
            var newColorNumber = Random.Range(0, colorSize);
            var newColor = (BlockColorType)newColorNumber;
            return newColor;
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