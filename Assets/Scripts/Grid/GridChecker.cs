using System;
using System.Collections.Generic;
using ColorBlast.Blocks;
using ColorBlast.Level;
using UnityEngine;

namespace ColorBlast.Grid
{
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
            visitedBlocks = new bool[levelProperties.RowCount, levelProperties.ColumnCount];
            matchQueue = new Queue<Vector2Int>((levelProperties.RowCount * levelProperties.ColumnCount) / 2);
            currentGroup = new List<Block>((levelProperties.RowCount * levelProperties.ColumnCount) / 2);
            affectedBlocks = new HashSet<Block>((levelProperties.RowCount * levelProperties.ColumnCount) / 2);
        }

        public void CheckAllGrid()
        {
            Array.Clear(visitedBlocks, 0, visitedBlocks.Length);

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
                    TryMatch(new Vector2Int(row, col), blockColor);
                    UpdateGroupIcons(currentGroup);
                }
            }
        }

        private void TryMatch(Vector2Int startBlock, BlockColorType color)
        {
            matchQueue.Clear();
            matchQueue.Enqueue(startBlock);

            while (matchQueue.Count > 0)
            {
                var currentBlock = matchQueue.Dequeue();

                if (!IsInsideGrid(currentBlock)) continue;
                if (visitedBlocks[currentBlock.x, currentBlock.y]) continue;
                if (blockGrid[currentBlock.x, currentBlock.y] == null) continue;
                if (color != blockGrid[currentBlock.x, currentBlock.y].ColorType) continue;

                visitedBlocks[currentBlock.x, currentBlock.y] = true;
                currentGroup.Add(blockGrid[currentBlock.x, currentBlock.y]);

                for (int i = 0; i < Neighbors.Length; i++)
                {
                    matchQueue.Enqueue(currentBlock + Neighbors[i]);
                }
            }
        }

        public void CheckAffectedBlocks(List<Block> destroyedBlocks, List<Block> newSpawnedBlocks, List<Block> movedBlocks)
        {
            affectedBlocks.Clear();

            AddDestroyedBlocksToAffected(destroyedBlocks);

            AddMovedBlocksToAffected(movedBlocks);

            AddNewBlocksToAffected(newSpawnedBlocks);

            Array.Clear(visitedBlocks, 0, visitedBlocks.Length);

            foreach (var block in affectedBlocks)
            {
                if (block == null || visitedBlocks[block.GridX, block.GridY])
                {
                    continue;
                }

                currentGroup.Clear();
                TryMatch(new Vector2Int(block.GridX, block.GridY), block.ColorType);
                UpdateGroupIcons(currentGroup);
            }
        }

        private void AddNewBlocksToAffected(List<Block> newSpawnedBlocks)
        {
            for (int i = 0; i < newSpawnedBlocks.Count; i++)
            {
                if (newSpawnedBlocks[i] != null)
                {
                    affectedBlocks.Add(newSpawnedBlocks[i]);
                }
            }
        }

        private void AddDestroyedBlocksToAffected(List<Block> destroyedBlocks)
        {
            for (int i = 0; i < destroyedBlocks.Count; i++)
            {
                if (destroyedBlocks[i] != null)
                {
                    AddNeighborsToAffectedGroup(destroyedBlocks[i].GridX, destroyedBlocks[i].GridY);
                }
            }
        }

        private void AddMovedBlocksToAffected(List<Block> movedBlocks)
        {
            for (int i = 0; i < movedBlocks.Count; i++)
            {
                if (movedBlocks[i] != null)
                {
                    affectedBlocks.Add(movedBlocks[i]);
                    AddNeighborsToAffectedGroup(movedBlocks[i].PrevGridX, movedBlocks[i].PrevGridY);
                }
            }
        }

        private void AddNeighborsToAffectedGroup(int row, int col)
        {
            TryAddAffectedBlock(row + 1, col);
            TryAddAffectedBlock(row, col + 1);
            TryAddAffectedBlock(row, col - 1);
            TryAddAffectedBlock(row - 1, col);
        }

        private void TryAddAffectedBlock(int row, int col)
        {
            if (row < 0 || row >= levelProperties.RowCount || col < 0 || col >= levelProperties.ColumnCount)
            {
                return;
            }

            var block = blockGrid[row, col];
            if (block != null)
            {
                affectedBlocks.Add(block);
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
            else if (groupSize > levelProperties.SecondIconThreshold)
            {
                return BlockIconType.SecondIcon;
            }
            else if (groupSize > levelProperties.FirstIconThreshold)
            {
                return BlockIconType.FirstIcon;
            }
            else
            {
                return BlockIconType.Default;
            }
        }

        public List<Block> GetGroup(int row, int col)
        {
            var block = blockGrid[row, col];

            if (block == null)
            {
                currentGroup.Clear();
                return currentGroup;
            }

            Array.Clear(visitedBlocks, 0, visitedBlocks.Length);
            currentGroup.Clear();

            var color = block.ColorType;
            TryMatch(new Vector2Int(row, col), color);

            return currentGroup;
        }

        private bool IsInsideGrid(Vector2Int currentBlock)
        {
            if (currentBlock.x < 0 || currentBlock.y < 0 || currentBlock.x >= levelProperties.RowCount || currentBlock.y >= levelProperties.ColumnCount)
            {
                return false;
            }

            return true;
        }

        public bool IsDeadlocked()
        {
            Array.Clear(visitedBlocks, 0, visitedBlocks.Length);

            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                for (int col = 0; col < levelProperties.ColumnCount; col++)
                {
                    if (visitedBlocks[row, col] || blockGrid[row, col] == null)
                        continue;

                    currentGroup.Clear();
                    TryMatch(new Vector2Int(row, col), blockGrid[row, col].ColorType);

                    if (currentGroup.Count >= LevelRule.MatchThreshold)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}