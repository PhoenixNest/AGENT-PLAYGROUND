using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace OrbitalSalvage.Loc.Tests
{
    public class LocFontRegistryTests
    {
        [SetUp]
        public void LoadShippedManifest()
        {
            string path = Path.Combine(Application.dataPath, "Resources", "Localization", "fonts.json");
            LocFonts.SetLoader(new TextLoader(File.ReadAllText(path)));
        }

        [TearDown]
        public void ResetFonts()
        {
            LocFonts.Reset();
        }

        [Test]
        public void EnglishChain_MatchesTodaysFaces_ForEverySpecPair()
        {
            var pairs = new[]
            {
                new[] { "Fredoka", "400", "Fredoka-400" },
                new[] { "Fredoka", "700", "Fredoka-700" },
                new[] { "Fredoka", "800", "Fredoka-700" },
                new[] { "Fredoka", "900", "Fredoka-700" },
                new[] { "Nunito", "400", "Nunito-700" },
                new[] { "Nunito", "700", "Nunito-700" },
                new[] { "Nunito", "800", "Nunito-800" },
                new[] { "Nunito", "900", "Nunito-900" },
            };
            for (int i = 0; i < pairs.Length; i++)
            {
                string[] names = LocFonts.GetChain("en", pairs[i][0], int.Parse(pairs[i][1]));
                Assert.AreEqual(3, names.Length, pairs[i][0] + " " + pairs[i][1]);
                Assert.AreEqual(pairs[i][2], names[0]);
                Assert.AreEqual("NotoSansSymbols2-Regular", names[1]);
                Assert.AreEqual("NotoSansSymbols-Regular", names[2]);
            }
            string[] other = LocFonts.GetChain("en", "serif", 800);
            Assert.AreEqual("Nunito-700", other[0]);
        }

        [Test]
        public void LatinLanguages_InsertNunitoAfterFredoka()
        {
            string[] codes = { "de", "fr", "es", "it", "pt-PT", "fil", "id", "vi", "qps" };
            for (int i = 0; i < codes.Length; i++)
            {
                string[] display = LocFonts.GetChain(codes[i], "Fredoka", 800);
                CollectionAssert.AreEqual(new[]
                {
                    "Fredoka-700", "Nunito-800", "NotoSansSymbols2-Regular", "NotoSansSymbols-Regular"
                }, display, codes[i]);
                string[] body = LocFonts.GetChain(codes[i], "Nunito", 400);
                CollectionAssert.AreEqual(new[]
                {
                    "Nunito-700", "NotoSansSymbols2-Regular", "NotoSansSymbols-Regular"
                }, body, codes[i] + " body");
            }
        }

        [Test]
        public void Uzbek_PutsTheModifierPatchFirst_AndKeepsOtherLatinFaces()
        {
            CollectionAssert.AreEqual(new[]
            {
                "Nunito-uz-patch-700", "Fredoka-700", "Nunito-700",
                "NotoSansSymbols2-Regular", "NotoSansSymbols-Regular"
            }, LocFonts.GetChain("uz", "Fredoka", 800));
            CollectionAssert.AreEqual(new[]
            {
                "Nunito-uz-patch-600", "Fredoka-600", "Nunito-700",
                "NotoSansSymbols2-Regular", "NotoSansSymbols-Regular"
            }, LocFonts.GetChain("uz", "Fredoka", 600));
            CollectionAssert.AreEqual(new[]
            {
                "Nunito-uz-patch-700", "Nunito-700",
                "NotoSansSymbols2-Regular", "NotoSansSymbols-Regular"
            }, LocFonts.GetChain("uz", "Nunito", 400));
            CollectionAssert.AreEqual(new[]
            {
                "Nunito-uz-patch-800", "Nunito-800",
                "NotoSansSymbols2-Regular", "NotoSansSymbols-Regular"
            }, LocFonts.GetChain("uz", "Nunito", 800));
            CollectionAssert.AreEqual(new[]
            {
                "Nunito-uz-patch-900", "Nunito-900",
                "NotoSansSymbols2-Regular", "NotoSansSymbols-Regular"
            }, LocFonts.GetChain("uz", "Nunito", 900));
            CollectionAssert.AreEqual(
                LocFonts.GetChain("de", "Fredoka", 800),
                LocFonts.GetChain("fr", "Fredoka", 800));
        }

        [Test]
        public void ScriptGroups_NameTheSpikeFace_ForEveryWeight()
        {
            Expect("zh-Hans", "ZCOOLKuaiLe-endonym");
            Expect("zh-Hant", "Huninn-endonym");
            Expect("ja", "MPLUSRounded1c-endonym");
            Expect("ko", "Jua-endonym");
            Expect("hi", "Baloo2-700");
            Expect("km", "KantumruyPro-700");
            Expect("ar", "Cairo-700");
        }

        [Test]
        public void BrokenManifest_DoesNotThrow_AndKeepsLatinFaces()
        {
            LocFonts.SetLoader(new TextLoader("{"));
            string[] names = LocFonts.GetChain("de", "Fredoka", 700);
            Assert.AreEqual("Fredoka-700", names[0]);
            Assert.AreEqual("Nunito-700", names[1]);
        }

        [Test]
        public void MissingGlyphAudit_ReportsTheBrokenCharacter_AndSkipsIgnoredCodePoints()
        {
            var table = new LocTable();
            table.Spec["Hello"] = "Hello Z";
            table.Str["wave"] = "A\uFE0F \u200D\u200B";
            var missing = new List<LocGlyphCoverage.Miss>();
            LocGlyphCoverage.CollectTable(table, cp => cp != 'Z', null, missing);
            Assert.AreEqual(1, missing.Count);
            Assert.AreEqual("spec", missing[0].Kind);
            Assert.AreEqual("Hello", missing[0].Key);
            Assert.AreEqual('Z', missing[0].CodePoint);

            var fromString = new List<int>();
            LocGlyphCoverage.CollectString("Hi\u0001 Z", cp => cp != 'Z', null, fromString);
            Assert.AreEqual(1, fromString.Count);
            LocGlyphCoverage.CollectString("\uD83D\uDE00", cp => false, g => g.Length > 1, fromString);
            Assert.AreEqual(1, fromString.Count, "emoji grapheme is not a missing glyph");
        }

        static void Expect(string code, string face)
        {
            string[] display = LocFonts.GetChain(code, "Fredoka", 800);
            string[] body = LocFonts.GetChain(code, "Nunito", 400);
            Assert.AreEqual(face, display[0], code);
            Assert.AreEqual(face, body[0], code + " body");
            Assert.AreEqual("Nunito-800", display[1], code);
            Assert.AreEqual("NotoSansSymbols2-Regular", display[2], code);
            Assert.AreEqual("NotoSansSymbols-Regular", display[3], code);
        }

        sealed class TextLoader : ILocFontManifestLoader
        {
            readonly string json;
            public TextLoader(string json) { this.json = json; }
            public bool TryLoad(out string text)
            {
                text = json;
                return !string.IsNullOrEmpty(json);
            }
        }
    }
}
