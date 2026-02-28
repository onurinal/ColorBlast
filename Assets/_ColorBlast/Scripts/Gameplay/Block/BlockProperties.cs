using UnityEngine;

namespace ColorBlast.Gameplay
{
    [CreateAssetMenu(fileName = "BlockProperties", menuName = "ColorBlast/Block Properties")]
    public class BlockProperties : ScriptableObject
    {
        private const float BaseBlockSizeX = 0.45f;
        private const float BaseBlockSizeY = 0.4f;

        private const float BaseSpacingX = -0.01f;
        private const float BaseSpacingY = -0.15f;

        [SerializeField] private Block blockPrefab;

        [Header("Block Size")]
        [SerializeField] private float blockSizeX;

        [SerializeField] private float blockSizeY;

        [Header("Animation Durations")]
        [SerializeField] private int destroyDuration;
        [SerializeField] private int moveDuration;
        [SerializeField] private int spawnDuration;
        [SerializeField] private int shuffleDuration;

        public float BlockSizeX => blockSizeX;
        public float BlockSizeY => blockSizeY;
        public float SpacingX => BaseSpacingX * (blockSizeX / BaseBlockSizeX);
        public float SpacingY => BaseSpacingY * (blockSizeY / BaseBlockSizeY);

        public int MoveDurationMs => moveDuration;
        public int DestroyDurationMs => destroyDuration;
        public int SpawnDurationMs => spawnDuration;
        public int ShuffleDurationMs => shuffleDuration;
        public float MoveDurationSec => moveDuration / 1000f;
        public float DestroyDurationSec => destroyDuration / 1000f;

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