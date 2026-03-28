using UnityEngine;

namespace ColorBlast.Gameplay
{
    public abstract class BlockData : ScriptableObject
    {
        [Header("Block Settings")]
        [SerializeField] private Block prefab;
        public Block Prefab => prefab;
        public abstract BlockType BlockType { get; }
    }
}