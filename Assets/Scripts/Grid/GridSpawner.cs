using System.Collections;
using System.Collections.Generic;
using ColorBlast.Blocks;
using ColorBlast.Level;
using ColorBlast.Manager;
using UnityEngine;

namespace ColorBlast.Grid
{
    /// <summary>
    /// Creates new blocks above the grid and animates them falling
    /// </summary>
    public class GridSpawner
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

        public void CreateNewBlocksAtStart()
        {
            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                for (int col = 0; col < levelProperties.ColumnCount; col++)
                {
                    if (blockGrid[row, col] == null)
                    {
                        var spawnPosition = gridManager.GetCellWorldPosition(row, levelProperties.ColumnCount + col);
                        blockGrid[row, col] = CreateBlock(row, col, spawnPosition);
                        var targetPosition = gridManager.GetCellWorldPosition(row, col);
                        blockGrid[row, col].MoveToPosition(targetPosition);
                    }
                }
            }
        }

        private Block CreateBlock(int row, int col, Vector2 position)
        {
            var newBlock = ObjectPoolManager.Instance.GetBlock();
            var randomColor = GetRandomColor();
            newBlock.Initialize(row, col, randomColor);
            newBlock.transform.position = position;

            return newBlock;
        }

        private BlockColorType GetRandomColor()
        {
            var colorIndex = Random.Range(0, levelProperties.ColorCount);
            var newColor = (BlockColorType)colorIndex;
            return newColor;
        }

        public IEnumerator SpawnNewBlocks(List<Block> newSpawnBlocks, WaitForSeconds spawnWait)
        {
            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                var emptyCount = CountEmptySlotsForColumn(row);

                if (emptyCount <= 0)
                {
                    continue;
                }

                for (int i = 0; i < emptyCount; i++)
                {
                    var targetCol = levelProperties.ColumnCount - emptyCount + i;
                    var newBlock = SpawnBlockWithFallAnimation(row, targetCol, i);
                    newSpawnBlocks.Add(newBlock);
                }

                yield return spawnWait;
            }
        }

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

        private Block SpawnBlockWithFallAnimation(int targetRow, int targetCol, int spawnOffset)
        {
            var spawnPosition = gridManager.GetCellWorldPosition(targetRow, levelProperties.ColumnCount + spawnOffset);

            var newBlock = CreateBlock(targetRow, targetCol, spawnPosition);
            blockGrid[targetRow, targetCol] = newBlock;

            var targetPosition = gridManager.GetCellWorldPosition(targetRow, targetCol);
            newBlock.MoveToPosition(targetPosition);
            return newBlock;
        }
    }
}