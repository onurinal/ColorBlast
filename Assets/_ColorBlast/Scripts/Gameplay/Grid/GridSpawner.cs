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
                    var spawnPosition = gridManager.GetCellWorldPosition(row, levelProperties.ColumnCount + i);
                    var newBlock = CreateRandomCubeBlockAt(row, targetCol, spawnPosition);

                    var targetPosition = gridManager.GetCellWorldPosition(newBlock.GridX, newBlock.GridY);
                    newBlock.MoveToPosition(targetPosition);
                    blockGrid[row, targetCol] = newBlock;
                }
            }
        }

        public Block SpawnBlockAt(BlockData blockData, int row, int col,
            Sprite sprite = null, BlockData targetCubeData = null)
        {
            var block = ObjectPoolManager.Instance.GetBlock(blockData);
            block.Initialize(row, col, blockData);
            block.transform.position = gridManager.GetCellWorldPosition(row, col);

            switch (block)
            {
                case DiscoBlock discoBlock:
                    discoBlock.SetTargetCubeData(targetCubeData, sprite);
                    break;
                case RocketBlock rocketBlock:
                    rocketBlock.SetupDirection();
                    break;
            }

            blockGrid[row, col] = block;
            return block;
        }

        private Block CreateRandomCubeBlockAt(int row, int col, Vector2 spawnPosition)
        {
            var randomColorData = cubeColorDatabase.GetRandomBlockColorData(levelProperties.ColorCount);
            var newBlock = ObjectPoolManager.Instance.GetBlock(randomColorData);
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