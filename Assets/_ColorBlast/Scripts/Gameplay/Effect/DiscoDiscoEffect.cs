using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    public class DiscoDiscoEffect : IBlockEffect
    {
        private readonly Block best;

        public Block Source { get; }

        public DiscoDiscoEffect(ComboResult comboResult)
        {
            Source = comboResult.Tapped;
            best = comboResult.Best;
        }

        public async UniTask Execute(EffectExecutionContext context, IEffectSchedular effectSchedular)
        {
            context.ReturnToPool(best);

            var affected = new HashSet<Block>();

            UpdateDiscoDiscoAffectedBlocks(context, affected);
            ProcessAffected(context, affected);

            await UniTask.Delay(TimeSpan.FromSeconds(context.Config.DestroyDuration));
        }

        private void UpdateDiscoDiscoAffectedBlocks(EffectExecutionContext context, HashSet<Block> affected)
        {
            for (int row = 0; row < context.LevelProperties.RowCount; row++)
            {
                for (int col = 0; col < context.LevelProperties.ColumnCount; col++)
                {
                    var block = context.BlockGrid[row, col];
                    if (block != null)
                    {
                        affected.Add(block);
                    }
                }
            }
        }

        private void ProcessAffected(EffectExecutionContext context, HashSet<Block> affectedBlocks)
        {
            foreach (var block in affectedBlocks)
            {
                context.TryDestroyBlock(block);
            }
        }
    }
}