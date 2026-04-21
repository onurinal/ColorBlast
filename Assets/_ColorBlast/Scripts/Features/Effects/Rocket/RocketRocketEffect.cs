using System.Collections.Generic;
using ColorBlast.Core;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Features
{
    public class RocketRocketEffect : IBlockEffect
    {
        private readonly HashSet<Block> affectedSpecials;
        private readonly BlockEffectFactory effectFactory;

        public Block Source { get; }

        public RocketRocketEffect(ComboResult comboResult, BlockEffectFactory effectFactory)
        {
            Source = comboResult.Tapped;
            affectedSpecials = comboResult.AffectedSpecials;
            this.effectFactory = effectFactory;
        }

        public async UniTask Execute(EffectExecutionContext context, IEffectScheduler effectScheduler)
        {
            EffectUtility.RemoveComboSpecials(context, affectedSpecials);

            context.HapticService.PlayImpact(HapticManagement.HapticTypes.MediumImpact);
            await UniTask.WhenAll(
                new RocketEffect(Source, effectFactory, RocketDirection.Horizontal).Execute(context, effectScheduler),
                new RocketEffect(Source, effectFactory, RocketDirection.Vertical).Execute(context, effectScheduler)
            );
        }
    }
}