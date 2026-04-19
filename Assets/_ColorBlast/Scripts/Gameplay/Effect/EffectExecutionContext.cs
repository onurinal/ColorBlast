using ColorBlast.Manager;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    public class EffectExecutionContext
    {
        public Block[,] BlockGrid { get; }
        public LevelProperties LevelProperties { get; }
        public GameplayConfig Config { get; }
        public IBlockParticleService ParticleService { get; }

        private readonly GridSpawner gridSpawner;
        private readonly GridManager gridManager;

        public EffectExecutionContext(Block[,] blockGrid, LevelProperties levelProperties, GameplayConfig config,
            GridSpawner gridSpawner, GridManager gridManager, IBlockParticleService particleService)
        {
            BlockGrid = blockGrid;
            LevelProperties = levelProperties;
            Config = config;
            this.gridSpawner = gridSpawner;
            this.gridManager = gridManager;
            ParticleService = particleService;
        }

        public bool TryDestroyBlock(Block block)
        {
            if (block == null || BlockGrid[block.GridX, block.GridY] != block)
            {
                return false;
            }

            BlockGrid[block.GridX, block.GridY] = null;
            ParticleService.PlayDestroyEffect(block);
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
            var block = BlockGrid[row, col];

            if (block != null)
            {
                TryRemoveBlock(block);
            }

            return gridSpawner.SpawnBlockAt(blockData, row, col, sprite, targetData);
        }

        public Vector2 GetCellWorldPosition(int row, int col)
        {
            return gridManager.GetCellWorldPosition(row, col);
        }

        public bool IsInBounds(int row, int col)
        {
            if (row < 0 || col < 0 || row >= LevelProperties.RowCount || col >= LevelProperties.ColumnCount)
            {
                return false;
            }

            return true;
        }

        public void UnlinkFromGrid(Block block)
        {
            if (block == null || BlockGrid[block.GridX, block.GridY] != block)
            {
                return;
            }

            BlockGrid[block.GridX, block.GridY] = null;
        }

        public void ReturnToPool(Block block)
        {
            if (block == null)
            {
                return;
            }

            block.RemoveBlock();
        }
    }
}