using System;
using System.Collections.Generic;
using ColorBlast._ColorBlast.Scripts.Gameplay;
using ColorBlast.Core;
using ColorBlast.Gameplay;
using UnityEngine;

namespace ColorBlast.Manager
{
    public class ParticlePoolManager : MonoBehaviour
    {
        public static ParticlePoolManager Instance { get; private set; }

        [SerializeField] private ParticlePoolEntry[] particlePoolEntry;
        [SerializeField] private int poolMultiplier = 2;

        private readonly Dictionary<BlockType, PoolableObject<PoolableParticle>> particlePool = new();
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

            foreach (var entry in particlePoolEntry)
            {
                var blockType = entry.data.BlockType;
                var size = blockType == BlockType.Cube ? baseSize * poolMultiplier : entry.initialSize;
                particlePool[blockType] =
                    new PoolableObject<PoolableParticle>(entry.data.ParticlePrefab, size, entry.parent);
            }

            isInitialized = true;
        }

        public PoolableParticle GetParticle(BlockData data)
        {
            if (!isInitialized || !particlePool.TryGetValue(data.BlockType, out var pool))
            {
                Debug.LogError($"No pool for: {data.BlockType}");
                return null;
            }

            return pool.Get();
        }

        public void ReturnParticle(Block block, PoolableParticle particle)
        {
            if (!isInitialized || !particlePool.TryGetValue(block.BlockType, out var pool))
            {
                return;
            }

            pool.Return(particle);
        }
    }

    [Serializable]
    public struct ParticlePoolEntry
    {
        public BlockData data;
        public Transform parent;
        public int initialSize;
    }
}