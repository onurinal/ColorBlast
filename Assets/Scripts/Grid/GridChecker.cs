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


        private static readonly Vector2Int[] Neighbors = new Vector2Int[]
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
                    if (visitedBlocks[row, col] || blockGrid[row, col] == null)
                    {
                        continue;
                    }

                    currentGroup.Clear();
                    var blockColor = blockGrid[row, col].ColorType;
                    FindConnectedMatch(row, col, blockColor);
                    UpdateGroupIcons(currentGroup);
                }
            }
        }

        private void FindConnectedMatch(int startRow, int startCol, BlockColorType color)
        {
            matchQueue.Clear();
            matchQueue.Enqueue(new Vector2Int(startRow, startCol));

            while (matchQueue.Count > 0)
            {
                var currentBlock = matchQueue.Dequeue();
                var row = currentBlock.x;
                var col = currentBlock.y;

                if (!IsInsideGrid(row, col)) continue;
                if (visitedBlocks[row, col]) continue;

                var block = blockGrid[row, col];
                if (block == null) continue;
                if (block.ColorType != color) continue;

                visitedBlocks[row, col] = true;
                currentGroup.Add(block);

                for (int i = 0; i < Neighbors.Length; i++)
                {
                    var next = currentBlock + Neighbors[i];
                    if (!IsInsideGrid(next.x, next.y))
                    {
                        continue;
                    }

                    if (visitedBlocks[next.x, next.y])
                    {
                        continue;
                    }

                    matchQueue.Enqueue(next);
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
                if (block == null || visitedBlocks[block.GridX, block.GridY])
                {
                    continue;
                }

                currentGroup.Clear();
                FindConnectedMatch(block.GridX, block.GridY, block.ColorType);
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
                if (block != null)
                {
                    affectedBlocks.Add(block);
                    AddNeighborsToAffectedGroup(block.GridX, block.GridY);
                }
            }
        }

        private void AddNeighborsToAffectedGroup(int row, int col)
        {
            for (int i = 0; i < Neighbors.Length; i++)
            {
                var neighborRow = row + Neighbors[i].x;
                var neighborCol = col + Neighbors[i].y;

                if (IsInsideGrid(neighborRow, neighborCol))
                {
                    var block = blockGrid[neighborRow, neighborCol];
                    if (block != null)
                    {
                        affectedBlocks.Add(block);
                    }
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

        public List<Block> GetGroup(int row, int col)
        {
            currentGroup.Clear();

            var block = blockGrid[row, col];
            if (block == null)
            {
                return currentGroup;
            }

            ClearVisitedBlocks();

            var color = block.ColorType;
            FindConnectedMatch(row, col, color);

            return currentGroup;
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
                    if (visitedBlocks[row, col] || blockGrid[row, col] == null)
                        continue;

                    currentGroup.Clear();
                    FindConnectedMatch(row, col, blockGrid[row, col].ColorType);

                    if (currentGroup.Count >= LevelRule.MatchThreshold)
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