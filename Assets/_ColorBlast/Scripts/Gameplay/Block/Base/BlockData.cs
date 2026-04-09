using ColorBlast._ColorBlast.Scripts.Gameplay;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    public abstract class BlockData : ScriptableObject
    {
        [Header("Block Settings")]
        [SerializeField] private Block prefab;
        [SerializeField] private PoolableParticle particlePrefab;
        public Block Prefab => prefab;
        public PoolableParticle ParticlePrefab => particlePrefab;
        public abstract BlockType BlockType { get; }
    }
}