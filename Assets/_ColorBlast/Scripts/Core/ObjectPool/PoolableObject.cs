using System;
using UnityEngine;
using System.Collections.Generic;

namespace ColorBlast.Core
{
    public class PoolableObject<T> where T : MonoBehaviour
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Queue<T> pool;
        private readonly Action<T> setupAction;

        public PoolableObject(T prefab, int initialPoolSize, Transform parent, Action<T> setupAction = null)
        {
            this.prefab = prefab;
            this.parent = parent;
            this.setupAction = setupAction;
            pool = new Queue<T>(initialPoolSize);

            for (int i = 0; i < initialPoolSize; i++)
            {
                var obj = UnityEngine.Object.Instantiate(prefab, parent);
                setupAction?.Invoke(obj);
                obj.gameObject.SetActive(false);
                pool.Enqueue(obj);
            }
        }

        public T Get()
        {
            if (pool.Count == 0)
            {
                CreateNewObject();
            }

            var obj = pool.Dequeue();
            obj.gameObject.SetActive(true);

            if (obj is IPoolable poolable)
            {
                poolable.OnSpawn();
            }

            return obj;
        }

        public void Return(T obj)
        {
            if (obj is IPoolable poolable)
            {
                poolable.OnDespawn();
            }

            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }

        private void CreateNewObject()
        {
            var obj = UnityEngine.Object.Instantiate(prefab, parent);
            setupAction?.Invoke(obj);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }
}