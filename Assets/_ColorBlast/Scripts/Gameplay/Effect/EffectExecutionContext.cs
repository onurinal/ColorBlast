using UnityEngine;

namespace ColorBlast.Gameplay
{
    public class EffectExecutionContext
    {
        public Block[,] BlockGrid { get; }
        public LevelProperties LevelProperties { get; }
        public GameplayConfig Config { get; }

        private readonly GridSpawner gridSpawner;

        public EffectExecutionContext(Block[,] blockGrid, LevelProperties levelProperties, GameplayConfig config,
            GridSpawner gridSpawner)
        {
            BlockGrid = blockGrid;
            LevelProperties = levelProperties;
            Config = config;
            this.gridSpawner = gridSpawner;
        }

        public bool TryDestroyBlock(Block block)
        {
            if (block == null || BlockGrid[block.GridX, block.GridY] != block)
            {
                return false;
            }

            BlockGrid[block.GridX, block.GridY] = null;
            block.DestroyBlock();
            return true;
        }

        public bool TryRemoveBlock(Block block)
        {
            if (block == null || BlockGrid[block.GridX, block.GridY] != block)
            {
                return false;
            }

            BlockGrid[block.GridX, block.GridY] = null;
            block.RemoveBlock();
            return true;
        }

        public Block SpawnBlockAt(BlockData blockData, int row, int col, Sprite sprite = null,
            BlockData targetData = null)
        {
            return gridSpawner.SpawnBlockAt(blockData, row, col, sprite, targetData);
        }

        public bool IsInBounds(int row, int col)
        {
            if (row < 0 || col < 0 || row >= LevelProperties.RowCount || col >= LevelProperties.ColumnCount)
            {
                return false;
            }

            return true;
        }
    }
}