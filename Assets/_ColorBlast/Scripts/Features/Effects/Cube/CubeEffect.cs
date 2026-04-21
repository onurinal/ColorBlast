using System;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Features
{
    public class CubeEffect : IBlockEffect
    {
        private readonly GridChecker gridChecker;
        private readonly GameConfig config;

        public Block Source { get; }

        public CubeEffect(Block source, GridChecker checker, GameConfig config)
        {
            Source = source;
            gridChecker = checker;
            this.config = config;
        }

        public async UniTask Execute(EffectExecutionContext context, IEffectScheduler effectScheduler)
        {
            var group = gridChecker.GetGroupAt(Source.GridX, Source.GridY);

            if (group == null || group.Count < config.MatchThreshold)
            {
                return;
            }

            var cubeData = (CubeBlockData)Source.BlockData;
            var rewardState = cubeData.GetRewardState(group.Count);

            context.HapticService.PlaySelection();

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
                context.SpawnBlockAt(rewardState.RewardBlockData, Source.GridX, Source.GridY, Source.BlockData);
            }
        }
    }
}