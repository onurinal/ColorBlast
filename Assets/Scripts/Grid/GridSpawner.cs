using ColorBlast.Blocks;
using ColorBlast.Level;
using ColorBlast.Manager;
using UnityEngine;

namespace ColorBlast.Grid
{
    public class GridSpawner
    {
        private BlockProperties blockProperties;
        private LevelProperties levelProperties;


        public void Initialize(BlockProperties blockProperties, LevelProperties levelProperties)
        {
            this.blockProperties = blockProperties;
            this.levelProperties = levelProperties;
        }

        public Block CreateBlock(int row, int col, Vector2 position)
        {
            var newBlock = ObjectPoolManager.Instance.GetBlock();
            newBlock.Initialize(row, col, GetRandomColor());
            newBlock.transform.position = position;
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