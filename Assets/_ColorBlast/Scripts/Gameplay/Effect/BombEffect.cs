using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    public class BombEffect : IBlockEffect
    {
        public Block Tapped { get; }

        private readonly BlockEffectFactory effectFactory;

        public BombEffect(Block source, BlockEffectFactory effectFactory)
        {
            Tapped = source;
            this.effectFactory = effectFactory;
        }

        public async UniTask Execute(EffectExecutionContext context, IEffectSchedular effectSchedular)
        {
            var bombData = (BombBlockData)Tapped.BlockData;
            var affected = CollectRadius(context, Tapped.GridX, Tapped.GridY, bombData.Radius);

            effectSchedular.MarkTriggered(Tapped);
            await context.ParticleService.PlayBombEffect(Tapped);

            foreach (var block in affected)
            {
                if (block is IActivatable && !effectSchedular.IsTriggered(block))
                {
                    effectSchedular.MarkTriggered(block);
                    effectSchedular.TriggerConcurrent(effectFactory.CreateEffect(block));
                }
                else
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