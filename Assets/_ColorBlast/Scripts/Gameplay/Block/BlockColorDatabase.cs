using System.Collections.Generic;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    [CreateAssetMenu(fileName = "BlockColorDatabase", menuName = "ColorBlast/Block Color Database")]
    public class BlockColorDatabase : ScriptableObject
    {
        [SerializeField] private List<BlockColorData> blockColorDataList;

        public List<BlockColorData> BlockColorDataList => blockColorDataList;

        private void OnEnable()
        {
            InitializeBlockColorDataDict();
        }

        private void InitializeBlockColorDataDict()
        {
            if (blockColorDataList == null || blockColorDataList.Count == 0)
            {
                Debug.LogWarning("BlockColorDataList is null or empty");
            }
        }

        public BlockColorData GetRandomBlockColorData(int maxColorCount)
        {
            if (blockColorDataList == null || blockColorDataList.Count == 0)
            {
                Debug.LogError("BlockColorDataList is empty");
                return null;
            }

            if (maxColorCount < 0 || maxColorCount > blockColorDataList.Count)
            {
                Debug.LogError("ColorCount is invalid");
                return null;
            }

            return blockColorDataList[Random.Range(0, maxColorCount)];
        }

        public BlockColorData GetColorDataByIndex(int index)
        {
            if (blockColorDataList == null || index < 0 || index >= blockColorDataList.Count)
            {
                return null;
            }

            return blockColorDataList[index];
        }
    }
}