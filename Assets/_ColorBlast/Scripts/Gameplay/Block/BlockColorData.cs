using System.Collections.Generic;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    /// <summary>
    /// Stores sprite data for a specific block color type
    /// Contains default icon and tier-based icons based on group size threshold
    /// </summary>
    [CreateAssetMenu(fileName = "BlockColorData", menuName = "ColorBlast/Block Color Data")]
    public class BlockColorData : ScriptableObject
    {
        [SerializeField] private Sprite defaultSprite;

        [SerializeField] private List<BlockRewardState> rewardStates;

        public Sprite GetVisual(int groupSize)
        {
            var rewardHint = GetRewardState(groupSize);

            if (rewardHint == null)
            {
                return defaultSprite;
            }

            return rewardHint.RewardHintSprite;
        }

        private BlockRewardState GetRewardState(int groupSize)
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