using System.Collections.Generic;
using ColorBlast.Manager;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ColorBlast.Features
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
            IEffectScheduler effectScheduler,
            BlockEffectFactory effectFactory)
        {
            var durationPerCell = 1f / rocketData.ProjectileSpeed;
            var spawnPos = new Vector3(originRow, originCol) * context.Config.CellUnitSize;

            void OnHit(Block b) => HandleHit(b, context, effectScheduler, effectFactory);

            List<(Vector3, Block)> targetsA, targetsB;
            RocketProjectile projectileA, projectileB;

            if (direction == RocketDirection.Horizontal)
            {
                targetsA = CollectTargets(context, originRow, originCol, -1, 0);
                targetsB = CollectTargets(context, originRow, originCol, 1, 0);
                projectileA = SpawnProjectile(rocketData, rocketData.LeftRocketSprite, spawnPos, RocketProjectileDirection.Left);
                projectileB = SpawnProjectile(rocketData, rocketData.RightRocketSprite, spawnPos, RocketProjectileDirection.Right);
            }
            else
            {
                targetsA = CollectTargets(context, originRow, originCol, 0, 1);
                targetsB = CollectTargets(context, originRow, originCol, 0, -1);
                projectileA = SpawnProjectile(rocketData, rocketData.UpRocketSprite, spawnPos, RocketProjectileDirection.Up);
                projectileB = SpawnProjectile(rocketData, rocketData.DownRocketSprite, spawnPos, RocketProjectileDirection.Down);
            }

            if (projectileA == null || projectileB == null)
            {
                Debug.LogError("[RocketFire] Failed to spawn one or both projectiles.");
                return;
            }

            await UniTask.WhenAll(
                projectileA.Launch(targetsA, durationPerCell, OnHit),
                projectileB.Launch(targetsB, durationPerCell, OnHit)
            );
        }

        /// <summary>
        /// Special blocks fire immediately as concurrent tasks (chain reaction).
        /// Normal blocks are destroyed in place.
        /// </summary>
        private static void HandleHit(Block block, EffectExecutionContext context, IEffectScheduler effectScheduler, BlockEffectFactory effectFactory)
        {
            EffectUtility.TriggerOrDestroy(block, context, effectScheduler, effectFactory);
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
                var block = context.Grid[row, col];

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