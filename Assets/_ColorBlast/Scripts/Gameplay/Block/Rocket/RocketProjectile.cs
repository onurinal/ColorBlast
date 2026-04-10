using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    public class RocketProjectile : PoolableParticle
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Tween activeTween;
        private RocketProjectileDirection direction;
        private readonly int maxCellDestination = 5;

        public void SetupVisual(Sprite sprite, RocketProjectileDirection direction)
        {
            spriteRenderer.sprite = sprite;
            this.direction = direction;
        }

        public async UniTask Launch(IReadOnlyList<(Vector3 worldPos, Block block)> steps, float durationPerCell,
            Action<Block> onArriveAtCell)
        {
            foreach (var (worldPos, block) in steps)
            {
                activeTween = transform.DOMove(worldPos, durationPerCell).SetEase(Ease.Linear);
                await activeTween.ToUniTask();
                onArriveAtCell?.Invoke(block);
            }

            for (int i = 0; i < maxCellDestination; i++)
            {
                var targetPos = GetTargetPosition(transform.position);
                activeTween = transform.DOMove(targetPos, durationPerCell).SetEase(Ease.Linear);
                await activeTween.ToUniTask();
            }
        }

        public override void OnDespawn()
        {
            base.OnDespawn();
            transform.position = new Vector3(0, 0, 0);
            activeTween?.Kill();
        }

        private Vector3 GetTargetPosition(Vector3 worldPos)
        {
            return direction switch
            {
                RocketProjectileDirection.Left => worldPos + (Vector3.left),
                RocketProjectileDirection.Right => worldPos + (Vector3.right),
                RocketProjectileDirection.Up => worldPos + (Vector3.up),
                RocketProjectileDirection.Down => worldPos + (Vector3.down),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}