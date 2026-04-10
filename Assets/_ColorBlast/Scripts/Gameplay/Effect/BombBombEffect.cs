using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    public class BombBombEffect : IBlockEffect
    {
        public Block Tapped { get; }

        private readonly Block best;
        private readonly HashSet<Block> affectedSpecials;
        private readonly BlockEffectFactory effectFactory;

        public BombBombEffect(ComboResult comboResult, BlockEffectFactory effectFactory)
        {
            Tapped = comboResult.Tapped;
            best = comboResult.Best;
            affectedSpecials = comboResult.AffectedSpecials;
            this.effectFactory = effectFactory;
        }

        public async UniTask Execute(EffectExecutionContext context, IChainSchedular chainSchedular)
        {
            var concurrentChains = new List<UniTask>();
            var affected = new HashSet<Block>();

            foreach (var block in affectedSpecials)
            {
                chainSchedular.MarkTriggered(block);
                affected.Add(block);
            }

            UpdateBombBombAffectedBlocks(context, affected);
            ProcessAffected(context, chainSchedular, affected, concurrentChains);

            if (concurrentChains.Count > 0)
            {
                await UniTask.WhenAll(concurrentChains);
            }
        }

        private void UpdateBombBombAffectedBlocks(EffectExecutionContext context, HashSet<Block> affected)
        {
            var bombData = (BombBlockData)best.BlockData;
            var radius = bombData.Radius * bombData.DoubleBombMultiplier;
            AddBlocksInRadius(context, affected, Tapped.GridX, Tapped.GridY, radius);
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

        private void ProcessAffected(EffectExecutionContext context, IChainSchedular chainSchedular,
            HashSet<Block> affectedBlocks, List<UniTask> concurrentChains)
        {
            foreach (var block in affectedBlocks)
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