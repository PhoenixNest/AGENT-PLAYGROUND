using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OrbitalSalvage.Parity
{
    public static class MiniJson
    {
        public static object Parse(string json)
        {
            var parser = new Parser(json);
            var value = parser.ReadValue();
            parser.SkipWhitespace();
            if (!parser.AtEnd) throw new FormatException($"Unexpected trailing JSON content at {parser.Position}");
            return value;
        }

        public static string Serialize(object value, bool pretty = false)
        {
            var sb = new StringBuilder(256);
            Write(sb, value, pretty, 0);
            return sb.ToString();
        }

        static void Write(StringBuilder sb, object value, bool pretty, int depth)
        {
            switch (value)
            {
                case null:
                    sb.Append("null");
                    return;
                case string s:
                    WriteString(sb, s);
                    return;
                case bool b:
                    sb.Append(b ? "true" : "false");
                    return;
                case int i:
                    sb.Append(i.ToString(CultureInfo.InvariantCulture));
                    return;
                case long l:
                    sb.Append(l.ToString(CultureInfo.InvariantCulture));
                    return;
                case float f:
                    WriteDouble(sb, f);
                    return;
                case double d:
                    WriteDouble(sb, d);
                    return;
                case IDictionary dict:
                    WriteObject(sb, dict, pretty, depth);
                    return;
                case IEnumerable list:
                    WriteArray(sb, list, pretty, depth);
                    return;
                default:
                    WriteString(sb, Convert.ToString(value, CultureInfo.InvariantCulture));
                    return;
            }
        }

        static void WriteDouble(StringBuilder sb, double d)
        {
            if (double.IsNaN(d) || double.IsInfinity(d))
            {
                sb.Append("null");
                return;
            }
            if (d == Math.Floor(d) && Math.Abs(d) < 1e15)
            {
                sb.Append(((long)d).ToString(CultureInfo.InvariantCulture));
                return;
            }
            sb.Append(d.ToString("R", CultureInfo.InvariantCulture));
        }

        static void WriteObject(StringBuilder sb, IDictionary dict, bool pretty, int depth)
        {
            if (dict.Count == 0)
            {
                sb.Append("{}");
                return;
            }
            sb.Append('{');
            bool first = true;
            foreach (DictionaryEntry entry in dict)
            {
                if (!first) sb.Append(',');
                first = false;
                if (pretty) NewLine(sb, depth + 1);
                WriteString(sb, Convert.ToString(entry.Key, CultureInfo.InvariantCulture));
                sb.Append(pretty ? ": " : ":");
                Write(sb, entry.Value, pretty, depth + 1);
            }
            if (pretty) NewLine(sb, depth);
            sb.Append('}');
        }

        static void WriteArray(StringBuilder sb, IEnumerable list, bool pretty, int depth)
        {
            var enumerator = list.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                sb.Append("[]");
                return;
            }
            sb.Append('[');
            bool first = true;
            do
            {
                if (!first) sb.Append(',');
                first = false;
                if (pretty) NewLine(sb, depth + 1);
                Write(sb, enumerator.Current, pretty, depth + 1);
            } while (enumerator.MoveNext());
            if (pretty) NewLine(sb, depth);
            sb.Append(']');
        }

        static void NewLine(StringBuilder sb, int depth)
        {
            sb.Append('\n');
            sb.Append(' ', depth * 2);
        }

        static void WriteString(StringBuilder sb, string s)
        {
            sb.Append('"');
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
        }

        sealed class Parser
        {
            readonly string s;
            int i;

            public Parser(string json)
            {
                s = json ?? throw new ArgumentNullException(nameof(json));
                if (s.Length > 0 && s[0] == '\uFEFF') i = 1;
            }

            public int Position => i;
            public bool AtEnd => i >= s.Length;

            public void SkipWhitespace()
            {
                while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
            }

            public object ReadValue()
            {
                SkipWhitespace();
                if (i >= s.Length) throw new FormatException("Unexpected end of JSON");
                char c = s[i];
                switch (c)
                {
                    case '{': return ReadObject();
                    case '[': return ReadArray();
                    case '"': return ReadString();
                    case 't': Expect("true"); return true;
                    case 'f': Expect("false"); return false;
                    case 'n': Expect("null"); return null;
                    default: return ReadNumber();
                }
            }

            void Expect(string literal)
            {
                if (string.CompareOrdinal(s, i, literal, 0, literal.Length) != 0)
                    throw new FormatException($"Invalid JSON literal at {i}");
                i += literal.Length;
            }

            Dictionary<string, object> ReadObject()
            {
                var result = new Dictionary<string, object>();
                i++;
                SkipWhitespace();
                if (i < s.Length && s[i] == '}') { i++; return result; }
                while (true)
                {
                    SkipWhitespace();
                    if (i >= s.Length || s[i] != '"') throw new FormatException($"Expected string key at {i}");
                    string key = ReadString();
                    SkipWhitespace();
                    if (i >= s.Length || s[i] != ':') throw new FormatException($"Expected ':' at {i}");
                    i++;
                    result[key] = ReadValue();
                    SkipWhitespace();
                    if (i >= s.Length) throw new FormatException("Unterminated object");
                    if (s[i] == ',') { i++; continue; }
                    if (s[i] == '}') { i++; return result; }
                    throw new FormatException($"Expected ',' or '}}' at {i}");
                }
            }

            List<object> ReadArray()
            {
                var result = new List<object>();
                i++;
                SkipWhitespace();
                if (i < s.Length && s[i] == ']') { i++; return result; }
                while (true)
                {
                    result.Add(ReadValue());
                    SkipWhitespace();
                    if (i >= s.Length) throw new FormatException("Unterminated array");
                    if (s[i] == ',') { i++; continue; }
                    if (s[i] == ']') { i++; return result; }
                    throw new FormatException($"Expected ',' or ']' at {i}");
                }
            }

            string ReadString()
            {
                var sb = new StringBuilder();
                i++;
                while (i < s.Length)
                {
                    char c = s[i++];
                    if (c == '"') return sb.ToString();
                    if (c != '\\') { sb.Append(c); continue; }
                    if (i >= s.Length) break;
                    char e = s[i++];
                    switch (e)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (i + 4 > s.Length) throw new FormatException("Bad unicode escape");
                            sb.Append((char)int.Parse(s.Substring(i, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                            i += 4;
                            break;
                        default: throw new FormatException($"Bad escape \\{e}");
                    }
                }
                throw new FormatException("Unterminated string");
            }

            object ReadNumber()
            {
                int start = i;
                bool floating = false;
                while (i < s.Length)
                {
                    char c = s[i];
                    if ((c >= '0' && c <= '9') || c == '-' || c == '+') i++;
                    else if (c == '.' || c == 'e' || c == 'E') { floating = true; i++; }
                    else break;
                }
                if (start == i) throw new FormatException($"Unexpected character '{s[i]}' at {i}");
                string token = s.Substring(start, i - start);
                if (!floating && long.TryParse(token, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long l)) return l;
                return double.Parse(token, NumberStyles.Float, CultureInfo.InvariantCulture);
            }
        }
    }
}
