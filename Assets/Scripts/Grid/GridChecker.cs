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
        private HashSet<Block> affectedBlocks; // blocks that need update visual


        private static readonly Vector2Int[] Neighbors = new Vector2Int[]
        {
            new Vector2Int(1, 0), // up
            new Vector2Int(-1, 0), // down
            new Vector2Int(0, 1), // right
            new Vector2Int(0, -1), // left
        };

        public void Initialize(Block[,] blockGrid, LevelProperties levelProperties)
        {
            this.blockGrid = blockGrid;
            this.levelProperties = levelProperties;

            InitializeLists();
            CheckAllGrid();
        }

        private void InitializeLists()
        {
            visitedBlocks = new bool[levelProperties.RowCount, levelProperties.ColumnCount];
            matchQueue = new Queue<Vector2Int>();
            currentGroup = new List<Block>();
            affectedBlocks = new HashSet<Block>();
        }

        /// <summary>
        /// Full grid scan - only using at initialization
        /// </summary>
        private void CheckAllGrid()
        {
            Array.Clear(visitedBlocks, 0, visitedBlocks.Length); // clear visited array

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

                foreach (var offset in Neighbors)
                {
                    matchQueue.Enqueue(currentBlock + offset);
                }
            }
        }

        /// <summary>
        /// It  checks neighbor block which destroyed and new spawned blocks
        /// </summary>
        /// <param name="destroyedBlocks">Blocks that were destroyed</param>
        /// <param name="movedBlocks">Blocks that moved during gravity</param>
        /// <param name="newSpawnedBlocks">New blocks that spawned from top</param>
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
            foreach (var newBlock in newSpawnedBlocks)
            {
                if (newBlock != null)
                {
                    affectedBlocks.Add(newBlock);
                }
            }
        }

        private void AddDestroyedBlocksToAffected(List<Block> destroyedBlocks)
        {
            foreach (var destroyedBlock in destroyedBlocks)
            {
                if (destroyedBlock != null)
                {
                    AddNeighborsToAffectedGroup(destroyedBlock.GridX, destroyedBlock.GridY);
                }
            }
        }

        private void AddMovedBlocksToAffected(List<Block> movedBlocks)
        {
            foreach (var movedBlock in movedBlocks)
            {
                if (movedBlock != null)
                {
                    // add moved blocks and their old position's neighbors in affected blocks
                    affectedBlocks.Add(movedBlock);
                    AddNeighborsToAffectedGroup(movedBlock.PrevGridX, movedBlock.PrevGridY);
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

            foreach (var block in group)
            {
                if (newIcon != block.IconType)
                {
                    block.UpdateIcon(newIcon);
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

            Array.Clear(visitedBlocks, 0, visitedBlocks.Length); // clear visited array
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

                    // If  ANY valid group >= 2 blocks, exit early
                    if (currentGroup.Count >= 2)
                    {
                        return false;
                    }
                }
            }

            // No valid groups found - deadlock!
            return true;
        }
    }
}