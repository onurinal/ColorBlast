using System;
using System.Collections.Generic;

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
    public class ComboEffectResolver
    {
        private Block[,] blockGrid;
        private LevelProperties levelProperties;

        public void Initialize(Block[,] blockGrid, LevelProperties levelProperties)
        {
            this.blockGrid = blockGrid;
            this.levelProperties = levelProperties;
        }

        public HashSet<Block> Resolve(Block tapped, Block partner, ComboType comboType)
        {
            var affectedBlocks = new HashSet<Block> { tapped, partner };

            switch (comboType)
            {
                case ComboType.DiscoBallDiscoBall:
                    ResolveDiscoBallDiscoBall(affectedBlocks);
                    break;
                case ComboType.DiscoBallBomb:
                    ResolveDiscoBallBomb(tapped, partner, affectedBlocks);
                    break;
                case ComboType.DiscoBallRocket:
                    ResolveDiscoBallRocket(tapped, partner, affectedBlocks);
                    break;
                case ComboType.BombBomb:
                    ResolveBombBomb(tapped, partner, affectedBlocks);
                    break;
                case ComboType.BombRocket:
                    ResolveBombRocket(tapped, partner, affectedBlocks);
                    break;
                case ComboType.RocketRocket:
                    ResolveRocketRocket(tapped, affectedBlocks);
                    break;
            }

            return affectedBlocks;
        }

        //------------------- COMBO IMPLEMENTATIONS -----------------
        private void ResolveDiscoBallDiscoBall(HashSet<Block> affectedBlocks)
        {
            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                for (int col = 0; col < levelProperties.ColumnCount; col++)
                {
                    if (blockGrid[row, col] != null)
                    {
                        affectedBlocks.Add(blockGrid[row, col]);
                    }
                }
            }
        }

        private void ResolveDiscoBallBomb(Block tapped, Block partner, HashSet<Block> affectedBlocks)
        {
            throw new NotImplementedException();
        }

        private void ResolveDiscoBallRocket(Block tapped, Block partner, HashSet<Block> affectedBlocks)
        {
            throw new NotImplementedException();
        }

        private void ResolveBombBomb(Block tapped, Block partner, HashSet<Block> affectedBlocks)
        {
            var radius1 = ((BombBlockData)tapped.BlockData).Radius;
            var radius2 = ((BombBlockData)partner.BlockData).Radius;
            var combinedRadius = radius1 + radius2;

            AddBlocksInRadius(tapped.GridX, tapped.GridY, combinedRadius, affectedBlocks);
            AddBlocksInRadius(partner.GridX, partner.GridY, combinedRadius, affectedBlocks);
        }

        private void ResolveBombRocket(Block tapped, Block partner, HashSet<Block> affectedBlocks)
        {
            var bombBlock = tapped.BlockType == BlockType.Bomb ? tapped : partner;
            var bombRadius = ((BombBlockData)bombBlock.BlockData).Radius;
            var centerRow = bombBlock.GridX;
            var centerCol = bombBlock.GridY;

            for (int row = centerRow - bombRadius; row <= centerRow + bombRadius; row++)
            {
                if (IsInBounds(row, centerCol))
                {
                    AddBlocksVerticalLine(row, affectedBlocks);
                }
            }

            for (int col = centerCol - bombRadius; col <= centerCol + bombRadius; col++)
            {
                if (IsInBounds(centerRow, col))
                {
                    AddBlocksHorizontalLine(col, affectedBlocks);
                }
            }
        }

        private void ResolveRocketRocket(Block tapped, HashSet<Block> affectedBlocks)
        {
            AddBlocksHorizontalLine(tapped.GridY, affectedBlocks);
            AddBlocksVerticalLine(tapped.GridX, affectedBlocks);
        }

        //------------------ HELPERS --------------------

        private void AddBlocksInRadius(int centerRow, int centerCol, int radius, HashSet<Block> affectedBlocks)
        {
            for (int row = centerRow - radius; row <= centerRow + radius; row++)
            {
                for (int col = centerCol - radius; col <= centerCol + radius; col++)
                {
                    if (IsInBounds(row, col) && blockGrid[row, col] != null)
                    {
                        affectedBlocks.Add(blockGrid[row, col]);
                    }
                }
            }
        }

        private void AddBlocksHorizontalLine(int col, HashSet<Block> affectedBlocks)
        {
            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                if (blockGrid[row, col] != null)
                {
                    affectedBlocks.Add(blockGrid[row, col]);
                }
            }
        }

        private void AddBlocksVerticalLine(int row, HashSet<Block> affectedBlocks)
        {
            for (int col = 0; col < levelProperties.ColumnCount; col++)
            {
                if (blockGrid[row, col] != null)
                {
                    affectedBlocks.Add(blockGrid[row, col]);
                }
            }
        }

        private bool IsInBounds(int row, int col)
        {
            if (row < 0 || col < 0 || row >= levelProperties.RowCount || col >= levelProperties.ColumnCount)
            {
                return false;
            }

            return true;
        }
    }
}