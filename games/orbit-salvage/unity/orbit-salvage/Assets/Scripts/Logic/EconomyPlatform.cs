using System;
using System.Collections.Generic;

namespace OrbitalSalvage.Logic
{
    public interface IKeyValueStore
    {
        string GetItem(string key);
        void SetItem(string key, string value);
    }

    public sealed class MemoryKeyValueStore : IKeyValueStore
    {
        public readonly Dictionary<string, string> Items = new Dictionary<string, string>();
        public int WriteCount;

        public string GetItem(string key)
        {
            string v;
            return Items.TryGetValue(key, out v) ? v : null;
        }

        public void SetItem(string key, string value)
        {
            WriteCount++;
            Items[key] = value;
        }
    }

    public interface IWallClock
    {
        double NowEpochMs { get; }
    }

    public sealed class FixedWallClock : IWallClock
    {
        public double NowEpochMs { get; set; }

        public FixedWallClock(double epochMs)
        {
            NowEpochMs = epochMs;
        }
    }

    public sealed class SystemWallClock : IWallClock
    {
        public double NowEpochMs
        {
            get { return Math.Floor((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds); }
        }
    }

    public sealed class OrderedMap<T>
    {
        private readonly List<string> _keys = new List<string>();
        private readonly Dictionary<string, T> _values = new Dictionary<string, T>();

        public IList<string> Keys
        {
            get { return _keys; }
        }

        public bool Has(string key)
        {
            return _values.ContainsKey(key);
        }

        public T Get(string key)
        {
            T v;
            return _values.TryGetValue(key, out v) ? v : default(T);
        }

        public void Set(string key, T value)
        {
            if (!_values.ContainsKey(key)) _keys.Add(key);
            _values[key] = value;
        }

        public T this[string key]
        {
            get { return Get(key); }
            set { Set(key, value); }
        }
    }
}
