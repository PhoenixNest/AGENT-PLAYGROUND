using System;
using System.Collections.Generic;

namespace OrbitalSalvage.JsCompat
{
    public enum JsonKind
    {
        Null,
        Bool,
        Number,
        String,
        Array,
        Object
    }

    public sealed class JsonValue
    {
        public JsonKind Kind;
        public bool Bool;
        public double Number;
        public string String;
        public List<JsonValue> Items;
        public List<KeyValuePair<string, JsonValue>> Members;

        public static readonly JsonValue Null = new JsonValue { Kind = JsonKind.Null };

        public static JsonValue FromBool(bool b)
        {
            return new JsonValue { Kind = JsonKind.Bool, Bool = b };
        }

        public static JsonValue FromNumber(double d)
        {
            return new JsonValue { Kind = JsonKind.Number, Number = d };
        }

        public static JsonValue FromString(string s)
        {
            return new JsonValue { Kind = JsonKind.String, String = s };
        }

        public static JsonValue NewArray()
        {
            return new JsonValue { Kind = JsonKind.Array, Items = new List<JsonValue>() };
        }

        public static JsonValue NewObject()
        {
            return new JsonValue { Kind = JsonKind.Object, Members = new List<KeyValuePair<string, JsonValue>>() };
        }

        public JsonValue Add(JsonValue v)
        {
            Items.Add(v);
            return this;
        }

        public JsonValue Set(string key, JsonValue v)
        {
            for (int i = 0; i < Members.Count; i++)
            {
                if (Members[i].Key == key)
                {
                    Members[i] = new KeyValuePair<string, JsonValue>(key, v);
                    return this;
                }
            }
            Members.Add(new KeyValuePair<string, JsonValue>(key, v));
            return this;
        }

        public JsonValue Set(string key, double d)
        {
            return Set(key, FromNumber(d));
        }

        public JsonValue Set(string key, string s)
        {
            return Set(key, FromString(s));
        }

        public JsonValue Set(string key, bool b)
        {
            return Set(key, FromBool(b));
        }

        public JsonValue Get(string key)
        {
            if (Kind != JsonKind.Object) return null;
            for (int i = 0; i < Members.Count; i++)
            {
                if (Members[i].Key == key) return Members[i].Value;
            }
            return null;
        }

        public bool Has(string key)
        {
            return Get(key) != null;
        }

        public int Count
        {
            get
            {
                if (Kind == JsonKind.Array) return Items.Count;
                if (Kind == JsonKind.Object) return Members.Count;
                return 0;
            }
        }

        public JsonValue this[int index]
        {
            get { return Items[index]; }
        }

        public JsonValue this[string key]
        {
            get
            {
                JsonValue v = Get(key);
                if (v == null) throw new KeyNotFoundException(key);
                return v;
            }
        }
    }
}
