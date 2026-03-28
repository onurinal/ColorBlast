using System;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    [Serializable]
    public class DiscoRewardState
    {
        [SerializeField] private BlockData targetCubeData;
        [SerializeField] private Sprite sprite;

        public Sprite GetSprite()
        {
            return sprite;
        }

        public BlockData TargetCubeData => targetCubeData;
    }
}