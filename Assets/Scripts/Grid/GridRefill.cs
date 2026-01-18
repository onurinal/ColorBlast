using System.Collections;
using System.Collections.Generic;
using ColorBlast.Blocks;
using ColorBlast.Level;
using ColorBlast.Manager;
using UnityEngine;

namespace ColorBlast.Grid
{
    /// <summary>
    /// Handles block gravity and refilling
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

        public IEnumerator ApplyGravity(List<Block> movedBlocks, WaitForSeconds moveDelay)
        {
            var anyBlockMoved = false;

            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                if (ApplyGravityToColumn(row, movedBlocks))
                {
                    anyBlockMoved = true;
                }
            }

            if (anyBlockMoved)
            {
                yield return moveDelay;
            }
        }

        private bool ApplyGravityToColumn(int row, List<Block> movedBlocks)
        {
            var columnChanged = false;
            var writeCol = 0;

            for (int col = 0; col < levelProperties.ColumnCount; col++)
            {
                var block = blockGrid[row, col];

                if (block != null)
                {
                    if (writeCol != col)
                    {
                        blockGrid[row, writeCol] = block;
                        blockGrid[row, col] = null;
                        block.SetGridPosition(row, writeCol);
                        var targetPosition = gridManager.GetCellWorldPosition(row, writeCol);
                        block.MoveTo(targetPosition);
                        movedBlocks.Add(block);

                        columnChanged = true;
                    }

                    writeCol++;
                }
            }

            return columnChanged;
        }
    }
}