using System;
using System.Collections.Generic;
using UnityEngine;
using ColorBlast.Gameplay;
using ColorBlast.Core;

namespace ColorBlast.Manager
{
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager Instance { get; private set; }

        [SerializeField] private BlockPoolEntry[] blockPoolEntry;
        [SerializeField] private int poolMultiplier = 2;

        private readonly Dictionary<BlockType, PoolableObject<Block>> blockPool = new();
        private bool isInitialized;

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
            if (isInitialized)
            {
                return;
            }

            var baseSize = levelProperties.RowCount * levelProperties.ColumnCount;

            foreach (var entry in blockPoolEntry)
            {
                var blockType = entry.data.BlockType;
                var size = blockType == BlockType.Cube ? baseSize * poolMultiplier : entry.initialSize;
                blockPool[blockType] =
                    new PoolableObject<Block>(entry.data.Prefab, size, entry.parent,
                        block => block.SetupVisual());
            }

            isInitialized = true;
        }

        public Block GetBlock(BlockData data)
        {
            if (!isInitialized || !blockPool.TryGetValue(data.BlockType, out var pool))
            {
                Debug.LogError($"No pool for: {data.BlockType}");
                return null;
            }

            return pool.Get();
        }

        public void ReturnBlock(Block block)
        {
            if (!isInitialized || !blockPool.TryGetValue(block.BlockType, out var pool))
            {
                return;
            }

            pool.Return(block);
        }
    }

    [Serializable]
    public struct BlockPoolEntry
    {
        public BlockData data;
        public Transform parent;
        public int initialSize;
    }

}