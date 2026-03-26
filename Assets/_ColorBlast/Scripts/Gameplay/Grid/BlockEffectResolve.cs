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

        public void Initialize(Block[,] blockGrid, LevelProperties levelProperties, GridChecker gridChecker,
            GameplayConfig gameplayConfig)
        {
            this.gameplayConfig = gameplayConfig;
            this.blockGrid = blockGrid;
            this.levelProperties = levelProperties;
            this.gridChecker = gridChecker;
        }

        public List<Block> Resolve(Block block)
        {
            return block.BlockType switch
            {
                BlockType.Cube => ResolveCubeMatch(block),
                BlockType.Tnt => ResolveTntMatch(block),
                BlockType.Rocket => ResolveRocketMatch(block),
                BlockType.Rainbow => ResolveRainbowMatch(block),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private List<Block> ResolveCubeMatch(Block block)
        {
            var matchGroup = gridChecker.GetGroupAt(block.GridX, block.GridY);

            if (matchGroup.Count < gameplayConfig.MatchThreshold)
            {
                return null;
            }

            var cubeBlockData = (CubeBlockData)block.BlockData;
            var rewardState = cubeBlockData.GetRewardState(matchGroup.Count);

            if (rewardState != null && rewardState.RewardPrefab != null)
            {
                // spawn special block
            }

            return matchGroup;
        }

        private List<Block> ResolveTntMatch(Block block)
        {
            return null;
        }

        private List<Block> ResolveRocketMatch(Block block)
        {
            return null;
        }

        private List<Block> ResolveRainbowMatch(Block block)
        {
            return null;
        }
    }
}