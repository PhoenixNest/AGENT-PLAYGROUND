using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using OrbitalSalvage.JsCompat;

namespace OrbitalSalvage.Loc
{
    /// <summary>
    /// Spec-run normalisation and placeholder formatting.
    /// Normalise trims and collapses every whitespace run (<see cref="char.IsWhiteSpace"/>) to one space.
    /// That string is the spec lookup key. Translations are stored and returned trimmed; the caller
    /// keeps the original run, including its layout spaces, when lookup fails. English always fails
    /// lookup so those spaces stay on the English run.
    /// </summary>
    public static class LocText
    {
        public static string Normalize(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            var sb = new StringBuilder(text.Length);
            bool pendingSpace = false;
            bool any = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (char.IsWhiteSpace(c))
                {
                    pendingSpace = true;
                    continue;
                }
                if (pendingSpace && any) sb.Append(' ');
                pendingSpace = false;
                any = true;
                sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>No Unicode letter: digits, joiners, symbols, and emoji-only runs.</summary>
        public static bool HasLetter(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsLetter(text[i])) return true;
            }
            return false;
        }

        public static bool HasDigit(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c >= '0' && c <= '9') return true;
            }
            return false;
        }

        /// <summary>
        /// A surrogate pair, or a BMP code point in Miscellaneous Symbols or Dingbats.
        /// Geometric shapes such as ▶ are symbols, not emoji.
        /// </summary>
        public static bool ContainsEmoji(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (char.IsHighSurrogate(c)) return true;
                if (c >= '\u2600' && c <= '\u27BF') return true;
            }
            return false;
        }

        public static bool HasTerminalPunctuation(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            char c = text[text.Length - 1];
            return c == '.' || c == '!' || c == '?' || c == '\u2026'
                || c == '\u3002' || c == '\uFF01' || c == '\uFF1F' || c == '\u061F';
        }

        /// <summary>
        /// <c>{n}</c> placeholders. A doubled brace <c>{{</c> is an escape and is not a placeholder.
        /// The set is the distinct index strings, so <c>{0}{0}</c> is just <c>0</c>.
        /// </summary>
        public static HashSet<string> PlaceholderIndexes(string text)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            if (string.IsNullOrEmpty(text)) return set;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != '{') continue;
                if (i + 1 < text.Length && text[i + 1] == '{')
                {
                    i++;
                    continue;
                }
                int j = i + 1;
                if (j >= text.Length || text[j] < '0' || text[j] > '9') continue;
                int start = j;
                while (j < text.Length && text[j] >= '0' && text[j] <= '9') j++;
                if (j < text.Length && text[j] == '}')
                {
                    set.Add(text.Substring(start, j - start));
                    i = j;
                }
            }
            return set;
        }

        public static bool SamePlaceholders(string a, string b)
        {
            HashSet<string> left = PlaceholderIndexes(a);
            HashSet<string> right = PlaceholderIndexes(b);
            if (left.Count != right.Count) return false;
            foreach (string index in left)
            {
                if (!right.Contains(index)) return false;
            }
            return true;
        }

        public static string Format(string template, object[] args)
        {
            if (template == null) template = "";
            if (args == null || args.Length == 0) return template;
            object[] prepared = PrepareArgs(args);
            try
            {
                return string.Format(CultureInfo.InvariantCulture, template, prepared);
            }
            catch (FormatException)
            {
                return template;
            }
        }

        static object[] PrepareArgs(object[] args)
        {
            var copy = new object[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                object value = args[i];
                if (value is double)
                {
                    copy[i] = JsNumber.ToString((double)value);
                }
                else if (value is float)
                {
                    copy[i] = JsNumber.ToString((float)value);
                }
                else if (value is int || value is long || value is short || value is byte || value is uint || value is ulong)
                {
                    copy[i] = Convert.ToString(value, CultureInfo.InvariantCulture);
                }
                else if (value is decimal)
                {
                    copy[i] = JsNumber.ToString((double)(decimal)value);
                }
                else
                {
                    copy[i] = value;
                }
            }
            return copy;
        }
    }
}
