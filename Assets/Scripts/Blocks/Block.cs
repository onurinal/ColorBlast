using ColorBlast.Manager;
using ColorBlast.ObjectPool;
using DG.Tweening;
using UnityEngine;

namespace ColorBlast.Blocks
{
    public class Block : MonoBehaviour, IPoolable
    {
        [SerializeField] private BlockProperties blockProperties;
        [SerializeField] private BlockColorDatabase blockColorDatabase;
        [SerializeField] private SpriteRenderer blockSpriteRenderer;
        [SerializeField] private Transform blockModelTransform;

        [SerializeField] private int gridX;
        [SerializeField] private int gridY;
        [SerializeField] private BlockColorType colorType;
        [SerializeField] private BlockIconType iconType;

        private Tween destroyTween;

        public int GridX => gridX;
        public int GridY => gridY;
        public BlockColorType ColorType => colorType;
        public BlockIconType IconType => iconType;

        // for tracking neighbors
        public int PrevGridX { get; private set; }
        public int PrevGridY { get; private set; }

        private void Awake()
        {
            UpdateBlockScale();
        }

        public void Initialize(int gridX, int gridY, BlockColorType colorType)
        {
            SetGridPosition(gridX, gridY);
            this.colorType = colorType;
            UpdateVisual();
        }

        public void SetGridPosition(int gridX, int gridY)
        {
            PrevGridX = this.gridX;
            PrevGridY = this.gridY;

            this.gridX = gridX;
            this.gridY = gridY;

            UpdateOrderLayer();
        }

        public void UpdateIcon(BlockIconType iconType)
        {
            this.iconType = iconType;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            blockSpriteRenderer.sprite = blockColorDatabase.GetSpriteForType(colorType, iconType);
        }

        private void ResetVisual()
        {
            iconType = BlockIconType.Default;
            UpdateVisual();
        }

        private void UpdateOrderLayer()
        {
            blockSpriteRenderer.sortingOrder = gridY;
        }

        // updating block local scale just once at start
        private void UpdateBlockScale()
        {
            blockModelTransform.localScale = new Vector3(blockProperties.BlockSizeX, blockProperties.BlockSizeY, 1);
        }

        public void MoveTo(Vector2 targetPosition)
        {
            transform.DOMove(targetPosition, blockProperties.FallDuration).SetEase(Ease.InOutCubic);
        }

        public void Destroy()
        {
            destroyTween =
                transform.DOScale(new Vector2(0f, 0f), blockProperties.DestroyDuration).SetEase(Ease.InOutBounce).OnComplete(() => ObjectPoolManager.Instance.ReturnBlock(this));
        }

        public void OnSpawn()
        {
            //  scale, alpha etc if needed
            transform.localScale = Vector2.one;
        }

        public void OnDespawn()
        {
            // stop tween, reset states, animations if needed
            destroyTween?.Kill();
            ResetVisual();
        }
    }
}