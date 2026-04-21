using ColorBlast.Core;
using UnityEngine;

namespace ColorBlast.Features
{
    public abstract class PoolableVFX : MonoBehaviour, IPoolable
    {
        protected abstract void Play();
        protected abstract void Stop();

        public virtual void OnSpawn()
        {
            Play();
        }

        public virtual void OnDespawn()
        {
            Stop();
        }
    }
}