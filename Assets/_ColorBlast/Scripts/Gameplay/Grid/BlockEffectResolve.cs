using System;
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

        public ResolveResult Resolve(Block block)
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
                rewardData: rewardState?.RewardBlockData != null ? rewardState.RewardBlockData : null,
                spawnRow: block.GridX,
                spawnColumn: block.GridY
            );
        }

        private ResolveResult ResolveTntMatch(Block block)
        {
            return null;
        }

        private ResolveResult ResolveRocketMatch(Block block)
        {
            return null;
        }

        private ResolveResult ResolveRainbowMatch(Block block)
        {
            return null;
        }
    }
}