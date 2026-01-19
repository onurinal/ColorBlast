using UnityEngine;

namespace ColorBlast.Blocks
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
        [SerializeField] private float destroyDuration;
        [SerializeField] private float moveDuration;
        [SerializeField] private float spawnDuration;
        [SerializeField] private float shuffleDuration;

        private WaitForSeconds cachedDestroyWait;
        private WaitForSeconds cachedMoveWait;
        private WaitForSeconds cachedSpawnWait;
        private WaitForSeconds cachedShuffleWait;

        private void OnEnable()
        {
            UpdateCacheValues();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateCacheValues();
        }
#endif

        private void UpdateCacheValues()
        {
            cachedDestroyWait = new WaitForSeconds(destroyDuration);
            cachedMoveWait = new WaitForSeconds(moveDuration);
            cachedSpawnWait = new WaitForSeconds(spawnDuration);
            cachedShuffleWait = new WaitForSeconds(shuffleDuration);
        }

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


        public float BlockSizeX => blockSizeX;
        public float BlockSizeY => blockSizeY;
        public float SpacingX => BaseSpacingX * (blockSizeX / BaseBlockSizeX);
        public float SpacingY => BaseSpacingY * (blockSizeY / BaseBlockSizeY);

        public float MoveDuration => moveDuration;
        public float DestroyDuration => destroyDuration;
        public float ShuffleDuration => shuffleDuration;

        public WaitForSeconds DestroyWait => cachedDestroyWait;
        public WaitForSeconds MoveWait => cachedMoveWait;
        public WaitForSeconds SpawnWait => cachedSpawnWait;
        public WaitForSeconds ShuffleWait => cachedShuffleWait;
    }
}