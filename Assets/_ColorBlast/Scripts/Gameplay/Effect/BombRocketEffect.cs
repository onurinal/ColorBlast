using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    public class BombRocketEffect : IBlockEffect
    {
        public Block Tapped { get; }

        private readonly Block best;
        private readonly Block partner;
        private readonly HashSet<Block> affectedSpecials;
        private readonly BlockEffectFactory effectFactory;

        public BombRocketEffect(ComboResult comboResult, BlockEffectFactory effectFactory)
        {
            Tapped = comboResult.Tapped;
            best = comboResult.Best;
            partner = comboResult.Partner;
            affectedSpecials = comboResult.AffectedSpecials;
            this.effectFactory = effectFactory;
        }

        public async UniTask Execute(EffectExecutionContext context, IChainSchedular chainSchedular)
        {
            var affected = new HashSet<Block>();

            foreach (var block in affectedSpecials)
            {
                chainSchedular.MarkTriggered(block);
                affected.Add(block);
            }

            UpdateBombRocketAffectedBlocks(context, affected);
            ProcessAffected(context, chainSchedular, affected);

            await UniTask.Delay(TimeSpan.FromSeconds(context.Config.DestroyDuration));
        }

        private void UpdateBombRocketAffectedBlocks(EffectExecutionContext context, HashSet<Block> affected)
        {
            var bombBlock = best.BlockType == BlockType.Bomb ? best : partner;
            var bombRadius = ((BombBlockData)bombBlock.BlockData).Radius;
            var centerRow = Tapped.GridX;
            var centerCol = Tapped.GridY;

            for (int row = centerRow - bombRadius; row <= centerRow + bombRadius; row++)
            {
                if (context.IsInBounds(row, centerCol))
                {
                    AddVerticalLine(context, affected, row);
                }
            }

            for (int col = centerCol - bombRadius; col <= centerCol + bombRadius; col++)
            {
                if (context.IsInBounds(centerRow, col))
                {
                    AddHorizontalLine(context, affected, col);
                }
            }
        }

        private void AddHorizontalLine(EffectExecutionContext context, HashSet<Block> affected, int col)
        {
            for (int row = 0; row < context.LevelProperties.RowCount; row++)
            {
                if (context.BlockGrid[row, col] != null)
                {
                    affected.Add(context.BlockGrid[row, col]);
                }
            }
        }

        private void AddVerticalLine(EffectExecutionContext context, HashSet<Block> affected, int row)
        {
            for (int col = 0; col < context.LevelProperties.ColumnCount; col++)
            {
                if (context.BlockGrid[row, col] != null)
                {
                    affected.Add(context.BlockGrid[row, col]);
                }
            }
        }

        private void ProcessAffected(EffectExecutionContext context, IChainSchedular chainSchedular,
            HashSet<Block> affectedBlocks)
        {
            foreach (var block in affectedBlocks)
            {
                if (block is IActivatable && !chainSchedular.IsTriggered(block))
                {
                    var chainedEffect = effectFactory.CreateEffect(block);
                    chainSchedular.EnqueueChained(chainedEffect);
                }
                else
                {
                    // Debug.Log($"blockType = {block.BlockType} is not IActivatable");
                    context.TryDestroyBlock(block);
                }
            }
        }
    }
}