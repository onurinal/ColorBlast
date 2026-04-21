using System;
using System.Collections.Generic;
using ColorBlast.Manager;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace ColorBlast.Features
{
    public class RocketProjectile : PoolableParticle
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private BlockType blockType;
        private RocketProjectileDirection direction;
        private readonly int maxCellDestination = 10;

        public void SetupVisual(Sprite sprite, RocketProjectileDirection direction, BlockType blockType)
        {
            spriteRenderer.sprite = sprite;
            this.direction = direction;
            this.blockType = blockType;
        }

        public async UniTask Launch(IReadOnlyList<(Vector3 worldPos, Block block)> steps, float durationPerCell,
            Action<Block> onArriveAtCell)
        {
            foreach (var (worldPos, block) in steps)
            {
                var activeTween = transform.DOMove(worldPos, durationPerCell).SetEase(Ease.Linear);
                await activeTween.ToUniTask();
                onArriveAtCell?.Invoke(block);
            }

            FlyOffScreenAndReturn(durationPerCell).Forget();
        }

        public override void OnDespawn()
        {
            base.OnDespawn();

            DOTween.Kill(transform);
            transform.position = Vector3.zero;
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

        /// <summary>
        /// Continues flying off-screen then returns itself to the pool.
        /// Call as fire-and-forget after LaunchImpact completes.
        /// </summary>
        private async UniTaskVoid FlyOffScreenAndReturn(float durationPerCell)
        {
            for (int i = 0; i < maxCellDestination; i++)
            {
                var targetPos = GetTargetPosition(transform.position);
                var activeTween = transform.DOMove(targetPos, durationPerCell).SetEase(Ease.Linear);
                await activeTween.ToUniTask();
            }

            ParticlePoolManager.Instance.ReturnParticle(blockType, this);
        }
    }
}