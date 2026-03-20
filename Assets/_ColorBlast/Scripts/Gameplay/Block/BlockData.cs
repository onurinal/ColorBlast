using UnityEngine;

namespace ColorBlast.Gameplay
{
    public abstract class BlockData : ScriptableObject
    {
        [Header("Block Settings")]
        [SerializeField] private Block prefab;
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private float sizeX = 0.45f;
        [SerializeField] private float sizeY = 0.45f;
        
        public Block Prefab => prefab;
        public float SizeX => sizeX;
        public float SizeY => sizeY;
        protected Sprite DefaultSprite => defaultSprite;
    }
}