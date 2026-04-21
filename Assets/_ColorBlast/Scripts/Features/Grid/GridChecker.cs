using System;
using UnityEngine;
using System.Collections.Generic;

namespace ColorBlast.Features
{
    /// <summary>
    /// Handles group detection, icon updates and deadlock detection on the game board
    /// Uses flood-fill (BFS) to find connected groups of same colored blocks
    /// </summary>
    public class GridChecker
    {
        private static readonly Vector2Int[] Neighbors =
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        private GameConfig gameplayConfig;
        private LevelProperties levelProperties;
        private Block[,] grid;

        private bool[,] visitedBlocks;
        private HashSet<Block> currentGroup;
        private Queue<Vector2Int> bfsQueue;

        public void Initialize(Block[,] grid, LevelProperties levelProperties, GameConfig gameplayConfig)
        {
            this.grid = grid;
            this.levelProperties = levelProperties;
            this.gameplayConfig = gameplayConfig;

            var gridCapacity = levelProperties.RowCount * levelProperties.ColumnCount;
            visitedBlocks = new bool[levelProperties.RowCount, levelProperties.ColumnCount];
            currentGroup = new HashSet<Block>(gridCapacity / 2);
            bfsQueue = new Queue<Vector2Int>(gridCapacity / 2);
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

                    if (grid[row, col] == null)
                    {
                        continue;
                    }

                    FindConnectedMatch(row, col);
                    UpdateGroupIcons(currentGroup);
                }
            }
        }

        public HashSet<Block> GetGroupAt(int row, int col)
        {
            var block = grid[row, col];
            if (block == null)
            {
                return null;
            }

            ClearVisitedBlocks();
            FindConnectedMatch(row, col);
            return currentGroup;
        }

        public bool IsDeadlocked()
        {
            ClearVisitedBlocks();

            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                for (int col = 0; col < levelProperties.ColumnCount; col++)
                {
                    var block = grid[row, col];

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
                        FindConnectedMatch(row, col);

                        if (currentGroup.Count >= gameplayConfig.MatchThreshold)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private void FindConnectedMatch(int startRow, int startCol)
        {
            currentGroup.Clear();

            var startBlock = grid[startRow, startCol];

            if (startBlock is null || startBlock is not IMatchable startMatchableBlock)
            {
                return;
            }

            bfsQueue.Clear();
            bfsQueue.Enqueue(new Vector2Int(startRow, startCol));

            while (bfsQueue.Count > 0)
            {
                var currentBlock = bfsQueue.Dequeue();
                var row = currentBlock.x;
                var col = currentBlock.y;

                if (!IsInBounds(row, col))
                {
                    continue;
                }

                if (visitedBlocks[row, col])
                {
                    continue;
                }

                var block = grid[row, col];
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
                currentGroup.Add(block);

                foreach (var neighbor in Neighbors)
                {
                    bfsQueue.Enqueue(currentBlock + neighbor);
                }
            }
        }

        private void UpdateGroupIcons(HashSet<Block> group)
        {
            var count = group.Count;
            foreach (var block in group)
            {
                block.UpdateIcon(count);
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

        private void ClearVisitedBlocks()
        {
            Array.Clear(visitedBlocks, 0, visitedBlocks.Length);
        }
    }
}