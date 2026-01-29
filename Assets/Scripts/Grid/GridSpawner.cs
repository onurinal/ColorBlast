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
        private BlockColorDatabase blockColorDatabase;

        public void Initialize(Block[,] blockGrid, GridManager gridManager, LevelProperties levelProperties, BlockColorDatabase blockColorDatabase)
        {
            this.blockGrid = blockGrid;
            this.gridManager = gridManager;
            this.levelProperties = levelProperties;
            this.blockColorDatabase = blockColorDatabase;
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
                        var newBlock = CreateBlock(row, col, spawnPosition);
                        blockGrid[row, col] = newBlock;
                        var targetPosition = gridManager.GetCellWorldPosition(row, col);
                        newBlock.MoveToPosition(targetPosition);
                    }
                }
            }
        }

        private Block CreateBlock(int row, int col, Vector2 spawnPosition)
        {
            var newBlock = ObjectPoolManager.Instance.GetBlock();
            var randomColorData = GetRandomColor();
            newBlock.Initialize(row, col, randomColorData);
            newBlock.transform.position = spawnPosition;

            return newBlock;
        }

        private BlockColorData GetRandomColor()
        {
            var randomColorData = blockColorDatabase.GetRandomBlockColorData(levelProperties.ColorCount);
            return randomColorData;
        }

        public void SpawnNewBlocks(List<Block> newSpawnBlocks)
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
                    var spawnPosition = gridManager.GetCellWorldPosition(row, levelProperties.ColumnCount + i);
                    var newBlock = CreateBlock(row, targetCol, spawnPosition);
                    blockGrid[row, targetCol] = newBlock;
                    newBlock.SetVisible(false);
                    newSpawnBlocks.Add(newBlock);
                }
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

        public void PlayNewSpawnBlocksAnimation(List<Block> newSpawnBlocks)
        {
            for (int i = 0; i < newSpawnBlocks.Count; i++)
            {
                if (newSpawnBlocks[i] == null)
                {
                    continue;
                }

                newSpawnBlocks[i].SetVisible(true);
                var targetPosition = gridManager.GetCellWorldPosition(newSpawnBlocks[i].GridX, newSpawnBlocks[i].GridY);
                newSpawnBlocks[i].MoveToPosition(targetPosition);
            }
        }
    }
}