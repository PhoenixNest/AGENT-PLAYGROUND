using System;
using System.Globalization;
using NUnit.Framework;

namespace OrbitalSalvage.Loc.Tests
{
    public class LocTableTests : LocTestBase
    {
        [Test]
        public void Normalize_CollapsesWhitespace()
        {
            Assert.AreEqual("RESUME PLAY", LocText.Normalize(" RESUME PLAY\n              "));
            Assert.AreEqual("Claim +100", LocText.Normalize("\n              Claim +100 "));
            Assert.AreEqual("a b", LocText.Normalize("a  \n\t b"));
            Assert.AreEqual("", LocText.Normalize("  \n\t"));
            Assert.AreEqual("", LocText.Normalize(null));
            Assert.AreEqual("a", LocText.Normalize("\u00A0a\u00A0"));
            Assert.AreEqual("ORBITAL SALVAGE", LocText.Normalize("ORBITAL SALVAGE"));
            Assert.IsFalse(LocText.HasLetter("250"));
            Assert.IsFalse(LocText.HasLetter("×"));
            Assert.IsFalse(LocText.HasLetter("▶"));
            Assert.IsFalse(LocText.HasLetter("/"));
            Assert.IsFalse(LocText.HasLetter("+100"));
            Assert.IsTrue(LocText.HasLetter("105 kg"));
            Assert.IsTrue(LocText.HasDigit("Claim +100"));
            Assert.IsFalse(LocText.HasDigit("RESUME PLAY"));
        }

        [Test]
        public void TryTranslateSpecText_EnglishIsIdentityAndOthersReturnTrimmed()
        {
            var tables = new MemoryTables();
            tables.Add("en", "\"RESUME PLAY\":\"RESUME PLAY\"", "");
            tables.Add("de", "\"RESUME PLAY\":\"  WEITERSPIELEN  \"", "\"ui.credits\":\"{0} Münzen\"");
            Loc.SetLoader(tables);

            string translated;
            Assert.IsFalse(Loc.TryTranslateSpecText(" RESUME PLAY\n              ", out translated));
            Assert.IsNull(translated);

            Assert.IsTrue(Loc.SetLanguage("de"));
            Assert.AreEqual("de", Loc.CurrentLanguage);
            Assert.IsTrue(Loc.TryTranslateSpecText(" RESUME PLAY\n              ", out translated));
            Assert.AreEqual("WEITERSPIELEN", translated);
            Assert.IsFalse(Loc.TryTranslateSpecText("\n              Claim +100 ", out translated));
            CollectionAssert.Contains(Loc.MissingKeys, "spec:Claim +100");

            Loc.SetLanguage("en");
            Assert.IsFalse(Loc.TryTranslateSpecText("RESUME PLAY", out translated));
        }

        [Test]
        public void Tr_FormatsWithInvariantNumbersAndFallsBack()
        {
            var tables = new MemoryTables();
            tables.Add("en", "", "\"ui.credits\":\"{0} coins\",\"ui.pair\":\"{0}/{1}\"");
            tables.Add("de", "", "\"ui.left\":\"{0} links\"");
            Loc.SetLoader(tables);
            Assert.IsTrue(Loc.SetLanguage("de"));

            CultureInfo previous = CultureInfo.CurrentCulture;
            try
            {
                var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
                culture.NumberFormat.NumberDecimalSeparator = ",";
                culture.NumberFormat.NumberGroupSeparator = ".";
                CultureInfo.CurrentCulture = culture;
                Assert.AreEqual("1.5 coins", Loc.Tr("ui.credits", 1.5d));
                Assert.AreEqual("2 links", Loc.Tr("ui.left", 2));
                Assert.AreEqual("250/3", Loc.Tr("ui.pair", 250, 3));
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }

            CollectionAssert.Contains(Loc.MissingKeys, "ui.credits");
            Assert.AreEqual("missing", Loc.Tr("missing"));
            // A broken format string is returned unchanged rather than thrown.
            tables.Files["de"] = tables.Files["de"].Replace("\"ui.left\":\"{0} links\"", "\"ui.left\":\"{0 links\"");
            Loc.SetLoader(tables);
            Loc.SetLanguage("en");
            Assert.IsTrue(Loc.SetLanguage("de"));
            Assert.AreEqual("{0 links", Loc.Tr("ui.left", 2));
        }

        [Test]
        public void MissingKeys_AreBoundedAndLanguageChangeClearsThem()
        {
            var tables = new MemoryTables();
            tables.Add("de", "", "");
            Loc.SetLoader(tables);
            Assert.IsTrue(Loc.SetLanguage("de"));
            for (int i = 0; i < 300; i++) Loc.Tr("k" + i.ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual(Loc.MissingKeyLimit, Loc.MissingKeys.Count);
            CollectionAssert.Contains(Loc.MissingKeys, "k0");
            CollectionAssert.DoesNotContain(Loc.MissingKeys, "k256");

            int seen = -1;
            Action handler = delegate { seen = Loc.Version; };
            Loc.LanguageChanged += handler;
            try
            {
                Assert.AreEqual(1, Loc.Version);
                Loc.SetLanguage("en");
                Assert.AreEqual(2, seen);
                Assert.AreEqual(0, Loc.MissingKeys.Count);
                Assert.AreEqual("en", Loc.CurrentLanguage);
                Loc.SetLanguage("en");
                Assert.AreEqual(2, Loc.Version);
            }
            finally
            {
                Loc.LanguageChanged -= handler;
            }
        }

        [Test]
        public void SetLanguage_RejectsUnknownCodesAndInactiveTables()
        {
            Assert.IsFalse(Loc.SetLanguage("NO"));
            Assert.IsFalse(Loc.SetLanguage("pt-BR"));
            Assert.IsFalse(Loc.SetLanguage(null));
            Assert.AreEqual("en", Loc.CurrentLanguage);
            Assert.AreEqual(0, Loc.Version);

            Assert.IsTrue(Loc.SetLanguage("de"));
            Assert.AreEqual("en", Loc.CurrentLanguage);
            Assert.AreEqual(0, Loc.Version);
        }
    }
}
