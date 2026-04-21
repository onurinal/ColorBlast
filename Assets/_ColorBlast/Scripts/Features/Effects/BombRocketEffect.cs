using System.Collections.Generic;
using ColorBlast.Core;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Features
{
    public class BombRocketEffect : IBlockEffect
    {
        private readonly Block best;
        private readonly Block partner;
        private readonly HashSet<Block> affectedSpecials;
        private readonly BlockEffectFactory effectFactory;
        public Block Source { get; }

        public BombRocketEffect(ComboResult comboResult, BlockEffectFactory effectFactory)
        {
            Source = comboResult.Tapped;
            best = comboResult.Best;
            partner = comboResult.Partner;
            affectedSpecials = comboResult.AffectedSpecials;
            this.effectFactory = effectFactory;
        }

        public async UniTask Execute(EffectExecutionContext context, IEffectScheduler effectScheduler)
        {
            var centerRow = Source.GridX;
            var centerCol = Source.GridY;
            var centerPosition = Source.transform.position;

            EffectUtility.RemoveComboSpecials(context, affectedSpecials, best, partner);

            var bombBlock = best.BlockType == BlockType.Bomb ? best : partner;
            var rocketBlock = best.BlockType == BlockType.Rocket ? best : partner;
            var bombData = (BombBlockData)bombBlock.BlockData;
            var rocketData = (RocketBlockData)rocketBlock.BlockData;

            var radius = bombData.Radius;
            var tasks = new List<UniTask>();

            context.HapticService.PlayImpact(HapticManagement.HapticTypes.HeavyImpact);
            await BlockAnimationHelper.PlayBombRocketComboAnimation(bombBlock, rocketBlock, centerPosition);
            context.TryRemoveBlock(best);
            context.TryRemoveBlock(partner);

            for (int row = centerRow - radius; row <= centerRow + radius; row++)
            {
                if (!context.IsInBounds(row, centerCol))
                {
                    continue;
                }

                tasks.Add(RocketFire.Execute(row, centerCol, RocketDirection.Vertical, rocketData, context, effectScheduler, effectFactory));
            }

            for (int col = centerCol - radius; col <= centerCol + radius; col++)
            {
                if (!context.IsInBounds(centerRow, col))
                {
                    continue;
                }

                tasks.Add(RocketFire.Execute(centerRow, col, RocketDirection.Horizontal, rocketData, context, effectScheduler, effectFactory));
            }

            await UniTask.WhenAll(tasks);
        }
    }
}