using ColorBlast.Manager;
using UnityEngine;

namespace ColorBlast.Features

{
    /// <summary>
    /// Creates new blocks above the grid and animates them falling
    /// </summary>
    public class GridSpawner
    {
        private Block[,] grid;
        private GridManager gridManager;
        private LevelProperties levelProperties;
        private CubeColorDatabase cubeColorDatabase;

        public void Initialize(Block[,] grid, GridManager gridManager, LevelProperties levelProperties)
        {
            this.grid = grid;
            this.gridManager = gridManager;
            this.levelProperties = levelProperties;
        }

        public void SpawnNewCubeBlocks()
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
                    // var spawnPosition = gridManager.GetCellWorldPosition(row, levelProperties.ColumnCount + i);
                    var spawnPosition =
                        gridManager.GetCellWorldPosition(row, levelProperties.ColumnCount + i + 5); // testing
                    var newBlock = CreateRandomCubeBlockAt(row, targetCol, spawnPosition);

                    var targetPosition = gridManager.GetCellWorldPosition(newBlock.GridX, newBlock.GridY);
                    newBlock.MoveToPosition(targetPosition);
                    grid[row, targetCol] = newBlock;
                }
            }
        }

        public Block SpawnBlockAt(BlockData blockData, int row, int col, BlockData targetCubeData = null)
        {
            var block = BlockPoolManager.Instance.GetBlock(blockData);
            block.Initialize(row, col, blockData);
            block.transform.position = gridManager.GetCellWorldPosition(row, col);

            switch (block)
            {
                case DiscoBlock discoBlock:
                    discoBlock.SetTargetCubeData(targetCubeData);
                    break;
                case RocketBlock rocketBlock:
                    rocketBlock.SetupDirection();
                    break;
            }

            grid[row, col] = block;
            return block;
        }

        public void SetColorDatabase(CubeColorDatabase cubeColorDatabase)
        {
            this.cubeColorDatabase = cubeColorDatabase;
        }

        private Block CreateRandomCubeBlockAt(int row, int col, Vector2 spawnPosition)
        {
            var randomColorData = cubeColorDatabase.GetRandomBlockColorData(levelProperties.ColorCount);
            var newBlock = BlockPoolManager.Instance.GetBlock(randomColorData);
            newBlock.Initialize(row, col, randomColorData);
            newBlock.transform.position = spawnPosition;

            return newBlock;
        }

        private int CountEmptySlotsForColumn(int row)
        {
            var count = 0;

            for (int col = levelProperties.ColumnCount - 1; col >= 0; col--)
            {
                if (grid[row, col] == null)
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