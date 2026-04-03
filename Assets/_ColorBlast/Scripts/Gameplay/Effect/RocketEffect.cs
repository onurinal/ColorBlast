using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    public class RocketEffect : IBlockEffect
    {
        public Block Source { get; }
        private readonly BlockEffectFactory effectFactory;

        public RocketEffect(Block source, BlockEffectFactory effectFactory)
        {
            Source = source;
            this.effectFactory = effectFactory;
        }

        public async UniTask Execute(EffectExecutionContext context, IChainSchedular chainSchedular)
        {
            var rocket = (RocketBlock)Source;
            var affected = rocket.Direction == RocketDirection.Horizontal
                ? CollectRow(context, rocket.GridY)
                : CollectColumn(context, rocket.GridX);

            if (affected.Count <= 0)
            {
                return;
            }

            chainSchedular.MarkTriggered(Source);
            ProcessAffected(context, chainSchedular, affected);
            await UniTask.Delay(TimeSpan.FromSeconds(context.Config.DestroyDuration));
        }

        private List<Block> CollectRow(EffectExecutionContext context, int col)
        {
            var result = new List<Block>();

            for (int row = 0; row < context.LevelProperties.RowCount; row++)
            {
                if (context.BlockGrid[row, col] != null)
                {
                    result.Add(context.BlockGrid[row, col]);
                }
            }

            return result;
        }

        private List<Block> CollectColumn(EffectExecutionContext context, int row)
        {
            var result = new List<Block>();

            for (int col = 0; col < context.LevelProperties.ColumnCount; col++)
            {
                if (context.BlockGrid[row, col] != null)
                {
                    result.Add(context.BlockGrid[row, col]);
                }
            }

            return result;
        }

        private void ProcessAffected(EffectExecutionContext context, IChainSchedular chainSchedular, List<Block> blocks)
        {
            foreach (var block in blocks)
            {
                if (block is IActivatable && !chainSchedular.IsTriggered(block))
                {
                    context.DestroyBlock(block);
                    var chainedEffect = effectFactory.CreateEffect(block);
                    chainSchedular.EnqueueChained(chainedEffect);
                }
                else
                {
                    context.DestroyBlock(block);
                }
            }
        }
    }
}