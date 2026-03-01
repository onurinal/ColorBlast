using System;
using UnityEngine;
using System.Collections.Generic;

namespace ColorBlast.Gameplay
{
    /// <summary>
    /// Handles group detection, icon updates and deadlock detection on the game board
    /// Uses flood-fill (BFS) to find connected groups of same colored blocks
    /// </summary>
    public class GridChecker
    {
        private static readonly Vector2Int[] Neighbors =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        private MatchRulesConfig matchRulesConfig;
        private LevelProperties levelProperties;
        private Block[,] blockGrid;

        private bool[,] visitedBlocks;
        private Queue<Vector2Int> queue;
        private List<Block> currentGroup;

        public void Initialize(Block[,] blockGrid, LevelProperties levelProperties, MatchRulesConfig matchRulesConfig)
        {
            this.blockGrid = blockGrid;
            this.levelProperties = levelProperties;
            this.matchRulesConfig = matchRulesConfig;

            var capacity = levelProperties.RowCount * levelProperties.ColumnCount;
            visitedBlocks = new bool[levelProperties.RowCount, levelProperties.ColumnCount];
            queue = new Queue<Vector2Int>(capacity / 2);
            currentGroup = new List<Block>(capacity / 2);
        }

        public void CheckAllGrid()
        {
            ClearVisitedBlocks();
            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                for (int col = 0; col < levelProperties.ColumnCount; col++)
                {
                    if (visitedBlocks[row, col])
                    {
                        continue;
                    }

                    if (blockGrid[row, col] == null)
                    {
                        continue;
                    }

                    FindConnectedMatch(row, col, blockGrid[row, col].ColorData, currentGroup);
                    UpdateGroupIcons(currentGroup);
                }
            }
        }

        public List<Block> GetGroupAt(int row, int col, List<Block> result)
        {
            var block = blockGrid[row, col];
            if (block == null)
            {
                return null;
            }

            ClearVisitedBlocks();

            FindConnectedMatch(row, col, block.ColorData, result);
            return result;
        }

        public bool IsDeadlocked()
        {
            ClearVisitedBlocks();

            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                for (int col = 0; col < levelProperties.ColumnCount; col++)
                {
                    if (visitedBlocks[row, col])
                    {
                        continue;
                    }

                    if (blockGrid[row, col] == null)
                    {
                        continue;
                    }

                    FindConnectedMatch(row, col, blockGrid[row, col].ColorData, currentGroup);

                    if (currentGroup.Count >= matchRulesConfig.MatchThreshold)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void FindConnectedMatch(int startRow, int startCol, BlockColorData colorData, List<Block> group)
        {
            group.Clear();
            queue.Clear();
            queue.Enqueue(new Vector2Int(startRow, startCol));

            while (queue.Count > 0)
            {
                var currentBlock = queue.Dequeue();
                var row = currentBlock.x;
                var col = currentBlock.y;

                if (!IsInsideGrid(row, col))
                {
                    continue;
                }

                if (visitedBlocks[row, col])
                {
                    continue;
                }

                var block = blockGrid[row, col];
                if (block == null)
                {
                    continue;
                }

                if (block.ColorData != colorData)
                {
                    continue;
                }

                visitedBlocks[row, col] = true;
                group.Add(block);

                for (int i = 0; i < Neighbors.Length; i++)
                {
                    queue.Enqueue(currentBlock + Neighbors[i]);
                }
            }
        }

        private void UpdateGroupIcons(List<Block> group)
        {
            var count = group.Count;
            foreach (var block in group)
            {
                if (block.CurrentGroupSize != count)
                {
                    block.UpdateGroupSize(count);
                }
            }
        }

        private bool IsInsideGrid(int row, int col)
        {
            if (row < 0 || col < 0 || row >= levelProperties.RowCount || col >= levelProperties.ColumnCount)
            {
                return false;
            }

            return true;
        }

        private void ClearVisitedBlocks()
        {
            Array.Clear(visitedBlocks, 0, visitedBlocks.Length);
        }
    }
}