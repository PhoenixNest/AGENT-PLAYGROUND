using System.Globalization;
using System.Text;

namespace OrbitalSalvage.JsCompat
{
    public static class JsonWriter
    {
        public static string Stringify(JsonValue value)
        {
            var sb = new StringBuilder();
            Write(sb, value);
            return sb.ToString();
        }

        public static void Write(StringBuilder sb, JsonValue v)
        {
            switch (v.Kind)
            {
                case JsonKind.Null:
                    sb.Append("null");
                    break;
                case JsonKind.Bool:
                    sb.Append(v.Bool ? "true" : "false");
                    break;
                case JsonKind.Number:
                    if (double.IsNaN(v.Number) || double.IsInfinity(v.Number)) sb.Append("null");
                    else sb.Append(JsNumber.ToString(v.Number));
                    break;
                case JsonKind.String:
                    Quote(sb, v.String);
                    break;
                case JsonKind.Array:
                    sb.Append('[');
                    for (int i = 0; i < v.Items.Count; i++)
                    {
                        if (i > 0) sb.Append(',');
                        Write(sb, v.Items[i]);
                    }
                    sb.Append(']');
                    break;
                case JsonKind.Object:
                    sb.Append('{');
                    for (int i = 0; i < v.Members.Count; i++)
                    {
                        if (i > 0) sb.Append(',');
                        Quote(sb, v.Members[i].Key);
                        sb.Append(':');
                        Write(sb, v.Members[i].Value);
                    }
                    sb.Append('}');
                    break;
            }
        }

        public static void Quote(StringBuilder sb, string s)
        {
            sb.Append('"');
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20)
                        {
                            sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else if (char.IsHighSurrogate(c))
                        {
                            if (i + 1 < s.Length && char.IsLowSurrogate(s[i + 1]))
                            {
                                sb.Append(c).Append(s[i + 1]);
                                i++;
                            }
                            else
                            {
                                sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                            }
                        }
                        else if (char.IsLowSurrogate(c))
                        {
                            sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            sb.Append('"');
        }
    }
}
