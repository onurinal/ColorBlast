using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace ColorBlast.Gameplay
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
                    sequence.Insert(0, block.transform.DOScale(1.1f, spreadDuration).SetEase(Ease.OutQuad));
                    sequence.Insert(spreadDuration, block.transform.DOScale(1f, mergeDuration).SetEase(Ease.InQuad));
                    continue;
                }

                Vector3 originalPosition = block.transform.position;

                Vector3 direction = originalPosition - targetPosition;
                Vector3 spreadPosition = originalPosition + (direction * SpreadDistance);

                sequence.Insert(0, block.transform.DOMove(spreadPosition, spreadDuration).SetEase(Ease.OutQuad));
                sequence.Insert(spreadDuration, block.transform.DOMove(targetPosition, mergeDuration).SetEase(Ease.InQuint));
            }

            await sequence.Play().ToUniTask();
        }
    }
}