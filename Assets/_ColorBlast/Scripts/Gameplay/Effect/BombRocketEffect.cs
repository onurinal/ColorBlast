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
            foreach (var block in affectedSpecials)
            {
                chainSchedular.MarkTriggered(block);
                context.TryRemoveBlock(block);
            }

            var bombBlock = best.BlockType == BlockType.Bomb ? best : partner;
            var rocketBlock = best.BlockType == BlockType.Rocket ? best : partner;
            var bombData = (BombBlockData)bombBlock.BlockData;
            var rocketData = (RocketBlockData)rocketBlock.BlockData;

            var radius = bombData.Radius;
            var centerRow = Tapped.GridX;
            var centerCol = Tapped.GridY;
            var tasks = new List<UniTask>();

            for (int row = centerRow - radius; row <= centerRow + radius; row++)
            {
                if (!context.IsInBounds(row, centerCol))
                {
                    continue;
                }

                // ClearOriginCell(row, centerCol, context, chainSchedular, tasks);
                tasks.Add(RocketFire.Execute(row, centerCol, RocketDirection.Vertical, rocketData, context, chainSchedular, effectFactory));
            }

            for (int col = centerCol - radius; col <= centerCol + radius; col++)
            {
                if (!context.IsInBounds(centerRow, col))
                {
                    continue;
                }

                // ClearOriginCell(centerRow, col, context, chainSchedular, tasks);
                tasks.Add(RocketFire.Execute(centerRow, col, RocketDirection.Horizontal, rocketData, context, chainSchedular, effectFactory));
            }

            await UniTask.WhenAll(tasks);
        }
    }
}