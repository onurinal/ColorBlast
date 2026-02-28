using UnityEngine;
using System.Collections.Generic;
using ColorBlast.Config;
using ColorBlast.Manager;

namespace ColorBlast.Gameplay
{
    /// <summary>
    /// Handles grid shuffling to prevent deadlocks
    /// Guarantees at least one valid match after shuffle
    /// </summary>
    public class GridShuffler
    {
        private readonly Dictionary<BlockColorData, List<Block>> colorToBlocks = new();
        private readonly HashSet<(int row, int col)> protectedPositions = new();
        private readonly List<Vector2Int> neighbors = new();
        private Block[,] blockGrid;
        private LevelProperties levelProperties;
        private GridManager gridManager;
        private BlockColorDatabase blockColorDatabase;

        private int maxAttempts;

        public void Initialize(Block[,] blockGrid, LevelProperties levelProperties, GridManager gridManager,
            BlockColorDatabase blockColorDatabase)
        {
            this.blockGrid = blockGrid;
            this.levelProperties = levelProperties;
            this.gridManager = gridManager;
            this.blockColorDatabase = blockColorDatabase;

            maxAttempts = levelProperties.ColumnCount * levelProperties.RowCount;
        }

        public void Shuffle()
        {
            UpdateAllColorToList(colorToBlocks);

            protectedPositions.Clear();

            var targetColor = FindColorForGuaranteedMatch(colorToBlocks);

            if (targetColor != null)
            {
                CreateGuaranteeMatchBySwap(targetColor);
            }

            else
            {
                CreateGuaranteeMatchByRecolor();
            }

            ShuffleGrid();

            AnimateBlocksToNewPositions();
        }

        private void UpdateAllColorToList(Dictionary<BlockColorData, List<Block>> colorToAllBlocks)
        {
            colorToBlocks.Clear();

            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                for (int col = 0; col < levelProperties.ColumnCount; col++)
                {
                    if (blockGrid[row, col] == null)
                    {
                        continue;
                    }

                    var color = blockGrid[row, col].ColorData;
                    if (!colorToAllBlocks.TryGetValue(color, out var newColorList))
                    {
                        newColorList = new List<Block>();
                        colorToAllBlocks.Add(color, newColorList);
                    }

                    newColorList.Add(blockGrid[row, col]);
                }
            }
        }

        private BlockColorData FindColorForGuaranteedMatch(Dictionary<BlockColorData, List<Block>> colorToAllBlocks)
        {
            for (int i = 0; i < levelProperties.ColorCount; i++)
            {
                var colorData = blockColorDatabase.GetColorDataByIndex(i);
                if (colorToAllBlocks.TryGetValue(colorData, out var list) &&
                    list.Count >= GameConstRules.MatchThreshold)
                {
                    return colorData;
                }
            }

            return null;
        }

        private void CreateGuaranteeMatchBySwap(BlockColorData targetColor)
        {
            var colorBlocks = colorToBlocks[targetColor];

            var first = colorBlocks[0];
            var second = colorBlocks[1];

            var (randomPosition, randomNeighbor) = GetRandomNeighbor();

            SwapBlocks(first, blockGrid[randomPosition.x, randomPosition.y]);
            SwapBlocks(second, blockGrid[randomNeighbor.x, randomNeighbor.y]);

            protectedPositions.Add((randomPosition.x, randomPosition.y));
            protectedPositions.Add((randomNeighbor.x, randomNeighbor.y));
        }

        private void CreateGuaranteeMatchByRecolor()
        {
            var (randomPosition, randomNeighbor) = GetRandomNeighbor();
            var targetColor = blockGrid[randomPosition.x, randomPosition.y].ColorData;
            blockGrid[randomNeighbor.x, randomNeighbor.y].SetColor(targetColor);

            protectedPositions.Add((randomPosition.x, randomPosition.y));
            protectedPositions.Add((randomNeighbor.x, randomNeighbor.y));
        }

        private (Vector2Int, Vector2Int) GetRandomNeighbor()
        {
            var randomRow = Random.Range(0, levelProperties.RowCount);
            var randomCol = Random.Range(0, levelProperties.ColumnCount);
            var position = new Vector2Int(randomRow, randomCol);

            neighbors.Clear();

            if (randomRow > 0)
            {
                neighbors.Add(new Vector2Int(randomRow - 1, randomCol));
            }

            if (randomRow < levelProperties.RowCount - 1)
            {
                neighbors.Add(new Vector2Int(randomRow + 1, randomCol));
            }

            if (randomCol > 0)
            {
                neighbors.Add(new Vector2Int(randomRow, randomCol - 1));
            }

            if (randomCol < levelProperties.ColumnCount - 1)
            {
                neighbors.Add(new Vector2Int(randomRow, randomCol + 1));
            }

            if (neighbors.Count == 0)
            {
                Debug.LogError("No neighbors found");
                return (position, position);
            }

            var randomNeighbor = neighbors[Random.Range(0, neighbors.Count)];
            return (position, randomNeighbor);
        }

        private void SwapBlocks(Block block1, Block block2)
        {
            var row1 = block1.GridX;
            var col1 = block1.GridY;
            var row2 = block2.GridX;
            var col2 = block2.GridY;

            blockGrid[row1, col1] = block2;
            blockGrid[row2, col2] = block1;

            block1.SetGridPosition(row2, col2);
            block2.SetGridPosition(row1, col1);
        }

        private void ShuffleGrid()
        {
            var totalColumn = levelProperties.ColumnCount;
            var totalBlock = levelProperties.RowCount * levelProperties.ColumnCount;

            for (int i = totalBlock - 1; i > 0; i--)
            {
                var rowI = i / totalColumn;
                var colI = i % totalColumn;

                if (blockGrid[rowI, colI] == null)
                {
                    continue;
                }

                if (protectedPositions.Contains((rowI, colI)))
                {
                    continue;
                }

                var j = FindValidSwapTarget(i, totalColumn);

                if (j < 0)
                {
                    continue;
                }

                var rowJ = j / totalColumn;
                var colJ = j % totalColumn;

                SwapBlocks(blockGrid[rowI, colI], blockGrid[rowJ, colJ]);
            }
        }

        private int FindValidSwapTarget(int index, int totalColumn)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var j = Random.Range(0, index + 1);
                var rowJ = j / totalColumn;
                var colJ = j % totalColumn;

                if (blockGrid[rowJ, colJ] != null && !protectedPositions.Contains((rowJ, colJ)))
                {
                    return j;
                }
            }

            return -1;
        }

        private void AnimateBlocksToNewPositions()
        {
            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                for (int col = 0; col < levelProperties.ColumnCount; col++)
                {
                    if (blockGrid[row, col] == null)
                    {
                        continue;
                    }

                    var block = blockGrid[row, col];
                    block.MoveToPosition(gridManager.GetCellWorldPosition(row, col));
                }
            }
        }
    }
}