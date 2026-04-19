using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    public class RocketRocketEffect : IBlockEffect
    {
        private readonly Block best;
        private readonly BlockEffectFactory effectFactory;

        public Block Source { get; }

        public RocketRocketEffect(ComboResult comboResult, BlockEffectFactory effectFactory)
        {
            Source = comboResult.Tapped;
            best = comboResult.Best;
            this.effectFactory = effectFactory;
        }

        public async UniTask Execute(EffectExecutionContext context, IEffectSchedular effectSchedular)
        {
            context.ReturnToPool(best);

            await UniTask.WhenAll(
                new RocketEffect(Source, effectFactory, RocketDirection.Horizontal).Execute(context, effectSchedular),
                new RocketEffect(Source, effectFactory, RocketDirection.Vertical).Execute(context, effectSchedular)
            );
        }
    }
}