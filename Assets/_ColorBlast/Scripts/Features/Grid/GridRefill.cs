using ColorBlast.Manager;

namespace ColorBlast.Features
{
    /// <summary>
    /// Handles block gravity (falling down) and refilling empty spaces with existing blocks
    /// </summary>
    public class GridRefill
    {
        private Block[,] grid;
        private GridManager gridManager;
        private LevelProperties levelProperties;

        public void Initialize(Block[,] grid, GridManager gridManager, LevelProperties levelProperties)
        {
            this.grid = grid;
            this.gridManager = gridManager;
            this.levelProperties = levelProperties;
        }

        public void ApplyGravity()
        {
            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                ApplyGravityToColumn(row);
            }
        }

        private void ApplyGravityToColumn(int row)
        {
            var writeCol = 0;

            for (int col = 0; col < levelProperties.ColumnCount; col++)
            {
                var block = grid[row, col];

                if (block == null)
                {
                    continue;
                }

                if (writeCol != col)
                {
                    grid[row, writeCol] = block;
                    grid[row, col] = null;
                    block.SetGridPosition(row, writeCol);
                    var targetPosition = gridManager.GetCellWorldPosition(block.GridX, block.GridY);
                    block.MoveToPosition(targetPosition);
                }

                writeCol++;
            }
        }
    }
}