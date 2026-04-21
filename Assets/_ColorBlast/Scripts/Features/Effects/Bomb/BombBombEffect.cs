using System.Collections.Generic;
using ColorBlast.Core;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Features
{
    public class BombBombEffect : IBlockEffect
    {
        private readonly Block best;
        private readonly HashSet<Block> affectedSpecials;
        private readonly BlockEffectFactory effectFactory;

        public Block Source { get; }

        public BombBombEffect(ComboResult comboResult, BlockEffectFactory effectFactory)
        {
            Source = comboResult.Tapped;
            best = comboResult.Best;
            affectedSpecials = comboResult.AffectedSpecials;
            this.effectFactory = effectFactory;
        }

        public async UniTask Execute(EffectExecutionContext context, IEffectScheduler effectScheduler)
        {
            EffectUtility.RemoveComboSpecials(context, affectedSpecials, best);

            await BlockAnimationHelper.PlayExpandAnimation(best);

            var bombData = (BombBlockData)best.BlockData;
            int expandedRadius = bombData.Radius * bombData.DoubleBombMultiplier;

            var affectedBlocks = EffectUtility.CollectBlocksInRadius(context, Source.GridX, Source.GridY, expandedRadius);

            context.TryRemoveBlock(best);
            context.HapticService.PlayImpact(HapticManagement.HapticTypes.HeavyImpact);

            foreach (var block in affectedBlocks)
            {
                EffectUtility.TriggerOrDestroy(block, context, effectScheduler, effectFactory);
            }
        }
    }
}