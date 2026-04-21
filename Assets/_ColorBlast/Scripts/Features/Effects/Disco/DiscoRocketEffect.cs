using System;
using System.Collections.Generic;
using ColorBlast.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace ColorBlast.Features
{
    /// <summary>
    /// Disco + Rocket combo:
    /// 1. Disco beam fires at every block matching TargetCubeData.
    /// 2. Each hit block is replaced with a Rocket (with spawn delay).
    /// 3. All spawned Rockets fire sequentially.
    /// </summary>
    public class DiscoRocketEffect : IBlockEffect
    {
        private readonly Block best;
        private readonly Block partner;
        private readonly HashSet<Block> affectedSpecials;
        private readonly BlockEffectFactory effectFactory;

        public Block Source { get; }

        public DiscoRocketEffect(ComboResult comboResult, BlockEffectFactory effectFactory)
        {
            Source = comboResult.Tapped;
            best = comboResult.Best;
            partner = comboResult.Partner;
            affectedSpecials = comboResult.AffectedSpecials;
            this.effectFactory = effectFactory;
        }

        public async UniTask Execute(EffectExecutionContext context, IEffectScheduler effectScheduler)
        {
            var discoBall = (DiscoBlock)(best.BlockType == BlockType.DiscoBall ? best : partner);
            var rocketData = best.BlockType == BlockType.Rocket ? best.BlockData : partner.BlockData;
            var discoData = (DiscoBlockData)discoBall.BlockData;

            var sourceRow = Source.GridX;
            var sourceCol = Source.GridY;

            foreach (var block in affectedSpecials)
            {
                if (block == best)
                {
                    context.UnlinkFromGrid(best);
                    continue;
                }

                context.TryRemoveBlock(block);
            }

            effectScheduler.SuspendGridUpdates();

            context.HapticService.PlayImpact(HapticManagement.HapticTypes.MediumImpact);
            var (shake, scale) = DiscoAnimationHelper.AnimateShakeAndScale(discoBall);
            var spawnedRockets = new List<Block>();

            try
            {
                var targetPositions = DiscoAnimationHelper.CollectPositions(context, discoBall.TargetCubeData, true);
                targetPositions.Remove(new Vector2Int(Source.GridX, Source.GridY));

                await DiscoAnimationHelper.AnimateBeams(context, targetPositions, discoBall, discoData,
                    position =>
                    {
                        var row = position.x;
                        var col = position.y;

                        context.TryRemoveBlock(context.Grid[row, col]);

                        var rocket = context.SpawnBlockAt(rocketData, row, col);
                        spawnedRockets.Add(rocket);
                    });

                shake.Kill();
                scale.Kill();

                context.ReturnToPool(best);
                var rocket = context.SpawnBlockAt(rocketData, sourceRow, sourceCol);
                spawnedRockets.Add(rocket);
            }
            finally
            {
                effectScheduler.ResumeGridUpdates();
            }

            // Fire all spawned Rockets sequentially
            foreach (var rocket in spawnedRockets)
            {
                if (effectScheduler.IsTriggered(rocket))
                {
                    continue;
                }

                effectScheduler.MarkTriggered(rocket);
                effectScheduler.TriggerConcurrent(effectFactory.CreateEffect(rocket));

                await UniTask.Delay(TimeSpan.FromSeconds(context.Config.RocketChainDelay));
            }
        }
    }
}