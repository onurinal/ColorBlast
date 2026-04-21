using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Features
{
    public class ComboEffectWrapper : IBlockEffect
    {
        private readonly IBlockEffect comboEffect;
        private readonly HashSet<Block> affectedSpecials;
        private readonly float mergeDuration;

        public Block Source { get; }

        public ComboEffectWrapper(IBlockEffect comboEffect, ComboResult comboResult, float mergeDuration)
        {
            this.comboEffect = comboEffect;
            affectedSpecials = comboResult.AffectedSpecials;
            Source = comboResult.Tapped;
            this.mergeDuration = mergeDuration;
        }

        public async UniTask Execute(EffectExecutionContext context, IEffectScheduler effectScheduler)
        {
            effectScheduler.SuspendGridUpdates();

            context.HapticService.PlaySelection();
            await BlockAnimationHelper.PlayMergeAnimation(affectedSpecials, Source, mergeDuration);

            effectScheduler.ResumeGridUpdates();
            await comboEffect.Execute(context, effectScheduler);
        }
    }
}