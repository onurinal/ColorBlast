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
        [SerializeField] private int destroyDuration = 200;
        [SerializeField] private int moveDuration = 250;
        [SerializeField] private int spawnDuration = 250;
        [SerializeField] private int shuffleDuration = 300;

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

        public Vector2 BlockSpriteBoundSize { get; private set; }

#if UNITY_EDITOR
        private void OnValidate()
        {
            CacheBlockSpriteBoundSize();
        }

        private void CacheBlockSpriteBoundSize()
        {
            if (blockPrefab == null)
            {
                Debug.LogWarning($"[{name}] Block prefab is not assigned.", this);
                BlockSpriteBoundSize = Vector2.zero;
                return;
            }

            var spriteRenderer = blockPrefab.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                Debug.LogWarning($"[{name}] Block prefab has no valid SpriteRenderer or Sprite.", this);
                BlockSpriteBoundSize = Vector2.zero;
                return;
            }

            var sprite = spriteRenderer.sprite;
            var spriteWidth = sprite.rect.width / sprite.pixelsPerUnit;
            var spriteHeight = sprite.rect.height / sprite.pixelsPerUnit;

            BlockSpriteBoundSize = new Vector2(spriteWidth * blockSizeX, spriteHeight * blockSizeY);
        }
#endif
    }
}