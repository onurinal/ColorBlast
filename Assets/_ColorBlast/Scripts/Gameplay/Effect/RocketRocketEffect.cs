using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    public class RocketRocketEffect : IBlockEffect
    {
        public Block Tapped { get; }
        private readonly HashSet<Block> affectedSpecials;
        private readonly BlockEffectFactory effectFactory;

        public RocketRocketEffect(ComboResult comboResult, BlockEffectFactory effectFactory)
        {
            Tapped = comboResult.Tapped;
            affectedSpecials = comboResult.AffectedSpecials;
            this.effectFactory = effectFactory;
        }

        public async UniTask Execute(EffectExecutionContext context, IChainSchedular chainSchedular)
        {
            foreach (var block in affectedSpecials)
            {
                context.TryRemoveBlock(block);
            }

            await UniTask.WhenAll(
                new RocketEffect(Tapped, effectFactory, RocketDirection.Horizontal).Execute(context, chainSchedular),
                new RocketEffect(Tapped, effectFactory, RocketDirection.Vertical).Execute(context, chainSchedular)
            );
        }
    }
}