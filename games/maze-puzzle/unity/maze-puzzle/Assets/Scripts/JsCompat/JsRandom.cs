using System.Collections.Generic;

namespace OrbitalSalvage.JsCompat
{
    public readonly struct RandomCall
    {
        public readonly long Index;
        public readonly string Tag;

        public RandomCall(long index, string tag)
        {
            Index = index;
            Tag = tag;
        }
    }

    public sealed class JsRandom
    {
        public const uint DefaultSeed = 0x5EED1234u;

        private static JsRandom _global = new JsRandom(DefaultSeed);

        private uint _state;
        private readonly List<RandomCall> _log = new List<RandomCall>();

        public JsRandom(uint seed)
        {
            _state = seed;
        }

        public static JsRandom Global
        {
            get { return _global; }
            set { _global = value ?? new JsRandom(DefaultSeed); }
        }

        public uint State
        {
            get { return _state; }
            set { _state = value; }
        }

        public long CallCount { get; private set; }

        public bool LogEnabled { get; set; }

        public IReadOnlyList<RandomCall> Log
        {
            get { return _log; }
        }

        public void Reset(uint seed)
        {
            _state = seed;
            CallCount = 0;
            _log.Clear();
        }

        public void ClearLog()
        {
            _log.Clear();
        }

        public double NextDouble()
        {
            return NextDouble(null);
        }

        public double NextDouble(string tag)
        {
            unchecked
            {
                CallCount++;
                if (LogEnabled)
                {
                    _log.Add(new RandomCall(CallCount, tag));
                }

                uint a = _state + 0x6d2b79f5u;
                _state = a;
                uint t = (a ^ (a >> 15)) * (1u | a);
                t = (t + (t ^ (t >> 7)) * (61u | t)) ^ t;
                return (double)(t ^ (t >> 14)) / 4294967296.0;
            }
        }

        public string DescribeLog(int maxEntries)
        {
            var sb = new System.Text.StringBuilder();
            int n = _log.Count < maxEntries ? _log.Count : maxEntries;
            for (int i = 0; i < n; i++)
            {
                sb.Append(_log[i].Index).Append(':').Append(_log[i].Tag ?? "?").Append('\n');
            }
            return sb.ToString();
        }
    }
}
