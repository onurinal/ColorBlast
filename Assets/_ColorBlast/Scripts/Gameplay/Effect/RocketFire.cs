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
            IChainSchedular chainSchedular,
            BlockEffectFactory effectFactory)
        {
            var durationPerCell = 1f / rocketData.ProjectileSpeed;
            var spawnPos = new Vector3(originRow, originCol) * context.Config.CellUnitSize;
            var concurrentChains = new List<UniTask>();

            void OnHit(Block b) => HandleHit(b, context, chainSchedular, effectFactory, concurrentChains);

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

            // Wait for any specials that were triggered mid-flight
            if (concurrentChains.Count > 0)
            {
                await UniTask.WhenAll(concurrentChains);
            }

            ParticlePoolManager.Instance.ReturnParticle(rocketData.BlockType, projectile1);
            ParticlePoolManager.Instance.ReturnParticle(rocketData.BlockType, projectile2);
        }

        /// <summary>
        /// Special blocks fire immediately as concurrent tasks (chain reaction).
        /// Normal blocks are destroyed in place.
        /// </summary>
        private static void HandleHit(Block block, EffectExecutionContext context, IChainSchedular chainSchedular, BlockEffectFactory effectFactory,
            List<UniTask> concurrentChains)
        {
            if (block == null)
            {
                return;
            }

            if (block is IActivatable && !chainSchedular.IsTriggered(block))
            {
                chainSchedular.MarkTriggered(block);
                concurrentChains.Add(effectFactory.CreateEffect(block).Execute(context, chainSchedular));
            }
            else
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

                if (block != null)
                {
                    result.Add((new Vector3(row, col) * cellSize, block));
                }

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
            projectile.SetupVisual(sprite, direction);
            return projectile;
        }
    }
}