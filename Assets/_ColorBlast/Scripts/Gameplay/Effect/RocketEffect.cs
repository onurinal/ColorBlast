using System.Collections.Generic;
using ColorBlast.Manager;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    public class RocketEffect : IBlockEffect
    {
        public Block Tapped { get; }
        private readonly BlockEffectFactory effectFactory;

        public RocketEffect(Block source, BlockEffectFactory effectFactory)
        {
            Tapped = source;
            this.effectFactory = effectFactory;
        }

        public async UniTask Execute(EffectExecutionContext context, IChainSchedular chainSchedular)
        {
            var rocket = (RocketBlock)Tapped;
            chainSchedular.MarkTriggered(Tapped);

            var rocketData = rocket.RocketBlockData;
            var spawnPos = rocket.transform.position;
            var durationPerCell = 1 / rocketData.ProjectileSpeed;

            List<(Vector3 worldPos, Block block)> firstTargets, secondTargets;
            RocketProjectile projectile1, projectile2;

            if (rocket.Direction == RocketDirection.Horizontal)
            {
                firstTargets = CollectTargets(context, rocket.GridX, rocket.GridY, -1, 0);
                secondTargets = CollectTargets(context, rocket.GridX, rocket.GridY, 1, 0);
                projectile1 = SpawnRocketProjectile(rocketData, rocketData.LeftRocketSprite, spawnPos, RocketProjectileDirection.Left);
                projectile2 = SpawnRocketProjectile(rocketData, rocketData.RightRocketSprite, spawnPos, RocketProjectileDirection.Right);
            }
            else
            {
                firstTargets = CollectTargets(context, rocket.GridX, rocket.GridY, 0, 1);
                secondTargets = CollectTargets(context, rocket.GridX, rocket.GridY, 0, -1);
                projectile1 = SpawnRocketProjectile(rocketData, rocketData.UpRocketSprite, spawnPos, RocketProjectileDirection.Up);
                projectile2 = SpawnRocketProjectile(rocketData, rocketData.DownRocketSprite, spawnPos, RocketProjectileDirection.Down);
            }

            context.TryRemoveBlock(rocket);
            await UniTask.WhenAll(
                projectile1.Launch(firstTargets, durationPerCell, (block) => OnHitBlock(block, context, chainSchedular)),
                projectile2.Launch(secondTargets, durationPerCell, (block) => OnHitBlock(block, context, chainSchedular))
            );

            ParticlePoolManager.Instance.ReturnParticle(rocket, projectile1);
            ParticlePoolManager.Instance.ReturnParticle(rocket, projectile2);
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
                    var worldPos = new Vector3(row, col) * cellSize;
                    result.Add((worldPos, block));
                }

                row += offRow;
                col += offCol;
            }

            return result;
        }

        private void OnHitBlock(Block block, EffectExecutionContext context, IChainSchedular chainSchedular)
        {
            if (block == null)
            {
                return;
            }

            if (block is IActivatable && !chainSchedular.IsTriggered(block))
            {
                var chainedEffect = effectFactory.CreateEffect(block);
                chainSchedular.EnqueueChained(chainedEffect);
            }
            else
            {
                context.TryDestroyBlock(block);
            }
        }

        private static RocketProjectile SpawnRocketProjectile(RocketBlockData data, Sprite sprite, Vector3 position, RocketProjectileDirection direction)
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