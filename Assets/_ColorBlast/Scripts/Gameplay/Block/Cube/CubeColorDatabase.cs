using System.Collections.Generic;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    [CreateAssetMenu(fileName = "CubeColorDatabase", menuName = "ColorBlast/Gameplay/Block/Cube/Cube Color Database")]
    public class CubeColorDatabase : ScriptableObject
    {
        [SerializeField] private List<CubeBlockData> cubeColorDataList;

        public CubeBlockData GetRandomBlockColorData(int maxColorCount)
        {
            if (cubeColorDataList == null || cubeColorDataList.Count == 0)
            {
                Debug.LogError("BlockColorDataList is empty");
                return null;
            }

            if (maxColorCount < 0 || maxColorCount > cubeColorDataList.Count)
            {
                Debug.LogError("ColorCount is invalid");
                return null;
            }

            return cubeColorDataList[Random.Range(0, maxColorCount)];
        }

        public CubeBlockData GetColorDataByIndex(int index)
        {
            if (cubeColorDataList == null || index < 0 || index >= cubeColorDataList.Count)
            {
                return null;
            }

            return cubeColorDataList[index];
        }
    }
}