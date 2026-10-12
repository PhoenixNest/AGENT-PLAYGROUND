using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace OrbitalSalvage.Loc.Tests
{
    /// <summary>
    /// Scans the UI-facing sources for string literals that look like player English.
    /// A hit must be an exact row in <c>localization/hardcoded-allowlist.txt</c>
    /// (literal, path relative to Assets, reason). Dot-keys, font stacks, colors, paths,
    /// and URLs are not player copy. Editor, Tests, and Parity are not scanned.
    /// <c>Scripts/Render/Game/LiveConformance.cs</c> is a file-level skip in the allowlist.
    /// </summary>
    public class LocHardcodedStringTests
    {
        static readonly string[] Files =
        {
            "Scripts/App/AppController.cs",
            "Scripts/Logic/DailyReset.cs",
            "Scripts/Render/Game/GameRenderer.cs",
            "Scripts/Render/Game/GameRenderer.Ring.cs",
            "Scripts/Render/Game/GameRenderer.Gates.cs",
            "Scripts/Render/Game/GameRenderer.World.cs",
            "Scripts/Render/Game/GameDraw.cs",
            "Scripts/Render/Game/LiveGameSession.cs",
            "Scripts/Render/Game/GameplayLabels.cs",
            "Scripts/Render/Game/LiveConformance.cs",
        };

        [Test]
        public void UiSources_HaveNoUnlistedEnglishLiterals()
        {
            string assets = Application.dataPath;
            string listPath = Path.GetFullPath(Path.Combine(assets, "..", "..", "..", "localization", "hardcoded-allowlist.txt"));
            Assert.IsTrue(File.Exists(listPath), listPath);
            List<Row> allow = ReadAllow(listPath);
            var english = new HashSet<string>(StringComparer.Ordinal);
            var defs = LocStrings.All;
            for (int i = 0; i < defs.Length; i++) english.Add(defs[i].English);

            var failures = new List<string>();
            var used = new HashSet<int>();
            for (int f = 0; f < Files.Length; f++)
            {
                string rel = Files[f];
                string path = Path.Combine(assets, rel);
                Assert.IsTrue(File.Exists(path), path);
                if (FileSkipped(allow, rel, used)) continue;
                List<Lit> lits = Scan(File.ReadAllText(path));
                for (int i = 0; i < lits.Count; i++)
                {
                    Lit lit = lits[i];
                    if (!Visible(lit.Text) && !english.Contains(lit.Text)) continue;
                    int row = Find(allow, lit.Text, rel);
                    if (row < 0)
                        failures.Add(rel + ":" + lit.Line + ": " + lit.Text);
                    else
                        used.Add(row);
                }
            }
            for (int i = 0; i < allow.Count; i++)
            {
                if (!used.Contains(i))
                    failures.Add("unused allowlist row: " + allow[i].Literal + " @ " + allow[i].Path);
            }
            if (failures.Count > 0) Assert.Fail(string.Join("\n", failures.ToArray()));
        }

        static bool FileSkipped(List<Row> allow, string rel, HashSet<int> used)
        {
            for (int i = 0; i < allow.Count; i++)
            {
                if (allow[i].Literal != "*") continue;
                if (!string.Equals(allow[i].Path, rel, StringComparison.Ordinal)) continue;
                used.Add(i);
                return true;
            }
            return false;
        }

        static int Find(List<Row> allow, string literal, string rel)
        {
            for (int i = 0; i < allow.Count; i++)
            {
                if (allow[i].Literal == "*") continue;
                if (allow[i].Literal == literal && allow[i].Path == rel) return i;
            }
            return -1;
        }

        static bool Visible(string text)
        {
            if (string.IsNullOrEmpty(text) || !LocText.HasLetter(text)) return false;
            if (IsDotKey(text)) return false;
            if (text.IndexOf("px", StringComparison.Ordinal) >= 0) return false;
            if (text.IndexOf("sans-serif", StringComparison.Ordinal) >= 0) return false;
            if (text.IndexOf("rgba", StringComparison.Ordinal) >= 0) return false;
            if (text.IndexOf("http", StringComparison.Ordinal) >= 0) return false;
            if (text[0] == '#') return false;
            if (text.IndexOf('/') >= 0 || text.IndexOf('\\') >= 0) return false;
            if (text.IndexOf(".json", StringComparison.Ordinal) >= 0) return false;
            if (text.IndexOf(".png", StringComparison.Ordinal) >= 0) return false;
            if (HasSpace(text) || HasSentence(text) || LocText.ContainsEmoji(text)) return true;
            return false;
        }

        static bool IsDotKey(string text)
        {
            int dots = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '.') { dots++; continue; }
                bool ok = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_';
                if (!ok) return false;
            }
            return dots > 0;
        }

        static bool HasSpace(string text)
        {
            for (int i = 0; i < text.Length; i++)
                if (char.IsWhiteSpace(text[i])) return true;
            return false;
        }

        static bool HasSentence(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '.' || c == '!' || c == '?' || c == '\u2026') return true;
            }
            return false;
        }

        static List<Lit> Scan(string text)
        {
            var list = new List<Lit>();
            int i = 0;
            int line = 1;
            while (i < text.Length)
            {
                char c = text[i];
                if (c == '\n') { line++; i++; continue; }
                if (c == '/' && i + 1 < text.Length && text[i + 1] == '/')
                {
                    while (i < text.Length && text[i] != '\n') i++;
                    continue;
                }
                if (c == '/' && i + 1 < text.Length && text[i + 1] == '*')
                {
                    i += 2;
                    while (i + 1 < text.Length && !(text[i] == '*' && text[i + 1] == '/'))
                    {
                        if (text[i] == '\n') line++;
                        i++;
                    }
                    i = Math.Min(text.Length, i + 2);
                    continue;
                }
                if (c == '@' && i + 1 < text.Length && text[i + 1] == '"')
                {
                    int start = line;
                    i += 2;
                    var sb = new StringBuilder();
                    while (i < text.Length)
                    {
                        if (text[i] == '"')
                        {
                            if (i + 1 < text.Length && text[i + 1] == '"') { sb.Append('"'); i += 2; continue; }
                            i++;
                            break;
                        }
                        if (text[i] == '\n') line++;
                        sb.Append(text[i]);
                        i++;
                    }
                    list.Add(new Lit { Line = start, Text = sb.ToString() });
                    continue;
                }
                if (c == '"')
                {
                    int start = line;
                    i++;
                    var raw = new StringBuilder();
                    while (i < text.Length)
                    {
                        if (text[i] == '\\' && i + 1 < text.Length)
                        {
                            raw.Append('\\');
                            raw.Append(text[i + 1]);
                            i += 2;
                            continue;
                        }
                        if (text[i] == '"') { i++; break; }
                        if (text[i] == '\n') line++;
                        raw.Append(text[i]);
                        i++;
                    }
                    list.Add(new Lit { Line = start, Text = Decode(raw.ToString()) });
                    continue;
                }
                i++;
            }
            return list;
        }

        static string Decode(string raw)
        {
            var sb = new StringBuilder(raw.Length);
            for (int i = 0; i < raw.Length; i++)
            {
                char c = raw[i];
                if (c != '\\' || i + 1 >= raw.Length) { sb.Append(c); continue; }
                char n = raw[++i];
                if (n == 'n') sb.Append('\n');
                else if (n == 'r') sb.Append('\r');
                else if (n == 't') sb.Append('\t');
                else if (n == '"' || n == '\\') sb.Append(n);
                else if (n == 'u' && i + 4 < raw.Length)
                {
                    sb.Append((char)Convert.ToInt32(raw.Substring(i + 1, 4), 16));
                    i += 4;
                }
                else sb.Append(n);
            }
            return sb.ToString();
        }

        static List<Row> ReadAllow(string path)
        {
            var rows = new List<Row>();
            string[] lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.Length == 0 || line[0] == '#') continue;
                string[] parts = line.Split('\t');
                Assert.AreEqual(3, parts.Length, "allowlist line " + (i + 1));
                Assert.IsNotEmpty(parts[2], "allowlist reason " + (i + 1));
                rows.Add(new Row { Literal = parts[0], Path = parts[1], Reason = parts[2] });
            }
            return rows;
        }

        struct Lit
        {
            public int Line;
            public string Text;
        }

        struct Row
        {
            public string Literal;
            public string Path;
            public string Reason;
        }
    }
}
