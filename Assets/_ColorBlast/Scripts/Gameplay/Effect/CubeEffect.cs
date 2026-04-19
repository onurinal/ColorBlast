using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    public class CubeEffect : IBlockEffect
    {
        private readonly GridChecker gridChecker;
        private readonly GameplayConfig config;

        public Block Source { get; }

        public CubeEffect(Block source, GridChecker checker, GameplayConfig config)
        {
            Source = source;
            gridChecker = checker;
            this.config = config;
        }

        public async UniTask Execute(EffectExecutionContext context, IEffectSchedular effectSchedular)
        {
            var group = gridChecker.GetGroupAt(Source.GridX, Source.GridY);

            if (group == null || group.Count < config.MatchThreshold)
            {
                return;
            }

            var cubeData = (CubeBlockData)Source.BlockData;
            var rewardState = cubeData.GetRewardState(group.Count);

            if (rewardState != null)
            {
                await BlockAnimationHelper.PlayMergeAnimation(group, Source, config.MergeDuration);
            }

            foreach (var block in group)
            {
                context.TryDestroyBlock(block);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(config.DestroyDuration));

            if (rewardState?.RewardBlockData != null)
            {
                var sprite = ResolveRewardSprite(cubeData, rewardState.RewardBlockData);
                context.SpawnBlockAt(rewardState.RewardBlockData, Source.GridX, Source.GridY, sprite, Source.BlockData);
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