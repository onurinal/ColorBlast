using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

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

        public async UniTask Execute(EffectExecutionContext context, IChainSchedular chainSchedular)
        {
            try
            {
                chainSchedular.BeginEffect();

                var rocket = (RocketBlock)Tapped;
                chainSchedular.MarkTriggered(Tapped);

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

                await RocketFire.Execute(rocket.GridX, rocket.GridY, rocketDirection, rocket.RocketBlockData, context, chainSchedular, effectFactory);
            }
            finally
            {
                chainSchedular.EndEffect();
            }
        }
    }
}