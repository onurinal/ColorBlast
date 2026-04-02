using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    public class DiscoBallEffect : IBlockEffect
    {
        public Block Source { get; }
        private readonly BlockEffectFactory effectFactory;

        public DiscoBallEffect(Block source, BlockEffectFactory effectFactory)
        {
            Source = source;
            this.effectFactory = effectFactory;
        }

        public async UniTask Execute(EffectExecutionContext context, IChainSchedular chainSchedular)
        {
            var discoBall = (DiscoBlock)Source;

            if (discoBall.TargetCubeData == null)
            {
                return;
            }

            var affected = CollectTargetColor(context, discoBall.TargetCubeData);

            if (affected.Count <= 0)
            {
                return;
            }

            context.DestroyBlock(Source);

            foreach (var block in affected)
            {
                context.DestroyBlock(block);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(context.Config.DestroyDuration));
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