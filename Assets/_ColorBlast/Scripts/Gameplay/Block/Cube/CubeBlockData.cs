using System.Collections.Generic;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    /// <summary>
    /// Stores sprite data for a specific block color type
    /// Contains default icon and tier-based icons based on group size threshold
    /// </summary>
    [CreateAssetMenu(fileName = "CubeBlockData", menuName = "ColorBlast/Gameplay/Block/Cube/CubeData")]
    public class CubeBlockData : BlockData
    {
        [Header("Reward Settings")]
        [SerializeField] private List<CubeRewardState> rewardStates;

        public override BlockType BlockType => BlockType.Cube;

        public Sprite GetVisual(int groupSize)
        {
            var rewardHint = GetRewardState(groupSize);

            if (rewardHint == null)
            {
                return DefaultSprite;
            }

            return rewardHint.RewardHintSprite;
        }

        public CubeRewardState GetRewardState(int groupSize)
        {
            if (rewardStates == null || rewardStates.Count == 0)
            {
                return null;
            }

            for (int i = rewardStates.Count - 1; i >= 0; i--)
            {
                if (groupSize >= rewardStates[i].MinGroupSize)
                {
                    return rewardStates[i];
                }
            }

            return null;
        }
    }
}