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
        private readonly List<Block> adjacentSpecials;
        private readonly ComboType comboType;
        private readonly BlockEffectFactory factory;

        public ComboEffect(Block source, Block partner, List<Block> adjacentSpecials, ComboType type,
            BlockEffectFactory factory)
        {
            Source = source;
            this.partner = partner;
            this.adjacentSpecials = adjacentSpecials;
            comboType = type;
            this.factory = factory;
        }

        public async UniTask Execute(EffectExecutionContext context, IChainSchedular chainSchedular)
        {
            foreach (var adjacent in adjacentSpecials)
            {
                chainSchedular.MarkTriggered(adjacent);
            }

            var affected = Resolve(context);
            ProcessAffected(context, chainSchedular, affected);

            await UniTask.Delay(TimeSpan.FromSeconds(context.Config.DestroyDuration));
        }

        private HashSet<Block> Resolve(EffectExecutionContext context)
        {
            var affected = new HashSet<Block> { Source };

            foreach (var adjacent in adjacentSpecials)
            {
                affected.Add(adjacent);
            }

            switch (comboType)
            {
                case ComboType.DiscoBallDiscoBall: DiscoBallDiscoBall(context, affected); break;
                case ComboType.DiscoBallBomb: DiscoBallBomb(context, affected); break;
                case ComboType.DiscoBallRocket: DiscoBallRocket(context, affected); break;
                case ComboType.BombBomb: BombBomb(context, affected); break;
                case ComboType.BombRocket: BombRocket(context, affected); break;
                case ComboType.RocketRocket: RocketRocket(context, affected); break;
            }

            return affected;
        }

        //------------------- COMBO IMPLEMENTATIONS -----------------
        private void DiscoBallDiscoBall(EffectExecutionContext context, HashSet<Block> affected)
        {
            for (int row = 0; row < context.LevelProperties.RowCount; row++)
            {
                for (int col = 0; col < context.LevelProperties.ColumnCount; col++)
                {
                    if (context.BlockGrid[row, col] != null)
                    {
                        affected.Add(context.BlockGrid[row, col]);
                    }
                }
            }
        }

        private void DiscoBallBomb(EffectExecutionContext context, HashSet<Block> affected)
        {
            throw new NotImplementedException();
        }

        private void DiscoBallRocket(EffectExecutionContext context, HashSet<Block> affected)
        {
            throw new NotImplementedException();
        }

        private void BombBomb(EffectExecutionContext context, HashSet<Block> affected)
        {
            var radius1 = ((BombBlockData)Source.BlockData).Radius;
            var radius2 = ((BombBlockData)partner.BlockData).Radius;
            var combined = radius1 + radius2;
            AddBlocksInRadius(context, affected, Source.GridX, Source.GridY, combined);
            AddBlocksInRadius(context, affected, partner.GridX, partner.GridY, combined);
        }

        private void BombRocket(EffectExecutionContext context, HashSet<Block> affected)
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
        }

        private void RocketRocket(EffectExecutionContext context, HashSet<Block> affected)
        {
            AddHorizontalLine(context, affected, Source.GridY);
            AddVerticalLine(context, affected, Source.GridX);
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
                    affected.Add(context.BlockGrid[row, col]);
                }
            }
        }

        private void AddVerticalLine(EffectExecutionContext context, HashSet<Block> affected, int row)
        {
            for (int col = 0; col < context.LevelProperties.ColumnCount; col++)
            {
                if (context.BlockGrid[row, col] != null)
                {
                    affected.Add(context.BlockGrid[row, col]);
                }
            }
        }

        private void ProcessAffected(EffectExecutionContext context, IChainSchedular chainSchedular,
            HashSet<Block> affectedBlocks)
        {
            foreach (var block in affectedBlocks)
            {
                if (block is IActivatable && !chainSchedular.IsTriggered(block))
                {
                    context.DestroyBlock(block);
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