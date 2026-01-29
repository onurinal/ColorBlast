using System.Collections.Generic;
using ColorBlast.Blocks;
using ColorBlast.Level;
using ColorBlast.Manager;

namespace ColorBlast.Grid
{
    /// <summary>
    /// Handles block gravity (falling down) and refilling empty spaces with existing blocks
    /// </summary>
    public class GridRefill
    {
        private Block[,] blockGrid;
        private GridManager gridManager;
        private LevelProperties levelProperties;


        public void Initialize(Block[,] blockGrid, GridManager gridManager, LevelProperties levelProperties)
        {
            this.blockGrid = blockGrid;
            this.gridManager = gridManager;
            this.levelProperties = levelProperties;
        }

        public void ApplyGravity(List<Block> movedBlocks)
        {
            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                ApplyGravityToColumn(row, movedBlocks);
            }
        }

        private void ApplyGravityToColumn(int row, List<Block> movedBlocks)
        {
            var writeCol = 0;

            for (int col = 0; col < levelProperties.ColumnCount; col++)
            {
                var block = blockGrid[row, col];

                if (block == null)
                {
                    continue;
                }

                if (writeCol != col)
                {
                    blockGrid[row, writeCol] = block;
                    blockGrid[row, col] = null;
                    block.SetGridPosition(row, writeCol);

                    if (block.IsVisible())
                    {
                        movedBlocks.Add(block);
                    }
                }

                writeCol++;
            }
        }

        public void PlayRefillAnimation(List<Block> movedBlocks)
        {
            for (int i = 0; i < movedBlocks.Count; i++)
            {
                if (movedBlocks[i] == null)
                {
                    continue;
                }

                var targetPosition = gridManager.GetCellWorldPosition(movedBlocks[i].GridX, movedBlocks[i].GridY);
                movedBlocks[i].MoveToPosition(targetPosition);
            }
        }
    }
}