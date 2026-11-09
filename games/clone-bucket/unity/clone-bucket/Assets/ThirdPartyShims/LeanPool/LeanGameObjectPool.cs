using System.Collections.Generic;
using UnityEngine;

namespace Lean.Pool
{
    public class LeanGameObjectPool : MonoBehaviour
    {
        public enum NotificationType
        {
            None = 0,
            SendMessage = 1,
            BroadcastMessage = 2,
            IPoolable = 3,
            BroadcastIPoolable = 4,
        }

        public enum StrategyType
        {
            ActivateAndDeactivate = 0,
            DeactivateViaHierarchy = 1,
        }

        [System.Serializable]
        public class Delay
        {
            public GameObject Clone;
            public float Life;
        }

        public static readonly LinkedList<LeanGameObjectPool> Instances = new LinkedList<LeanGameObjectPool>();

        private static readonly Dictionary<GameObject, LeanGameObjectPool> prefabMap = new Dictionary<GameObject, LeanGameObjectPool>();
        private static readonly List<IPoolable> tempPoolables = new List<IPoolable>();

        private bool registered;
        private int spawnCount;

        [SerializeField] private GameObject prefab;
        [SerializeField] private NotificationType notification;
        [SerializeField] private StrategyType strategy;
        [SerializeField] private int preload;
        [SerializeField] private int capacity;
        [SerializeField] private bool recycle = true;
        [SerializeField] private bool persist;
        [SerializeField] private bool stamp;
        [SerializeField] private bool warnings = true;
        [SerializeField] private List<GameObject> spawnedClonesList = new List<GameObject>();
        [SerializeField] private List<GameObject> despawnedClones = new List<GameObject>();
        [SerializeField] private List<Delay> delays = new List<Delay>();
        [SerializeField] private Transform deactivatedChild;

        private readonly HashSet<GameObject> spawnedClonesHashSet = new HashSet<GameObject>();

        public GameObject Prefab { get { return prefab; } }
        public int Capacity { get { return capacity; } }
        public bool Recycle { get { return recycle; } }
        public bool Persist { get { return persist; } }
        public int SpawnedCount { get { return spawnedClonesHashSet.Count; } }

        internal static LeanGameObjectPool FindPoolForPrefab(GameObject prefab)
        {
            LeanGameObjectPool pool;
            if (prefab != null && prefabMap.TryGetValue(prefab, out pool) && pool != null) return pool;
            return null;
        }

        internal static LeanGameObjectPool CreatePool(GameObject prefab)
        {
            var go = new GameObject("LeanGameObjectPool (" + prefab.name + ")");
            go.SetActive(false);
            var pool = go.AddComponent<LeanGameObjectPool>();
            pool.prefab = prefab;
            go.SetActive(true);
            return pool;
        }

        protected virtual void Awake()
        {
            Register();
        }

        protected virtual void Update()
        {
            for (int i = delays.Count - 1; i >= 0; i--)
            {
                var delay = delays[i];
                if (delay.Clone == null)
                {
                    delays.RemoveAt(i);
                    continue;
                }

                delay.Life -= Time.deltaTime;
                if (delay.Life <= 0.0f)
                {
                    delays.RemoveAt(i);
                    Despawn(delay.Clone);
                }
            }
        }

        protected virtual void OnDestroy()
        {
            Instances.Remove(this);

            if (prefab != null && prefabMap.ContainsKey(prefab) && prefabMap[prefab] == this)
            {
                prefabMap.Remove(prefab);
            }
        }

        private void Register()
        {
            if (registered) return;
            registered = true;

            Instances.AddLast(this);

            if (prefab != null && !prefabMap.ContainsKey(prefab))
            {
                prefabMap.Add(prefab, this);
            }

            for (int i = 0; i < preload && prefab != null; i++)
            {
                var clone = Instantiate(prefab);
                clone.SetActive(false);
                if (strategy == StrategyType.DeactivateViaHierarchy && deactivatedChild != null)
                {
                    clone.transform.SetParent(deactivatedChild, false);
                }
                despawnedClones.Add(clone);
                LeanPool.Links[clone] = this;
            }
        }

        public GameObject Spawn()
        {
            return SpawnInternal(false, Vector3.zero, Quaternion.identity, null);
        }

        public GameObject Spawn(Transform parent)
        {
            return SpawnInternal(false, Vector3.zero, Quaternion.identity, parent);
        }

        public GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            return SpawnInternal(true, position, rotation, null);
        }

        public GameObject Spawn(Vector3 position, Quaternion rotation, Transform parent)
        {
            return SpawnInternal(true, position, rotation, parent);
        }

        public void Despawn(GameObject clone, float delay = 0.0f)
        {
            if (clone == null) return;

            if (delay > 0.0f)
            {
                delays.Add(new Delay { Clone = clone, Life = delay });
                return;
            }

            if (!spawnedClonesHashSet.Remove(clone))
            {
                if (warnings) Debug.LogWarning("Despawn: '" + clone.name + "' was not spawned by this pool.", this);
                return;
            }

            spawnedClonesList.Remove(clone);
            Notify(clone, false);

            if (recycle)
            {
                clone.SetActive(false);
                clone.transform.SetParent(
                    strategy == StrategyType.DeactivateViaHierarchy && deactivatedChild != null ? deactivatedChild : transform,
                    false);
                despawnedClones.Add(clone);
            }
            else
            {
                LeanPool.Links.Remove(clone);
                Destroy(clone);
            }
        }

        public void DespawnAll()
        {
            PruneDestroyed();
            delays.Clear();

            for (int i = spawnedClonesList.Count - 1; i >= 0; i--)
            {
                Despawn(spawnedClonesList[i]);
            }
        }

        private void PruneDestroyed()
        {
            for (int i = spawnedClonesList.Count - 1; i >= 0; i--)
            {
                if (spawnedClonesList[i] == null)
                {
                    spawnedClonesList.RemoveAt(i);
                }
            }
            spawnedClonesHashSet.RemoveWhere(IsDestroyed);
        }

        private static bool IsDestroyed(GameObject clone)
        {
            return clone == null;
        }

        private GameObject PopDespawned()
        {
            GameObject clone = null;
            while (despawnedClones.Count > 0 && clone == null)
            {
                int last = despawnedClones.Count - 1;
                clone = despawnedClones[last];
                despawnedClones.RemoveAt(last);
            }
            return clone;
        }

        private GameObject SpawnInternal(bool setTransform, Vector3 position, Quaternion rotation, Transform parent)
        {
            PruneDestroyed();

            GameObject clone = PopDespawned();
            if (clone == null)
            {
                if (capacity > 0 && spawnedClonesList.Count >= capacity)
                {
                    if (!recycle)
                    {
                        if (warnings) Debug.LogWarning("Spawn: pool '" + name + "' reached its capacity of " + capacity + ".", this);
                        return null;
                    }

                    Despawn(spawnedClonesList[0]);
                    clone = PopDespawned();
                }
                else
                {
                    if (prefab == null)
                    {
                        if (warnings) Debug.LogWarning("Spawn: pool '" + name + "' has no prefab.", this);
                        return null;
                    }

                    clone = Instantiate(prefab);
                }

                if (clone == null) return null;
            }

            clone.transform.SetParent(parent, false);
            if (setTransform)
            {
                clone.transform.SetPositionAndRotation(position, rotation);
            }

            spawnedClonesList.Add(clone);
            spawnedClonesHashSet.Add(clone);
            LeanPool.Links[clone] = this;

            if (stamp)
            {
                spawnCount += 1;
                clone.name = (prefab != null ? prefab.name : clone.name) + " #" + spawnCount;
            }

            clone.SetActive(true);
            Notify(clone, true);

            return clone;
        }

        private void Notify(GameObject clone, bool spawned)
        {
            string message = spawned ? "OnSpawn" : "OnDespawn";

            switch (notification)
            {
                case NotificationType.SendMessage:
                    clone.SendMessage(message, SendMessageOptions.DontRequireReceiver);
                    break;

                case NotificationType.BroadcastMessage:
                    clone.BroadcastMessage(message, SendMessageOptions.DontRequireReceiver);
                    break;

                case NotificationType.IPoolable:
                    clone.GetComponents<IPoolable>(tempPoolables);
                    CallPoolables(spawned);
                    break;

                case NotificationType.BroadcastIPoolable:
                    clone.GetComponentsInChildren<IPoolable>(true, tempPoolables);
                    CallPoolables(spawned);
                    break;
            }
        }

        private static void CallPoolables(bool spawned)
        {
            for (int i = 0; i < tempPoolables.Count; i++)
            {
                if (spawned) tempPoolables[i].OnSpawn();
                else tempPoolables[i].OnDespawn();
            }
            tempPoolables.Clear();
        }
    }
}
