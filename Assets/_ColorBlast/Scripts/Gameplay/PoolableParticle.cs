using ColorBlast.Core;
using UnityEngine;

namespace ColorBlast._ColorBlast.Scripts.Gameplay
{
    public class PoolableParticle : MonoBehaviour, IPoolable
    {
        private ParticleSystem particle;

        private void Awake()
        {
            particle = GetComponent<ParticleSystem>();
        }

        public void OnSpawn()
        {
            particle.Play();
        }

        public void OnDespawn()
        {
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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

                case ParticleSystemCurveMode.Curve:
                    startLifetime = main.startLifetime.curve.Evaluate(1f);
                    break;

                case ParticleSystemCurveMode.TwoCurves:
                    startLifetime = main.startLifetime.curveMax.Evaluate(1f);
                    break;
            }

            return duration + startLifetime;
        }
    }
}