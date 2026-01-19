using ColorBlast.Manager;
using ColorBlast.ObjectPool;
using DG.Tweening;
using UnityEngine;

namespace ColorBlast.Blocks
{
    /// <summary>
    /// Represents a single block on the game board.
    /// Handles grid position data, visual updates, animations and pooling
    /// </summary>
    public class Block : MonoBehaviour, IPoolable
    {
        [Header("References")]
        [SerializeField] private BlockProperties blockProperties;
        [SerializeField] private BlockColorDatabase blockColorDatabase;
        [SerializeField] private SpriteRenderer blockSpriteRenderer;
        [SerializeField] private Transform blockModelTransform;

        private int gridX;
        private int gridY;
        private BlockColorType colorType;
        private BlockIconType iconType;

        private Tween destroyTween;
        private Tween moveTween;

        public int GridX => gridX;
        public int GridY => gridY;
        public BlockColorType ColorType => colorType;
        public BlockIconType IconType => iconType;

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
            this.gridX = gridX;
            this.gridY = gridY;

            UpdateOrderLayer();
        }

        public void UpdateIcon(BlockIconType iconType)
        {
            this.iconType = iconType;
            UpdateVisual();
        }

        public void UpdateColor(BlockColorType colorType)
        {
            this.colorType = colorType;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (blockSpriteRenderer != null && blockColorDatabase != null)
            {
                blockSpriteRenderer.sprite = blockColorDatabase.GetSpriteForType(colorType, iconType);
            }
        }

        private void ResetVisual()
        {
            iconType = BlockIconType.Default;
            UpdateVisual();
        }

        private void UpdateOrderLayer()
        {
            if (blockSpriteRenderer != null)
            {
                blockSpriteRenderer.sortingOrder = gridY;
            }
        }

        private void UpdateBlockScale()
        {
            if (blockModelTransform != null && blockProperties != null)
            {
                blockModelTransform.localScale = new Vector3(blockProperties.BlockSizeX, blockProperties.BlockSizeY, 1);
            }
        }

        public void MoveToPosition(Vector2 targetPosition)
        {
            moveTween?.Kill();
            moveTween = transform.DOMove(targetPosition, blockProperties.MoveDuration).SetEase(Ease.InOutCubic);
        }

        public void PlayDestroyAnimation()
        {
            destroyTween?.Kill();
            destroyTween =
                transform.DOScale(Vector2.zero, blockProperties.DestroyDuration).SetEase(Ease.InOutBounce).OnComplete(ReturnToPool);
        }

        private void ReturnToPool()
        {
            ObjectPoolManager.Instance.ReturnBlock(this);
        }

        public void OnSpawn()
        {
            //  when block has some animations after spawning
        }

        public void OnDespawn()
        {
            moveTween?.Kill();
            destroyTween?.Kill();

            moveTween = null;
            destroyTween = null;

            transform.localScale = Vector3.one;
            ResetVisual();
        }
    }
}