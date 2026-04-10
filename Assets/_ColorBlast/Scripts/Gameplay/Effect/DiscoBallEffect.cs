using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    public class DiscoBallEffect : IBlockEffect, IParallelEffect
    {
        public Block Tapped { get; }
        private readonly BlockEffectFactory effectFactory;

        public DiscoBallEffect(Block source, BlockEffectFactory effectFactory)
        {
            Tapped = source;
            this.effectFactory = effectFactory;
        }

        public async UniTask Execute(EffectExecutionContext context, IChainSchedular chainSchedular)
        {
            var discoBall = (DiscoBlock)Tapped;

            if (discoBall.TargetCubeData == null)
            {
                return;
            }

            var affected = CollectTargetColor(context, discoBall.TargetCubeData);

            await UniTask.Delay(TimeSpan.FromSeconds(context.Config.DiscoBallAnimationDuration));

            affected.Add(discoBall);

            foreach (var block in affected)
            {
                context.TryDestroyBlock(block);
            }
        }

        private List<Block> CollectTargetColor(EffectExecutionContext context, BlockData targetData)
        {
            var result = new List<Block>();

            for (int row = 0; row < context.LevelProperties.RowCount; row++)
            {
                for (int col = 0; col < context.LevelProperties.ColumnCount; col++)
                {
                    if (context.BlockGrid[row, col] != null && context.BlockGrid[row, col].BlockData == targetData)
                    {
                        result.Add(context.BlockGrid[row, col]);
                    }
                }
            }

            return result;
        }
    }
}