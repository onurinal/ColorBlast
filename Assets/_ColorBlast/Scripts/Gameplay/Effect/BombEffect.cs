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

        public async UniTask Execute(EffectExecutionContext context, IChainSchedular chainSchedular)
        {
            var bombData = (BombBlockData)Tapped.BlockData;
            var affected = CollectRadius(context, Tapped.GridX, Tapped.GridY, bombData.Radius);

            chainSchedular.MarkTriggered(Tapped);
            await context.ParticleService.PlayBombEffect(Tapped);

            List<Block> specialBlocksToChain = new();

            foreach (var block in affected)
            {
                if (block is IActivatable && !chainSchedular.IsTriggered(block))
                {
                    chainSchedular.MarkTriggered(block);
                    specialBlocksToChain.Add(block);
                }
                else
                {
                    context.TryDestroyBlock(block);
                }
            }

            foreach (var block in specialBlocksToChain)
            {
                // if (block.BlockType == BlockType.Bomb)
                // {
                //     await UniTask.Delay(TimeSpan.FromSeconds(context.Config.BombChainDelay));
                // }

                chainSchedular.TriggerEffectAsync(effectFactory.CreateEffect(block)).Forget();
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