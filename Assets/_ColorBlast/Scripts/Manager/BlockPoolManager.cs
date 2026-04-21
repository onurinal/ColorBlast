using System;
using UnityEngine;
using ColorBlast.Features;
using ColorBlast.Core;

namespace ColorBlast.Manager
{
    public class BlockPoolManager : PoolManager<BlockType, Block>
    {
        public static BlockPoolManager Instance { get; private set; }

        [SerializeField] private BlockPoolEntry[] entries;
        [SerializeField] private int poolMultiplier = 2;

        private void Awake()
        {
            InitializeSingleton();
        }

        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void InitializePool(LevelProperties levelProperties)
        {
            if (IsInitialized)
            {
                return;
            }

            var baseSize = levelProperties.RowCount * levelProperties.ColumnCount;

            foreach (var entry in entries)
            {
                var size = entry.data.BlockType == BlockType.Cube ? baseSize * poolMultiplier : entry.initialSize;
                CreatePool(entry.data.BlockType, entry.data.Prefab, size, entry.parent,
                    block => block.SetupVisual());
            }

            IsInitialized = true;
        }

        public Block GetBlock(BlockData data) => Get(data.BlockType);
        public void ReturnBlock(Block block) => Return(block.BlockType, block);
    }

    [Serializable]
    public struct BlockPoolEntry
    {
        public BlockData data;
        public Transform parent;
        public int initialSize;
    }
}