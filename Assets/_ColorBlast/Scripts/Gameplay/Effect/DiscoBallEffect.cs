using System.Collections.Generic;
using ColorBlast.Manager;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    public class DiscoBallEffect : IBlockEffect
    {
        public Block Tapped { get; }
        private readonly BlockEffectFactory effectFactory;

        public DiscoBallEffect(Block source, BlockEffectFactory effectFactory)
        {
            Tapped = source;
            this.effectFactory = effectFactory;
        }

        public async UniTask Execute(EffectExecutionContext context, IEffectSchedular effectSchedular)
        {
            var discoBall = (DiscoBlock)Tapped;
            var discoData = (DiscoBlockData)discoBall.BlockData;

            effectSchedular.SuspendGridUpdates();
            discoBall.PlayParticle();

            var (shakeTween, scaleTween) = AnimateShakeAndScale(discoBall);
            var activeBeams = new List<DiscoBallBeam>();

            try
            {
                var affected = CollectTargetColor(context, discoBall.TargetCubeData);

                await AnimateDiscoBeam(affected, discoBall, activeBeams, discoData);

                shakeTween.Kill();
                scaleTween.Kill();

                TryDestroyAffected(context, affected, discoBall);
            }
            finally
            {
                effectSchedular.ResumeGridUpdates();
            }
        }

        private static (Tweener shakeTween, Tweener scaleTween) AnimateShakeAndScale(DiscoBlock discoBall)
        {
            var shakeTween = discoBall.transform.DOShakePosition(1f, strength: 0.1f, vibrato: 5, randomness: 90)
                .SetLoops(-1);

            var scaleTween =
                discoBall.transform.DOScale(discoBall.transform.localScale + Vector3.one * 0.2f, 0.2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            return (shakeTween, scaleTween);
        }

        private static List<Block> CollectTargetColor(EffectExecutionContext context, BlockData targetData)
        {
            var affected = new List<Block>();

            for (int row = 0; row < context.LevelProperties.RowCount; row++)
            {
                for (int col = 0; col < context.LevelProperties.ColumnCount; col++)
                {
                    if (context.BlockGrid[row, col] != null && context.BlockGrid[row, col].BlockData == targetData)
                    {
                        affected.Add(context.BlockGrid[row, col]);
                    }
                }
            }

            return affected;
        }

        private static async UniTask AnimateDiscoBeam(List<Block> affected, DiscoBlock discoBall, List<DiscoBallBeam> activeBeams, DiscoBlockData discoData)
        {
            foreach (var block in affected)
            {
                if (block.IsBusy)
                {
                    continue;
                }

                var vfx = ParticlePoolManager.Instance.GetParticle(discoBall.BlockData);
                if (vfx is DiscoBallBeam discoBeam)
                {
                    activeBeams.Add(discoBeam);
                    await discoBeam.AnimateLine(discoBall.transform.position, block.transform.position, discoData.LineAnimateDuration);
                }
            }

            foreach (var beam in activeBeams)
            {
                ParticlePoolManager.Instance.ReturnParticle(discoBall.BlockData.BlockType, beam);
            }
        }

        private static void TryDestroyAffected(EffectExecutionContext context, List<Block> affected, DiscoBlock discoBall)
        {
            affected.Add(discoBall);
            foreach (var block in affected)
            {
                context.TryDestroyBlock(block);
            }
        }
    }
}