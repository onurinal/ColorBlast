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
            var concurrentChains = new List<UniTask>();
            var bombData = (BombBlockData)Tapped.BlockData;

            var affected = CollectRadius(context, Tapped.GridX, Tapped.GridY, bombData.Radius);

            chainSchedular.MarkTriggered(Tapped);
            context.ParticleService.PlayBombEffect(Tapped);
            ProcessAffected(context, chainSchedular, affected, concurrentChains);

            if (concurrentChains.Count > 0)
            {
                await UniTask.WhenAll(concurrentChains);
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

        /// <summary>
        /// Normal blocks → destroy.
        /// Special blocks (not yet triggered) → enqueue chain their effect.
        /// </summary>
        private void ProcessAffected(EffectExecutionContext context, IChainSchedular chainSchedular, List<Block> blocks,
            List<UniTask> concurrentChains)
        {
            foreach (var block in blocks)
            {
                if (block is IActivatable && !chainSchedular.IsTriggered(block))
                {
                    chainSchedular.MarkTriggered(block);
                    concurrentChains.Add(effectFactory.CreateEffect(block).Execute(context, chainSchedular));
                }
                else
                {
                    context.TryDestroyBlock(block);
                }
            }
        }
    }
}