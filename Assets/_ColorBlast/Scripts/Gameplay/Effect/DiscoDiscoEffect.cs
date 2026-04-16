using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    public class DiscoDiscoEffect : IBlockEffect
    {
        public Block Tapped { get; }

        private readonly HashSet<Block> affectedSpecials;

        public DiscoDiscoEffect(ComboResult comboResult)
        {
            Tapped = comboResult.Tapped;
            affectedSpecials = comboResult.AffectedSpecials;
        }

        public async UniTask Execute(EffectExecutionContext context, IEffectSchedular effectSchedular)
        {
            var affected = new HashSet<Block>();

            foreach (var block in affectedSpecials)
            {
                effectSchedular.MarkTriggered(block);
                affected.Add(block);
            }

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