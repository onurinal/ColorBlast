using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    public class ComboEffectWrapper : IBlockEffect
    {
        private readonly IBlockEffect comboEffect;
        private readonly HashSet<Block> affectedSpecials;
        private readonly float mergeDuration;

        private readonly Block bestBlock;
        public Block Source { get; }

        public ComboEffectWrapper(IBlockEffect comboEffect, ComboResult comboResult, float mergeDuration)
        {
            this.comboEffect = comboEffect;
            affectedSpecials = comboResult.AffectedSpecials;
            bestBlock = comboResult.Best;
            Source = comboResult.Tapped;
            this.mergeDuration = mergeDuration;
        }

        public async UniTask Execute(EffectExecutionContext context, IEffectSchedular effectSchedular)
        {
            effectSchedular.SuspendGridUpdates();

            await BlockAnimationHelper.PlayMergeAnimation(affectedSpecials, Source, mergeDuration);

            effectSchedular.ResumeGridUpdates();
            await comboEffect.Execute(context, effectSchedular);
        }
    }
}