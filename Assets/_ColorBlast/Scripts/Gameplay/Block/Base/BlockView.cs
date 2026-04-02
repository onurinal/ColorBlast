using System;
using UnityEngine;
using DG.Tweening;

namespace ColorBlast.Gameplay
{
    public class BlockView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform modelTransform;

        private Tween activeTween;

        private void OnDestroy()
        {
            activeTween?.Kill();
        }

        public void MoveToPosition(Vector2 targetPosition, float duration)
        {
            activeTween?.Kill();
            activeTween = transform.DOMove(targetPosition, duration).SetEase(Ease.InOutCubic);
        }

        public void HandleDestroy(float duration, Action onComplete)
        {
            activeTween?.Kill();
            activeTween = transform.DOScale(Vector2.zero, duration).SetEase(Ease.InOutBounce)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void UpdateSortingOrder(int gridY)
        {
            spriteRenderer.sortingOrder = gridY;
        }

        public void UpdateVisual(Sprite sprite)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
            }
        }

        public void SetModelScale(float sizeX, float sizeY)
        {
            modelTransform.transform.localScale = new Vector2(sizeX, sizeY);
        }

        public void ResetView()
        {
            activeTween?.Kill();
            transform.localScale = Vector3.one;
        }
    }
}