using UnityEngine;
using System.Collections.Generic;
using ColorBlast.Blocks;
using ColorBlast.Level;
using ColorBlast.Manager;

namespace ColorBlast.Grid
{
    public class GridShuffler
    {
        private Block[,] blockGrid;
        private LevelProperties levelProperties;
        private GridManager gridManager;

        public void Initialize(Block[,] blockGrid, LevelProperties levelProperties, GridManager gridManager)
        {
            this.blockGrid = blockGrid;
            this.levelProperties = levelProperties;
            this.gridManager = gridManager;
        }

        public void Shuffle()
        {
            var protectedPositions = new HashSet<(int row, int col)>();
            var colorToAllBlocks = new Dictionary<BlockColorType, List<Block>>();

            UpdateAllColorToList(colorToAllBlocks);

            var targetColor = GetColorToGuaranteeMatch(colorToAllBlocks);

            if (targetColor != BlockColorType.None)
            {
                var colorBlocks = colorToAllBlocks[targetColor];
                var first = colorBlocks[0];
                var second = colorBlocks[1];

                var (randomPosition, randomNeighbor) = GetRandomNeighbor();

                // update guarantee same colors grid position
                SwapBlocks(first, blockGrid[randomPosition.x, randomPosition.y]);
                SwapBlocks(second, blockGrid[randomNeighbor.x, randomNeighbor.y]);

                // save these same color block positions before shuffle
                protectedPositions.Add((randomPosition.x, randomPosition.y));
                protectedPositions.Add((randomNeighbor.x, randomNeighbor.y));
            }

            // if can't find 2 same color in grid, change forcefully
            else
            {
                var (randomPosition, randomNeighbor) = GetRandomNeighbor();
                targetColor = blockGrid[randomPosition.x, randomPosition.y].ColorType;
                blockGrid[randomNeighbor.x, randomNeighbor.y].UpdateColor(targetColor);

                protectedPositions.Add((randomPosition.x, randomPosition.y));
                protectedPositions.Add((randomNeighbor.x, randomNeighbor.y));
            }

            // shuffle rest with Fisher-Yates except [0] and [1] index
            ShuffleGrid(protectedPositions);

            ApplyShuffleToGrid();
        }

        private void UpdateAllColorToList(Dictionary<BlockColorType, List<Block>> colorToAllBlocks)
        {
            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                for (int col = 0; col < levelProperties.ColumnCount; col++)
                {
                    if (blockGrid[row, col] != null)
                    {
                        var color = blockGrid[row, col].ColorType;
                        if (!colorToAllBlocks.TryGetValue(color, out var newColorList))
                        {
                            newColorList = new List<Block>();
                            colorToAllBlocks.Add(color, newColorList);
                        }

                        newColorList.Add(blockGrid[row, col]);
                    }
                }
            }
        }

        private BlockColorType GetColorToGuaranteeMatch(Dictionary<BlockColorType, List<Block>> colorToAllBlocks)
        {
            for (int i = 0; i < levelProperties.ColorCount; i++)
            {
                var color = (BlockColorType)i;
                if (colorToAllBlocks.TryGetValue(color, out var list) && list.Count >= LevelRule.MatchThreshold)
                {
                    return color;
                }
            }

            return BlockColorType.None;
        }

        private (Vector2Int, Vector2Int) GetRandomNeighbor()
        {
            var randomRow = Random.Range(0, levelProperties.RowCount);
            var randomCol = Random.Range(0, levelProperties.ColumnCount);
            var newPosition = new Vector2Int(randomRow, randomCol);

            List<Vector2Int> neighbors = new List<Vector2Int>();

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

            var randomNeighbor = neighbors[Random.Range(0, neighbors.Count)];
            return (newPosition, randomNeighbor);
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

        private void ShuffleGrid(HashSet<(int row, int col)> protectedPositions)
        {
            var totalCols = levelProperties.ColumnCount;
            var totalBlock = levelProperties.RowCount * levelProperties.ColumnCount;

            for (int i = totalBlock - 1; i > 0; i--)
            {
                var rowI = i / totalCols;
                var colI = i % totalCols;

                if (blockGrid[rowI, colI] == null || protectedPositions.Contains((rowI, colI)))
                {
                    continue;
                }

                int j, rowJ, colJ;
                do
                {
                    j = Random.Range(0, i + 1);
                    rowJ = j / totalCols;
                    colJ = j % totalCols;
                } while (blockGrid[rowJ, colJ] == null || protectedPositions.Contains((rowJ, colJ)));

                if (i != j)
                {
                    SwapBlocks(blockGrid[rowI, colI], blockGrid[rowJ, colJ]);
                }
            }
        }

        private void ApplyShuffleToGrid()
        {
            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                for (int col = 0; col < levelProperties.ColumnCount; col++)
                {
                    if (blockGrid[row, col] != null)
                    {
                        var block = blockGrid[row, col];
                        block.MoveTo(gridManager.GetCellWorldPosition(row, col));
                    }
                }
            }
        }
    }
}