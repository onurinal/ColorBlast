using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    public class DiscoBallEffect : IBlockEffect
    {
        public Block Source { get; }

        public DiscoBallEffect(Block source)
        {
            Source = source;
        }

        public async UniTask Execute(EffectExecutionContext context, IEffectSchedular effectSchedular)
        {
            var discoBall = (DiscoBlock)Source;
            var discoData = (DiscoBlockData)discoBall.BlockData;

            effectSchedular.SuspendGridUpdates();
            discoBall.PlayParticle();

            var (shake, scale) = DiscoAnimationHelper.AnimateShakeAndScale(discoBall);

            try
            {
                var targetPositions = DiscoAnimationHelper.CollectPositions(context, discoBall.TargetCubeData);
                await DiscoAnimationHelper.AnimateBeams(context, targetPositions, discoBall, discoData);

                shake.Kill();
                scale.Kill();

                targetPositions.Add(new Vector2Int(discoBall.GridX, discoBall.GridY));

                foreach (var position in targetPositions)
                {
                    var block = context.BlockGrid[position.x, position.y];

                    if (block == null)
                    {
                        continue;
                    }

                    context.TryDestroyBlock(block);
                }
            }
            finally
            {
                effectSchedular.ResumeGridUpdates();
            }
        }
    }
}