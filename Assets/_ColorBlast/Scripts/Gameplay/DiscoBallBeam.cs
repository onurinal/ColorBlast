using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    public class DiscoBallBeam : PoolableVFX
    {
        [SerializeField] private LineRenderer lineRenderer;
        private Tween activeTween;

        private readonly float minWidth = 0.04f;
        private readonly float maxWidth = 0.1f;

        private void Awake()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }
        }

        protected override void Play()
        {
            ResetVFX();
        }

        protected override void Stop()
        {
            activeTween?.Kill();
        }

        public async UniTask AnimateLine(Vector3 from, Vector3 to, float duration)
        {
            lineRenderer.SetPosition(0, from);
            lineRenderer.SetPosition(1, from);

            activeTween = DOTween.To(() => from, x => lineRenderer.SetPosition(1, x), to, duration).SetEase(Ease.Linear);
            await activeTween.ToUniTask();

            StartLoopingFadedEffect();
        }

        private void StartLoopingFadedEffect()
        {
            activeTween.Kill();

            activeTween = DOTween.To(() => minWidth, x => lineRenderer.startWidth = x, maxWidth, 0.2f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }

        private void ResetVFX()
        {
            activeTween.Kill();
        }
    }
}