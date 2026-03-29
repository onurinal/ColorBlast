using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    public class BlockEffectResolve
    {
        private static readonly Vector2Int[] Neighbors =
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        private GameplayConfig gameplayConfig;
        private Block[,] blockGrid;
        private LevelProperties levelProperties;
        private GridChecker gridChecker;

        private ComboDetector comboDetector;
        private ComboEffectResolver comboEffectResolver;

        public void Initialize(Block[,] blockGrid, LevelProperties levelProperties, GridChecker gridChecker,
            GameplayConfig gameplayConfig)
        {
            this.gameplayConfig = gameplayConfig;
            this.blockGrid = blockGrid;
            this.levelProperties = levelProperties;
            this.gridChecker = gridChecker;

            comboDetector = new ComboDetector();
            comboDetector.Initialize(blockGrid, levelProperties);
            comboEffectResolver = new ComboEffectResolver();
            comboEffectResolver.Initialize(blockGrid, levelProperties);
        }

        public ResolveResult Resolve(Block block)
        {
            if (block is IActivatable)
            {
                var combo = comboDetector.TryDetect(block);
                if (combo is not null)
                {
                    var (partner, comboType) = combo.Value;
                    var affected = comboEffectResolver.Resolve(block, partner, comboType);
                    return new ResolveResult(affected);
                }
            }

            return block.BlockType switch
            {
                BlockType.Cube => ResolveCubeMatch(block),
                BlockType.Bomb => ResolveBombMatch(block),
                BlockType.Rocket => ResolveRocketMatch(block),
                BlockType.DiscoBall => ResolveDiscoBallMatch(block),
                _ => null
            };
        }

        private ResolveResult ResolveCubeMatch(Block block)
        {
            var group = gridChecker.GetGroupAt(block.GridX, block.GridY);

            if (group == null || group.Count < gameplayConfig.MatchThreshold)
            {
                return null;
            }

            var cubeBlockData = (CubeBlockData)block.BlockData;
            var rewardState = cubeBlockData.GetRewardState(group.Count);

            if (rewardState == null)
            {
                return new ResolveResult(group);
            }

            var rewardSprite = ResolveRewardSprite(cubeBlockData, rewardState.RewardBlockData);

            return new ResolveResult(
                blocksToClear: group,
                rewardData: rewardState.RewardBlockData,
                rewardSprite: rewardSprite,
                targetCubeData: block.BlockData,
                spawnRow: block.GridX,
                spawnColumn: block.GridY
            );
        }

        private ResolveResult ResolveBombMatch(Block block)
        {
            var affectedBlocks = new HashSet<Block>();

            var bombData = (BombBlockData)block.BlockData;
            var centerRow = block.GridX;
            var centerCol = block.GridY;
            var bombRadius = bombData.Radius;

            for (int row = centerRow - bombRadius; row <= centerRow + bombRadius; row++)
            {
                for (int col = centerCol - bombRadius; col <= centerCol + bombRadius; col++)
                {
                    if (!IsInBounds(row, col))
                    {
                        continue;
                    }

                    if (blockGrid[row, col] == null)
                    {
                        continue;
                    }

                    affectedBlocks.Add(blockGrid[row, col]);
                }
            }

            return new ResolveResult(affectedBlocks);
        }

        private ResolveResult ResolveRocketMatch(Block block)
        {
            var affectedBlocks = new HashSet<Block>();

            if (block is not RocketBlock rocketBlock)
            {
                return null;
            }

            return rocketBlock.Direction switch
            {
                RocketDirection.Horizontal => ResolveRowBlocks(block, affectedBlocks),
                RocketDirection.Vertical => ResolveColumnBlocks(block, affectedBlocks),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private ResolveResult ResolveRowBlocks(Block block, HashSet<Block> affectedBlocks)
        {
            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                if (blockGrid[row, block.GridY] != null)
                {
                    affectedBlocks.Add(blockGrid[row, block.GridY]);
                }
            }

            return new ResolveResult(affectedBlocks);
        }

        private ResolveResult ResolveColumnBlocks(Block block, HashSet<Block> affectedBlocks)
        {
            for (int col = 0; col < levelProperties.ColumnCount; col++)
            {
                if (blockGrid[block.GridX, col] != null)
                {
                    affectedBlocks.Add(blockGrid[block.GridX, col]);
                }
            }

            return new ResolveResult(affectedBlocks);
        }

        private ResolveResult ResolveDiscoBallMatch(Block block)
        {
            var affectedBlocks = new HashSet<Block>();

            if (block is not DiscoBlock discoBlock)
            {
                return null;
            }

            affectedBlocks.Add(discoBlock);
            var targetCube = discoBlock.TargetCubeData;

            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                for (int col = 0; col < levelProperties.ColumnCount; col++)
                {
                    if (blockGrid[row, col] == null)
                    {
                        continue;
                    }

                    if (blockGrid[row, col].BlockData == targetCube)
                    {
                        affectedBlocks.Add(blockGrid[row, col]);
                    }
                }
            }

            return new ResolveResult(affectedBlocks);
        }

        private Sprite ResolveRewardSprite(BlockData cubeData, BlockData rewardData)
        {
            if (rewardData is DiscoBlockData discoBlockData)
            {
                var rewardState = discoBlockData.GetRewardState(cubeData);
                return rewardState?.GetSprite();
            }

            return null;
        }

        private bool IsInBounds(int row, int col)
        {
            if (row < 0 || col < 0 || row >= levelProperties.RowCount || col >= levelProperties.ColumnCount)
            {
                return false;
            }

            return true;
        }
    }
}