using UnityEngine;

namespace ColorBlast.Blocks
{
    [CreateAssetMenu(fileName = "BlockProperties", menuName = "ColorBlast/Block Properties")]
    public class BlockProperties : ScriptableObject
    {
        [SerializeField] private Block blockPrefab;

        [Header("Block Settings")]
        [SerializeField] private float blockSizeX;
        [SerializeField] private float blockSizeY;
        [SerializeField] private float spacingX;
        [SerializeField] private float spacingY;

        [SerializeField] private float destroyDuration;
        [SerializeField] private float fallDuration; // delay for falling and moving
        [SerializeField] private float spawnDelayBetweenBlocks; // small delay between block spawn
        [SerializeField] private float spawnDuration; // delay for all new  block spawn

        public Block BlockPrefab => blockPrefab;
        public float BlockSizeX => blockSizeX;
        public float BlockSizeY => blockSizeY;
        public float SpacingX => spacingX;
        public float SpacingY => spacingY;
        public float DestroyDuration => destroyDuration;

        public float FallDuration => fallDuration;

        public float SpawnDelayBetweenBlocks => spawnDelayBetweenBlocks;
        public float SpawnDuration => spawnDuration;


        public Vector2 GetBlockSpriteBoundSize()
        {
            var blockSpriteRenderer = blockPrefab.GetComponentInChildren<SpriteRenderer>();
            if (blockSpriteRenderer == null && blockSpriteRenderer.sprite == null)
            {
                Debug.LogWarning("Block Sprite Renderer is null!");
                return Vector2.zero;
            }

            var blockSprite = blockSpriteRenderer.sprite;

            var baseSpriteWidth = blockSprite.rect.width / blockSprite.pixelsPerUnit;
            var baseSpriteHeight = blockSprite.rect.height / blockSprite.pixelsPerUnit;

            var blockSize = new Vector2(baseSpriteWidth * blockSizeX, baseSpriteHeight * blockSizeY);
            return blockSize;
        }
    }
}