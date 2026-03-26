using UnityEngine;

namespace ColorBlast.Gameplay
{
    public abstract class BlockData : ScriptableObject
    {
        [Header("Block Settings")]
        [SerializeField] private Block prefab;
        [SerializeField] private Sprite defaultSprite;

        public Block Prefab => prefab;
        protected Sprite DefaultSprite => defaultSprite;

        public abstract BlockType BlockType { get; }
    }
}