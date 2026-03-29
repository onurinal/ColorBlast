using System.Collections.Generic;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    public class ResolveResult
    {
        public HashSet<Block> BlocksToClear { get; }
        public BlockData RewardData { get; }
        public Sprite RewardSprite { get; }
        public BlockData TargetCubeData { get; }
        public int SpawnRow { get; }
        public int SpawnColumn { get; }

        public bool HasReward => RewardData != null;

        public ResolveResult(HashSet<Block> blocksToClear, BlockData rewardData = null, Sprite rewardSprite = null,
            BlockData targetCubeData = null,
            int spawnRow = 0,
            int spawnColumn = 0)
        {
            BlocksToClear = blocksToClear;
            RewardData = rewardData;
            RewardSprite = rewardSprite;
            TargetCubeData = targetCubeData;
            SpawnRow = spawnRow;
            SpawnColumn = spawnColumn;
        }
    }
}