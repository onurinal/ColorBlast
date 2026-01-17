using System.Collections.Generic;
using UnityEngine;

namespace ColorBlast.Blocks
{
    [CreateAssetMenu(fileName = "BlockColorDatabase", menuName = "ColorBlast/Block Color Database")]
    public class BlockColorDatabase : ScriptableObject
    {
        [SerializeField] private List<BlockColorData> blockColorDataList;

        private Dictionary<BlockColorType, BlockColorData> blockColorDataDict;

        private void InitializeBlockColorDataDict()
        {
            blockColorDataDict = new Dictionary<BlockColorType, BlockColorData>();

            foreach (var data in blockColorDataList)
            {
                blockColorDataDict.Add(data.ColorType, data);
            }
        }

        public Sprite GetSpriteForType(BlockColorType colorType, BlockIconType iconType)
        {
            if (blockColorDataDict == null) InitializeBlockColorDataDict();

            if (blockColorDataDict != null && blockColorDataDict.TryGetValue(colorType, out var colorData))
            {
                return colorData.GetSprite(iconType);
            }

            return null;
        }
    }
}