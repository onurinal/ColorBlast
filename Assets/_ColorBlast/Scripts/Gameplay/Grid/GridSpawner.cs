using ColorBlast.Manager;
using UnityEngine;

namespace ColorBlast.Gameplay

{
    /// <summary>
    /// Creates new blocks above the grid and animates them falling
    /// </summary>
    public class GridSpawner
    {
        private Block[,] blockGrid;
        private GridManager gridManager;
        private LevelProperties levelProperties;
        private CubeColorDatabase cubeColorDatabase;

        public void Initialize(Block[,] blockGrid, GridManager gridManager, LevelProperties levelProperties,
            CubeColorDatabase cubeColorDatabase)
        {
            this.blockGrid = blockGrid;
            this.gridManager = gridManager;
            this.levelProperties = levelProperties;
            this.cubeColorDatabase = cubeColorDatabase;
        }

        public void SpawnNewBlocks()
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
                    var newBlock = CreateBlockAt(row, targetCol, spawnPosition);

                    var targetPosition = gridManager.GetCellWorldPosition(newBlock.GridX, newBlock.GridY);
                    newBlock.MoveToPosition(targetPosition);
                    blockGrid[row, targetCol] = newBlock;
                }
            }
        }

        private Block CreateBlockAt(int row, int col, Vector2 spawnPosition)
        {
            var newBlock = ObjectPoolManager.Instance.GetBlock();
            var randomColorData = cubeColorDatabase.GetRandomBlockColorData(levelProperties.ColorCount);
            newBlock.Initialize(row, col, randomColorData);
            newBlock.transform.position = spawnPosition;

            return newBlock;
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
    }
}