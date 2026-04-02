using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    /// <summary>
    /// Detects if a tapped special block has an adjacent special neighbor
    /// and determines which combo type should be applied.
    /// Priority: DiscoBall > Bomb > Rocket
    /// </summary>
    public class ComboDetector
    {
        private static readonly Vector2Int[] NeighborOffsets =
        {
            new(1, 0), new(-1, 0), new(0, 1), new(0, -1)
        };

        private readonly Queue<Vector2Int> bfsQueue = new();
        private readonly HashSet<Vector2Int> visited = new();

        private Block[,] blockGrid;
        private LevelProperties levelProperties;

        public void Initialize(Block[,] blockGrid, LevelProperties levelProperties)
        {
            this.blockGrid = blockGrid;
            this.levelProperties = levelProperties;
        }

        public (Block partner, List<Block> adjacentSpecials, ComboType comboType)? TryDetect(Block tapped)
        {
            if (tapped is not IActivatable)
            {
                return null;
            }

            var adjacentSpecials = GetAdjacentSpecials(tapped);

            if (adjacentSpecials.Count == 0)
            {
                return null;
            }

            Block partner = SelectBestPartner(adjacentSpecials);
            var comboType = DetermineComboType(tapped.BlockType, partner.BlockType);

            return (partner, adjacentSpecials, comboType);
        }

        private List<Block> GetAdjacentSpecials(Block startBlock)
        {
            var specialBlocks = new List<Block>();
            visited.Clear();
            bfsQueue.Clear();

            bfsQueue.Enqueue(new Vector2Int(startBlock.GridX, startBlock.GridY));
            visited.Add(new Vector2Int(startBlock.GridX, startBlock.GridY));

            while (bfsQueue.Count > 0)
            {
                var currentBlock = bfsQueue.Dequeue();
                foreach (var offset in NeighborOffsets)
                {
                    var row = currentBlock.x + offset.x;
                    var col = currentBlock.y + offset.y;

                    if (!IsInBounds(row, col))
                    {
                        continue;
                    }

                    if (visited.Contains(new Vector2Int(row, col)))
                    {
                        continue;
                    }

                    visited.Add(new Vector2Int(row, col));

                    var neighbor = blockGrid[row, col];
                    if (neighbor is not IActivatable)
                    {
                        continue;
                    }

                    specialBlocks.Add(neighbor);
                    bfsQueue.Enqueue(new Vector2Int(row, col));
                }
            }

            return specialBlocks;
        }

        private Block SelectBestPartner(List<Block> specialBlocks)
        {
            var best = specialBlocks[0];
            var bestPriority = GetPriority(best.BlockType);

            foreach (var special in specialBlocks)
            {
                var priority = GetPriority(special.BlockType);
                if (priority < bestPriority)
                {
                    best = special;
                    bestPriority = priority;
                }
            }

            return best;
        }

        private ComboType DetermineComboType(BlockType a, BlockType b)
        {
            if (GetPriority(a) > GetPriority(b))
            {
                (b, a) = (a, b);
            }

            return (a, b) switch
            {
                (BlockType.DiscoBall, BlockType.DiscoBall) => ComboType.DiscoBallDiscoBall,
                (BlockType.DiscoBall, BlockType.Bomb) => ComboType.DiscoBallBomb,
                (BlockType.DiscoBall, BlockType.Rocket) => ComboType.DiscoBallRocket,
                (BlockType.Bomb, BlockType.Bomb) => ComboType.BombBomb,
                (BlockType.Bomb, BlockType.Rocket) => ComboType.BombRocket,
                (BlockType.Rocket, BlockType.Rocket) => ComboType.RocketRocket,
                _ => throw new ArgumentOutOfRangeException($"Unknown combo")
            };
        }

        private static int GetPriority(BlockType blockType)
        {
            return blockType switch
            {
                BlockType.DiscoBall => 0,
                BlockType.Bomb => 1,
                BlockType.Rocket => 2,
                _ => int.MaxValue
            };
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