using System.Collections.Generic;
using UnityEngine;

namespace ColorBlast.Core
{
    public class PoolableObject<T> where T : MonoBehaviour
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Queue<T> pool;

        public PoolableObject(T prefab, int initialPoolSize, Transform parent)
        {
            this.prefab = prefab;
            this.parent = parent;
            pool = new Queue<T>(initialPoolSize);

            for (int i = 0; i < initialPoolSize; i++)
            {
                var obj = Object.Instantiate(prefab, parent);
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
            var obj = Object.Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }
}