using ColorBlast.Blocks;
using ColorBlast.Level;
using UnityEngine;

namespace ColorBlast.Grid
{
    public class GridSpawner
    {
        // private Block[,] blockGrid;
        private BlockProperties blockProperties;
        private LevelProperties levelProperties;

        private Transform blocksParent;

        public void Initialize(BlockProperties blockProperties, LevelProperties levelProperties, Transform blocksParent)
        {
            // this.blockGrid = blockGrid;
            this.blockProperties = blockProperties;
            this.levelProperties = levelProperties;
            this.blocksParent = blocksParent;
        }

        public Block CreateBlock(int row, int col, Vector2 position)
        {
            var newBlock = Object.Instantiate(blockProperties.BlockPrefab, position, Quaternion.identity, blocksParent);
            newBlock.name = $"Block_{row}_{col}";
            newBlock.Initialize(row, col, GetRandomColor());
            return newBlock;
        }

        private BlockColorType GetRandomColor()
        {
            var colorSize = levelProperties.ColorCount;
            var newColorNumber = Random.Range(0, colorSize);
            var newColor = (BlockColorType)newColorNumber;
            return newColor;
        }
    }
}