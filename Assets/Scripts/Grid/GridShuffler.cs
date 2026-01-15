using UnityEngine;
using System.Collections;
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

        public IEnumerator Shuffle()
        {
            List<Block> allBlocks = new List<Block>();
            Dictionary<BlockColorType, List<Block>> colorToAllBlocks = new Dictionary<BlockColorType, List<Block>>();
            Dictionary<Block, int> blockToIndex = new Dictionary<Block, int>();

            yield return new WaitForSeconds(2f);

            // get all block and positions first
            UpdateShuffleData(allBlocks, colorToAllBlocks, blockToIndex);

            var targetColor = GetColorToGuaranteeMatch(colorToAllBlocks);
            if (targetColor != BlockColorType.None)
            {
                var list = colorToAllBlocks[targetColor];
                var first = list[0];
                var second = list[1];

                SwapBlocks(first, allBlocks[0], allBlocks, blockToIndex);
                SwapBlocks(second, allBlocks[1], allBlocks, blockToIndex);
                Debug.Log($"targetColor  = {targetColor}");
            }

            else
            {
                // change a color of any block to get 1 matches
                // UPDATE THIS HERE !!!!!!!!!!!!!!!!!!!!
                Debug.Log("Couldn't find any 2 same color");
            }

            // shuffle rest with Fisher-Yates except [0] and [1] index
            for (int i = allBlocks.Count - 1; i > 1; i--)
            {
                var j = Random.Range(2, i + 1);
                SwapBlocks(allBlocks[i], allBlocks[j], allBlocks, blockToIndex);
            }

            ApplyShuffleToGrid(allBlocks);
        }

        private void UpdateShuffleData(List<Block> allBlocks, Dictionary<BlockColorType, List<Block>> colorToAllBlocks, Dictionary<Block, int> blockToIndex)
        {
            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                for (int col = 0; col < levelProperties.ColumnCount; col++)
                {
                    if (blockGrid[row, col] != null)
                    {
                        allBlocks.Add(blockGrid[row, col]);
                        blockToIndex[blockGrid[row, col]] = allBlocks.Count - 1; // add block with same index

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
                if (colorToAllBlocks.TryGetValue(color, out var list) && list.Count >= 2)
                {
                    return color;
                }
            }

            return BlockColorType.None;
        }

        private void SwapBlocks(Block block1, Block block2, List<Block> allBlocks, Dictionary<Block, int> blockToIndex)
        {
            var firstIndex = blockToIndex[block1];
            var secondIndex = blockToIndex[block2];

            var temp = allBlocks[firstIndex];
            allBlocks[firstIndex] = allBlocks[secondIndex];
            allBlocks[secondIndex] = temp;

            blockToIndex[block1] = secondIndex;
            blockToIndex[block2] = firstIndex;
        }

        private void ApplyShuffleToGrid(List<Block> allBlocks)
        {
            var index = 0;

            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                for (int col = 0; col < levelProperties.ColumnCount; col++)
                {
                    var block = allBlocks[index++];
                    block.SetGridPosition(row, col);
                    blockGrid[row, col] = block;
                    block.MoveTo(gridManager.GetCellWorldPosition(row, col));
                }
            }
        }
    }
}