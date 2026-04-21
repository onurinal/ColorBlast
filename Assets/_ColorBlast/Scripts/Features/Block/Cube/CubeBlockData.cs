using System.Collections.Generic;
using UnityEngine;

namespace ColorBlast.Features
{
    /// <summary>
    /// Stores sprite data for a specific block color type
    /// Contains default icon and tier-based icons based on group size threshold
    /// </summary>
    [CreateAssetMenu(fileName = "CubeBlockData", menuName = "ColorBlast/Gameplay/Block/Cube/CubeData")]
    public class CubeBlockData : BlockData
    {
        [Header("Visual Settings")]
        [SerializeField] private Sprite defaultSprite;

        [Header("Particle Settings")]
        [SerializeField] private Color particleColor = Color.white;

        [Header("Reward Settings")]
        [SerializeField] private List<CubeRewardState> rewardStates;

        public override BlockType BlockType => BlockType.Cube;

        public Sprite GetVisual(int groupSize)
        {
            var rewardHint = GetRewardState(groupSize);

            if (rewardHint == null)
            {
                return defaultSprite;
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

        public Color ParticleColor => particleColor;
    }
}