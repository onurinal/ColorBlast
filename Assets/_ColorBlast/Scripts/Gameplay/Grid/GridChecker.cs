using System;
using UnityEngine;
using System.Collections.Generic;
using ColorBlast.Config;

namespace ColorBlast.Gameplay
{
    /// <summary>
    /// Handles group detection, icon updates and deadlock detection on the game board
    /// Uses flood-fill algorithm to find connected groups of same colored blocks
    /// </summary>
    public class GridChecker
    {
        private static readonly Vector2Int[] Neighbors = new Vector2Int[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };
        private LevelProperties levelProperties;
        private Block[,] blockGrid;

        private bool[,] visitedBlocks;
        private Queue<Vector2Int> matchQueue;
        private List<Block> currentGroup;
        private HashSet<Block> affectedBlocks;

        public void Initialize(Block[,] blockGrid, LevelProperties levelProperties)
        {
            this.blockGrid = blockGrid;
            this.levelProperties = levelProperties;

            InitializeLists();
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

                    var blockColorData = blockGrid[row, col].ColorData;
                    FindConnectedMatch(row, col, blockColorData, currentGroup);
                    UpdateGroupIcons(currentGroup);
                }
            }
        }

        public void CheckAffectedBlocks(List<Block> newSpawnedBlocks, List<Block> movedBlocks)
        {
            affectedBlocks.Clear();

            AddBlocksToAffected(movedBlocks);
            AddBlocksToAffected(newSpawnedBlocks);

            ClearVisitedBlocks();

            foreach (var block in affectedBlocks)
            {
                if (block == null)
                {
                    continue;
                }

                if (visitedBlocks[block.GridX, block.GridY])
                {
                    continue;
                }

                FindConnectedMatch(block.GridX, block.GridY, block.ColorData, currentGroup);
                UpdateGroupIcons(currentGroup);
            }
        }

        public void GetGroup(int row, int col, List<Block> resultList)
        {
            var block = blockGrid[row, col];
            if (block == null)
            {
                return;
            }

            ClearVisitedBlocks();

            var blockColorData = block.ColorData;
            FindConnectedMatch(row, col, blockColorData, resultList);
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

                    if (currentGroup.Count >= GameConstRules.MatchThreshold)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void InitializeLists()
        {
            var capacity = levelProperties.RowCount * levelProperties.ColumnCount / 2;
            visitedBlocks = new bool[levelProperties.RowCount, levelProperties.ColumnCount];
            matchQueue = new Queue<Vector2Int>(capacity);
            currentGroup = new List<Block>(capacity);
            affectedBlocks = new HashSet<Block>(capacity);
        }

        private void FindConnectedMatch(int startRow, int startCol, BlockColorData blockColorData,
            List<Block> targetList)
        {
            targetList.Clear();
            matchQueue.Clear();
            matchQueue.Enqueue(new Vector2Int(startRow, startCol));

            while (matchQueue.Count > 0)
            {
                var currentBlock = matchQueue.Dequeue();
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

                if (block.ColorData != blockColorData)
                {
                    continue;
                }

                visitedBlocks[row, col] = true;
                targetList.Add(block);

                for (int i = 0; i < Neighbors.Length; i++)
                {
                    matchQueue.Enqueue(currentBlock + Neighbors[i]);
                }
            }
        }

        private void AddBlocksToAffected(List<Block> blocks)
        {
            if (blocks == null || blocks.Count == 0)
            {
                return;
            }

            for (int i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                if (block == null)
                {
                    continue;
                }

                affectedBlocks.Add(block);
                AddNeighborsToAffectedGroup(block.GridX, block.GridY);
            }
        }

        private void AddNeighborsToAffectedGroup(int row, int col)
        {
            for (int i = 0; i < Neighbors.Length; i++)
            {
                var neighborRow = row + Neighbors[i].x;
                var neighborCol = col + Neighbors[i].y;

                if (!IsInsideGrid(neighborRow, neighborCol))
                {
                    continue;
                }

                var block = blockGrid[neighborRow, neighborCol];
                if (block != null)
                {
                    affectedBlocks.Add(block);
                }
            }
        }

        private void UpdateGroupIcons(List<Block> group)
        {
            for (int i = 0; i < group.Count; i++)
            {
                if (group.Count != group[i].CurrentGroupSize)
                {
                    group[i].UpdateGroupSize(group.Count);
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