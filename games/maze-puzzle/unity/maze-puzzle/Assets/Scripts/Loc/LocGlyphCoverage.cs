using System;
using System.Collections.Generic;
using System.Globalization;

namespace OrbitalSalvage.Loc
{
    /// <summary>
    /// Characters in a translation that no face in the chain can draw.
    /// Whitespace, controls, U+FE0F, U+200D, and U+200B are ignored. An emoji grapheme is ignored
    /// when <paramref name="isEmoji"/> says the sprite path draws it. The check is a cmap hit
    /// (<c>hasGlyph</c>), so a Python twin can walk the same code points.
    /// </summary>
    public static class LocGlyphCoverage
    {
        public struct Miss
        {
            public string Kind;
            public string Key;
            public int CodePoint;
        }

        public static void CollectTable(LocTable table, Func<int, bool> hasGlyph, Func<string, bool> isEmoji, IList<Miss> missing)
        {
            if (table == null || hasGlyph == null || missing == null) return;
            Scan(table.Spec, "spec", hasGlyph, isEmoji, missing);
            Scan(table.Str, "str", hasGlyph, isEmoji, missing);
        }

        public static void CollectString(string text, Func<int, bool> hasGlyph, Func<string, bool> isEmoji, IList<int> missing)
        {
            if (string.IsNullOrEmpty(text) || hasGlyph == null || missing == null) return;
            TextElementEnumerator en = StringInfo.GetTextElementEnumerator(text);
            while (en.MoveNext())
            {
                string grapheme = (string)en.Current;
                if (string.IsNullOrEmpty(grapheme)) continue;
                if (isEmoji != null && isEmoji(grapheme)) continue;
                for (int i = 0; i < grapheme.Length; i++)
                {
                    int cp = char.ConvertToUtf32(grapheme, i);
                    if (char.IsHighSurrogate(grapheme[i])) i++;
                    if (Skip(cp)) continue;
                    if (!hasGlyph(cp)) missing.Add(cp);
                }
            }
        }

        static void Scan(Dictionary<string, string> map, string kind, Func<int, bool> hasGlyph, Func<string, bool> isEmoji, IList<Miss> missing)
        {
            if (map == null) return;
            var found = new List<int>(4);
            foreach (KeyValuePair<string, string> kv in map)
            {
                found.Clear();
                CollectString(kv.Value, hasGlyph, isEmoji, found);
                for (int i = 0; i < found.Count; i++)
                {
                    missing.Add(new Miss { Kind = kind, Key = kv.Key, CodePoint = found[i] });
                }
            }
        }

        static bool Skip(int cp)
        {
            if (cp == 0xFE0F || cp == 0x200D || cp == 0x200B) return true;
            if (cp > 0xFFFF) return false;
            char ch = (char)cp;
            return char.IsWhiteSpace(ch) || char.IsControl(ch);
        }
    }
}
