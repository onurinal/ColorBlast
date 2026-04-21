using System;
using System.Collections.Generic;
using ColorBlast.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace ColorBlast.Features
{
    public class DiscoDiscoEffect : IBlockEffect
    {
        private readonly Block best;
        private readonly HashSet<Block> affectedSpecials;

        public Block Source { get; }

        public DiscoDiscoEffect(ComboResult comboResult)
        {
            Source = comboResult.Tapped;
            best = comboResult.Best;
            affectedSpecials = comboResult.AffectedSpecials;
        }

        public async UniTask Execute(EffectExecutionContext context, IEffectScheduler effectScheduler)
        {
            EffectUtility.RemoveComboSpecials(context, affectedSpecials, best);

            var discoBlock = (DiscoBlock)best;
            var discoData = (DiscoBlockData)discoBlock.BlockData;
            var affected = new HashSet<Block>();

            context.HapticService.PlayImpact(HapticManagement.HapticTypes.HeavyImpact);
            var (shake, scale) = DiscoAnimationHelper.AnimateShakeAndScale(discoBlock);
            await DiscoAnimationHelper.CycleColors(discoBlock, discoData.GetAllColors(), 1.5f, 0.05f);
            shake.Kill();
            scale.Kill();

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
                    var block = context.Grid[row, col];
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