using UnityEngine;
using System.Collections.Generic;
using ColorBlast.Core;
using ColorBlast.Manager;

namespace ColorBlast.Features
{
    /// <summary>
    /// Handles grid shuffling to prevent deadlocks
    /// Only shuffles on matchable blocks
    /// Guarantees at least one valid match after shuffle
    /// </summary>
    public class GridShuffler
    {
        private readonly Dictionary<BlockData, List<Block>> matchableByData = new();
        private readonly HashSet<(int row, int col)> protectedPositions = new();
        private readonly List<Vector2Int> matchablePositions = new();
        private readonly List<Vector2Int> neighborBuffer = new();

        private Block[,] grid;
        private LevelProperties levelProperties;
        private GridManager gridManager;
        private GameConfig gameplayConfig;

        private int maxAttempts;

        public void Initialize(Block[,] grid, LevelProperties levelProperties, GridManager gridManager,
            GameConfig gameplayConfig)
        {
            this.grid = grid;
            this.levelProperties = levelProperties;
            this.gridManager = gridManager;
            this.gameplayConfig = gameplayConfig;

            maxAttempts = levelProperties.ColumnCount * levelProperties.RowCount;
        }

        public void Shuffle()
        {
            PrepareShuffleList();
            protectedPositions.Clear();

            EnsureGuaranteedMatch();

            ShuffleGrid();

            AnimateBlocksToNewPositions();
        }

        private void PrepareShuffleList()
        {
            matchableByData.Clear();
            matchablePositions.Clear();

            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                for (int col = 0; col < levelProperties.ColumnCount; col++)
                {
                    Block block = grid[row, col];

                    if (block == null)
                    {
                        continue;
                    }

                    if (block is not IMatchable)
                    {
                        continue;
                    }

                    matchablePositions.Add(new Vector2Int(row, col));
                    if (!matchableByData.TryGetValue(block.BlockData, out var list))
                    {
                        list = new List<Block>();
                        matchableByData.Add(block.BlockData, list);
                    }

                    list.Add(block);
                }
            }
        }

        private void EnsureGuaranteedMatch()
        {
            var targetData = FindColorForGuaranteedMatch();

            if (targetData != null)
            {
                CreateGuaranteeMatchBySwap(targetData);
            }
            else
            {
                CreateGuaranteeMatchByRecolor();
            }
        }

        private BlockData FindColorForGuaranteedMatch()
        {
            foreach (var kvp in matchableByData)
            {
                if (kvp.Value.Count >= gameplayConfig.MatchThreshold)
                {
                    return kvp.Key;
                }
            }

            return null;
        }

        private void CreateGuaranteeMatchBySwap(BlockData targetData)
        {
            var blocks = matchableByData[targetData];

            var first = blocks[0];
            var second = blocks[1];

            var (pos, neighbor) = GetRandomNeighbor();

            SwapBlocks(first, grid[pos.x, pos.y]);
            SwapBlocks(second, grid[neighbor.x, neighbor.y]);

            protectedPositions.Add((pos.x, pos.y));
            protectedPositions.Add((neighbor.x, neighbor.y));
        }

        private void CreateGuaranteeMatchByRecolor()
        {
            var (pos, neighbor) = GetRandomNeighbor();

            var targetBlock = grid[pos.x, pos.y];
            var neighborBlock = grid[neighbor.x, neighbor.y];

            if (neighborBlock is IRecolorable recolorableBlock)
            {
                recolorableBlock.SetColor(targetBlock.BlockData);
            }

            protectedPositions.Add((pos.x, pos.y));
            protectedPositions.Add((neighbor.x, neighbor.y));
        }

        private (Vector2Int, Vector2Int) GetRandomNeighbor()
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector2Int pos = matchablePositions[Random.Range(0, matchablePositions.Count)];
                List<Vector2Int> matchableNeighbors = GetMatchableNeighborsAt(pos.x, pos.y);

                if (matchableNeighbors.Count > 0)
                {
                    var neighbor = matchableNeighbors[Random.Range(0, matchableNeighbors.Count)];
                    return (pos, neighbor);
                }
            }

            Debug.LogError("No valid matchable neighbor pair found.");
            var fallback = matchablePositions[0];
            return (fallback, fallback);
        }

        private List<Vector2Int> GetMatchableNeighborsAt(int row, int col)
        {
            neighborBuffer.Clear();
            TryAddMatchable(row + 1, col);
            TryAddMatchable(row - 1, col);
            TryAddMatchable(row, col + 1);
            TryAddMatchable(row, col - 1);
            return neighborBuffer;
        }

        private void TryAddMatchable(int row, int col)
        {
            if (row >= 0 && col >= 0 && row < levelProperties.RowCount && col < levelProperties.ColumnCount &&
                IsMatchable(row, col))
            {
                neighborBuffer.Add(new Vector2Int(row, col));
            }
        }

        private bool IsMatchable(int row, int col)
        {
            var block = grid[row, col];

            if (block != null && block is IMatchable)
            {
                return true;
            }

            return false;
        }

        private void ShuffleGrid()
        {
            var totalColumn = levelProperties.ColumnCount;
            var totalBlock = levelProperties.RowCount * levelProperties.ColumnCount;

            for (int i = totalBlock - 1; i > 0; i--)
            {
                var rowI = i / totalColumn;
                var colI = i % totalColumn;

                if (grid[rowI, colI] == null)
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

                SwapBlocks(grid[rowI, colI], grid[rowJ, colJ]);
            }
        }

        private int FindValidSwapTarget(int index, int totalColumn)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var j = Random.Range(0, index + 1);
                var rowJ = j / totalColumn;
                var colJ = j % totalColumn;

                if (grid[rowJ, colJ] != null && !protectedPositions.Contains((rowJ, colJ)))
                {
                    return j;
                }
            }

            return -1;
        }

        private void SwapBlocks(Block block1, Block block2)
        {
            var row1 = block1.GridX;
            var col1 = block1.GridY;
            var row2 = block2.GridX;
            var col2 = block2.GridY;

            grid[row1, col1] = block2;
            grid[row2, col2] = block1;

            block1.SetGridPosition(row2, col2);
            block2.SetGridPosition(row1, col1);
        }

        private void AnimateBlocksToNewPositions()
        {
            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                for (int col = 0; col < levelProperties.ColumnCount; col++)
                {
                    if (grid[row, col] == null)
                    {
                        continue;
                    }

                    grid[row, col].MoveToPosition(gridManager.GetCellWorldPosition(row, col));
                }
            }
        }
    }
}