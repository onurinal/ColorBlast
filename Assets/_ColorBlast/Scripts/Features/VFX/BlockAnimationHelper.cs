using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace ColorBlast.Features
{
    public static class BlockAnimationHelper
    {
        private const float SpreadDistance = 0.2f;

        public static async UniTask PlayMergeAnimation(HashSet<Block> group, Block targetBlock, float duration)
        {
            var sequence = DOTween.Sequence();
            var targetPosition = targetBlock.transform.position;

            var spreadDuration = duration * 0.3f;
            var mergeDuration = duration * 0.7f;

            foreach (var block in group)
            {
                block.SetSortingOrder(99);

                if (block == targetBlock)
                {
                    _ = sequence.Insert(0, block.transform.DOScale(1.1f, spreadDuration).SetEase(Ease.OutQuad));
                    _ = sequence.Insert(spreadDuration, block.transform.DOScale(1f, mergeDuration).SetEase(Ease.InQuad));
                    continue;
                }

                Vector3 originalPosition = block.transform.position;

                Vector3 direction = originalPosition - targetPosition;
                Vector3 spreadPosition = originalPosition + (direction * SpreadDistance);

                _ = sequence.Insert(0, block.transform.DOMove(spreadPosition, spreadDuration).SetEase(Ease.OutQuad));
                _ = sequence.Insert(spreadDuration, block.transform.DOMove(targetPosition, mergeDuration).SetEase(Ease.InQuint));
            }

            await sequence.Play().ToUniTask();
        }

        public static async UniTask PlayExpandAnimation(Block block, float scaleMultiplier = 4f, float duration = 1.5f)
        {
            if (block == null)
            {
                return;
            }

            block.SetSortingOrder(100);

            Transform blockTransform = block.transform;
            Vector3 initialScale = blockTransform.localScale;
            Vector3 targetScale = initialScale * scaleMultiplier;

            await DOTween.Sequence()
                .Append(blockTransform.DOScale(targetScale, duration).SetEase(Ease.OutBack))
                .ToUniTask();
        }

        /// <summary>
        /// Merges the bomb and rocket to the tapped center, then orbits them around a vertical axis.
        /// Starts slow and accelerates.
        /// </summary>
        public static async UniTask PlayBombRocketComboAnimation(Block bomb, Block rocket, Vector3 center, float duration = 1.5f)
        {
            if (bomb == null || rocket == null) return;

            bomb.SetSortingOrder(101);
            rocket.SetSortingOrder(100);

            const float startTiltDeg = 30f;
            const float endTiltDeg = 0f;
            const float orbitRadius = 0.45f;
            const int totalSpins = 10;
            const float depthScaleVariance = 0.12f;

            float t = 0f;

            // We use Ease.Linear to ensure the speed is constant from start to finish
            await DOTween.To(() => t, x =>
                {
                    t = x;

                    // 1. Calculate dynamic tilt
                    float currentTiltRad = Mathf.Lerp(startTiltDeg, endTiltDeg, t) * Mathf.Deg2Rad;

                    // 2. Calculate rotation angle (Now linear because t is linear)
                    float currentAngle = t * totalSpins * Mathf.PI * 2f;

                    // 3. Position Calculation
                    float rawOffset = Mathf.Sin(currentAngle) * orbitRadius;
                    float xPos = rawOffset * Mathf.Cos(currentTiltRad);
                    float yPos = rawOffset * Mathf.Sin(currentTiltRad);

                    Vector3 offsetVector = new Vector3(xPos, yPos, 0);

                    bomb.transform.position = center + offsetVector;
                    rocket.transform.position = center - offsetVector;

                    // 4. Fake 3D Depth Scaling
                    float bombDepth = Mathf.Cos(currentAngle);
                    float rocketDepth = Mathf.Cos(currentAngle + Mathf.PI);

                    bomb.transform.localScale = Vector3.one * (1f + (bombDepth * depthScaleVariance));
                    rocket.transform.localScale = Vector3.one * (1f + (rocketDepth * depthScaleVariance));
                }, 1f, duration)
                .SetEase(Ease.Linear)
                .ToUniTask();
        }
    }
}