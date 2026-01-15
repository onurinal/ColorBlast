using System.Collections;
using System.Collections.Generic;
using ColorBlast.Blocks;
using ColorBlast.Level;
using ColorBlast.Manager;
using UnityEngine;

namespace ColorBlast.Grid
{
    public class GridSpawner
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

            CreateBlocksAtStart();
        }

        private void CreateBlocksAtStart()
        {
            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                for (int col = 0; col < levelProperties.ColumnCount; col++)
                {
                    var blockPosition = gridManager.GetCellWorldPosition(row, col);
                    blockGrid[row, col] = CreateBlock(row, col, blockPosition);
                }
            }
        }

        private Block CreateBlock(int row, int col, Vector2 position)
        {
            var newBlock = ObjectPoolManager.Instance.GetBlock();
            newBlock.Initialize(row, col, GetRandomColor());
            newBlock.transform.position = position;
            return newBlock;
        }

        private BlockColorType GetRandomColor()
        {
            var colorSize = levelProperties.ColorCount;
            var newColorNumber = Random.Range(0, colorSize);
            var newColor = (BlockColorType)newColorNumber;
            return newColor;
        }

        /// <summary>
        /// Spawn new blocks at the top each column to fill empty slots
        /// </summary>
        public IEnumerator SpawnNewBlocks(List<Block> newSpawnBlocks)
        {
            newSpawnBlocks.Clear();

            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                var emptyCount = CountEmptySlotsForColumn(row);

                if (emptyCount > 0)
                {
                    for (int i = 0; i < emptyCount; i++)
                    {
                        // target position in grid from top of column to the bottom
                        var targetCol = levelProperties.ColumnCount - emptyCount + i;
                        var newBlock = SpawnBlockAboveGrid(row, targetCol, i);
                        newSpawnBlocks.Add(newBlock);

                        yield return new WaitForSeconds(blockProperties.SpawnDelayBetweenBlocks);
                    }
                }
            }

            yield return new WaitForSeconds(blockProperties.SpawnDuration);
        }

        /// <summary>
        /// Count how many empty slots exist at the top of a column
        /// It scans from top to bottom, stop at first non-null block
        /// </summary>
        private int CountEmptySlotsForColumn(int row)
        {
            var count = 0;

            for (int col = levelProperties.ColumnCount - 1; col >= 0; col--)
            {
                if (blockGrid[row, col] == null)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            return count;
        }

        private Block SpawnBlockAboveGrid(int targetRow, int targetCol, int spawnOffset)
        {
            // calculate spawn position above the grid, offset makes blocks spawn higher
            var spawnPosition = gridManager.GetCellWorldPosition(targetRow, levelProperties.ColumnCount + spawnOffset);

            var newBlock = CreateBlock(targetRow, targetCol, spawnPosition);
            blockGrid[targetRow, targetCol] = newBlock;

            var targetPosition = gridManager.GetCellWorldPosition(targetRow, targetCol);
            newBlock.MoveTo(targetPosition);
            return newBlock;
        }
    }
}