using UnityEngine;

namespace ColorBlast.Gameplay
{
    public abstract class BlockData : ScriptableObject
    {
        [Header("Block Settings")]
        [SerializeField] private Block prefab;
        [SerializeField] private ParticleSystem particlePrefab;
        public Block Prefab => prefab;
        public ParticleSystem ParticlePrefab => particlePrefab;
        public abstract BlockType BlockType { get; }
    }
}