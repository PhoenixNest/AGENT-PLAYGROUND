using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OrbitalSalvage.Render
{
    /// <summary>Minimal JSON reader: objects become Dictionary&lt;string, object&gt;, arrays List&lt;object&gt;, numbers double.</summary>
    public sealed class MiniJson
    {
        readonly string s;
        int i;

        MiniJson(string text) { s = text; }

        public static object Parse(string text)
        {
            var p = new MiniJson(text);
            p.Ws();
            var v = p.Value();
            p.Ws();
            if (p.i != p.s.Length) throw new FormatException("trailing characters at " + p.i);
            return v;
        }

        void Ws()
        {
            while (i < s.Length && (s[i] == ' ' || s[i] == '\n' || s[i] == '\r' || s[i] == '\t')) i++;
        }

        object Value()
        {
            if (i >= s.Length) throw new FormatException("unexpected end");
            char c = s[i];
            switch (c)
            {
                case '{': return Obj();
                case '[': return Arr();
                case '"': return Str();
                case 't': i += 4; return true;
                case 'f': i += 5; return false;
                case 'n': i += 4; return null;
                default: return Num();
            }
        }

        Dictionary<string, object> Obj()
        {
            var d = new Dictionary<string, object>();
            i++;
            Ws();
            if (s[i] == '}') { i++; return d; }
            while (true)
            {
                Ws();
                string k = Str();
                Ws();
                if (s[i++] != ':') throw new FormatException("expected ':' at " + i);
                Ws();
                d[k] = Value();
                Ws();
                char c = s[i++];
                if (c == '}') return d;
                if (c != ',') throw new FormatException("expected ',' at " + i);
            }
        }

        List<object> Arr()
        {
            var l = new List<object>();
            i++;
            Ws();
            if (s[i] == ']') { i++; return l; }
            while (true)
            {
                Ws();
                l.Add(Value());
                Ws();
                char c = s[i++];
                if (c == ']') return l;
                if (c != ',') throw new FormatException("expected ',' at " + i);
            }
        }

        string Str()
        {
            if (s[i] != '"') throw new FormatException("expected string at " + i);
            i++;
            int start = i;
            while (i < s.Length && s[i] != '"' && s[i] != '\\') i++;
            if (s[i] == '"') { string r = s.Substring(start, i - start); i++; return r; }
            var sb = new StringBuilder();
            sb.Append(s, start, i - start);
            while (true)
            {
                char c = s[i++];
                if (c == '"') return sb.ToString();
                if (c != '\\') { sb.Append(c); continue; }
                char e = s[i++];
                switch (e)
                {
                    case 'n': sb.Append('\n'); break;
                    case 't': sb.Append('\t'); break;
                    case 'r': sb.Append('\r'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'u': sb.Append((char)Convert.ToInt32(s.Substring(i, 4), 16)); i += 4; break;
                    default: sb.Append(e); break;
                }
            }
        }

        double Num()
        {
            int start = i;
            while (i < s.Length && "+-0123456789.eE".IndexOf(s[i]) >= 0) i++;
            return double.Parse(s.Substring(start, i - start), CultureInfo.InvariantCulture);
        }
    }
}
