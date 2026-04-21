using UnityEngine;

namespace ColorBlast.Features
{
    public abstract class BlockData : ScriptableObject
    {
        [Header("Block Settings")]
        [SerializeField] private Block prefab;
        [SerializeField] private PoolableVFX particlePrefab;
        public Block Prefab => prefab;
        public PoolableVFX ParticlePrefab => particlePrefab;
        public abstract BlockType BlockType { get; }
    }
}