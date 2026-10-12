using System;
using System.Collections.Generic;
using OrbitalSalvage.JsCompat;

namespace OrbitalSalvage.Loc
{
    public sealed class LocTable
    {
        public string Code = "";
        public string Name = "";
        public readonly Dictionary<string, string> Spec = new Dictionary<string, string>(StringComparer.Ordinal);
        public readonly Dictionary<string, string> Str = new Dictionary<string, string>(StringComparer.Ordinal);

        public static bool TryParse(string json, out LocTable table)
        {
            table = null;
            if (string.IsNullOrEmpty(json)) return false;
            JsonValue root;
            if (!JsonParser.TryParse(json, out root) || root == null || root.Kind != JsonKind.Object) return false;

            var parsed = new LocTable();
            JsonValue meta = root.Get("meta");
            if (meta != null && meta.Kind == JsonKind.Object)
            {
                JsonValue code = meta.Get("code");
                JsonValue name = meta.Get("name");
                if (code != null && code.Kind == JsonKind.String) parsed.Code = code.String;
                if (name != null && name.Kind == JsonKind.String) parsed.Name = name.String;
            }
            if (!ReadMap(root.Get("spec"), parsed.Spec)) return false;
            if (!ReadMap(root.Get("str"), parsed.Str)) return false;
            table = parsed;
            return true;
        }

        static bool ReadMap(JsonValue section, Dictionary<string, string> dest)
        {
            if (section == null) return true;
            if (section.Kind != JsonKind.Object) return false;
            for (int i = 0; i < section.Members.Count; i++)
            {
                KeyValuePair<string, JsonValue> kv = section.Members[i];
                if (kv.Value == null || kv.Value.Kind != JsonKind.String) return false;
                dest[kv.Key] = kv.Value.String;
            }
            return true;
        }
    }

    public sealed class LocInitResult
    {
        public string ActiveLanguage;
        /// <summary>Registry code the host should store, or null when the profile must not be written.</summary>
        public string PersistLanguage;
    }
}
