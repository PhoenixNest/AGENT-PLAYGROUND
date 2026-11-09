using System.Collections.Generic;
using UnityEngine;

namespace Lean.Pool
{
    public static class LeanPool
    {
        public static readonly Dictionary<GameObject, LeanGameObjectPool> Links = new Dictionary<GameObject, LeanGameObjectPool>();

        private static LeanGameObjectPool GetPool(GameObject prefab)
        {
            var pool = LeanGameObjectPool.FindPoolForPrefab(prefab);
            return pool != null ? pool : LeanGameObjectPool.CreatePool(prefab);
        }

        public static GameObject Spawn(GameObject prefab)
        {
            if (prefab == null) return null;
            return GetPool(prefab).Spawn();
        }

        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null) return null;
            return GetPool(prefab).Spawn(position, rotation, parent);
        }

        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
        {
            if (prefab == null) return null;
            var clone = Spawn(prefab.gameObject, position, rotation, parent);
            return clone != null ? clone.GetComponent<T>() : null;
        }

        public static void Despawn(GameObject clone, float delay = 0.0f)
        {
            if (clone == null) return;

            LeanGameObjectPool pool;
            if (Links.TryGetValue(clone, out pool) && pool != null)
            {
                pool.Despawn(clone, delay);
            }
            else
            {
                Object.Destroy(clone, delay);
            }
        }

        public static void Despawn(Component component, float delay = 0.0f)
        {
            if (component == null) return;
            Despawn(component.gameObject, delay);
        }

        public static void DespawnAll()
        {
            var pools = new List<LeanGameObjectPool>(LeanGameObjectPool.Instances);
            for (int i = 0; i < pools.Count; i++)
            {
                if (pools[i] != null)
                {
                    pools[i].DespawnAll();
                }
            }
        }
    }
}
