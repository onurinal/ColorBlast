using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    public class BlockEffectResolve
    {
        private static readonly Vector2Int[] NeighborOffsets =
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        private GameplayConfig gameplayConfig;
        private Block[,] blockGrid;
        private LevelProperties levelProperties;
        private GridChecker gridChecker;

        private readonly HashSet<Block> affectedBlocks = new();
        // private readonly HashSet<Block> neighbors = new();

        public void Initialize(Block[,] blockGrid, LevelProperties levelProperties, GridChecker gridChecker,
            GameplayConfig gameplayConfig)
        {
            this.gameplayConfig = gameplayConfig;
            this.blockGrid = blockGrid;
            this.levelProperties = levelProperties;
            this.gridChecker = gridChecker;
        }

        public ResolveResult Resolve(Block block)
        {
            return block.BlockType switch
            {
                BlockType.Cube => ResolveCubeMatch(block),
                BlockType.Bomb => ResolveBombMatch(block),
                BlockType.Rocket => ResolveRocketMatch(block),
                BlockType.DiscoBall => ResolveDiscoBallMatch(block),
                _ => throw new ArgumentOutOfRangeException()
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

            return new ResolveResult(
                blocksToClear: group,
                targetCubeData: block.BlockData,
                rewardData: rewardState?.RewardBlockData != null ? rewardState.RewardBlockData : null,
                spawnRow: block.GridX,
                spawnColumn: block.GridY
            );
        }

        private ResolveResult ResolveBombMatch(Block block)
        {
            affectedBlocks.Clear();

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
            affectedBlocks.Clear();

            if (block is not RocketBlock rocketBlock)
            {
                return null;
            }

            return rocketBlock.Direction switch
            {
                RocketDirection.Horizontal => ResolveRowBlocks(block),
                RocketDirection.Vertical => ResolveColumnBlocks(block),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private ResolveResult ResolveRowBlocks(Block block)
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

        private ResolveResult ResolveColumnBlocks(Block block)
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
            affectedBlocks.Clear();

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

        private bool IsInBounds(int row, int col)
        {
            if (row < 0 || col < 0 || row >= levelProperties.RowCount || col >= levelProperties.ColumnCount)
            {
                return false;
            }

            return true;
        }

        // private void TryComboChain(Block block)
        // {
        //     neighbors.Clear();
        //
        //     var row = block.GridX;
        //     var col = block.GridY;
        //
        //     foreach (var neighbor in NeighborOffsets)
        //     {
        //         var neighborRow = row + neighbor.x;
        //         var neighborCol = col + neighbor.y;
        //
        //         if (!IsInBounds(neighborRow, neighborCol))
        //         {
        //             continue;
        //         }
        //
        //         if (blockGrid[neighborRow, neighborCol] == null)
        //         {
        //             continue;
        //         }
        //
        //         if (block is IActivatable activatableBlock)
        //         {
        //             neighbors.Add(blockGrid[neighborRow, neighborCol]);
        //         }
        //     }
        //
        //     if (neighbors.Count > 0)
        //     {
        //         // apply combo chain
        //     }
        // }
        //
        // private void ResolveComboChain(Block block, HashSet<Block> otherSpecials)
        // {
        //     // priority disco ball > bomb > rocket
        // }
    }
}