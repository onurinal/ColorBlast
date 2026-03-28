using System.Collections.Generic;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    [CreateAssetMenu(fileName = "DiscoBlockData", menuName = "ColorBlast/Gameplay/Block/DiscoBlock")]
    public class DiscoBlockData : BlockData
    {
        [SerializeField] private List<DiscoRewardState> rewardStates;
        public override BlockType BlockType => BlockType.DiscoBall;

        public DiscoRewardState GetRewardState(BlockData blockData)
        {
            foreach (var rewardState in rewardStates)
            {
                if (blockData == rewardState.TargetCubeData)
                {
                    return rewardState;
                }
            }

            return null;
        }
    }
}