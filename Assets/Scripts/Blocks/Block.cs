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

        public void Initialize(int gridX, int gridY, BlockColorType colorType)
        {
            SetIndices(gridX, gridY);
            this.colorType = colorType;
            iconType = BlockIconType.Default;

            UpdateBlockScale();
            UpdateVisual();
            UpdateOrderLayer();
        }

        public void SetIndices(int gridX, int gridY)
        {
            this.gridX = gridX;
            this.gridY = gridY;
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

        private void UpdateBlockScale()
        {
            blockModelTransform.localScale = new Vector3(blockProperties.BlockSizeX, blockProperties.BlockSizeY, 1);
        }

        public void Destroy()
        {
            destroyTween =
                transform.DOScale(new Vector2(0f, 0f), blockProperties.DestroyDuration).SetEase(Ease.InOutBounce).OnComplete(() => ObjectPoolManager.Instance.ReturnBlock(this));
        }

        public void OnSpawn()
        {
            //  scale, alpha etc if needed
        }

        public void OnDespawn()
        {
            // stop tween, reset states, animations if needed
            ResetVisual();
            destroyTween?.Kill();
        }
    }
}