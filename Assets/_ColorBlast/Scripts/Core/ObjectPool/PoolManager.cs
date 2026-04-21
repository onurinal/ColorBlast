using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlast.Core
{
    /// <summary>
    /// Generic singleton pool manager. Subclasses register pools during InitializePool()
    /// and inherit thread-safe Get/Return with consistent error logging.
    /// </summary>
    public class PoolManager<TKey, TObject> : MonoBehaviour where TObject : MonoBehaviour
    {
        private readonly Dictionary<TKey, PoolableObject<TObject>> pools = new();
        protected bool IsInitialized;

        protected void CreatePool(TKey key, TObject prefab, int size, Transform parent,
            Action<TObject> onSetup = null)
        {
            if (pools.ContainsKey(key))
            {
                Debug.LogWarning($"[{GetType().Name}] Pool already exists for key: {key}. Skipping.");
                return;
            }

            pools[key] = new PoolableObject<TObject>(prefab, size, parent, onSetup);
        }

        protected TObject Get(TKey key)
        {
            if (!TryGetPool(key, out var pool))
            {
                return null;
            }

            return pool.Get();
        }

        protected void Return(TKey key, TObject obj)
        {
            if (obj == null)
            {
                return;
            }

            if (!TryGetPool(key, out var pool))
            {
                return;
            }

            pool.Return(obj);
        }

        private bool TryGetPool(TKey key, out PoolableObject<TObject> pool)
        {
            if (!IsInitialized)
            {
                Debug.LogError($"[{GetType().Name}] Attempted pool access before initialization.");
                pool = null;
                return false;
            }

            if (!pools.TryGetValue(key, out pool))
            {
                Debug.LogError($"[{GetType().Name}] No pool registered for key: {key}");
                return false;
            }

            return true;
        }
    }
}