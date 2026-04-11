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
            context.ParticleService.PlayBombEffect(Tapped);

            var concurrentChains = new List<UniTask>();
            var sequentialBombs = new List<Block>();

            ProcessAffected(context, chainSchedular, affected, concurrentChains, sequentialBombs);

            if (concurrentChains.Count > 0)
            {
                await UniTask.WhenAll(concurrentChains);
            }

            foreach (var bomb in sequentialBombs)
            {
                if (chainSchedular.IsTriggered(bomb)) continue;

                chainSchedular.MarkTriggered(bomb);
                await UniTask.Delay(TimeSpan.FromSeconds(context.Config.BombChainDelay));
                await effectFactory.CreateEffect(bomb).Execute(context, chainSchedular);
                chainSchedular.RunGravityAndRefill();
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
        private void ProcessAffected(EffectExecutionContext context, IChainSchedular chainSchedular, List<Block> affected,
            List<UniTask> concurrentChains, List<Block> sequentialBombs)
        {
            foreach (var block in affected)
            {
                if (block is IActivatable && !chainSchedular.IsTriggered(block))
                {
                    if (block.BlockType == BlockType.Bomb)
                    {
                        sequentialBombs.Add(block);
                    }
                    else
                    {
                        chainSchedular.MarkTriggered(block);
                        concurrentChains.Add(effectFactory.CreateEffect(block).Execute(context, chainSchedular));
                    }
                }
                else
                {
                    context.TryDestroyBlock(block);
                }
            }
        }
    }
}