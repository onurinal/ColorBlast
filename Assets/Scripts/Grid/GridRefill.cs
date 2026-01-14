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
        private BlockProperties blockProperties;

        public void Initialize(Block[,] blockGrid, GridManager gridManager, LevelProperties levelProperties, BlockProperties blockProperties)
        {
            this.blockGrid = blockGrid;
            this.gridManager = gridManager;
            this.levelProperties = levelProperties;
            this.blockProperties = blockProperties;
        }

        public IEnumerator StartRefillToEmptySlots(List<Block> movedBlocks)
        {
            // Apply gravity - existing blocks fall down
            yield return ApplyGravity(movedBlocks);
        }


        /// <summary>
        /// Apply gravity - make existing blocks fall to fill gaps
        /// </summary>
        private IEnumerator ApplyGravity(List<Block> movedBlocks)
        {
            movedBlocks.Clear();
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
                yield return new WaitForSeconds(blockProperties.FallDuration);
            }
        }

        /// <summary>
        /// Apply gravity a single row, bottom-up scan
        /// </summary>
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
                        block.FallTo(targetPosition);

                        // for affected blocks
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