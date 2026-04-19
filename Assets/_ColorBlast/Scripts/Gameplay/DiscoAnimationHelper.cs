using System;
using System.Collections.Generic;
using ColorBlast.Manager;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    /// <summary>
    /// Shared animation and collection utilities for all disco-based effects.
    /// </summary>
    public static class DiscoAnimationHelper
    {
        public static (Tweener shake, Tweener scale) AnimateShakeAndScale(DiscoBlock discoBall)
        {
            var shake = discoBall.transform
                .DOShakePosition(1f, strength: 0.1f, vibrato: 5, randomness: 90)
                .SetLoops(-1);

            var scale = discoBall.transform
                .DOScale(discoBall.transform.localScale + Vector3.one * 0.2f, 0.2f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);

            return (shake, scale);
        }

        public static List<Vector2Int> CollectPositions(EffectExecutionContext context, BlockData targetData, bool isCombo = false)
        {
            var affected = new List<Vector2Int>();

            for (int col = context.LevelProperties.ColumnCount - 1; col >= 0; col--)
            {
                for (int row = 0; row < context.LevelProperties.RowCount; row++)
                {
                    var block = context.BlockGrid[row, col];

                    if (!isCombo)
                    {
                        if (block != null && block.BlockData == targetData)
                        {
                            affected.Add(new Vector2Int(row, col));
                        }
                    }
                    else
                    {
                        if (block == null || block.BlockData == targetData)
                        {
                            affected.Add(new Vector2Int(row, col));
                        }
                    }
                }
            }

            return affected;
        }

        /// <summary>
        /// Shoots a beam from the disco ball to each target block sequentially.
        /// Returns beams to the pool when finished.
        /// </summary>
        public static async UniTask AnimateBeams(EffectExecutionContext context, List<Vector2Int> targets, DiscoBlock discoBall, DiscoBlockData discoData,
            Action<Vector2Int> onBeamArrived = null)
        {
            var activeBeams = new List<DiscoBallBeam>();

            foreach (var position in targets)
            {
                var block = context.BlockGrid[position.x, position.y];

                if (block != null && block.IsBusy)
                {
                    continue;
                }

                var worldPos = context.GetCellWorldPosition(position.x, position.y);

                var vfx = ParticlePoolManager.Instance.GetParticle(discoBall.BlockData);
                if (vfx is DiscoBallBeam beam)
                {
                    activeBeams.Add(beam);
                    await beam.AnimateLine(discoBall.transform.position, worldPos, discoData.LineAnimateDuration);
                }

                onBeamArrived?.Invoke(position);
            }

            foreach (var beam in activeBeams)
            {
                ParticlePoolManager.Instance.ReturnParticle(discoBall.BlockData.BlockType, beam);
            }
        }
    }
}