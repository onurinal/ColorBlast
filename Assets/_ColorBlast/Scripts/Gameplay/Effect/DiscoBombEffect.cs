using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    /// <summary>
    /// Disco + Bomb combo:
    /// 1. Disco beam fires at every block matching TargetCubeData.
    /// 2. Each hit block is replaced with a Bomb (with spawn delay).
    /// 3. All spawned Bombs detonate sequentially.
    /// </summary>
    public class DiscoBombEffect : IBlockEffect
    {
        private readonly Block best;
        private readonly Block partner;
        private readonly HashSet<Block> affectedSpecials;
        private readonly BlockEffectFactory effectFactory;

        public Block Source { get; }

        public DiscoBombEffect(ComboResult comboResult, BlockEffectFactory effectFactory)
        {
            Source = comboResult.Tapped;
            best = comboResult.Best;
            partner = comboResult.Partner;
            affectedSpecials = comboResult.AffectedSpecials;
            this.effectFactory = effectFactory;
        }

        public async UniTask Execute(EffectExecutionContext context, IEffectSchedular effectSchedular)
        {
            var discoBall = (DiscoBlock)(best.BlockType == BlockType.DiscoBall ? best : partner);
            var bombData = best.BlockType == BlockType.Bomb ? best.BlockData : partner.BlockData;
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

            effectSchedular.SuspendGridUpdates();
            discoBall.PlayParticle();

            var (shake, scale) = DiscoAnimationHelper.AnimateShakeAndScale(discoBall);
            var spawnedBombs = new List<Block>();

            try
            {
                var targetPositions = DiscoAnimationHelper.CollectPositions(context, discoBall.TargetCubeData, true);
                targetPositions.Remove(new Vector2Int(Source.GridX, Source.GridY));

                await DiscoAnimationHelper.AnimateBeams(context, targetPositions, discoBall, discoData,
                    position =>
                    {
                        var row = position.x;
                        var col = position.y;

                        context.TryRemoveBlock(context.BlockGrid[row, col]);

                        var bomb = context.SpawnBlockAt(bombData, row, col);
                        spawnedBombs.Add(bomb);
                    });

                shake.Kill();
                scale.Kill();

                context.ReturnToPool(best);
                var bomb = context.SpawnBlockAt(bombData, sourceRow, sourceCol);
                spawnedBombs.Add(bomb);
            }
            finally
            {
                effectSchedular.ResumeGridUpdates();
            }

            foreach (var bomb in spawnedBombs)
            {
                if (effectSchedular.IsTriggered(bomb))
                {
                    continue;
                }

                effectSchedular.MarkTriggered(bomb);
                effectSchedular.TriggerConcurrent(effectFactory.CreateEffect(bomb));

                await UniTask.Delay(TimeSpan.FromSeconds(context.Config.BombChainDelay));
            }
        }
    }
}