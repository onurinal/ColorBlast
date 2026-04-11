using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    public class DiscoBombEffect : IBlockEffect
    {
        public Block Tapped { get; }

        private readonly Block best;
        private readonly Block partner;
        private readonly HashSet<Block> affectedSpecials;
        private readonly List<Block> affectedList = new(); // for execution order
        private readonly BlockEffectFactory effectFactory;

        public DiscoBombEffect(ComboResult comboResult, BlockEffectFactory effectFactory)
        {
            Tapped = comboResult.Tapped;
            best = comboResult.Best;
            partner = comboResult.Partner;
            affectedSpecials = comboResult.AffectedSpecials;
            this.effectFactory = effectFactory;
        }

        public async UniTask Execute(EffectExecutionContext context, IChainSchedular chainSchedular)
        {
            await UpdateDiscoBombAffectedBlocks(context);

            for (int i = 0; i < affectedList.Count; i++)
            {
                if (affectedList[i] == null || chainSchedular.IsTriggered(affectedList[i]))
                {
                    continue;
                }

                chainSchedular.MarkTriggered(affectedList[i]);
                await UniTask.Delay(TimeSpan.FromSeconds(context.Config.BombChainDelay));
                await effectFactory.CreateEffect(affectedList[i]).Execute(context, chainSchedular);
                chainSchedular.RunGravityAndRefill();
            }
        }

        private async UniTask UpdateDiscoBombAffectedBlocks(EffectExecutionContext context)
        {
            var discoBall = best.BlockType == BlockType.DiscoBall ? best : partner;
            var bombBlock = best.BlockType == BlockType.Bomb ? best : partner;

            if (discoBall is not DiscoBlock discoBlock)
            {
                return;
            }

            await TransformByDisco(context, bombBlock.BlockData, discoBlock);
        }

        private async UniTask TransformByDisco(EffectExecutionContext context, BlockData bombBlock,
            DiscoBlock discoBlock)
        {
            var targetCube = discoBlock.TargetCubeData;

            if (targetCube == null)
            {
                return;
            }

            for (int col = context.LevelProperties.ColumnCount - 1; col >= 0; col--)
            {
                for (int row = 0; row < context.LevelProperties.RowCount; row++)
                {
                    var block = context.BlockGrid[row, col];

                    if (block != null && block.BlockData == targetCube)
                    {
                        context.TryRemoveBlock(block);
                        var newBlock = context.SpawnBlockAt(bombBlock, row, col);
                        await UniTask.Delay(TimeSpan.FromSeconds(context.Config.SpawnDurationBetweenSpecials));
                    }
                }
            }

            foreach (var block in affectedSpecials)
            {
                var row = block.GridX;
                var col = block.GridY;

                context.TryRemoveBlock(block);
                var newBlock = context.SpawnBlockAt(bombBlock, row, col);
                await UniTask.Delay(TimeSpan.FromSeconds(context.Config.SpawnDurationBetweenSpecials));
            }

            for (int col = context.LevelProperties.ColumnCount - 1; col >= 0; col--)
            {
                for (int row = 0; row < context.LevelProperties.RowCount; row++)
                {
                    var block = context.BlockGrid[row, col];
                    if (block.BlockData == bombBlock)
                    {
                        affectedList.Add(block);
                    }
                }
            }
        }
    }
}