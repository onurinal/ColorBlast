using UnityEngine;

namespace ColorBlast.Gameplay
{
    public class PoolableParticle : PoolableVFX
    {
        [SerializeField] private ParticleSystem particle;

        private void Awake()
        {
            if (particle == null)
            {
                particle = GetComponent<ParticleSystem>();
            }
        }

        protected override void Play()
        {
            particle?.Play();
        }

        protected override void Stop()
        {
            particle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void SetColor(Color color)
        {
            var main = particle.main;
            main.startColor = color;
        }

        public float GetParticleDuration()
        {
            var main = particle.main;

            float duration = main.duration;

            float startLifetime = 0f;

            switch (main.startLifetime.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    startLifetime = main.startLifetime.constant;
                    break;

                case ParticleSystemCurveMode.TwoConstants:
                    startLifetime = main.startLifetime.constantMax;
                    break;
            }

            return duration + startLifetime;
        }
    }
}