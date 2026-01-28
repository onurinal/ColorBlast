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
        [SerializeField] private SpriteRenderer blockSpriteRenderer;
        [SerializeField] private Transform blockModelTransform;

        private Tween destroyTween;
        private Tween moveTween;

        public BlockColorData BlockColorData { get; private set; }

        public int CurrentGroupSize { get; private set; }
        public int GridX { get; private set; }

        public int GridY { get; private set; }

        private void Awake()
        {
            UpdateBlockScale();
        }

        public void Initialize(int gridX, int gridY, BlockColorData blockColorData)
        {
            SetGridPosition(gridX, gridY);
            BlockColorData = blockColorData;
            UpdateVisual();
        }

        public void SetGridPosition(int gridX, int gridY)
        {
            GridX = gridX;
            GridY = gridY;
        }

        public void UpdateIcon(int groupSize)
        {
            CurrentGroupSize = groupSize;
            UpdateVisual();
        }

        public void UpdateColor(BlockColorData newColorData)
        {
            BlockColorData = newColorData;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (blockSpriteRenderer != null)
            {
                blockSpriteRenderer.sprite = BlockColorData.GetVisual(CurrentGroupSize);
            }
        }

        private void ResetVisual()
        {
            CurrentGroupSize = 0;
            UpdateVisual();
        }

        private void UpdateOrderLayer()
        {
            if (blockSpriteRenderer != null)
            {
                blockSpriteRenderer.sortingOrder = GridY;
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
            moveTween = transform.DOMove(targetPosition, blockProperties.MoveDuration).SetEase(Ease.InOutCubic).OnComplete(UpdateOrderLayer);
        }

        public void PlayDestroyAnimation()
        {
            destroyTween?.Kill();
            destroyTween =
                transform.DOScale(Vector2.zero, blockProperties.DestroyDuration).SetEase(Ease.InOutBounce).OnComplete(ReturnToPool);
        }

        private void ReturnToPool()
        {
            moveTween?.Kill();
            destroyTween?.Kill();
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