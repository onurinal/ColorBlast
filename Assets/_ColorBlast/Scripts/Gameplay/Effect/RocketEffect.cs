using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    public class RocketEffect : IBlockEffect
    {
        public Block Tapped { get; }

        private readonly BlockEffectFactory effectFactory;
        private readonly RocketDirection? directionOverride;

        public RocketEffect(Block source, BlockEffectFactory effectFactory, RocketDirection? directionOverride = null)
        {
            Tapped = source;
            this.effectFactory = effectFactory;
            this.directionOverride = directionOverride;
        }

        public async UniTask Execute(EffectExecutionContext context, IEffectSchedular effectSchedular)
        {
            var rocket = (RocketBlock)Tapped;
            effectSchedular.MarkTriggered(Tapped);

            context.TryRemoveBlock(rocket);

            RocketDirection rocketDirection;

            if (directionOverride != null)
            {
                rocketDirection = directionOverride.Value;
            }
            else
            {
                rocketDirection = rocket.Direction;
            }

            await RocketFire.Execute(rocket.GridX, rocket.GridY, rocketDirection, rocket.RocketBlockData, context, effectSchedular, effectFactory);
        }
    }
}