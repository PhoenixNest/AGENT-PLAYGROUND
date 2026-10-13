using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
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
                if (code == "uz")
                {
                    Assert.AreEqual("Nunito-uz-patch-700", chain[0].name, code);
                    Assert.AreEqual("Fredoka-700", chain[1].name, code);
                    Assert.AreEqual("Nunito-700", chain[2].name, code);
                }
                else
                {
                    Assert.AreEqual("Fredoka-700", chain[0].name, code);
                    Assert.AreEqual("Nunito-800", chain[1].name, code);
                }
                string holes = Uncovered(chain, Alphabets[code]);
                Assert.AreEqual("", holes, code + " " + holes);
            }
        }

        [Test]
        public void UzbekPatch_IsNarrowerThanTheLetter_AndOtherLanguagesStayOnNunito()
        {
            AssertAdvance("Nunito-uz-patch-600.ttf", "Fredoka-600.ttf");
            AssertAdvance("Nunito-uz-patch-700.ttf", "Fredoka-700.ttf");
            AssertAdvance("Nunito-uz-patch-700.ttf", "Nunito-700.ttf");
            AssertAdvance("Nunito-uz-patch-800.ttf", "Nunito-800.ttf");
            AssertAdvance("Nunito-uz-patch-900.ttf", "Nunito-900.ttf");

            const string phrase = "O\u02BBYNA g\u02BBa";
            TrueTypeFont[] de = OutlineFonts.ChainFor("de", "Fredoka", 800);
            TrueTypeFont[] fr = OutlineFonts.ChainFor("fr", "Fredoka", 800);
            TrueTypeFont[] en = OutlineFonts.ChainFor("en", "Fredoka", 800);
            TrueTypeFont[] uz = OutlineFonts.ChainFor("uz", "Fredoka", 800);
            Assert.AreEqual("Fredoka-700", de[0].name);
            Assert.AreEqual("Nunito-800", de[1].name);
            Assert.AreEqual("Fredoka-700", fr[0].name);
            Assert.AreSame(OutlineFonts.Chain(OutlineFonts.Primary("Fredoka", 800)), en);
            string deHash = HashPhrase(de, phrase);
            string frHash = HashPhrase(fr, phrase);
            string enHash = HashPhrase(en, phrase);
            string uzHash = HashPhrase(uz, phrase);
            Assert.AreEqual(deHash, frHash, "de and fr both use the unpatched Nunito mark");
            Assert.AreEqual(deHash, HashPhrase(de, phrase), "de phrase is stable");
            Assert.AreNotEqual(deHash, uzHash, "uz phrase still uses the wide Nunito mark");
            Assert.AreNotEqual(enHash, deHash);
            Assert.Less(Measure(uz, phrase), Measure(de, phrase));
        }

        static void AssertAdvance(string patchFile, string letterFile)
        {
            TrueTypeFont patch = LoadDisk(patchFile);
            TrueTypeFont letter = LoadDisk(letterFile);
            int mark = patch.GlyphIndex(0x02BB);
            int oh = letter.GlyphIndex((int)'o');
            Assert.AreNotEqual(0, mark, patchFile);
            Assert.AreNotEqual(0, oh, letterFile);
            Assert.Less(patch.Advance(mark), letter.Advance(oh) * 0.4, patchFile + " vs " + letterFile);
        }

        static double Measure(TrueTypeFont[] chain, string text)
        {
            using (var canvas = new Canvas2D(32, 32))
            {
                canvas.Font = "800 32px Fredoka";
                canvas.TextChain = chain;
                canvas.TextChainStamp = chain[0].GlyphIndex(0x02BB) + chain.Length * 17;
                return canvas.MeasureText(text);
            }
        }

        static string HashPhrase(TrueTypeFont[] chain, string text)
        {
            const int w = 420;
            const int h = 96;
            using (var canvas = new Canvas2D(w, h))
            {
                canvas.BeginFrame();
                canvas.Font = "800 48px Fredoka";
                canvas.FillStyle = "#ffffff";
                canvas.TextChain = chain;
                canvas.TextChainStamp = 9;
                canvas.FillText(text, 4, 72);
                canvas.EndFrame();
                var prev = RenderTexture.active;
                RenderTexture.active = canvas.Target;
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, true);
                tex.ReadPixels(new Rect(0, 0, w, h), 0, 0, false);
                RenderTexture.active = prev;
                byte[] raw = tex.GetRawTextureData();
                Object.DestroyImmediate(tex);
                using (SHA256 sha = SHA256.Create())
                {
                    byte[] hash = sha.ComputeHash(raw);
                    return System.BitConverter.ToString(hash);
                }
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
            Assert.AreEqual("MPLUSRounded1c-700", ja[0].name);
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
                Assert.AreEqual("MPLUSRounded1c-700", ja[0].name);
                TrueTypeFont[] title = chrome.ResolveRunChain("language-modal-title", "800 17px Fredoka");
                Assert.AreEqual("Fredoka-700", title[0].name);
                Assert.AreEqual(3, title.Length, "English title stays on the shipped chain");

                Assert.IsTrue(Loc.SetLanguage("de"));
                Assert.IsFalse(Loc.EnglishActive);
                TrueTypeFont[] jaDe = chrome.ResolveRunChain("btn-lang-ja", "800 14px Fredoka");
                Assert.AreEqual("MPLUSRounded1c-700", jaDe[0].name);
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
