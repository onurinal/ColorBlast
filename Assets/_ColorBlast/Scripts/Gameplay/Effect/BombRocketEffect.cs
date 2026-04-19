using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    public class BombRocketEffect : IBlockEffect
    {
        private readonly Block best;
        private readonly Block partner;
        private readonly BlockEffectFactory effectFactory;
        public Block Source { get; }

        public BombRocketEffect(ComboResult comboResult, BlockEffectFactory effectFactory)
        {
            Source = comboResult.Tapped;
            best = comboResult.Best;
            partner = comboResult.Partner;
            this.effectFactory = effectFactory;
        }

        public async UniTask Execute(EffectExecutionContext context, IEffectSchedular effectSchedular)
        {
            context.ReturnToPool(best);

            var bombBlock = best.BlockType == BlockType.Bomb ? best : partner;
            var rocketBlock = best.BlockType == BlockType.Rocket ? best : partner;
            var bombData = (BombBlockData)bombBlock.BlockData;
            var rocketData = (RocketBlockData)rocketBlock.BlockData;

            var radius = bombData.Radius;
            var centerRow = Source.GridX;
            var centerCol = Source.GridY;
            var tasks = new List<UniTask>();

            for (int row = centerRow - radius; row <= centerRow + radius; row++)
            {
                if (!context.IsInBounds(row, centerCol))
                {
                    continue;
                }

                tasks.Add(RocketFire.Execute(row, centerCol, RocketDirection.Vertical, rocketData, context, effectSchedular, effectFactory));
            }

            for (int col = centerCol - radius; col <= centerCol + radius; col++)
            {
                if (!context.IsInBounds(centerRow, col))
                {
                    continue;
                }

                tasks.Add(RocketFire.Execute(centerRow, col, RocketDirection.Horizontal, rocketData, context, effectSchedular, effectFactory));
            }

            await UniTask.WhenAll(tasks);
        }
    }
}