using System;
using UnityEngine;
using DG.Tweening;

namespace ColorBlast.Gameplay
{
    public class BlockView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform modelTransform;

        private Tween moveTween;
        private Tween destroyTween;

        private void OnDestroy()
        {
            moveTween?.Kill();
            destroyTween?.Kill();
        }

        public void SetBlockScale(float scaleX, float scaleY)
        {
            modelTransform.localScale = new Vector3(scaleX, scaleY, 1);
        }

        public void UpdateVisual(Sprite sprite)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
            }
        }

        public void UpdateSortingOrder(int gridY)
        {
            spriteRenderer.sortingOrder = gridY;
        }

        public void ResetView()
        {
            moveTween?.Kill();
            destroyTween?.Kill();
            transform.localScale = Vector3.one;
        }

        public void PlayMoveAnim(Vector2 targetPosition, float duration, Action onComplete)
        {
            moveTween?.Kill();
            moveTween = transform.DOMove(targetPosition, duration).SetEase(Ease.InOutCubic)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void PlayDestroyAnim(float duration, Action onComplete)
        {
            destroyTween?.Kill();
            destroyTween = transform.DOScale(Vector2.zero, duration).SetEase(Ease.InOutBounce)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void SetVisible(bool visible) => spriteRenderer.enabled = visible;
        public bool IsVisible() => spriteRenderer.enabled;
    }
}