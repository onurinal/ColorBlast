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
        private List<Block> currentGroup;

        private List<List<Block>> testBlockMatches = new List<List<Block>>();

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
            currentGroup = new List<Block>();
        }

        private void CheckAllGrid()
        {
            Array.Clear(visitedBlocks, 0, visitedBlocks.Length); // clear visited array

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

                    TryMatch(new Vector2Int(row, col), blockColor);

                    // Update icons based on group size
                    if (currentGroup.Count > levelProperties.FirstIconThreshold)
                    {
                        UpdateGroupIcons(currentGroup);
                    }

                    // just test add all groups to list print them - REMOVE IT AFTER TESTING
                    if (currentGroup.Count >= 2)
                    {
                        Debug.Log($"Found a new group and count of the group is:  {currentGroup.Count}");
                        foreach (Block b in currentGroup)
                        {
                            Debug.Log(b.gameObject.name);
                        }
                    }
                }
            }
        }

        private void TryMatch(Vector2Int startBlock, BlockColorType color)
        {
            Queue<Vector2Int> group = new Queue<Vector2Int>();
            group.Enqueue(startBlock);

            while (group.Count > 0)
            {
                var currentBlock = group.Dequeue();

                if (!IsInsideGrid(currentBlock)) continue;
                if (visitedBlocks[currentBlock.x, currentBlock.y]) continue;
                if (blockGrid[currentBlock.x, currentBlock.y] == null) continue;
                if (color != blockGrid[currentBlock.x, currentBlock.y].ColorType) continue;

                visitedBlocks[currentBlock.x, currentBlock.y] = true;
                currentGroup.Add(blockGrid[currentBlock.x, currentBlock.y]);

                group.Enqueue(currentBlock + Vector2Int.up);
                group.Enqueue(currentBlock + Vector2Int.down);
                group.Enqueue(currentBlock + Vector2Int.left);
                group.Enqueue(currentBlock + Vector2Int.right);
            }
        }

        private void UpdateGroupIcons(List<Block> group)
        {
            var iconType = DetermineBlockIconType(group.Count);

            foreach (var block in group)
            {
                block.UpdateIcon(iconType);
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

        private List<Block> GetGroup(int row, int col)
        {
            Array.Clear(visitedBlocks, 0, visitedBlocks.Length); // clear visited array

            var color = blockGrid[row, col].ColorType;
            TryMatch(new Vector2Int(row, col), color);

            return currentGroup;
        }

        private bool IsInsideGrid(Vector2Int position)
        {
            if (position.x >= levelProperties.RowCount || position.y >= levelProperties.ColumnCount || position.x < 0 || position.y < 0) return false;

            return true;
        }
    }
}