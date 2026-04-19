using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
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

        public async UniTask Execute(EffectExecutionContext context, IEffectSchedular effectSchedular)
        {
            var bombData = (BombBlockData)Source.BlockData;
            var affected = CollectRadius(context, Source.GridX, Source.GridY, bombData.Radius);

            effectSchedular.MarkTriggered(Source);
            await context.ParticleService.PlayBombEffect(Source);
            context.TryRemoveBlock(Source);

            foreach (var block in affected)
            {
                if (block is IActivatable && !effectSchedular.IsTriggered(block))
                {
                    effectSchedular.MarkTriggered(block);
                    effectSchedular.TriggerConcurrent(effectFactory.CreateEffect(block));
                }
                else if (block is not IActivatable)
                {
                    context.TryDestroyBlock(block);
                }
            }
        }

        private List<Block> CollectRadius(EffectExecutionContext context, int centerRow, int centerCol, int radius)
        {
            var result = new List<Block>();

            for (int row = centerRow - radius; row <= centerRow + radius; row++)
            {
                for (int col = centerCol - radius; col <= centerCol + radius; col++)
                {
                    if (context.IsInBounds(row, col) && context.BlockGrid[row, col] != null)
                    {
                        result.Add(context.BlockGrid[row, col]);
                    }
                }
            }

            return result;
        }
    }
}