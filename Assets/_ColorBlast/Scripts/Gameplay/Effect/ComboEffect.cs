using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    /// <summary>
    /// Executes combo effects between two special blocks.
    ///
    /// Combo effects:
    /// DiscoBall + DiscoBall → Clear entire board
    /// Bomb + Bomb           → Combined enlarged radius from both centers
    /// Bomb + Rocket         → Bomb radius + rocket's line clear
    /// Rocket + Rocket       → Cross clear (horizontal + vertical)
    /// </summary>
    public class ComboEffect : IBlockEffect
    {
        public Block Source { get; }

        private readonly Block partner;
        private readonly List<Block> affectedSpecials;
        private readonly ComboType comboType;
        private readonly BlockEffectFactory factory;

        public ComboEffect(Block source, Block partner, List<Block> affectedSpecials, ComboType type,
            BlockEffectFactory factory)
        {
            Source = source;
            this.partner = partner;
            this.affectedSpecials = affectedSpecials;
            comboType = type;
            this.factory = factory;
        }

        public async UniTask Execute(EffectExecutionContext context, IChainSchedular chainSchedular)
        {
            foreach (var affectedSpecial in affectedSpecials)
            {
                chainSchedular.MarkTriggered(affectedSpecial);
            }

            var affected = await Resolve(context, chainSchedular);
            ProcessAffected(context, chainSchedular, affected);

            await UniTask.Delay(TimeSpan.FromSeconds(context.Config.DestroyDuration));
        }

        private async UniTask<HashSet<Block>> Resolve(EffectExecutionContext context, IChainSchedular chainSchedular)
        {
            var affected = new HashSet<Block>();

            foreach (var affectedSpecial in affectedSpecials)
            {
                affected.Add(affectedSpecial);
            }

            switch (comboType)
            {
                case ComboType.DiscoBallDiscoBall: await DiscoBallDiscoBall(context, chainSchedular, affected); break;
                case ComboType.DiscoBallBomb: await DiscoBallBomb(context, affected); break;
                case ComboType.DiscoBallRocket: await DiscoBallRocket(context, affected); break;
                case ComboType.BombBomb: await BombBomb(context, affected); break;
                case ComboType.BombRocket: await BombRocket(context, affected); break;
                case ComboType.RocketRocket: await RocketRocket(context, affected); break;
                default: throw new ArgumentOutOfRangeException();
            }

            return affected;
        }

        //------------------- COMBO IMPLEMENTATIONS -----------------
        private async UniTask DiscoBallDiscoBall(EffectExecutionContext context, IChainSchedular chainSchedular,
            HashSet<Block> affected)
        {
            for (int row = 0; row < context.LevelProperties.RowCount; row++)
            {
                for (int col = 0; col < context.LevelProperties.ColumnCount; col++)
                {
                    var block = context.BlockGrid[row, col];
                    if (block != null)
                    {
                        affected.Add(block);

                        if (block is IActivatable)
                        {
                            chainSchedular.MarkTriggered(block);
                        }
                    }
                }
            }

            await UniTask.CompletedTask;
        }

        private async UniTask DiscoBallBomb(EffectExecutionContext context, HashSet<Block> affected)
        {
            var discoBall = Source.BlockType == BlockType.DiscoBall ? Source : partner;
            var bombBlock = Source.BlockType == BlockType.Bomb ? Source : partner;

            if (discoBall is not DiscoBlock discoBlock)
            {
                return;
            }

            await TransformByDisco(context, affected, bombBlock.BlockData, discoBlock);
        }

        private async UniTask DiscoBallRocket(EffectExecutionContext context, HashSet<Block> affected)
        {
            var discoBall = Source.BlockType == BlockType.DiscoBall ? Source : partner;
            var rocketBlock = Source.BlockType == BlockType.Rocket ? Source : partner;

            if (discoBall is not DiscoBlock discoBlock)
            {
                return;
            }

            await TransformByDisco(context, affected, rocketBlock.BlockData, discoBlock);
        }

        private async UniTask TransformByDisco(EffectExecutionContext context, HashSet<Block> affected,
            BlockData specialBlockData, DiscoBlock discoBlock)
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
                        context.SpawnBlockAt(specialBlockData, row, col);
                        await UniTask.Delay(TimeSpan.FromSeconds(context.Config.SpawnDurationBetweenSpecials));
                    }
                }
            }

            foreach (var block in affectedSpecials)
            {
                affected.Remove(block);
                var row = block.GridX;
                var col = block.GridY;

                context.RemoveBlock(block);
                context.SpawnBlockAt(specialBlockData, row, col);
                await UniTask.Delay(TimeSpan.FromSeconds(context.Config.SpawnDurationBetweenSpecials));
            }

            for (int col = context.LevelProperties.ColumnCount - 1; col >= 0; col--)
            {
                for (int row = 0; row < context.LevelProperties.RowCount; row++)
                {
                    var block = context.BlockGrid[row, col];
                    if (block.BlockData == specialBlockData)
                    {
                        affected.Add(block);
                    }
                }
            }
        }

        private async UniTask BombBomb(EffectExecutionContext context, HashSet<Block> affected)
        {
            var radius1 = ((BombBlockData)Source.BlockData).Radius;
            var radius2 = ((BombBlockData)partner.BlockData).Radius;
            var combined = radius1 + radius2;
            AddBlocksInRadius(context, affected, Source.GridX, Source.GridY, combined);
            AddBlocksInRadius(context, affected, partner.GridX, partner.GridY, combined);

            await UniTask.CompletedTask;
        }

        private async UniTask BombRocket(EffectExecutionContext context, HashSet<Block> affected)
        {
            var bombBlock = Source.BlockType == BlockType.Bomb ? Source : partner;
            var bombRadius = ((BombBlockData)bombBlock.BlockData).Radius;
            var centerRow = bombBlock.GridX;
            var centerCol = bombBlock.GridY;

            for (int row = centerRow - bombRadius; row <= centerRow + bombRadius; row++)
            {
                if (context.IsInBounds(row, centerCol))
                {
                    AddVerticalLine(context, affected, row);
                }
            }

            for (int col = centerCol - bombRadius; col <= centerCol + bombRadius; col++)
            {
                if (context.IsInBounds(centerRow, col))
                {
                    AddHorizontalLine(context, affected, col);
                }
            }

            await UniTask.CompletedTask;
        }

        private async UniTask RocketRocket(EffectExecutionContext context, HashSet<Block> affected)
        {
            AddHorizontalLine(context, affected, Source.GridY);
            AddVerticalLine(context, affected, Source.GridX);

            await UniTask.CompletedTask;
        }

        //------------------ HELPERS --------------------

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

        private void AddHorizontalLine(EffectExecutionContext context, HashSet<Block> affected, int col)
        {
            for (int row = 0; row < context.LevelProperties.RowCount; row++)
            {
                if (context.BlockGrid[row, col] != null)
                {
                    TryAddBlock(context, affected, row, col);
                }
            }
        }

        private void AddVerticalLine(EffectExecutionContext context, HashSet<Block> affected, int row)
        {
            for (int col = 0; col < context.LevelProperties.ColumnCount; col++)
            {
                if (context.BlockGrid[row, col] != null)
                {
                    TryAddBlock(context, affected, row, col);
                }
            }
        }

        private void TryAddBlock(EffectExecutionContext context, HashSet<Block> affected, int row, int col)
        {
            if (context.IsInBounds(row, col) && context.BlockGrid[row, col] != null)
            {
                affected.Add(context.BlockGrid[row, col]);
            }
        }

        private void ProcessAffected(EffectExecutionContext context, IChainSchedular chainSchedular,
            HashSet<Block> affectedBlocks)
        {
            foreach (var block in affectedBlocks)
            {
                if (block is IActivatable && !chainSchedular.IsTriggered(block))
                {
                    var chainedEffect = factory.CreateEffect(block);
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