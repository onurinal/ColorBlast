using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    public class CubeEffect : IBlockEffect
    {
        public Block Tapped { get; }

        private readonly GridChecker gridChecker;
        private readonly GameplayConfig config;

        public CubeEffect(Block source, GridChecker checker, GameplayConfig config)
        {
            Tapped = source;
            gridChecker = checker;
            this.config = config;
        }

        public async UniTask Execute(EffectExecutionContext context, IEffectSchedular effectSchedular)
        {
            var group = gridChecker.GetGroupAt(Tapped.GridX, Tapped.GridY);

            if (group == null || group.Count < config.MatchThreshold)
            {
                return;
            }

            var cubeData = (CubeBlockData)Tapped.BlockData;
            var rewardState = cubeData.GetRewardState(group.Count);

            int rewardRow = Tapped.GridX;
            int rewardCol = Tapped.GridY;

            if (rewardState != null)
            {
                await BlockAnimationHelper.PlayMergeAnimation(group, Tapped, config.MergeDuration);
            }

            foreach (var block in group)
            {
                context.TryDestroyBlock(block);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(config.DestroyDuration));

            if (rewardState?.RewardBlockData != null)
            {
                var sprite = ResolveRewardSprite(cubeData, rewardState.RewardBlockData);
                context.SpawnBlockAt(rewardState.RewardBlockData, rewardRow, rewardCol,
                    sprite, Tapped.BlockData);
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