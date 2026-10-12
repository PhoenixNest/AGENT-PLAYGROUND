using System;
using System.Collections.Generic;

namespace OrbitalSalvage.Loc
{
    /// <summary>Wraps one string into lines. <paramref name="measure"/> is the width of a candidate line in CSS px.</summary>
    public interface ILineBreaker
    {
        void Wrap(string text, double width, double slack, Func<string, double> measure, List<string> lines);
    }

    /// <summary>
    /// Line breaking per language. Latin matches <c>HomeChrome.WrapWords</c> (split on U+0020 only).
    /// Khmer, Arabic, and Hindi return that Latin breaker until shaping exists.
    /// </summary>
    public static class LineBreaker
    {
        public static readonly ILineBreaker Latin = new LatinBreaker();
        public static readonly ILineBreaker Cjk = new CjkBreaker();
        public static readonly ILineBreaker Korean = new KoreanBreaker();

        public static ILineBreaker For(string languageCode)
        {
            if (languageCode == PseudoLocale.Code) return Latin;
            LanguageInfo info;
            if (languageCode == null || !LanguageRegistry.TryGet(languageCode, out info)) return Latin;
            switch (info.LineBreak)
            {
                case LineBreakKind.Cjk: return Cjk;
                case LineBreakKind.Korean: return Korean;
                case LineBreakKind.Complex:
                    // TODO: Khmer, Arabic, and Hindi wrap on shaped clusters. Latin spaces are a stand-in.
                    return Latin;
                default: return Latin;
            }
        }

        /// <summary>The English wrap, copied from HomeChrome.WrapWords. Empty pieces from Split are dropped.</summary>
        public static void WrapLatin(string text, double width, double slack, Func<string, double> measure, List<string> lines)
        {
            lines.Clear();
            if (text == null) text = "";
            if (measure(text) <= width + slack) { lines.Add(text); return; }
            string cur = "";
            string[] words = text.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                if (word.Length == 0) continue;
                string trial = cur.Length == 0 ? word : cur + " " + word;
                if (cur.Length > 0 && measure(trial) > width + 0.5)
                {
                    lines.Add(cur);
                    cur = word;
                }
                else cur = trial;
            }
            if (cur.Length > 0) lines.Add(cur);
            if (lines.Count == 0) lines.Add(text);
        }

        sealed class LatinBreaker : ILineBreaker
        {
            public void Wrap(string text, double width, double slack, Func<string, double> measure, List<string> lines)
            {
                WrapLatin(text, width, slack, measure, lines);
            }
        }

        sealed class KoreanBreaker : ILineBreaker
        {
            public void Wrap(string text, double width, double slack, Func<string, double> measure, List<string> lines)
            {
                WrapLatin(text, width, slack, measure, lines);
                var extra = new List<string>();
                bool split = false;
                for (int i = 0; i < lines.Count; i++)
                {
                    if (measure(lines[i]) <= width + 0.5)
                    {
                        extra.Add(lines[i]);
                        continue;
                    }
                    split = true;
                    BreakSyllables(lines[i], width, measure, extra);
                }
                if (!split) return;
                lines.Clear();
                for (int i = 0; i < extra.Count; i++) lines.Add(extra[i]);
                if (lines.Count == 0) lines.Add(text ?? "");
            }

            static void BreakSyllables(string word, double width, Func<string, double> measure, List<string> dest)
            {
                string cur = "";
                int i = 0;
                while (i < word.Length)
                {
                    int n = CharLen(word, i);
                    string cp = word.Substring(i, n);
                    string trial = cur.Length == 0 ? cp : cur + cp;
                    if (cur.Length > 0 && measure(trial) > width + 0.5)
                    {
                        dest.Add(cur);
                        cur = cp;
                    }
                    else cur = trial;
                    i += n;
                }
                if (cur.Length > 0) dest.Add(cur);
            }
        }

        sealed class CjkBreaker : ILineBreaker
        {
            // No line start: closing punctuation, small kana, long-vowel mark.
            const string NoStart = "、。，．：；！？）」』】》〉」,.!?:;)]}»”’ゃゅょっぁぃぅぇぉャュョッァィゥェォー";
            // No line end: opening punctuation.
            const string NoEnd = "（「『【《〈([{«“‘";

            public void Wrap(string text, double width, double slack, Func<string, double> measure, List<string> lines)
            {
                lines.Clear();
                if (text == null) text = "";
                if (text.Length == 0 || measure(text) <= width + slack) { lines.Add(text); return; }
                var atoms = new List<string>();
                Tokenize(text, atoms);
                string cur = "";
                bool pendingSpace = false;
                for (int i = 0; i < atoms.Count; i++)
                {
                    string atom = atoms[i];
                    if (atom == " ")
                    {
                        if (cur.Length > 0) pendingSpace = true;
                        continue;
                    }
                    string trial = cur.Length == 0 ? atom : (pendingSpace ? cur + " " + atom : cur + atom);
                    pendingSpace = false;
                    if (cur.Length > 0 && measure(trial) > width + 0.5)
                    {
                        lines.Add(cur);
                        cur = atom;
                    }
                    else cur = trial;
                }
                if (cur.Length > 0) lines.Add(cur);
                if (lines.Count == 0) lines.Add(text);
                ApplyKinsoku(lines);
            }

            static void Tokenize(string text, List<string> atoms)
            {
                int i = 0;
                while (i < text.Length)
                {
                    char c = text[i];
                    if (c == ' ' || c == '\n' || c == '\t')
                    {
                        atoms.Add(" ");
                        i++;
                        continue;
                    }
                    if (IsDigit(c))
                    {
                        int j = ConsumeNumber(text, i);
                        atoms.Add(text.Substring(i, j - i));
                        i = j;
                        continue;
                    }
                    if (IsLatinLetter(c))
                    {
                        int j = i + 1;
                        while (j < text.Length && IsLatinLetter(text[j])) j++;
                        atoms.Add(text.Substring(i, j - i));
                        i = j;
                        continue;
                    }
                    int n = CharLen(text, i);
                    atoms.Add(text.Substring(i, n));
                    i += n;
                }
            }

            static int ConsumeNumber(string text, int i)
            {
                int j = i;
                while (j < text.Length && IsDigit(text[j])) j++;
                if (j + 1 < text.Length && (text[j] == '.' || text[j] == ',') && IsDigit(text[j + 1]))
                {
                    j++;
                    while (j < text.Length && IsDigit(text[j])) j++;
                }
                int u = j;
                if (u < text.Length && text[u] == ' ' && u + 1 < text.Length && IsLatinLetter(text[u + 1])) u++;
                if (u < text.Length && IsLatinLetter(text[u]))
                {
                    while (u < text.Length && IsLatinLetter(text[u])) u++;
                    j = u;
                }
                return j;
            }

            static void ApplyKinsoku(List<string> lines)
            {
                for (int pass = 0; pass < 8; pass++)
                {
                    bool changed = false;
                    for (int i = 1; i < lines.Count; i++)
                    {
                        if (lines[i].Length == 0) continue;
                        if (lines[i - 1].Length > 0 && NoEnd.IndexOf(LastChar(lines[i - 1])) >= 0)
                        {
                            string ch = LastChar(lines[i - 1]);
                            lines[i - 1] = lines[i - 1].Substring(0, lines[i - 1].Length - ch.Length);
                            lines[i] = ch + lines[i];
                            changed = true;
                        }
                        if (lines[i].Length > 0 && NoStart.IndexOf(FirstChar(lines[i])) >= 0 && lines[i - 1].Length > 0)
                        {
                            string ch = FirstChar(lines[i]);
                            lines[i - 1] = lines[i - 1] + ch;
                            lines[i] = lines[i].Substring(ch.Length);
                            changed = true;
                        }
                    }
                    for (int i = lines.Count - 1; i >= 0; i--)
                        if (lines[i].Length == 0) lines.RemoveAt(i);
                    if (!changed) break;
                }
                if (lines.Count == 0) lines.Add("");
            }

            static string FirstChar(string s)
            {
                return s.Substring(0, CharLen(s, 0));
            }

            static string LastChar(string s)
            {
                if (s.Length >= 2 && char.IsLowSurrogate(s[s.Length - 1]) && char.IsHighSurrogate(s[s.Length - 2]))
                    return s.Substring(s.Length - 2, 2);
                return s.Substring(s.Length - 1, 1);
            }
        }

        static int CharLen(string s, int i)
        {
            if (i + 1 < s.Length && char.IsHighSurrogate(s[i]) && char.IsLowSurrogate(s[i + 1])) return 2;
            return 1;
        }

        static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        static bool IsLatinLetter(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '\u00C0' && c <= '\u00FF');
        }
    }
}
