using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using OrbitalSalvage.Loc;
using UnityEngine;

namespace OrbitalSalvage.Render.Tests
{
    using Loc = OrbitalSalvage.Loc.Loc;

    public class LocFontChainTests
    {
        static readonly string[] LatinCodes = { "de", "fr", "es", "it", "pt-PT", "fil", "id", "uz", "vi" };

        static readonly Dictionary<string, string> Alphabets = new Dictionary<string, string>
        {
            { "de", "äöüßÄÖÜ" },
            { "fr", "àâæçéèêëîïôœùûÿÀÂÇÉÈÊËÎÏÔÙÛŸŒ" },
            { "es", "ñáéíóúü¿¡ÑÁÉÍÓÚÜ" },
            { "it", "àèéìòùÀÈÉÌÒÙ" },
            { "pt-PT", "ãõáàâéêíóôúçÃÕÁÀÂÉÊÍÓÔÚÇ" },
            { "fil", "ñÑ" },
            { "id", "BahasaIndonesia" },
            { "uz", "O\u02BBzbekcha" },
            { "vi", "ăâêôơưđáàảãạắằẳẵặấầẩẫậếềểễệíìỉĩịóòỏõọốồổỗộớờởỡợúùủũụứừửữựýỳỷỹỵĂÂÊÔƠƯĐ" },
        };

        [SetUp]
        public void Manifest()
        {
            string path = Path.Combine(Application.dataPath, "Resources", "Localization", "fonts.json");
            LocFonts.SetLoader(new TextLoader(File.ReadAllText(path)));
            OutlineFonts.Clear();
        }

        [TearDown]
        public void ResetFonts()
        {
            OutlineFonts.Clear();
            LocFonts.Reset();
            Loc.Reset(null);
        }

        [Test]
        public void EnglishChain_IsTheSameFontObjects_ForSpecPairs()
        {
            var pairs = SpecPairs();
            Assert.GreaterOrEqual(pairs.Count, 8);
            foreach (KeyValuePair<string, int> pair in pairs)
            {
                TrueTypeFont primary = OutlineFonts.Primary(pair.Key, pair.Value);
                TrueTypeFont[] shipped = OutlineFonts.Chain(primary);
                TrueTypeFont[] via = OutlineFonts.ChainFor("en", pair.Key, pair.Value);
                Assert.AreSame(shipped, via, pair.Key + " " + pair.Value);
                string[] names = LocFonts.GetChain("en", pair.Key, pair.Value);
                Assert.AreEqual(shipped.Length, names.Length);
                for (int i = 0; i < shipped.Length; i++)
                {
                    Assert.AreSame(shipped[i], via[i]);
                    Assert.AreEqual(shipped[i].name, names[i]);
                }
            }
        }

        [Test]
        public void LatinAlphabets_AreInTheChain_AndVietnameseHolesAreNotInFredoka()
        {
            TrueTypeFont fredoka = LoadDisk("Fredoka-700.ttf");
            TrueTypeFont nunito = LoadDisk("Nunito-800.ttf");
            Assert.AreEqual(0, fredoka.GlyphIndex(0x1EBF), "Fredoka has no ế");
            Assert.AreEqual(0, fredoka.GlyphIndex(0x1EC7), "Fredoka has no ệ");
            Assert.AreEqual(0, fredoka.GlyphIndex(0x02BB), "Fredoka has no U+02BB");
            Assert.AreNotEqual(0, nunito.GlyphIndex(0x1EBF));
            Assert.AreNotEqual(0, nunito.GlyphIndex(0x1EC7));
            Assert.AreNotEqual(0, nunito.GlyphIndex(0x02BB));
            for (int i = 0; i < LatinCodes.Length; i++)
            {
                string code = LatinCodes[i];
                TrueTypeFont[] chain = OutlineFonts.ChainFor(code, "Fredoka", 800);
                Assert.AreEqual("Fredoka-700", chain[0].name, code);
                Assert.AreEqual("Nunito-800", chain[1].name, code);
                string holes = Uncovered(chain, Alphabets[code]);
                Assert.AreEqual("", holes, code + " " + holes);
            }
        }

        [Test]
        public void MissingScriptFace_IsSkipped_AndDoesNotThrow()
        {
            TrueTypeFont[] hi = OutlineFonts.ChainFor("hi", "Fredoka", 800);
            TrueTypeFont[] de = OutlineFonts.ChainFor("de", "Fredoka", 800);
            Assert.AreEqual(de.Length, hi.Length);
            for (int i = 0; i < de.Length; i++) Assert.AreSame(de[i], hi[i], hi[i].name);
            TrueTypeFont[] ja = OutlineFonts.ChainFor("ja", "Fredoka", 800);
            Assert.AreEqual("MPLUSRounded1c-endonym", ja[0].name);
            Assert.AreNotEqual(0, ja[0].GlyphIndex(0x65E5));
            Assert.AreNotEqual(0, OutlineFonts.ChainFor("zh-Hans", "Fredoka", 800)[0].GlyphIndex(0x7B80));
            Assert.AreNotEqual(0, OutlineFonts.ChainFor("zh-Hant", "Fredoka", 800)[0].GlyphIndex(0x9AD4));
            Assert.AreNotEqual(0, OutlineFonts.ChainFor("ko", "Fredoka", 800)[0].GlyphIndex(0xD55C));
        }

        [Test]
        public void PickerButton_UsesItsOwnChain_WhileActiveLanguageIsEnglishOrGerman()
        {
            var tables = new MemTables();
            tables.Add("de");
            Loc.Reset(tables);
            var chrome = HomeChrome.Load("settings_modal_language", "zero");
            try
            {
                chrome.ApplyPickerFontOverrides();
                Assert.IsTrue(Loc.EnglishActive);
                TrueTypeFont[] ja = chrome.ResolveRunChain("btn-lang-ja", "800 14px Fredoka");
                Assert.AreEqual("MPLUSRounded1c-endonym", ja[0].name);
                TrueTypeFont[] title = chrome.ResolveRunChain("language-modal-title", "800 17px Fredoka");
                Assert.AreEqual("Fredoka-700", title[0].name);
                Assert.AreEqual(3, title.Length, "English title stays on the shipped chain");

                Assert.IsTrue(Loc.SetLanguage("de"));
                Assert.IsFalse(Loc.EnglishActive);
                TrueTypeFont[] jaDe = chrome.ResolveRunChain("btn-lang-ja", "800 14px Fredoka");
                Assert.AreEqual("MPLUSRounded1c-endonym", jaDe[0].name);
                TrueTypeFont[] deTitle = chrome.ResolveRunChain("language-modal-title", "800 17px Fredoka");
                Assert.AreEqual("Fredoka-700", deTitle[0].name);
                Assert.AreEqual("Nunito-800", deTitle[1].name);
            }
            finally
            {
                chrome.Dispose();
            }
        }

        static TrueTypeFont LoadDisk(string file)
        {
            string path = Path.Combine(Application.dataPath, "Fonts", file);
            return new TrueTypeFont(file, File.ReadAllBytes(path));
        }

        static string Uncovered(TrueTypeFont[] chain, string text)
        {
            var holes = "";
            var seen = new HashSet<int>();
            for (int i = 0; i < text.Length; i++)
            {
                int cp = char.ConvertToUtf32(text, i);
                if (char.IsHighSurrogate(text[i])) i++;
                if (!seen.Add(cp)) continue;
                bool hit = false;
                for (int f = 0; f < chain.Length; f++)
                    if (chain[f].GlyphIndex(cp) != 0) { hit = true; break; }
                if (!hit) holes += " U+" + cp.ToString("X4");
            }
            return holes;
        }

        static List<KeyValuePair<string, int>> SpecPairs()
        {
            var set = new HashSet<string>();
            var pairs = new List<KeyValuePair<string, int>>();
            string dir = Path.Combine(Application.dataPath, "Resources", "Parity", "Spec");
            string[] files = Directory.GetFiles(dir, "*__zero.json");
            for (int i = 0; i < files.Length; i++)
            {
                var root = MiniJson.Parse(File.ReadAllText(files[i])) as Dictionary<string, object>;
                if (root == null || !(root["elements"] is List<object> elements)) continue;
                for (int e = 0; e < elements.Count; e++)
                {
                    var el = elements[e] as Dictionary<string, object>;
                    if (el == null || !el.ContainsKey("runs")) continue;
                    string family = Family(el.ContainsKey("ff") ? el["ff"] as string : "");
                    int weight = 400;
                    if (el.ContainsKey("fw") && el["fw"] != null) int.TryParse(el["fw"].ToString(), out weight);
                    string key = family + "\n" + weight;
                    if (!set.Add(key)) continue;
                    pairs.Add(new KeyValuePair<string, int>(family, weight));
                }
            }
            return pairs;
        }

        static string Family(string ff)
        {
            if (string.IsNullOrEmpty(ff)) return "Fredoka";
            int comma = ff.IndexOf(',');
            return (comma >= 0 ? ff.Substring(0, comma) : ff).Trim().Trim('"').Trim('\'');
        }

        sealed class TextLoader : ILocFontManifestLoader
        {
            readonly string json;
            public TextLoader(string json) { this.json = json; }
            public bool TryLoad(out string text) { text = json; return true; }
        }

        sealed class MemTables : ILocTableLoader
        {
            readonly Dictionary<string, string> files = new Dictionary<string, string>();
            public void Add(string code)
            {
                files[code] = "{\"meta\":{\"code\":\"" + code + "\",\"name\":\"" + code + "\",\"version\":1,\"source\":\"en\"},\"spec\":{},\"str\":{}}";
            }
            public bool TryLoad(string code, out string json) { return files.TryGetValue(code, out json); }
        }
    }
}
