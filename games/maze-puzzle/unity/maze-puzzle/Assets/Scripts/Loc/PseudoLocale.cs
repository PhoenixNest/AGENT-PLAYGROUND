using System;
using System.Text;

namespace OrbitalSalvage.Loc
{
    /// <summary>
    /// Diagnostic pseudo-locale. Not a registry language and never written to the profile.
    /// Accented look-alikes stay inside Latin-1 so Fredoka and Nunito can draw them.
    /// </summary>
    public static class PseudoLocale
    {
        public const string Code = "qps";
        /// <summary>Body length target is <c>ceil(source.Length * factor)</c>. 1.4 is +40%.</summary>
        public const double DefaultFactor = 1.4;
        public const char Filler = '~';

        public static string Expand(string source)
        {
            return Expand(source, DefaultFactor);
        }

        public static string Expand(string source, double factor)
        {
            if (source == null) source = "";
            if (factor < 1) factor = 1;
            var body = new StringBuilder(source.Length + 8);
            int i = 0;
            while (i < source.Length)
            {
                if (TryPlaceholder(source, i, out int n) || TryEmoji(source, i, out n))
                {
                    body.Append(source, i, n);
                    i += n;
                    continue;
                }
                char c = source[i];
                if (c >= '0' && c <= '9')
                {
                    body.Append(c);
                    i++;
                    continue;
                }
                if (c == '/' || c == '\u00D7' || c == '\u25B6')
                {
                    body.Append(c);
                    i++;
                    continue;
                }
                body.Append(Map(c));
                i++;
            }
            int target = (int)Math.Ceiling(source.Length * factor);
            while (body.Length < target) body.Append(Filler);
            return "[" + body.ToString() + "]";
        }

        public static LocTable BuildTable(LocTable english)
        {
            return BuildTable(english, DefaultFactor);
        }

        public static LocTable BuildTable(LocTable english, double factor)
        {
            var table = new LocTable();
            table.Code = Code;
            table.Name = "Pseudo";
            if (english == null) return table;
            foreach (var kv in english.Spec)
                table.Spec[kv.Key] = Expand(kv.Key, factor);
            foreach (var kv in english.Str)
                table.Str[kv.Key] = Expand(kv.Value, factor);
            return table;
        }

        static bool TryPlaceholder(string source, int i, out int n)
        {
            n = 0;
            if (source[i] != '{') return false;
            int j = i + 1;
            if (j >= source.Length || source[j] < '0' || source[j] > '9') return false;
            while (j < source.Length && source[j] >= '0' && source[j] <= '9') j++;
            if (j >= source.Length || source[j] != '}') return false;
            n = j - i + 1;
            return true;
        }

        static bool TryEmoji(string source, int i, out int n)
        {
            n = 0;
            char c = source[i];
            if (char.IsHighSurrogate(c) && i + 1 < source.Length && char.IsLowSurrogate(source[i + 1]))
            {
                n = 2;
                return true;
            }
            if (c >= '\u2600' && c <= '\u27BF')
            {
                n = 1;
                return true;
            }
            return false;
        }

        static char Map(char c)
        {
            switch (c)
            {
                case 'A': return '\u00C0';
                case 'E': return '\u00C9';
                case 'I': return '\u00CD';
                case 'O': return '\u00D3';
                case 'U': return '\u00DA';
                case 'C': return '\u00C7';
                case 'N': return '\u00D1';
                case 'Y': return '\u00DD';
                case 'a': return '\u00E0';
                case 'e': return '\u00E9';
                case 'i': return '\u00ED';
                case 'o': return '\u00F3';
                case 'u': return '\u00FA';
                case 'c': return '\u00E7';
                case 'n': return '\u00F1';
                case 'y': return '\u00FF';
                default: return c;
            }
        }
    }
}
