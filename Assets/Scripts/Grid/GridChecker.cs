using System;
using System.Collections.Generic;
using ColorBlast.Blocks;
using ColorBlast.Level;
using UnityEngine;

namespace ColorBlast.Grid
{
    /// <summary>
    /// Handles group detection, icon updates and deadlock detection on the game board
    /// Uses flood-fill algorithm to find connected groups of same colored blocks
    /// </summary>
    public class GridChecker
    {
        private LevelProperties levelProperties;
        private Block[,] blockGrid;

        private bool[,] visitedBlocks;
        private Queue<Vector2Int> matchQueue;
        private List<Block> currentGroup;
        private HashSet<Block> affectedBlocks;


        private static readonly Vector2Int[] NEIGHBORS = new Vector2Int[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        public void Initialize(Block[,] blockGrid, LevelProperties levelProperties)
        {
            this.blockGrid = blockGrid;
            this.levelProperties = levelProperties;

            InitializeLists();
        }

        private void InitializeLists()
        {
            var capacity = levelProperties.RowCount * levelProperties.ColumnCount / 2;
            visitedBlocks = new bool[levelProperties.RowCount, levelProperties.ColumnCount];
            matchQueue = new Queue<Vector2Int>(capacity);
            currentGroup = new List<Block>(capacity);
            affectedBlocks = new HashSet<Block>(capacity);
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

                    currentGroup.Clear();
                    var blockColor = blockGrid[row, col].ColorType;
                    FindConnectedMatch(row, col, blockColor, currentGroup);
                    UpdateGroupIcons(currentGroup);
                }
            }
        }

        private void FindConnectedMatch(int startRow, int startCol, BlockColorType color, List<Block> targetList)
        {
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

                if (block.ColorType != color)
                {
                    continue;
                }

                visitedBlocks[row, col] = true;
                targetList.Add(block);

                for (int i = 0; i < NEIGHBORS.Length; i++)
                {
                    matchQueue.Enqueue(currentBlock + NEIGHBORS[i]);
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

                currentGroup.Clear();
                FindConnectedMatch(block.GridX, block.GridY, block.ColorType, currentGroup);
                UpdateGroupIcons(currentGroup);
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
            for (int i = 0; i < NEIGHBORS.Length; i++)
            {
                var neighborRow = row + NEIGHBORS[i].x;
                var neighborCol = col + NEIGHBORS[i].y;

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
            var newIcon = DetermineBlockIconType(group.Count);

            for (int i = 0; i < group.Count; i++)
            {
                if (newIcon != group[i].IconType)
                {
                    group[i].UpdateIcon(newIcon);
                }
            }
        }

        private BlockIconType DetermineBlockIconType(int groupSize)
        {
            if (groupSize > levelProperties.ThirdIconThreshold)
            {
                return BlockIconType.ThirdIcon;
            }

            if (groupSize > levelProperties.SecondIconThreshold)
            {
                return BlockIconType.SecondIcon;
            }

            if (groupSize > levelProperties.FirstIconThreshold)
            {
                return BlockIconType.FirstIcon;
            }

            return BlockIconType.Default;
        }

        public void GetGroup(int row, int col, List<Block> resultList)
        {
            var block = blockGrid[row, col];
            if (block == null)
            {
                return;
            }

            ClearVisitedBlocks();

            var color = block.ColorType;
            FindConnectedMatch(row, col, color, resultList);
        }

        private bool IsInsideGrid(int row, int col)
        {
            if (row < 0 || col < 0 || row >= levelProperties.RowCount || col >= levelProperties.ColumnCount)
            {
                return false;
            }

            return true;
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

                    currentGroup.Clear();
                    FindConnectedMatch(row, col, blockGrid[row, col].ColorType, currentGroup);

                    if (currentGroup.Count >= GameRule.MatchThreshold)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void ClearVisitedBlocks()
        {
            Array.Clear(visitedBlocks, 0, visitedBlocks.Length);
        }
    }
}