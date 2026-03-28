using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    public class ResolveResult
    {
        public HashSet<Block> BlocksToClear { get; }
        public BlockData RewardData { get; }
        public Sprite RewardSprite { get; }
        public int SpawnRow { get; }
        public int SpawnColumn { get; }

        public bool HasReward => RewardData != null;

        public ResolveResult(HashSet<Block> blocksToClear, BlockData cubeData = null, BlockData rewardData = null,
            int spawnRow = 0,
            int spawnColumn = 0)
        {
            BlocksToClear = blocksToClear;
            RewardData = rewardData;
            RewardSprite = rewardData ? GetRewardSprite(cubeData, rewardData) : null;
            SpawnRow = spawnRow;
            SpawnColumn = spawnColumn;
        }

        private Sprite GetRewardSprite(BlockData cubeData, BlockData rewardData)
        {
            var blockType = rewardData.BlockType;

            return blockType switch
            {
                BlockType.Bomb => null,
                BlockType.DiscoBall => ResolveDiscoBallSprite(cubeData, (DiscoBlockData)rewardData),
                BlockType.Rocket => null,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private Sprite ResolveDiscoBallSprite(BlockData cubeData, DiscoBlockData discoBlockData)
        {
            var rewardState = discoBlockData.GetRewardState(cubeData);
            return rewardState.GetSprite();
        }
    }
}