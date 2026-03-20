using System;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    [Serializable]
    public class CubeRewardState
    {
        [Tooltip("Minimum connected group size required to unlock this reward")]
        [SerializeField] private int minGroupSize;

        [Tooltip("Reward icon shows on the block before it gets destroyed")]
        [SerializeField] private Sprite rewardHintSprite;

        [Tooltip("Prefab spawns when the group of this block is destroyed")]
        [SerializeField] private Block rewardPrefab;

        public int MinGroupSize => minGroupSize;
        public Sprite RewardHintSprite => rewardHintSprite;
        public Block RewardPrefab => rewardPrefab;
    }
}