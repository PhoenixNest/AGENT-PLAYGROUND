using System;
using System.Globalization;
using System.Text;

namespace OrbitalSalvage.JsCompat
{
    public static class JsonParser
    {
        public static JsonValue Parse(string text)
        {
            int pos = 0;
            SkipWs(text, ref pos);
            JsonValue v = ParseValue(text, ref pos);
            SkipWs(text, ref pos);
            if (pos != text.Length) throw Error(text, pos, "unexpected trailing characters");
            return v;
        }

        public static bool TryParse(string text, out JsonValue value)
        {
            try
            {
                value = Parse(text);
                return true;
            }
            catch (FormatException)
            {
                value = null;
                return false;
            }
        }

        private static FormatException Error(string text, int pos, string message)
        {
            return new FormatException("JSON parse error at " + pos + ": " + message);
        }

        private static void SkipWs(string s, ref int pos)
        {
            while (pos < s.Length)
            {
                char c = s[pos];
                if (c == ' ' || c == '\t' || c == '\n' || c == '\r') pos++;
                else break;
            }
        }

        private static JsonValue ParseValue(string s, ref int pos)
        {
            if (pos >= s.Length) throw Error(s, pos, "unexpected end");
            char c = s[pos];
            switch (c)
            {
                case '{':
                    return ParseObject(s, ref pos);
                case '[':
                    return ParseArray(s, ref pos);
                case '"':
                    return JsonValue.FromString(ParseString(s, ref pos));
                case 't':
                    Expect(s, ref pos, "true");
                    return JsonValue.FromBool(true);
                case 'f':
                    Expect(s, ref pos, "false");
                    return JsonValue.FromBool(false);
                case 'n':
                    Expect(s, ref pos, "null");
                    return JsonValue.Null;
                default:
                    if (c == '-' || (c >= '0' && c <= '9')) return ParseNumber(s, ref pos);
                    throw Error(s, pos, "unexpected character '" + c + "'");
            }
        }

        private static void Expect(string s, ref int pos, string word)
        {
            if (string.CompareOrdinal(s, pos, word, 0, word.Length) != 0) throw Error(s, pos, "expected " + word);
            pos += word.Length;
        }

        private static JsonValue ParseNumber(string s, ref int pos)
        {
            int start = pos;
            if (s[pos] == '-') pos++;
            if (pos >= s.Length) throw Error(s, pos, "bad number");
            if (s[pos] == '0')
            {
                pos++;
            }
            else if (s[pos] >= '1' && s[pos] <= '9')
            {
                while (pos < s.Length && s[pos] >= '0' && s[pos] <= '9') pos++;
            }
            else
            {
                throw Error(s, pos, "bad number");
            }
            if (pos < s.Length && s[pos] == '.')
            {
                pos++;
                int fs = pos;
                while (pos < s.Length && s[pos] >= '0' && s[pos] <= '9') pos++;
                if (pos == fs) throw Error(s, pos, "bad fraction");
            }
            if (pos < s.Length && (s[pos] == 'e' || s[pos] == 'E'))
            {
                pos++;
                if (pos < s.Length && (s[pos] == '+' || s[pos] == '-')) pos++;
                int es = pos;
                while (pos < s.Length && s[pos] >= '0' && s[pos] <= '9') pos++;
                if (pos == es) throw Error(s, pos, "bad exponent");
            }
            string token = s.Substring(start, pos - start);
            double d;
            try
            {
                d = double.Parse(token, NumberStyles.Float, CultureInfo.InvariantCulture);
            }
            catch (OverflowException)
            {
                d = token[0] == '-' ? double.NegativeInfinity : double.PositiveInfinity;
            }
            return JsonValue.FromNumber(d);
        }

        private static string ParseString(string s, ref int pos)
        {
            pos++;
            StringBuilder sb = null;
            int runStart = pos;
            while (true)
            {
                if (pos >= s.Length) throw Error(s, pos, "unterminated string");
                char c = s[pos];
                if (c == '"')
                {
                    string result;
                    if (sb == null) result = s.Substring(runStart, pos - runStart);
                    else
                    {
                        sb.Append(s, runStart, pos - runStart);
                        result = sb.ToString();
                    }
                    pos++;
                    return result;
                }
                if (c == '\\')
                {
                    if (sb == null) sb = new StringBuilder();
                    sb.Append(s, runStart, pos - runStart);
                    pos++;
                    if (pos >= s.Length) throw Error(s, pos, "bad escape");
                    char e = s[pos];
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
                            if (pos + 4 >= s.Length) throw Error(s, pos, "bad unicode escape");
                            sb.Append((char)int.Parse(s.Substring(pos + 1, 4), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture));
                            pos += 4;
                            break;
                        default:
                            throw Error(s, pos, "bad escape");
                    }
                    pos++;
                    runStart = pos;
                    continue;
                }
                if (c < 0x20) throw Error(s, pos, "control character in string");
                pos++;
            }
        }

        private static JsonValue ParseArray(string s, ref int pos)
        {
            JsonValue arr = JsonValue.NewArray();
            pos++;
            SkipWs(s, ref pos);
            if (pos < s.Length && s[pos] == ']')
            {
                pos++;
                return arr;
            }
            while (true)
            {
                SkipWs(s, ref pos);
                arr.Items.Add(ParseValue(s, ref pos));
                SkipWs(s, ref pos);
                if (pos >= s.Length) throw Error(s, pos, "unterminated array");
                if (s[pos] == ',')
                {
                    pos++;
                    continue;
                }
                if (s[pos] == ']')
                {
                    pos++;
                    return arr;
                }
                throw Error(s, pos, "expected , or ]");
            }
        }

        private static JsonValue ParseObject(string s, ref int pos)
        {
            JsonValue obj = JsonValue.NewObject();
            pos++;
            SkipWs(s, ref pos);
            if (pos < s.Length && s[pos] == '}')
            {
                pos++;
                return obj;
            }
            while (true)
            {
                SkipWs(s, ref pos);
                if (pos >= s.Length || s[pos] != '"') throw Error(s, pos, "expected string key");
                string key = ParseString(s, ref pos);
                SkipWs(s, ref pos);
                if (pos >= s.Length || s[pos] != ':') throw Error(s, pos, "expected :");
                pos++;
                SkipWs(s, ref pos);
                JsonValue v = ParseValue(s, ref pos);
                obj.Set(key, v);
                SkipWs(s, ref pos);
                if (pos >= s.Length) throw Error(s, pos, "unterminated object");
                if (s[pos] == ',')
                {
                    pos++;
                    continue;
                }
                if (s[pos] == '}')
                {
                    pos++;
                    return obj;
                }
                throw Error(s, pos, "expected , or }");
            }
        }
    }
}
