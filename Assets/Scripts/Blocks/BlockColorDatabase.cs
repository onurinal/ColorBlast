using System.Collections.Generic;
using UnityEngine;

namespace ColorBlast.Blocks
{
    [CreateAssetMenu(fileName = "BlockColorDatabase", menuName = "ColorBlast/Block Color Database")]
    public class BlockColorDatabase : ScriptableObject
    {
        [SerializeField] private List<BlockColorData> blockColorDataList;

        private Dictionary<BlockColorType, BlockColorData> blockColorDataDict;

        private void OnEnable()
        {
            InitializeBlockColorDataDict();
        }

        private void InitializeBlockColorDataDict()
        {
            if (blockColorDataList == null || blockColorDataList.Count == 0)
            {
                Debug.LogWarning("BlockColorDataList is null or empty");
                return;
            }

            blockColorDataDict = new Dictionary<BlockColorType, BlockColorData>(blockColorDataList.Count);

            for (int i = 0; i < blockColorDataList.Count; i++)
            {
                var data = blockColorDataList[i];
                if (data != null)
                {
                    blockColorDataDict[data.ColorType] = data;
                }
            }
        }

        public Sprite GetSpriteForType(BlockColorType colorType, BlockIconType iconType)
        {
            if (blockColorDataDict.TryGetValue(colorType, out var colorData))
            {
                return colorData.GetSprite(iconType);
            }

            Debug.LogWarning($"BlockColorDatabase Color type '{colorType}' not found");
            return blockColorDataList[0].GetSprite(BlockIconType.Default);
        }
    }
}