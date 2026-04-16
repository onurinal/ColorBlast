using System.Collections.Generic;
using ColorBlast.Manager;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    /// <summary>
    /// Core rocket line-firing logic shared across RocketEffect, RocketRocketEffect and BombRocketEffect.
    /// Special blocks hit mid-flight are triggered immediately as concurrent tasks,
    /// </summary>
    public static class RocketFire
    {
        public static async UniTask Execute(
            int originRow, int originCol,
            RocketDirection direction,
            RocketBlockData rocketData,
            EffectExecutionContext context,
            IEffectSchedular effectSchedular,
            BlockEffectFactory effectFactory)
        {
            var durationPerCell = 1f / rocketData.ProjectileSpeed;
            var spawnPos = new Vector3(originRow, originCol) * context.Config.CellUnitSize;

            void OnHit(Block b) => HandleHit(b, context, effectSchedular, effectFactory);

            List<(Vector3, Block)> targets1, targets2;
            RocketProjectile projectile1, projectile2;

            if (direction == RocketDirection.Horizontal)
            {
                targets1 = CollectTargets(context, originRow, originCol, -1, 0);
                targets2 = CollectTargets(context, originRow, originCol, 1, 0);
                projectile1 = SpawnProjectile(rocketData, rocketData.LeftRocketSprite, spawnPos, RocketProjectileDirection.Left);
                projectile2 = SpawnProjectile(rocketData, rocketData.RightRocketSprite, spawnPos, RocketProjectileDirection.Right);
            }
            else
            {
                targets1 = CollectTargets(context, originRow, originCol, 0, 1);
                targets2 = CollectTargets(context, originRow, originCol, 0, -1);
                projectile1 = SpawnProjectile(rocketData, rocketData.UpRocketSprite, spawnPos, RocketProjectileDirection.Up);
                projectile2 = SpawnProjectile(rocketData, rocketData.DownRocketSprite, spawnPos, RocketProjectileDirection.Down);
            }

            await UniTask.WhenAll(
                projectile1.Launch(targets1, durationPerCell, OnHit),
                projectile2.Launch(targets2, durationPerCell, OnHit)
            );

            // projectile1.FlyOffScreenAndReturn(durationPerCell, rocketData.BlockType).Forget();
            // projectile2.FlyOffScreenAndReturn(durationPerCell, rocketData.BlockType).Forget();
        }

        /// <summary>
        /// Special blocks fire immediately as concurrent tasks (chain reaction).
        /// Normal blocks are destroyed in place.
        /// </summary>
        private static void HandleHit(Block block, EffectExecutionContext context, IEffectSchedular effectSchedular, BlockEffectFactory effectFactory)
        {
            if (block == null)
            {
                return;
            }

            if (block is IActivatable && !effectSchedular.IsTriggered(block))
            {
                effectSchedular.MarkTriggered(block);
                effectSchedular.TriggerConcurrent(effectFactory.CreateEffect(block));
            }
            else if(block is not IActivatable)
            {
                context.TryDestroyBlock(block);
            }
        }

        /// <summary>
        /// Collects (worldPos, block) pairs in a direction from center outward.
        /// </summary>
        private static List<(Vector3 worldPos, Block block)> CollectTargets(EffectExecutionContext context, int startRow, int startCol, int offRow, int offCol)
        {
            var result = new List<(Vector3 worldPos, Block block)>();
            var row = startRow + offRow;
            var col = startCol + offCol;
            float cellSize = context.Config.CellUnitSize;

            while (context.IsInBounds(row, col))
            {
                var block = context.BlockGrid[row, col];

                result.Add((new Vector3(row, col) * cellSize, block));

                row += offRow;
                col += offCol;
            }

            return result;
        }

        private static RocketProjectile SpawnProjectile(RocketBlockData data, Sprite sprite, Vector3 position, RocketProjectileDirection direction)
        {
            var projectile = ParticlePoolManager.Instance.GetParticle(data) as RocketProjectile;

            if (projectile == null)
            {
                return null;
            }

            projectile.transform.position = position;
            projectile.SetupVisual(sprite, direction, data.BlockType);
            return projectile;
        }
    }
}