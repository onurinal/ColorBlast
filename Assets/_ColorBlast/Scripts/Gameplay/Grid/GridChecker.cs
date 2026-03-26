using System;
using UnityEngine;
using System.Collections.Generic;
using ColorBlast.Core;

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

        private GameplayConfig gameplayConfig;
        private LevelProperties levelProperties;
        private Block[,] blockGrid;

        private bool[,] visitedBlocks;
        private Queue<Vector2Int> queue;
        private List<Block> currentGroup;

        public void Initialize(Block[,] blockGrid, LevelProperties levelProperties, GameplayConfig gameplayConfig)
        {
            this.blockGrid = blockGrid;
            this.levelProperties = levelProperties;
            this.gameplayConfig = gameplayConfig;

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

                    FindConnectedMatch(row, col, currentGroup);
                    UpdateGroupIcons(currentGroup);
                }
            }
        }

        public List<Block> GetGroupAt(int row, int col)
        {
            var block = blockGrid[row, col];
            if (block == null)
            {
                return null;
            }

            ClearVisitedBlocks();

            FindConnectedMatch(row, col, currentGroup);
            return currentGroup;
        }

        public bool IsDeadlocked()
        {
            ClearVisitedBlocks();

            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                for (int col = 0; col < levelProperties.ColumnCount; col++)
                {
                    var block = blockGrid[row, col];

                    if (block == null)
                    {
                        continue;
                    }

                    if (visitedBlocks[row, col])
                    {
                        continue;
                    }

                    // if it's special type then it's not a deadlock
                    if (block is IActivatable)
                    {
                        return false;
                    }

                    // it is checking basic block type who can match with other cubes
                    if (block is IMatchable)
                    {
                        FindConnectedMatch(row, col, currentGroup);

                        if (currentGroup.Count >= gameplayConfig.MatchThreshold)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private void FindConnectedMatch(int startRow, int startCol, List<Block> group)
        {
            group.Clear();

            var startBlock = blockGrid[startRow, startCol];

            if (startBlock is null || startBlock is not IMatchable startMatchableBlock)
            {
                return;
            }

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

                if (block is not IMatchable matchableBlock)
                {
                    continue;
                }

                if (!startMatchableBlock.CanMatchWith(matchableBlock))
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
                block.UpdateIcon(count);
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