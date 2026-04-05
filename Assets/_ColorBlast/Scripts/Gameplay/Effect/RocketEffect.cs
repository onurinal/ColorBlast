using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    public class RocketEffect : IBlockEffect
    {
        public Block Tapped { get; }
        private readonly BlockEffectFactory effectFactory;

        public RocketEffect(Block source, BlockEffectFactory effectFactory)
        {
            Tapped = source;
            this.effectFactory = effectFactory;
        }

        public async UniTask Execute(EffectExecutionContext context, IChainSchedular chainSchedular)
        {
            var rocket = (RocketBlock)Tapped;
            var affected = rocket.Direction == RocketDirection.Horizontal
                ? CollectRow(context, chainSchedular, rocket.GridY)
                : CollectColumn(context, chainSchedular, rocket.GridX);

            if (affected.Count <= 0)
            {
                return;
            }

            chainSchedular.MarkTriggered(Tapped);
            ProcessAffected(context, chainSchedular, affected);

            await UniTask.Delay(TimeSpan.FromSeconds(context.Config.DestroyDuration));
        }

        private List<Block> CollectRow(EffectExecutionContext context, IChainSchedular chainSchedular, int col)
        {
            var result = new List<Block>();

            for (int row = 0; row < context.LevelProperties.RowCount; row++)
            {
                var block = context.BlockGrid[row, col];

                if (block != null)
                {
                    if (block is RocketBlock rocketBlock && !chainSchedular.IsTriggered(rocketBlock))
                    {
                        if (rocketBlock.Direction == RocketDirection.Horizontal)
                        {
                            chainSchedular.MarkTriggered(rocketBlock);
                        }
                    }

                    result.Add(block);
                }
            }

            return result;
        }

        private List<Block> CollectColumn(EffectExecutionContext context, IChainSchedular chainSchedular, int row)
        {
            var result = new List<Block>();

            for (int col = 0; col < context.LevelProperties.ColumnCount; col++)
            {
                var block = context.BlockGrid[row, col];

                if (block != null)
                {
                    if (block is RocketBlock rocketBlock && !chainSchedular.IsTriggered(rocketBlock))
                    {
                        if (rocketBlock.Direction == RocketDirection.Vertical)
                        {
                            chainSchedular.MarkTriggered(rocketBlock);
                        }
                    }

                    result.Add(block);
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