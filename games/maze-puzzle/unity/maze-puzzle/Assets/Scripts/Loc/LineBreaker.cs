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
                    BreakPieces(lines[i], width, measure, extra);
                }
                if (!split) return;
                lines.Clear();
                for (int i = 0; i < extra.Count; i++) lines.Add(extra[i]);
                if (lines.Count == 0) lines.Add(text ?? "");
            }

            /// <summary>
            /// A single space-delimited word that is wider than the line. Hangul breaks per syllable.
            /// A number or a Latin word stays whole unless that token alone is wider than the line.
            /// </summary>
            static void BreakPieces(string word, double width, Func<string, double> measure, List<string> dest)
            {
                var pieces = new List<Piece>();
                Collect(word, pieces);
                if (pieces.Count == 0)
                {
                    dest.Add(word ?? "");
                    return;
                }
                string cur = "";
                for (int i = 0; i < pieces.Count; i++)
                {
                    string atom = pieces[i].Text;
                    if (measure(atom) > width + 0.5)
                    {
                        if (cur.Length > 0)
                        {
                            dest.Add(cur);
                            cur = "";
                        }
                        SplitChars(atom, width, measure, dest);
                        continue;
                    }
                    string trial = cur.Length == 0 ? atom : (pieces[i].SpaceBefore ? cur + " " + atom : cur + atom);
                    if (cur.Length > 0 && measure(trial) > width + 0.5)
                    {
                        dest.Add(cur);
                        cur = atom;
                    }
                    else cur = trial;
                }
                if (cur.Length > 0) dest.Add(cur);
            }

            static void SplitChars(string atom, double width, Func<string, double> measure, List<string> dest)
            {
                string cur = "";
                int i = 0;
                while (i < atom.Length)
                {
                    int n = CharLen(atom, i);
                    string cp = atom.Substring(i, n);
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
            public void Wrap(string text, double width, double slack, Func<string, double> measure, List<string> lines)
            {
                lines.Clear();
                if (text == null) text = "";
                if (text.Length == 0 || measure(text) <= width + slack) { lines.Add(text); return; }
                var pieces = new List<Piece>();
                Collect(text, pieces);
                if (pieces.Count == 0) { lines.Add(text); return; }
                int i = 0;
                while (i < pieces.Count)
                {
                    int best = i;
                    string built = pieces[i].Text;
                    int k = i + 1;
                    while (k < pieces.Count)
                    {
                        string trial = built + (pieces[k].SpaceBefore ? " " : "") + pieces[k].Text;
                        if (measure(trial) > width + 0.5) break;
                        built = trial;
                        best = k;
                        k++;
                    }
                    int next = best + 1;
                    if (next < pieces.Count && !CanBreak(pieces[best], pieces[next]))
                    {
                        int cut = best;
                        bool found = false;
                        while (cut > i)
                        {
                            if (CanBreak(pieces[cut - 1], pieces[cut]))
                            {
                                best = cut - 1;
                                built = Join(pieces, i, best);
                                found = true;
                                break;
                            }
                            cut--;
                        }
                        if (!found)
                        {
                            while (next < pieces.Count && !CanBreak(pieces[best], pieces[next]))
                            {
                                built += (pieces[next].SpaceBefore ? " " : "") + pieces[next].Text;
                                best = next;
                                next++;
                            }
                        }
                    }
                    lines.Add(built);
                    i = best + 1;
                }
                if (lines.Count == 0) lines.Add(text);
            }
        }

        // Closing punctuation, the ellipsis, the long-vowel mark, small kana, and 々
        // may not start a line. Opening brackets may not end a line.
        const string NoStart = "，。、．！？：；）」』】》〉…ーぁぃぅぇぉっゃゅょゎァィゥェォッャュョヮ々";
        const string NoEnd = "（「『【《〈";

        struct Piece
        {
            public string Text;
            public bool SpaceBefore;
            public Piece(string text, bool spaceBefore)
            {
                Text = text;
                SpaceBefore = spaceBefore;
            }
        }

        static void Collect(string text, List<Piece> dest)
        {
            var atoms = new List<string>();
            Tokenize(text, atoms);
            bool started = false;
            bool space = false;
            for (int i = 0; i < atoms.Count; i++)
            {
                if (atoms[i] == " ")
                {
                    if (started) space = true;
                    continue;
                }
                dest.Add(new Piece(atoms[i], space && started));
                space = false;
                started = true;
            }
        }

        static void Tokenize(string text, List<string> atoms)
        {
            int i = 0;
            while (i < text.Length)
            {
                char c = text[i];
                if (c == ' ' || c == '\n' || c == '\t' || c == '\u3000')
                {
                    atoms.Add(" ");
                    i++;
                    continue;
                }
                if (c == '{' && i + 2 < text.Length && IsDigit(text[i + 1]))
                {
                    int j = i + 1;
                    while (j < text.Length && IsDigit(text[j])) j++;
                    if (j < text.Length && text[j] == '}')
                    {
                        atoms.Add(text.Substring(i, j + 1 - i));
                        i = j + 1;
                        continue;
                    }
                }
                if (c == '\u2026' && i + 1 < text.Length && text[i + 1] == '\u2026')
                {
                    atoms.Add(text.Substring(i, 2));
                    i += 2;
                    continue;
                }
                if (c == '\u2014' && i + 1 < text.Length && text[i + 1] == '\u2014')
                {
                    atoms.Add(text.Substring(i, 2));
                    i += 2;
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

        /// <summary>Western digits, grouped with <c>,</c> <c>.</c> and a trailing <c>%</c>, plus a following Latin unit.</summary>
        static int ConsumeNumber(string text, int i)
        {
            int j = i;
            while (j < text.Length && IsDigit(text[j])) j++;
            while (j + 1 < text.Length && (text[j] == '.' || text[j] == ',') && IsDigit(text[j + 1]))
            {
                j++;
                while (j < text.Length && IsDigit(text[j])) j++;
            }
            if (j < text.Length && text[j] == '%') j++;
            int u = j;
            if (u < text.Length && text[u] == ' ' && u + 1 < text.Length && IsLatinLetter(text[u + 1])) u++;
            if (u < text.Length && IsLatinLetter(text[u]) && (u == j || text[j] == ' '))
            {
                while (u < text.Length && IsLatinLetter(text[u])) u++;
                j = u;
            }
            return j;
        }

        static bool CanBreak(Piece left, Piece right)
        {
            if (EndsForbidden(left.Text)) return false;
            if (StartsForbidden(right.Text)) return false;
            return true;
        }

        static bool StartsForbidden(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            return NoStart.IndexOf(FirstChar(text), StringComparison.Ordinal) >= 0;
        }

        static bool EndsForbidden(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            return NoEnd.IndexOf(LastChar(text), StringComparison.Ordinal) >= 0;
        }

        static string Join(List<Piece> pieces, int from, int to)
        {
            string s = "";
            for (int i = from; i <= to; i++)
            {
                if (i > from && pieces[i].SpaceBefore) s += " ";
                s += pieces[i].Text;
            }
            return s;
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
