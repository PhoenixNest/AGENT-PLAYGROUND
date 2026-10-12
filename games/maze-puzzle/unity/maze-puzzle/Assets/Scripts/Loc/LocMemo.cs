namespace OrbitalSalvage.Loc
{
    /// <summary>
    /// One call site's last <see cref="Loc.Tr"/> result, keyed by language <see cref="Loc.Version"/>
    /// and arguments. A repeat with the same inputs returns the same string and does not format again.
    /// English steady state therefore allocates nothing where the previous frame already resolved the line.
    /// Keep one memo per call site. Two different keys in the same frame need two memos.
    /// </summary>
    public struct LocMemo
    {
        int _version;
        string _key;
        int _kind;
        double _a;
        double _b;
        string _s0;
        string _s1;
        string _text;

        public string Plain(string key)
        {
            int version = Loc.Version;
            if (_text != null && _kind == 0 && _version == version && _key == key) return _text;
            _version = version;
            _key = key;
            _kind = 0;
            _text = Loc.Tr(key);
            return _text;
        }

        public string Num(string key, double value)
        {
            int version = Loc.Version;
            if (_text != null && _kind == 1 && _version == version && _key == key && _a == value) return _text;
            _version = version;
            _key = key;
            _kind = 1;
            _a = value;
            _text = Loc.Tr(key, value);
            return _text;
        }

        public string Num(string key, double a, double b)
        {
            int version = Loc.Version;
            if (_text != null && _kind == 2 && _version == version && _key == key && _a == a && _b == b) return _text;
            _version = version;
            _key = key;
            _kind = 2;
            _a = a;
            _b = b;
            _text = Loc.Tr(key, a, b);
            return _text;
        }

        public string Text(string key, string value)
        {
            int version = Loc.Version;
            if (_text != null && _kind == 3 && _version == version && _key == key && _s0 == value) return _text;
            _version = version;
            _key = key;
            _kind = 3;
            _s0 = value;
            _text = Loc.Tr(key, value);
            return _text;
        }

        public string Text(string key, string a, string b)
        {
            int version = Loc.Version;
            if (_text != null && _kind == 4 && _version == version && _key == key && _s0 == a && _s1 == b) return _text;
            _version = version;
            _key = key;
            _kind = 4;
            _s0 = a;
            _s1 = b;
            _text = Loc.Tr(key, a, b);
            return _text;
        }

        public string Int(string key, int value)
        {
            int version = Loc.Version;
            if (_text != null && _kind == 5 && _version == version && _key == key && _a == value) return _text;
            _version = version;
            _key = key;
            _kind = 5;
            _a = value;
            _text = Loc.Tr(key, value);
            return _text;
        }
    }

    /// <summary>
    /// A few <c>(key, number)</c> labels, for gate badges drawn every frame.
    /// After the first frame of a language, a repeated value does not allocate.
    /// </summary>
    public sealed class LocNumCache
    {
        struct Slot
        {
            public string Key;
            public double Value;
            public int Version;
            public string Text;
        }

        readonly Slot[] _slots;
        int _clock;

        public LocNumCache(int capacity)
        {
            if (capacity < 1) capacity = 1;
            _slots = new Slot[capacity];
        }

        public string Get(string key, double value)
        {
            int version = Loc.Version;
            int empty = -1;
            int same = -1;
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].Text == null)
                {
                    if (empty < 0) empty = i;
                    continue;
                }
                if (_slots[i].Key != key || _slots[i].Value != value) continue;
                if (_slots[i].Version == version) return _slots[i].Text;
                same = i;
            }
            string text = Loc.Tr(key, value);
            int dest = same >= 0 ? same : (empty >= 0 ? empty : (_clock++ % _slots.Length));
            _slots[dest].Key = key;
            _slots[dest].Value = value;
            _slots[dest].Version = version;
            _slots[dest].Text = text;
            return text;
        }
    }

    /// <summary>
    /// Last <see cref="Loc.TrName"/> results for one screen, keyed by the English text and
    /// <see cref="Loc.Version"/>. English returns the input and does not store a slot.
    /// </summary>
    public sealed class LocNameCache
    {
        struct Slot
        {
            public string Source;
            public int Version;
            public string Text;
        }

        readonly Slot[] _slots;
        int _clock;

        public LocNameCache(int capacity)
        {
            if (capacity < 1) capacity = 1;
            _slots = new Slot[capacity];
        }

        public string Get(string english)
        {
            if (english == null) english = "";
            if (Loc.EnglishActive) return english;
            int version = Loc.Version;
            int empty = -1;
            int same = -1;
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].Text == null)
                {
                    if (empty < 0) empty = i;
                    continue;
                }
                if (_slots[i].Source != english) continue;
                if (_slots[i].Version == version) return _slots[i].Text;
                same = i;
            }
            string text = Loc.TrName(english);
            int dest = same >= 0 ? same : (empty >= 0 ? empty : (_clock++ % _slots.Length));
            _slots[dest].Source = english;
            _slots[dest].Version = version;
            _slots[dest].Text = text;
            return text;
        }
    }
}
