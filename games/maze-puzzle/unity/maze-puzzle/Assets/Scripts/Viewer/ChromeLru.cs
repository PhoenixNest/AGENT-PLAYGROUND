using System;
using System.Collections.Generic;

namespace OrbitalSalvage.Viewer
{
    /// <summary>
    /// Small most-recently-used map. Without a byte cost, pinned keys stay even past the
    /// count cap so a modal cannot push a tab out. When <see cref="Cost"/> is set,
    /// a resident set over <see cref="ByteBudget"/> evicts the oldest unpinned entry and then the
    /// oldest pinned entry. <see cref="ProtectKey"/> (the page on screen) is never evicted.
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

        /// <summary>Resident byte ceiling. Ignored until <see cref="Cost"/> is set. The default is no byte ceiling.</summary>
        public long ByteBudget = long.MaxValue;

        /// <summary>Bytes charged for a resident value. Null keeps the cache on the count cap only.</summary>
        public Func<T, long> Cost;

        /// <summary>Key that stays resident even when it alone is over <see cref="ByteBudget"/>.</summary>
        public string ProtectKey;

        public int Count => map.Count;

        public long TotalBytes
        {
            get
            {
                if (Cost == null) return 0;
                long n = 0;
                foreach (var kv in map)
                {
                    if (kv.Value != null) n += Cost(kv.Value);
                }
                return n;
            }
        }

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
                EvictOverflow(evicted);
                return;
            }

            map[key] = value;
            order.Add(key);
            EvictOverflow(evicted);
        }

        /// <summary>Evict until the count cap and the byte budget both hold, appending removed values to <paramref name="evicted"/>.</summary>
        public void Trim(IList<T> evicted) => EvictOverflow(evicted);

        void EvictOverflow(IList<T> evicted)
        {
            while (OverCapacity())
            {
                int victim = Oldest(skipPinned: true);
                if (victim < 0)
                {
                    // Count overflow leaves pins in place. Byte pressure may release them, oldest first.
                    if (!BytesOver()) break;
                    victim = Oldest(skipPinned: false);
                    if (victim < 0) break;
                }
                string oldest = order[victim];
                order.RemoveAt(victim);
                var doomed = map[oldest];
                map.Remove(oldest);
                if (doomed != null && evicted != null) evicted.Add(doomed);
            }
        }

        bool OverCapacity()
        {
            return map.Count > capacity || BytesOver();
        }

        bool BytesOver()
        {
            return Cost != null && TotalBytes > ByteBudget;
        }

        int Oldest(bool skipPinned)
        {
            for (int i = 0; i < order.Count; i++)
            {
                string key = order[i];
                if (ProtectKey != null && key == ProtectKey) continue;
                if (skipPinned && pinned.Contains(key)) continue;
                return i;
            }
            return -1;
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
