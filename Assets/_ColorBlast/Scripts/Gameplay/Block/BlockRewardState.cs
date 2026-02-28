using System;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    [Serializable]
    public class BlockRewardState
    {
        [Tooltip("Minimum connected group size required to unlock this reward")]
        [SerializeField] private int minGroupSize;

        [Tooltip("Reward icon shows on the block before it gets destroyed")]
        [SerializeField] private Sprite rewardHintSprite;

        [Tooltip("Prefab spawns when the group of this block is destroyed")]
        [SerializeField] private GameObject rewardPrefab;

        public int MinGroupSize => minGroupSize;
        public Sprite RewardHintSprite => rewardHintSprite;
        public GameObject RewardPrefab => rewardPrefab;
    }
}