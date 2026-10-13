using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using OrbitalSalvage.Loc;
using UnityEngine;

namespace OrbitalSalvage.Render.Tests
{
    using Loc = OrbitalSalvage.Loc.Loc;

    /// <summary>
    /// Fixture translations. Not shipped under Resources/Localization. The real tables
    /// are owned by the translators. These strings only exercise the paint hook.
    /// </summary>
    public class CjkRenderTests
    {
        // FIXTURE — not a translation.
        static readonly Dictionary<string, string> Hans = new Dictionary<string, string>
        {
            { "PLAY", "开始" },
            { "PLAY MISSION", "开始吧" },
            { "SHOP", "好的" },
            { "Language", "你的" },
            { "Choose Language", "你要什么" },
            { "Reset", "回来" },
            { "GAME SETTINGS", "我的家" },
            { "Sound: ON", "开" },
            { "TOY SHOP", "小的" },
            { "\u2190 Back", "回来" },
            { "Goal: Feed Nibbles 20 kg space snacks", "看 Nibbles 20 kg" },
            { "Nibbles is still hungry! Out of orbital scrap!", "Nibbles 不想" }
        };

        // FIXTURE — not a translation.
        static readonly Dictionary<string, string> Hant = new Dictionary<string, string>
        {
            { "PLAY", "開始" },
            { "PLAY MISSION", "開始吧" },
            { "SHOP", "好的" },
            { "Language", "你的" },
            { "Choose Language", "你要什麼" },
            { "Reset", "回來" },
            { "GAME SETTINGS", "我的家" },
            { "Sound: ON", "開" },
            { "TOY SHOP", "小的" },
            { "\u2190 Back", "回來" },
            { "Goal: Feed Nibbles 20 kg space snacks", "看 Nibbles 20 kg" },
            { "Nibbles is still hungry! Out of orbital scrap!", "Nibbles 不想" }
        };

        // FIXTURE — not a translation.
        static readonly Dictionary<string, string> Ja = new Dictionary<string, string>
        {
            { "PLAY", "はじめる" },
            { "PLAY MISSION", "はじめる" },
            { "SHOP", "みせ" },
            { "Language", "ことば" },
            { "Choose Language", "ことばをえらぶ" },
            { "Reset", "もどる" },
            { "GAME SETTINGS", "せってい" },
            { "Sound: ON", "おん" },
            { "TOY SHOP", "おもちゃ" },
            { "\u2190 Back", "もどる" },
            { "Goal: Feed Nibbles 20 kg space snacks", "Nibbles 20 kg" },
            { "Nibbles is still hungry! Out of orbital scrap!", "Nibbles はまだ" }
        };

        // FIXTURE — not a translation.
        static readonly Dictionary<string, string> Ko = new Dictionary<string, string>
        {
            { "PLAY", "하나" },
            { "PLAY MISSION", "하나 해" },
            { "SHOP", "가게" },
            { "Language", "말" },
            { "Choose Language", "말을 해" },
            { "Reset", "다시" },
            { "GAME SETTINGS", "내 것" },
            { "Sound: ON", "소리" },
            { "TOY SHOP", "하나" },
            { "\u2190 Back", "다시" },
            { "Goal: Feed Nibbles 20 kg space snacks", "Nibbles 20 kg" },
            { "Nibbles is still hungry! Out of orbital scrap!", "Nibbles 는 안 해" }
        };

        [SetUp]
        public void Manifest()
        {
            string path = Path.Combine(Application.dataPath, "Resources", "Localization", "fonts.json");
            LocFonts.SetLoader(new TextLoader(File.ReadAllText(path)));
            OutlineFonts.Clear();
            Loc.Reset(null);
            HomeChrome.RecordRuns = false;
        }

        [TearDown]
        public void ResetFonts()
        {
            HomeChrome.RecordRuns = false;
            OutlineFonts.Clear();
            LocFonts.Reset();
            Loc.Reset(null);
        }

        [Test]
        public void Manifest_LoadsEveryCjkGroup_AndCoversTheEndonym()
        {
            Expect("zh-Hans", "ZCOOLKuaiLe-sc", "简体中文");
            Expect("zh-Hant", "Huninn-tc", "繁體中文");
            Expect("ja", "MPLUSRounded1c-700", "日本語");
            Expect("ko", "Jua-kr", "한국어");
        }

        [Test]
        public void CjkBaseline_MatchesTheLatinStrut()
        {
            using (var canvas = new Canvas2D(32, 32))
            {
                canvas.Font = "700 32px Fredoka";
                canvas.TextChain = null;
                double latin = canvas.FontAscent();
                string[] codes = { "zh-Hans", "zh-Hant", "ja", "ko" };
                for (int i = 0; i < codes.Length; i++)
                {
                    canvas.TextChain = OutlineFonts.ChainFor(codes[i], "Fredoka", 700);
                    canvas.TextChainStamp = i + 3;
                    Assert.AreEqual(latin, canvas.FontAscent(), 0.01, codes[i]);
                }
                canvas.TextChain = null;
                Assert.AreEqual(latin, canvas.FontAscent(), 0.01, "English chain restored");
            }
        }

        [TestCase("zh-Hans")]
        [TestCase("zh-Hant")]
        [TestCase("ja")]
        [TestCase("ko")]
        public void FixtureTable_PaintsATranslatedRun_AndEnglishDoesNot(string code)
        {
            Dictionary<string, string> table = Table(code);
            string play = table["PLAY"];
            AssertCovers(code, play);
            AssertCovers(code, table["Goal: Feed Nibbles 20 kg space snacks"]);

            Loc.Reset(Loader(code, table));
            Assert.IsTrue(Loc.SetLanguage("en"));
            string english = Drawn("home_default");
            Assert.IsTrue(english.Contains("PLAY"), english);
            Assert.IsFalse(english.Contains(play), english);

            Assert.IsTrue(Loc.SetLanguage(code), code);
            Assert.IsFalse(Loc.EnglishActive);
            string painted = Drawn("home_default");
            Assert.IsTrue(painted.Contains(play), painted);
            // Uppercase is a no-op for CJK, so kg and Nibbles stay as the fixture wrote them.
            Assert.AreEqual(table["Goal: Feed Nibbles 20 kg space snacks"],
                LocCase.Upper(table["Goal: Feed Nibbles 20 kg space snacks"], code));
        }

        static void Expect(string code, string face, string endonym)
        {
            TrueTypeFont[] chain = OutlineFonts.ChainFor(code, "Fredoka", 900);
            Assert.AreEqual(face, chain[0].name, code);
            Assert.GreaterOrEqual(chain.Length, 3, code);
            for (int i = 0; i < endonym.Length; i++)
            {
                int cp = char.ConvertToUtf32(endonym, i);
                if (char.IsHighSurrogate(endonym[i])) i++;
                bool hit = false;
                for (int f = 0; f < chain.Length; f++)
                    if (chain[f].GlyphIndex(cp) != 0) { hit = true; break; }
                Assert.IsTrue(hit, code + " U+" + cp.ToString("X4"));
            }
        }

        static void AssertCovers(string code, string text)
        {
            TrueTypeFont[] chain = OutlineFonts.ChainFor(code, "Fredoka", 700);
            var missing = new List<int>();
            LocGlyphCoverage.CollectString(text, cp =>
            {
                for (int i = 0; i < chain.Length; i++)
                    if (chain[i].GlyphIndex(cp) != 0) return true;
                return false;
            }, null, missing);
            Assert.AreEqual(0, missing.Count, code + " " + text);
        }

        static string Drawn(string page)
        {
            HomeChrome.RecordRuns = true;
            HomeChrome chrome = HomeChrome.Load(page, "zero");
            try
            {
                chrome.Draw();
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < chrome.RecordedRuns.Count; i++)
                {
                    HomeChrome.PaintedRun run = chrome.RecordedRuns[i];
                    sb.Append(run.Id ?? "").Append('\t').Append(run.Text ?? "").Append('\n');
                }
                return sb.ToString();
            }
            finally
            {
                chrome.Dispose();
                HomeChrome.RecordRuns = false;
            }
        }

        static Dictionary<string, string> Table(string code)
        {
            if (code == "zh-Hans") return Hans;
            if (code == "zh-Hant") return Hant;
            if (code == "ja") return Ja;
            return Ko;
        }

        static MemoryLoader Loader(string code, Dictionary<string, string> spec)
        {
            var loader = new MemoryLoader();
            var sb = new System.Text.StringBuilder();
            bool first = true;
            foreach (KeyValuePair<string, string> kv in spec)
            {
                if (!first) sb.Append(',');
                first = false;
                sb.Append('"').Append(kv.Key.Replace("\\", "\\\\").Replace("\"", "\\\"")).Append("\":\"");
                sb.Append(kv.Value.Replace("\\", "\\\\").Replace("\"", "\\\"")).Append('"');
            }
            LanguageInfo info;
            string name = LanguageRegistry.TryGet(code, out info) ? info.Endonym : code;
            loader.Json = "{\"meta\":{\"code\":\"" + code + "\",\"name\":\"" + name
                + "\",\"version\":1,\"source\":\"en\"},\"spec\":{" + sb + "},\"str\":{}}";
            loader.Code = code;
            return loader;
        }

        sealed class TextLoader : ILocFontManifestLoader
        {
            readonly string json;
            public TextLoader(string json) { this.json = json; }
            public bool TryLoad(out string text) { text = json; return true; }
        }

        sealed class MemoryLoader : ILocTableLoader
        {
            public string Code;
            public string Json;
            public bool TryLoad(string code, out string json)
            {
                if (code == Code) { json = Json; return true; }
                json = null;
                return false;
            }
        }
    }
}
