using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace OrbitalSalvage.Loc.Tests
{
    public class LineBreakerTests : LocTestBase
    {
        static double Width(string text)
        {
            if (text == null) return 0;
            double n = 0;
            for (int i = 0; i < text.Length; i++) n += text[i] == ' ' ? 4 : 10;
            return n;
        }

        /// <summary>The WrapWords body as it stood before the breaker, so a drift fails this test.</summary>
        static void Legacy(string text, double width, double slack, Func<string, double> measure, List<string> lines)
        {
            lines.Clear();
            if (measure(text) <= width + slack) { lines.Add(text); return; }
            string cur = "";
            foreach (var word in text.Split(' '))
            {
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

        [Test]
        public void Latin_MatchesLegacyWrapWords()
        {
            string[] samples =
            {
                "PLAY MISSION",
                "Goal: Feed Nibbles 20 kg space snacks",
                "LVL 1 • Bubblegum Chute 1",
                "a",
                "one two three four",
                "   spaced   out  ",
                "",
                "CannotSplitThisWord",
                "short",
                "Mission paused for a very long reason today"
            };
            double[] widths = { 40, 80, 120, 200, 1000 };
            var legacy = new List<string>();
            var live = new List<string>();
            for (int s = 0; s < samples.Length; s++)
            {
                for (int w = 0; w < widths.Length; w++)
                {
                    Legacy(samples[s], widths[w], Math.Max(8, widths[w] * 0.08), Width, legacy);
                    LineBreaker.WrapLatin(samples[s], widths[w], Math.Max(8, widths[w] * 0.08), Width, live);
                    CollectionAssert.AreEqual(legacy, live, samples[s] + " @ " + widths[w]);
                }
            }
            Assert.AreSame(LineBreaker.Latin, LineBreaker.For("en"));
            Assert.AreSame(LineBreaker.Latin, LineBreaker.For("de"));
            Assert.AreSame(LineBreaker.Latin, LineBreaker.For("qps"));
            Assert.AreSame(LineBreaker.Latin, LineBreaker.For("ar"));
            Assert.AreSame(LineBreaker.Latin, LineBreaker.For("hi"));
            Assert.AreSame(LineBreaker.Latin, LineBreaker.For("km"));
            Assert.AreSame(LineBreaker.Cjk, LineBreaker.For("zh-Hans"));
            Assert.AreSame(LineBreaker.Cjk, LineBreaker.For("ja"));
            Assert.AreSame(LineBreaker.Korean, LineBreaker.For("ko"));
        }

        [Test]
        public void Cjk_BreaksBetweenCharactersAndKeepsTokens()
        {
            var lines = new List<string>();
            LineBreaker.Cjk.Wrap("你好世界", 20, 0, Width, lines);
            CollectionAssert.AreEqual(new[] { "你好", "世界" }, lines);

            LineBreaker.Cjk.Wrap("你好。世界", 20, 0, Width, lines);
            Assert.IsFalse(lines[1].StartsWith("。"), string.Join("|", lines));

            LineBreaker.Cjk.Wrap("（你好）", 20, 0, Width, lines);
            Assert.IsFalse(lines[0].EndsWith("（"), string.Join("|", lines));

            LineBreaker.Cjk.Wrap("きゃ", 10, 0, Width, lines);
            Assert.AreEqual(1, lines.Count, string.Join("|", lines));
            Assert.AreEqual("きゃ", lines[0]);

            LineBreaker.Cjk.Wrap("重量20kg以上", 40, 0, Width, lines);
            Assert.IsTrue(string.Join("", lines).Contains("20kg"), string.Join("|", lines));
            foreach (string line in lines)
                Assert.IsFalse(line.Contains("20k") && !line.Contains("20kg") && line.EndsWith("20k"), line);

            LineBreaker.Cjk.Wrap("Hello世界", 50, 0, Width, lines);
            Assert.IsTrue(lines[0].StartsWith("Hello"), string.Join("|", lines));
        }

        [TestCase("zh-Hans")]
        [TestCase("zh-Hant")]
        [TestCase("ja")]
        public void Cjk_KinsokuKeepsNumbersLatinAndMarks(string code)
        {
            Assert.AreSame(LineBreaker.Cjk, LineBreaker.For(code));
            var lines = new List<string>();
            ILineBreaker breaker = LineBreaker.For(code);

            breaker.Wrap("你好。世界", 20, 0, Width, lines);
            AssertNoStart(lines, "。");

            breaker.Wrap("（你好）", 20, 0, Width, lines);
            AssertNoEnd(lines, "（");

            breaker.Wrap("きゃっ", 10, 0, Width, lines);
            Assert.AreEqual("きゃっ", lines[0]);
            AssertNoStart(lines, "ゃ");
            AssertNoStart(lines, "っ");

            breaker.Wrap("花々", 10, 0, Width, lines);
            Assert.AreEqual("花々", string.Join("", lines));
            AssertNoStart(lines, "々");

            breaker.Wrap("他说……完了", 20, 0, Width, lines);
            Assert.IsTrue(JoinedOnOneLine(lines, "……"), string.Join("|", lines));

            breaker.Wrap("走——停", 10, 0, Width, lines);
            Assert.IsTrue(JoinedOnOneLine(lines, "——"), string.Join("|", lines));

            breaker.Wrap("获得1,234.50%个", 30, 0, Width, lines);
            Assert.IsTrue(JoinedOnOneLine(lines, "1,234.50%"), string.Join("|", lines));

            breaker.Wrap("分数{0}点", 20, 0, Width, lines);
            Assert.IsTrue(JoinedOnOneLine(lines, "{0}"), string.Join("|", lines));

            breaker.Wrap("看Nibbles 20 kg吧", 40, 0, Width, lines);
            Assert.IsTrue(JoinedOnOneLine(lines, "Nibbles"), string.Join("|", lines));
            Assert.IsTrue(JoinedOnOneLine(lines, "20 kg"), string.Join("|", lines));

            breaker.Wrap("目标：喂", 20, 0, Width, lines);
            AssertNoStart(lines, "：");
        }

        [Test]
        public void Korean_BreaksOnSpacesThenSyllables_AndKeepsLatin()
        {
            var lines = new List<string>();
            LineBreaker.Korean.Wrap("안녕 하세요", 50, 0, Width, lines);
            CollectionAssert.AreEqual(new[] { "안녕", "하세요" }, lines);

            LineBreaker.Korean.Wrap("가나다라마바", 20, 0, Width, lines);
            Assert.Greater(lines.Count, 1);
            Assert.AreEqual("가나다라마바", string.Join("", lines));
            foreach (string line in lines) Assert.LessOrEqual(Width(line), 20.5);

            LineBreaker.Korean.Wrap("안녕Hello세요", 60, 0, Width, lines);
            Assert.IsTrue(JoinedOnOneLine(lines, "Hello"), string.Join("|", lines));
            Assert.IsFalse(lines.Exists(line => line.Contains("Hel") && !line.Contains("Hello")));

            LineBreaker.Korean.Wrap("가격1,234원", 60, 0, Width, lines);
            Assert.IsTrue(JoinedOnOneLine(lines, "1,234"), string.Join("|", lines));
        }

        [Test]
        public void LatinLanguages_StayOnWrapLatin()
        {
            string[] samples =
            {
                "PLAY MISSION",
                "Goal: Feed Nibbles 20 kg space snacks",
                "one two three four"
            };
            var latin = new List<string>();
            var via = new List<string>();
            string[] codes = { "en", "de", "fr", "es", "it", "pt-PT", "vi", "fil", "id", "uz" };
            for (int c = 0; c < codes.Length; c++)
            {
                for (int s = 0; s < samples.Length; s++)
                {
                    LineBreaker.WrapLatin(samples[s], 80, 8, Width, latin);
                    LineBreaker.For(codes[c]).Wrap(samples[s], 80, 8, Width, via);
                    CollectionAssert.AreEqual(latin, via, codes[c] + " " + samples[s]);
                }
            }
        }

        static void AssertNoStart(List<string> lines, string mark)
        {
            for (int i = 1; i < lines.Count; i++)
                Assert.IsFalse(lines[i].StartsWith(mark), string.Join("|", lines));
        }

        static void AssertNoEnd(List<string> lines, string mark)
        {
            for (int i = 0; i < lines.Count - 1; i++)
                Assert.IsFalse(lines[i].EndsWith(mark), string.Join("|", lines));
        }

        static bool JoinedOnOneLine(List<string> lines, string token)
        {
            for (int i = 0; i < lines.Count; i++)
                if (lines[i].Contains(token)) return true;
            return false;
        }

        [Test]
        public void Korean_BreaksOnSpacesThenSyllables()
        {
            var lines = new List<string>();
            LineBreaker.Korean.Wrap("안녕 하세요", 50, 0, Width, lines);
            CollectionAssert.AreEqual(new[] { "안녕", "하세요" }, lines);

            LineBreaker.Korean.Wrap("가나다라마바", 20, 0, Width, lines);
            Assert.Greater(lines.Count, 1);
            Assert.AreEqual("가나다라마바", string.Join("", lines));
            foreach (string line in lines) Assert.LessOrEqual(Width(line), 20.5);
        }

        [Test]
        public void Registry_LineBreakKinds()
        {
            Assert.AreEqual(LineBreakKind.Latin, Kind("en"));
            Assert.AreEqual(LineBreakKind.Cjk, Kind("zh-Hans"));
            Assert.AreEqual(LineBreakKind.Cjk, Kind("zh-Hant"));
            Assert.AreEqual(LineBreakKind.Cjk, Kind("ja"));
            Assert.AreEqual(LineBreakKind.Korean, Kind("ko"));
            Assert.AreEqual(LineBreakKind.Complex, Kind("km"));
            Assert.AreEqual(LineBreakKind.Complex, Kind("hi"));
            Assert.AreEqual(LineBreakKind.Complex, Kind("ar"));
            Assert.AreEqual(17, LanguageRegistry.All.Count);
            Assert.IsFalse(LanguageRegistry.IsSupported("qps"));
        }

        static LineBreakKind Kind(string code)
        {
            LanguageInfo info;
            Assert.IsTrue(LanguageRegistry.TryGet(code, out info));
            return info.LineBreak;
        }
    }

    public class LocCaseTests : LocTestBase
    {
        [Test]
        public void Upper_LatinScripts_AndCaselessTokens()
        {
            Assert.AreEqual("STRASSE", LocCase.Upper("straße", "de"));
            Assert.AreEqual("GROSS", LocCase.Upper("groß", "de"));
            Assert.AreEqual("RESUME", LocCase.Upper("Resume", "fr"));

            string vi = "Tiếng Việt";
            string upper = LocCase.Upper(vi, "vi");
            Assert.AreEqual(vi.ToLowerInvariant(), upper.ToLowerInvariant());
            Assert.AreEqual("Ệ", LocCase.Upper("ệ", "vi"));
            Assert.AreEqual("ệ", LocCase.Upper("ệ", "vi").ToLowerInvariant());

            string mark = "e\u0301";
            string marked = LocCase.Upper(mark, "fr");
            Assert.AreEqual("E\u0301", marked);
            Assert.IsTrue(marked.IndexOf('\u0301') >= 0);

            string uz = "o\u02BBzbek o\u02BCza";
            string uzUp = LocCase.Upper(uz, "uz");
            Assert.IsTrue(uzUp.IndexOf('\u02BB') >= 0, uzUp);
            Assert.IsTrue(uzUp.IndexOf('\u02BC') >= 0, uzUp);
            Assert.AreEqual("O\u02BBZBEK O\u02BCZA", uzUp);

            Assert.AreEqual("20 kg", LocCase.Upper("20 kg", "zh-Hans"));
            Assert.AreEqual("Play", LocCase.Upper("Play", "ja"));
            Assert.AreEqual("Brand", LocCase.Upper("Brand", "ar"));
            Assert.AreEqual("kg", LocCase.Upper("kg", "ko"));
            Assert.AreEqual("kg", LocCase.Upper("kg", "km"));
            Assert.AreEqual("kg", LocCase.Upper("kg", "hi"));
            Assert.AreSame("kg", LocCase.Upper("kg", "zh-Hant"));
            // A CJK string is not uppercased, including a Latin token, even when the spec says uppercase.
            Assert.AreEqual("喂 Nibbles 20 kg", LocCase.Upper("喂 Nibbles 20 kg", "zh-Hans"));
            Assert.AreEqual("Nibbles", LocCase.Upper("Nibbles", "ja"));
            Assert.AreEqual("20 kg", LocCase.Upper("20 kg", "ko"));
        }
    }

    public class LocTextStyleTests : LocTestBase
    {
        [Test]
        public void Cjk_DropsTracking_LatinKeepsIt_MultiLinePitchUsesTheEnglishBox()
        {
            Assert.AreEqual(0, LocTextStyle.LetterSpacing("zh-Hans", 0.8));
            Assert.AreEqual(0, LocTextStyle.LetterSpacing("zh-Hant", 1));
            Assert.AreEqual(0, LocTextStyle.LetterSpacing("ja", 0.5));
            Assert.AreEqual(0, LocTextStyle.LetterSpacing("ko", 0.4));
            Assert.AreEqual(0, LocTextStyle.LetterSpacing("ar", 0.8));
            Assert.AreEqual(0, LocTextStyle.LetterSpacing("hi", 1));
            Assert.AreEqual(0, LocTextStyle.LetterSpacing("km", 0.4));
            Assert.AreEqual(0.8, LocTextStyle.LetterSpacing("de", 0.8));
            Assert.AreEqual(0.8, LocTextStyle.LetterSpacing("en", 0.8));
            Assert.AreEqual(0.8, LocTextStyle.LetterSpacing(null, 0.8));
            Assert.AreEqual(19, LocTextStyle.LinePitchOverride("ja", "one\ntwo", 38), 0.001);
            Assert.AreEqual(0, LocTextStyle.LinePitchOverride("ja", "one line", 18));
            Assert.AreEqual(0, LocTextStyle.LinePitchOverride("fr", "one\ntwo", 38));
        }
    }

    public class PseudoLocaleTests : LocTestBase
    {
        [Test]
        public void Expand_AccentsPadsAndKeepsTokens()
        {
            Assert.AreEqual("[Pl\u00E0\u00FF~~]", PseudoLocale.Expand("Play"));
            Assert.AreEqual("[20 kg~~]", PseudoLocale.Expand("20 kg"));
            Assert.AreEqual("[\u00E0/b~~]", PseudoLocale.Expand("a/b"));
            Assert.AreEqual("[{0}~~]", PseudoLocale.Expand("{0}"));
            Assert.AreEqual("[\u25B6~]", PseudoLocale.Expand("\u25B6"));
            Assert.AreEqual("[\u00D7~]", PseudoLocale.Expand("\u00D7"));
            string emoji = "\U0001F600";
            string expanded = PseudoLocale.Expand(emoji);
            Assert.IsTrue(expanded.StartsWith("[" + emoji), expanded);
            Assert.IsTrue(expanded.EndsWith("]"), expanded);
            Assert.IsFalse(expanded.Contains("\u00E0"));

            var english = new LocTable();
            english.Spec["Play"] = "Play";
            LocTable table = PseudoLocale.BuildTable(english);
            Assert.AreEqual("qps", table.Code);
            Assert.AreEqual("[Pl\u00E0\u00FF~~]", table.Spec["Play"]);
        }

        [Test]
        public void SetDiagnostic_ActivatesQpsWithoutRegistry()
        {
            var tables = new MemoryTables();
            tables.Add("en", "\"Play\":\"Play\"", "");
            tables.Add("de", "\"Play\":\"Spiel\"", "");
            Loc.SetLoader(tables);
            Assert.IsFalse(Loc.SetLanguage("qps"));
            Assert.IsTrue(Loc.EnglishActive);
            Assert.AreEqual(0, Loc.Version);

            Assert.IsTrue(Loc.SetDiagnostic("qps"));
            Assert.IsFalse(Loc.EnglishActive);
            Assert.AreEqual("qps", Loc.CurrentLanguage);
            Assert.AreEqual(1, Loc.Version);
            string translated;
            Assert.IsTrue(Loc.TryTranslateSpecText("Play", out translated));
            Assert.AreEqual("[Pl\u00E0\u00FF~~]", translated);
            Assert.IsFalse(LanguageRegistry.IsSupported("qps"));

            Assert.IsTrue(Loc.SetLanguage("en"));
            Assert.IsTrue(Loc.EnglishActive);
            Assert.IsFalse(Loc.TryTranslateSpecText("Play", out translated));
        }
    }
}
