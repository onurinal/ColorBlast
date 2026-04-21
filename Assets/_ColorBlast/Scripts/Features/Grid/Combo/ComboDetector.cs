using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlast.Features
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

        private Block[,] grid;
        private LevelProperties levelProperties;

        public void Initialize(Block[,] grid, LevelProperties levelProperties)
        {
            this.grid = grid;
            this.levelProperties = levelProperties;
        }

        public bool TryDetect(Block tapped, out ComboResult result)
        {
            result = default;

            if (tapped is not IActivatable)
            {
                return false;
            }

            var affectedSpecials = GetAffectedSpecials(tapped);

            if (affectedSpecials == null || affectedSpecials.Count < 2)
            {
                return false;
            }

            var (best, partner) = SelectBestCombo(affectedSpecials);
            var comboType = DetermineComboType(best.BlockType, partner.BlockType);

            result = new ComboResult(tapped, best, partner, affectedSpecials, comboType);
            return true;
        }

        private HashSet<Block> GetAffectedSpecials(Block startBlock)
        {
            var specialBlocks = new HashSet<Block> { startBlock };
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

                    var neighbor = grid[row, col];
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

        /// <summary>
        /// Returns the two highest-priority specials from the connected group.
        /// Only the top two participate in the combo; extras are removed via RemoveComboSpecials.
        /// Priority order: DiscoBall > Bomb > Rocket.
        /// </summary>
        private (Block, Block) SelectBestCombo(HashSet<Block> specialBlocks)
        {
            Block first = null;
            Block second = null;
            var firstPriority = int.MaxValue;
            var secondPriority = int.MaxValue;

            foreach (var block in specialBlocks)
            {
                var priority = GetPriority(block.BlockType);

                if (priority < firstPriority)
                {
                    second = first;
                    secondPriority = firstPriority;

                    first = block;
                    firstPriority = priority;
                }
                else if (priority < secondPriority)
                {
                    second = block;
                    secondPriority = priority;
                }
            }

            return (first, second);
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