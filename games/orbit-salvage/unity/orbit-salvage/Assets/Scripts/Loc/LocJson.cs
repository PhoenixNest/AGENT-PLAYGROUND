using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using OrbitalSalvage.JsCompat;

namespace OrbitalSalvage.Loc
{
    public static class LocJson
    {
        public static string WriteTable(
            string code,
            string endonym,
            IList<string> specKeys,
            Func<string, string> specValue,
            IList<KeyValuePair<string, string>> strPairs,
            IList<string> doNotTranslate)
        {
            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("  \"meta\": {\n");
            sb.Append("    \"code\": ");
            JsonWriter.Quote(sb, code);
            sb.Append(",\n");
            sb.Append("    \"name\": ");
            JsonWriter.Quote(sb, endonym ?? "");
            sb.Append(",\n");
            sb.Append("    \"version\": 1,\n");
            sb.Append("    \"source\": \"en\"\n");
            sb.Append("  },\n");
            sb.Append("  \"spec\": ");
            WriteMap(sb, specKeys, specValue);
            sb.Append(",\n");
            sb.Append("  \"str\": ");
            WritePairs(sb, strPairs);
            sb.Append(",\n");
            sb.Append("  \"doNotTranslate\": ");
            WriteArray(sb, doNotTranslate);
            sb.Append("\n}\n");
            return sb.ToString();
        }

        static void WriteMap(StringBuilder sb, IList<string> keys, Func<string, string> value)
        {
            if (keys == null || keys.Count == 0)
            {
                sb.Append("{}");
                return;
            }
            sb.Append("{\n");
            for (int i = 0; i < keys.Count; i++)
            {
                sb.Append("    ");
                JsonWriter.Quote(sb, keys[i]);
                sb.Append(": ");
                JsonWriter.Quote(sb, value(keys[i]));
                if (i + 1 < keys.Count) sb.Append(',');
                sb.Append('\n');
            }
            sb.Append("  }");
        }

        static void WritePairs(StringBuilder sb, IList<KeyValuePair<string, string>> pairs)
        {
            if (pairs == null || pairs.Count == 0)
            {
                sb.Append("{}");
                return;
            }
            sb.Append("{\n");
            for (int i = 0; i < pairs.Count; i++)
            {
                sb.Append("    ");
                JsonWriter.Quote(sb, pairs[i].Key);
                sb.Append(": ");
                JsonWriter.Quote(sb, pairs[i].Value ?? "");
                if (i + 1 < pairs.Count) sb.Append(',');
                sb.Append('\n');
            }
            sb.Append("  }");
        }

        static void WriteArray(StringBuilder sb, IList<string> items)
        {
            if (items == null || items.Count == 0)
            {
                sb.Append("[]");
                return;
            }
            sb.Append("[\n");
            for (int i = 0; i < items.Count; i++)
            {
                sb.Append("    ");
                JsonWriter.Quote(sb, items[i]);
                if (i + 1 < items.Count) sb.Append(',');
                sb.Append('\n');
            }
            sb.Append("  ]");
        }
    }
}
