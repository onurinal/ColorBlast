using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    public class CubeEffect : IBlockEffect
    {
        public Block Source { get; }

        private readonly GridChecker gridChecker;
        private readonly GameplayConfig config;

        public CubeEffect(Block source, GridChecker checker, GameplayConfig config)
        {
            Source = source;
            gridChecker = checker;
            this.config = config;
        }

        public async UniTask Execute(EffectExecutionContext context, IChainSchedular chainSchedular)
        {
            var group = gridChecker.GetGroupAt(Source.GridX, Source.GridY);

            if (group == null || group.Count < config.MatchThreshold)
            {
                return;
            }

            var cubeData = (CubeBlockData)Source.BlockData;
            var rewardState = cubeData.GetRewardState(group.Count);

            int rewardRow = Source.GridX;
            int rewardCol = Source.GridY;

            foreach (var block in group)
            {
                context.DestroyBlock(block);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(config.DestroyDuration));

            if (rewardState?.RewardBlockData != null)
            {
                var sprite = ResolveRewardSprite(cubeData, rewardState.RewardBlockData);
                context.SpawnBlockAt(rewardState.RewardBlockData, rewardRow, rewardCol,
                    sprite, Source.BlockData);
            }
        }

        private Sprite ResolveRewardSprite(BlockData cubeData, BlockData rewardData)
        {
            if (rewardData is DiscoBlockData discoData)
            {
                return discoData.GetRewardState(cubeData)?.GetSprite();
            }

            return null;
        }
    }
}