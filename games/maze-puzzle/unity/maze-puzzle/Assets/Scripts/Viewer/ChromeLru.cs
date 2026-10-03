using System;
using System.Collections.Generic;

namespace OrbitalSalvage.Viewer
{
    /// <summary>
    /// Small most-recently-used map. Capacity is tiny (the viewer keeps three painted pages),
    /// so a list is enough and eviction order is obvious to test.
    /// </summary>
    public sealed class ChromeLru<T> where T : class
    {
        readonly int capacity;
        readonly List<string> order = new List<string>();
        readonly Dictionary<string, T> map = new Dictionary<string, T>();

        public ChromeLru(int capacity)
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
            this.capacity = capacity;
        }

        public int Count => map.Count;

        public bool Contains(string key) => map.ContainsKey(key);

        public bool TryGet(string key, out T value)
        {
            if (!map.TryGetValue(key, out value)) return false;
            Touch(key);
            return true;
        }

        /// <summary>Inserts or replaces. Evicted values (oldest first, then a replaced instance) are appended to <paramref name="evicted"/>.</summary>
        public void Add(string key, T value, IList<T> evicted)
        {
            if (map.TryGetValue(key, out var old))
            {
                Touch(key);
                map[key] = value;
                if (!ReferenceEquals(old, value) && old != null && evicted != null) evicted.Add(old);
                return;
            }

            map[key] = value;
            order.Add(key);
            while (map.Count > capacity)
            {
                string oldest = order[0];
                order.RemoveAt(0);
                var doomed = map[oldest];
                map.Remove(oldest);
                if (doomed != null && evicted != null) evicted.Add(doomed);
            }
        }

        public void Clear(Action<T> dispose)
        {
            if (dispose != null)
                foreach (var kv in map) dispose(kv.Value);
            map.Clear();
            order.Clear();
        }

        void Touch(string key)
        {
            order.Remove(key);
            order.Add(key);
        }
    }
}
