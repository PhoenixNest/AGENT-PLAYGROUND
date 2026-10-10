using System;
using System.Collections.Generic;

namespace OrbitalSalvage.Viewer
{
    /// <summary>
    /// Small most-recently-used map. Pinned keys (the main tabs) are kept; overflow evicts the
    /// oldest unpinned entry so modals stay bounded and cannot push a tab out.
    /// </summary>
    public sealed class ChromeLru<T> where T : class
    {
        readonly int capacity;
        readonly List<string> order = new List<string>();
        readonly Dictionary<string, T> map = new Dictionary<string, T>();
        readonly HashSet<string> pinned = new HashSet<string>();

        public ChromeLru(int capacity)
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
            this.capacity = capacity;
        }

        public int Count => map.Count;

        public bool Contains(string key) => map.ContainsKey(key);

        public void Pin(string key)
        {
            if (!string.IsNullOrEmpty(key)) pinned.Add(key);
        }

        public bool IsPinned(string key) => key != null && pinned.Contains(key);

        /// <summary>Drop pins that are no longer in the live main-tab set. Those pages become evictable.</summary>
        public void RetainPins(ICollection<string> keep)
        {
            if (pinned.Count == 0) return;
            if (keep == null || keep.Count == 0)
            {
                pinned.Clear();
                return;
            }
            var drop = new List<string>();
            foreach (var key in pinned)
                if (!keep.Contains(key)) drop.Add(key);
            for (int i = 0; i < drop.Count; i++) pinned.Remove(drop[i]);
        }

        public void ForEach(Action<string, T> visit)
        {
            if (visit == null) return;
            for (int i = 0; i < order.Count; i++)
            {
                string key = order[i];
                if (map.TryGetValue(key, out var value)) visit(key, value);
            }
        }

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
            EvictOverflow(evicted);
        }

        void EvictOverflow(IList<T> evicted)
        {
            while (map.Count > capacity)
            {
                int victim = -1;
                for (int i = 0; i < order.Count; i++)
                {
                    if (!pinned.Contains(order[i])) { victim = i; break; }
                }
                if (victim < 0) break;
                string oldest = order[victim];
                order.RemoveAt(victim);
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
