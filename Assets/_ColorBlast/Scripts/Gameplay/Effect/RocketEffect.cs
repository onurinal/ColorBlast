using ColorBlast.Core;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    public class RocketEffect : IBlockEffect
    {
        private readonly BlockEffectFactory effectFactory;
        private readonly RocketDirection? directionOverride;
        public Block Source { get; }

        public RocketEffect(Block source, BlockEffectFactory effectFactory, RocketDirection? directionOverride = null)
        {
            Source = source;
            this.effectFactory = effectFactory;
            this.directionOverride = directionOverride;
        }

        public async UniTask Execute(EffectExecutionContext context, IEffectSchedular effectSchedular)
        {
            var rocket = (RocketBlock)Source;
            effectSchedular.MarkTriggered(Source);
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

            context.HapticService.PlayImpact(HapticManagement.HapticTypes.MediumImpact);
            await RocketFire.Execute(rocket.GridX, rocket.GridY, rocketDirection, rocket.RocketBlockData, context, effectSchedular, effectFactory);
        }
    }
}