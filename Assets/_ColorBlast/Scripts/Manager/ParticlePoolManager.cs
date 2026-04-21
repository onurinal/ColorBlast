using System;
using ColorBlast.Core;
using ColorBlast.Features;
using UnityEngine;

namespace ColorBlast.Manager
{
    public class ParticlePoolManager : PoolManager<BlockType, PoolableVFX>
    {
        public static ParticlePoolManager Instance { get; private set; }

        [SerializeField] private ParticlePoolEntry[] entries;
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
                CreatePool(entry.data.BlockType, entry.data.ParticlePrefab, size, entry.parent);
            }

            IsInitialized = true;
        }

        public PoolableVFX GetParticle(BlockData data) => Get(data.BlockType);
        public void ReturnParticle(BlockType blockType, PoolableVFX particle) => Return(blockType, particle);

        [Serializable]
        public struct ParticlePoolEntry
        {
            public BlockData data;
            public Transform parent;
            public int initialSize;
        }
    }
}