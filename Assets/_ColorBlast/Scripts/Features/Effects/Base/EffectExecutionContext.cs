using ColorBlast.Core;
using ColorBlast.Manager;
using UnityEngine;

namespace ColorBlast.Features
{
    public class EffectExecutionContext
    {
        public Block[,] Grid { get; }
        public LevelProperties LevelProperties { get; }
        public GameConfig Config { get; }
        public IBlockParticleService ParticleService { get; }
        public IHapticService HapticService { get; }

        private readonly GridSpawner gridSpawner;
        private readonly GridManager gridManager;

        public EffectExecutionContext(Block[,] grid, LevelProperties levelProperties, GameConfig config,
            GridSpawner gridSpawner, GridManager gridManager, IBlockParticleService particleService, IHapticService hapticService)
        {
            Grid = grid;
            LevelProperties = levelProperties;
            Config = config;
            this.gridSpawner = gridSpawner;
            this.gridManager = gridManager;
            ParticleService = particleService;
            HapticService = hapticService;
        }

        public bool TryDestroyBlock(Block block)
        {
            if (block == null || Grid[block.GridX, block.GridY] != block)
            {
                return false;
            }

            Grid[block.GridX, block.GridY] = null;
            ParticleService.PlayDestroyEffect(block);
            block.DestroyBlock();
            return true;
        }

        public bool TryRemoveBlock(Block block)
        {
            if (block == null || Grid[block.GridX, block.GridY] != block)
            {
                return false;
            }

            Grid[block.GridX, block.GridY] = null;
            block.RemoveBlock();
            return true;
        }

        public Block SpawnBlockAt(BlockData blockData, int row, int col, BlockData targetData = null)
        {
            var block = Grid[row, col];

            if (block != null)
            {
                TryRemoveBlock(block);
            }

            return gridSpawner.SpawnBlockAt(blockData, row, col, targetData);
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
            if (block == null || Grid[block.GridX, block.GridY] != block)
            {
                return;
            }

            Grid[block.GridX, block.GridY] = null;
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