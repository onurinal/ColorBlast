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
            try
            {
                var bombData = (BombBlockData)Tapped.BlockData;
                var affected = CollectRadius(context, Tapped.GridX, Tapped.GridY, bombData.Radius);

                chainSchedular.MarkTriggered(Tapped);
                context.ParticleService.PlayBombEffect(Tapped);

                ProcessAffected(context, chainSchedular, affected);
            }
            finally
            {
                await UniTask.CompletedTask;
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
        private void ProcessAffected(EffectExecutionContext context, IChainSchedular chainSchedular, List<Block> affected)
        {
            foreach (var block in affected)
            {
                if (block is IActivatable && !chainSchedular.IsTriggered(block))
                {
                    chainSchedular.MarkTriggered(block);
                    chainSchedular.TriggerEffect(effectFactory.CreateEffect(block));
                }
                else
                {
                    context.TryDestroyBlock(block);
                }
            }
        }
    }
}