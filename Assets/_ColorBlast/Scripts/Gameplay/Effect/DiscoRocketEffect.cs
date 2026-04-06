using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    public class DiscoRocketEffect : IBlockEffect
    {
        public Block Tapped { get; }

        private readonly Block best;
        private readonly Block partner;
        private readonly HashSet<Block> affectedSpecials;
        private readonly List<Block> affectedList = new(); // for execution order
        private readonly BlockEffectFactory effectFactory;

        private readonly HashSet<int> triggeredRows = new();
        private readonly HashSet<int> triggeredCols = new();

        public DiscoRocketEffect(ComboResult comboResult, BlockEffectFactory effectFactory)
        {
            Tapped = comboResult.Tapped;
            best = comboResult.Best;
            partner = comboResult.Partner;
            affectedSpecials = comboResult.AffectedSpecials;
            this.effectFactory = effectFactory;
        }

        public async UniTask Execute(EffectExecutionContext context, IChainSchedular chainSchedular)
        {
            await UpdateDiscoRocketAffectedBlocks(context);
            ProcessAffected(context, chainSchedular);

            await UniTask.Delay(TimeSpan.FromSeconds(context.Config.DestroyDuration));
        }

        private async UniTask UpdateDiscoRocketAffectedBlocks(EffectExecutionContext context)
        {
            var discoBall = best.BlockType == BlockType.DiscoBall ? best : partner;
            var rocketBlock = best.BlockType == BlockType.Rocket ? best : partner;

            if (discoBall is not DiscoBlock discoBlock)
            {
                return;
            }

            await TransformByDisco(context, rocketBlock.BlockData, discoBlock);
        }

        private async UniTask TransformByDisco(EffectExecutionContext context, BlockData rocketBlockData,
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
                        context.RemoveBlock(block);
                        var newBlock = context.SpawnBlockAt(rocketBlockData, row, col);

                        await UniTask.Delay(TimeSpan.FromSeconds(context.Config.SpawnDurationBetweenSpecials));
                    }
                }
            }

            foreach (var block in affectedSpecials)
            {
                var row = block.GridX;
                var col = block.GridY;

                context.RemoveBlock(block);
                var newBlock = context.SpawnBlockAt(rocketBlockData, row, col);

                await UniTask.Delay(TimeSpan.FromSeconds(context.Config.SpawnDurationBetweenSpecials));
            }

            for (int col = 0; col < context.LevelProperties.ColumnCount; col++)
            {
                for (int row = 0; row < context.LevelProperties.RowCount; row++)
                {
                    var block = context.BlockGrid[row, col];
                    if (block.BlockData == rocketBlockData)
                    {
                        affectedList.Add(block);
                    }
                }
            }
        }

        private bool ShouldTriggerRocket(Block block)
        {
            if (block is not RocketBlock rocketBlock)
            {
                return false;
            }

            int row = rocketBlock.GridX;
            int col = rocketBlock.GridY;

            if (rocketBlock.Direction == RocketDirection.Horizontal)
            {
                return triggeredCols.Add(col);
            }

            if (rocketBlock.Direction == RocketDirection.Vertical)
            {
                return triggeredRows.Add(row);
            }

            return false;
        }

        private void ProcessAffected(EffectExecutionContext context, IChainSchedular chainSchedular)
        {
            foreach (var block in affectedList)
            {
                if (block is RocketBlock)
                {
                    if (!ShouldTriggerRocket(block))
                    {
                        chainSchedular.MarkTriggered(block);
                    }
                    else if (!chainSchedular.IsTriggered(block))
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
}