using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    public class BombBombEffect : IBlockEffect
    {
        private readonly Block best;
        private readonly BlockEffectFactory effectFactory;

        public Block Source { get; }

        public BombBombEffect(ComboResult comboResult, BlockEffectFactory effectFactory)
        {
            Source = comboResult.Tapped;
            best = comboResult.Best;
            this.effectFactory = effectFactory;
        }

        public UniTask Execute(EffectExecutionContext context, IEffectSchedular effectSchedular)
        {
            context.ReturnToPool(best);

            var affected = new HashSet<Block>();

            UpdateBombBombAffectedBlocks(context, affected);
            ProcessAffected(context, effectSchedular, affected);

            return UniTask.CompletedTask;
        }

        private void UpdateBombBombAffectedBlocks(EffectExecutionContext context, HashSet<Block> affected)
        {
            var bombData = (BombBlockData)best.BlockData;
            var radius = bombData.Radius * bombData.DoubleBombMultiplier;
            AddBlocksInRadius(context, affected, Source.GridX, Source.GridY, radius);
        }

        private void AddBlocksInRadius(EffectExecutionContext context, HashSet<Block> affected, int centerRow,
            int centerCol, int radius)
        {
            for (int row = centerRow - radius; row <= centerRow + radius; row++)
            {
                for (int col = centerCol - radius; col <= centerCol + radius; col++)
                {
                    if (context.IsInBounds(row, col) && context.BlockGrid[row, col] != null)
                    {
                        affected.Add(context.BlockGrid[row, col]);
                    }
                }
            }
        }

        private void ProcessAffected(EffectExecutionContext context, IEffectSchedular effectSchedular,
            HashSet<Block> affectedBlocks)
        {
            foreach (var block in affectedBlocks)
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
    }
}