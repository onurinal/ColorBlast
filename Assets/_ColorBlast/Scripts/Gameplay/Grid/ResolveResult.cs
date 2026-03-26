using System.Collections.Generic;

namespace ColorBlast.Gameplay
{
    public class ResolveResult
    {
        public List<Block> BlocksToClear { get; }
        public BlockData RewardData { get; }
        public int SpawnRow { get; }
        public int SpawnColumn { get; }

        public bool HasReward => RewardData != null;

        public ResolveResult(List<Block> blocksToClear, BlockData rewardData = null, int spawnRow = 0,
            int spawnColumn = 0)
        {
            BlocksToClear = blocksToClear;
            RewardData = rewardData;
            SpawnRow = spawnRow;
            SpawnColumn = spawnColumn;
        }
    }
}