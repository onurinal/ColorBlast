using ColorBlast.Core;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Features
{
    public class BombEffect : IBlockEffect
    {
        private readonly BlockEffectFactory effectFactory;
        public Block Source { get; }

        public BombEffect(Block source, BlockEffectFactory effectFactory)
        {
            Source = source;
            this.effectFactory = effectFactory;
        }

        public async UniTask Execute(EffectExecutionContext context, IEffectScheduler effectScheduler)
        {
            var bombData = (BombBlockData)Source.BlockData;
            var affectedBlocks = EffectUtility.CollectBlocksInRadius(context, Source.GridX, Source.GridY, bombData.Radius);

            effectScheduler.MarkTriggered(Source);
            await context.ParticleService.PlayBombEffect(Source);
            context.TryRemoveBlock(Source);
            context.HapticService.PlayImpact(HapticManagement.HapticTypes.LightImpact);

            foreach (var block in affectedBlocks)
            {
                EffectUtility.TriggerOrDestroy(block, context, effectScheduler, effectFactory);
            }
        }
    }
}