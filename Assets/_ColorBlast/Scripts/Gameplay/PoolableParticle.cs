using ColorBlast.Core;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    public class PoolableParticle : MonoBehaviour, IPoolable
    {
        [SerializeField] private ParticleSystem particle;

        private void Awake()
        {
            if (particle == null)
            {
                particle = GetComponent<ParticleSystem>();
            }
        }

        public void OnSpawn()
        {
            if (particle != null)
            {
                particle.Play();
            }
        }

        public virtual void OnDespawn()
        {
            if (particle != null)
            {
                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        public void SetColor(Color color)
        {
            var main = particle.main;
            main.startColor = color;
        }

        public float GetParticleDuration()
        {
            var main = particle.main;

            // float duration = main.duration;

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

            return startLifetime;
        }
    }
}