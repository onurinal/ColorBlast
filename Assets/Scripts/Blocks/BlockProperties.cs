using UnityEngine;

namespace ColorBlast.Blocks
{
    [CreateAssetMenu(fileName = "BlockProperties", menuName = "ColorBlast/Block Properties")]
    public class BlockProperties : ScriptableObject
    {
        private const float BaseBlockSizeX = 0.45f;
        private const float BaseBlockSizeY = 0.4f;

        private const float BaseSpacingX = -0.15f;
        private const float BaseSpacingY = -0.15f;

        [SerializeField] private Block blockPrefab;

        [Header("Block Settings")]
        [SerializeField] private float blockSizeX;
        [SerializeField] private float blockSizeY;

        [SerializeField] private float destroyDuration;
        [SerializeField] private float moveDuration;
        [SerializeField] private float spawnDelayBetweenBlocks;
        [SerializeField] private float spawnDuration;
        [SerializeField] private float shuffleDuration;


        public float BlockSizeX => blockSizeX;
        public float BlockSizeY => blockSizeY;
        public float SpacingX => BaseSpacingX * (blockSizeX / BaseBlockSizeX);
        public float SpacingY => BaseSpacingY * (blockSizeY / BaseBlockSizeY);

        public float DestroyDuration => destroyDuration;
        public float MoveDuration => moveDuration;
        public float SpawnDelayBetweenBlocks => spawnDelayBetweenBlocks;
        public float SpawnDuration => spawnDuration;
        public float ShuffleDuration => shuffleDuration;


        public Vector2 GetBlockSpriteBoundSize()
        {
            var blockSpriteRenderer = blockPrefab.GetComponentInChildren<SpriteRenderer>();
            if (blockSpriteRenderer == null || blockSpriteRenderer.sprite == null)
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