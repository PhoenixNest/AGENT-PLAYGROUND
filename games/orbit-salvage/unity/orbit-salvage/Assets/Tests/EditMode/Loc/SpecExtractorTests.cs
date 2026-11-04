using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace OrbitalSalvage.Loc.Tests
{
    public class SpecExtractorTests : LocTestBase
    {
        [Test]
        public void Extract_FixtureRules()
        {
            string dir = Path.Combine(Path.GetTempPath(), "loc-extract-" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "sample__zero.json"), Fixture(), new UTF8Encoding(false));
                SpecScan first = SpecExtractor.Scan(dir, "__zero.json");
                SpecScan second = SpecExtractor.Scan(dir, "__zero.json");
                Assert.AreEqual(first.EnglishJson, second.EnglishJson);
                Assert.AreEqual(first.Csv, second.Csv);
                CollectionAssert.AreEqual(new[] { "/", "250", "×", "▶" }, first.SkipKeys);
                Assert.IsFalse(first.SpecKeys.Contains("250"));
                Assert.AreEqual(6, first.SpecKeys.Count);

                int claim = first.EnglishJson.IndexOf("Claim your free daily +100", System.StringComparison.Ordinal);
                int go = first.EnglishJson.IndexOf("Go \U0001F680", System.StringComparison.Ordinal);
                int hello = first.EnglishJson.IndexOf("Hello, world", System.StringComparison.Ordinal);
                int resume = first.EnglishJson.IndexOf("RESUME PLAY", System.StringComparison.Ordinal);
                int allowance = first.EnglishJson.IndexOf("allowance.", System.StringComparison.Ordinal);
                int mass = first.EnglishJson.IndexOf("105 kg", System.StringComparison.Ordinal);
                Assert.Less(mass, claim);
                Assert.Less(claim, go);
                Assert.Less(go, hello);
                Assert.Less(hello, resume);
                Assert.Less(resume, allowance);
                StringAssert.Contains("\"doNotTranslate\"", first.EnglishJson);
                Assert.IsTrue(first.EnglishJson.EndsWith("\n"));
                Assert.IsFalse(first.EnglishJson.Contains("\r"));

                var rows = Index(first.Csv);
                Assert.AreEqual("spec", rows["105 kg"]["kind"]);
                Assert.AreEqual("1: sample__zero.json", rows["105 kg"]["files"]);
                Assert.AreEqual("mass", rows["105 kg"]["element_ids"]);
                Assert.AreEqual("12px", rows["105 kg"]["fz"]);
                Assert.AreEqual("none", rows["105 kg"]["uppercase"]);
                Assert.AreEqual("40", rows["105 kg"]["max_width"]);
                Assert.AreEqual("false", rows["105 kg"]["hasEmoji"]);
                Assert.AreEqual("true", rows["105 kg"]["hasPlaceholderLike"]);
                Assert.AreEqual("mass", rows["105 kg"]["group"]);
                Assert.AreEqual("0", rows["105 kg"]["run_index"]);
                Assert.AreEqual("no-terminal-punct", rows["105 kg"]["notes"]);

                Assert.AreEqual("fragment;no-terminal-punct", rows["Claim your free daily +100"]["notes"]);
                Assert.AreEqual("gift-banner-desc", rows["Claim your free daily +100"]["group"]);
                Assert.AreEqual("0", rows["Claim your free daily +100"]["run_index"]);
                Assert.AreEqual("120", rows["Claim your free daily +100"]["max_width"]);
                Assert.AreEqual("banner", rows["Claim your free daily +100"]["element_classes"]);
                Assert.AreEqual("true", rows["Claim your free daily +100"]["hasPlaceholderLike"]);
                Assert.AreEqual("11px", rows["Claim your free daily +100"]["fz"]);

                Assert.AreEqual("fragment", rows["allowance."]["notes"]);
                Assert.AreEqual("1", rows["allowance."]["run_index"]);
                Assert.AreEqual("60", rows["allowance."]["max_width"]);
                Assert.AreEqual("false", rows["allowance."]["hasPlaceholderLike"]);
                Assert.AreEqual("gift-banner-desc", rows["allowance."]["element_ids"]);

                Assert.AreEqual("sample__zero.json#2", rows["RESUME PLAY"]["group"]);
                Assert.AreEqual("80", rows["RESUME PLAY"]["max_width"]);
                Assert.AreEqual("uppercase", rows["RESUME PLAY"]["uppercase"]);
                Assert.AreEqual("no-terminal-punct", rows["RESUME PLAY"]["notes"]);
                Assert.AreEqual("0", rows["RESUME PLAY"]["run_index"]);

                string goKey = "Go \U0001F680";
                Assert.AreEqual("true", rows[goKey]["hasEmoji"]);
                Assert.AreEqual("4", rows[goKey]["chars"]);
                Assert.AreEqual("go", rows[goKey]["group"]);
                Assert.AreEqual("30", rows[goKey]["max_width"]);

                Assert.AreEqual("hello", rows["Hello, world"]["element_ids"]);
                Assert.AreEqual("90", rows["Hello, world"]["max_width"]);
                Assert.AreEqual("no-terminal-punct", rows["Hello, world"]["notes"]);
            }
            finally
            {
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }

        [Test]
        public void Extract_EndonymsAreNeverTranslated()
        {
            var languages = LanguageRegistry.All;
            for (int i = 0; i < languages.Count; i++)
            {
                string endonym = LocText.Normalize(languages[i].Endonym);
                Assert.IsTrue(SpecExtractor.IsNeverTranslate(endonym), languages[i].Code);
                Assert.IsTrue(SpecExtractor.IsNeverTranslate(LocText.Normalize(LanguageMarker.Mark(languages[i].Endonym))), languages[i].Code);
            }
            Assert.IsFalse(SpecExtractor.IsNeverTranslate("Choose Language"));
            Assert.IsFalse(SpecExtractor.IsNeverTranslate("Hello"));

            string dir = Path.Combine(Path.GetTempPath(), "loc-endonym-" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                string marked = LanguageMarker.Mark("English");
                File.WriteAllText(Path.Combine(dir, "lang__zero.json"),
                    "{\"elements\":["
                    + "{\"id\":\"settings-language-value\",\"runs\":[{\"s\":\"English\",\"r\":[0,0,40,11]}]},"
                    + "{\"id\":\"btn-lang-zh-Hans\",\"runs\":[{\"s\":\"简体中文\",\"r\":[0,0,56,17]}]},"
                    + "{\"id\":\"btn-lang-en\",\"runs\":[{\"s\":\"" + marked + "\",\"r\":[0,0,70,17]}]},"
                    + "{\"id\":\"language-modal-title\",\"runs\":[{\"s\":\"Choose Language\",\"r\":[0,0,120,17]}]}"
                    + "]}",
                    new UTF8Encoding(false));
                SpecScan scan = SpecExtractor.Scan(dir, "__zero.json");
                CollectionAssert.Contains(scan.SkipKeys, "English");
                CollectionAssert.Contains(scan.SkipKeys, "简体中文");
                CollectionAssert.Contains(scan.SkipKeys, marked);
                CollectionAssert.DoesNotContain(scan.SpecKeys, "English");
                CollectionAssert.DoesNotContain(scan.SpecKeys, "简体中文");
                CollectionAssert.DoesNotContain(scan.SpecKeys, marked);
                CollectionAssert.Contains(scan.SpecKeys, "Choose Language");
                StringAssert.Contains("\"doNotTranslate\"", scan.EnglishJson);
                int specAt = scan.EnglishJson.IndexOf("\"spec\"", System.StringComparison.Ordinal);
                int skipAt = scan.EnglishJson.IndexOf("\"doNotTranslate\"", System.StringComparison.Ordinal);
                int englishInSpec = scan.EnglishJson.IndexOf("English", specAt, skipAt - specAt, System.StringComparison.Ordinal);
                Assert.Less(englishInSpec, 0, "endonym must not be a spec value");
            }
            finally
            {
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }

        [Test]
        public void Extract_IphoneMatchesZeroAndCommittedTableIsFresh()
        {
            string specDir = Path.Combine(Application.dataPath, "Resources", "Parity", "Spec");
            SpecScan zero = SpecExtractor.Scan(specDir, "__zero.json");
            SpecScan again = SpecExtractor.Scan(specDir, "__zero.json");
            SpecScan iphone = SpecExtractor.Scan(specDir, "__iphone.json");
            Assert.AreEqual(zero.EnglishJson, again.EnglishJson);
            Assert.AreEqual(zero.Csv, again.Csv);
            Assert.Greater(zero.FileCount, 0);
            Assert.AreEqual(zero.FileCount, iphone.FileCount);
            CollectionAssert.AreEqual(zero.SpecKeys, iphone.SpecKeys);
            CollectionAssert.AreEqual(zero.SkipKeys, iphone.SkipKeys);
            CollectionAssert.Contains(zero.SkipKeys, "250");
            CollectionAssert.Contains(zero.SkipKeys, "▶");
            CollectionAssert.Contains(zero.SkipKeys, "×");
            CollectionAssert.Contains(zero.SpecKeys, "ORBITAL SALVAGE");
            CollectionAssert.DoesNotContain(zero.SpecKeys, "250");

            string jsonPath = Path.Combine(Application.dataPath, "Resources", "Localization", "en.json");
            string csvPath = SpecExtractor.HandoffCsvPath(Application.dataPath);
            Assert.IsTrue(File.Exists(jsonPath), jsonPath);
            Assert.IsTrue(File.Exists(csvPath), csvPath);
            Assert.AreEqual(zero.EnglishJson, File.ReadAllText(jsonPath));
            Assert.AreEqual(zero.Csv, File.ReadAllText(csvPath));
            Assert.IsFalse(zero.EnglishJson.Contains("\r"));

            Assert.AreEqual(LocStrings.Count, zero.StrKeys.Count);
            Assert.Greater(zero.StrKeys.Count, 0);
            var strOrder = new List<string>(zero.StrKeys);
            strOrder.Sort(System.StringComparer.Ordinal);
            CollectionAssert.AreEqual(strOrder, zero.StrKeys);
            CollectionAssert.Contains(zero.StrKeys, "loading.tip.3");
            CollectionAssert.Contains(zero.StrKeys, "fx.shred");
            CollectionAssert.DoesNotContain(zero.SpecKeys, "loading.tip.3");
            int specAt = zero.EnglishJson.IndexOf("\"spec\"", System.StringComparison.Ordinal);
            int strAt = zero.EnglishJson.IndexOf("\"str\"", System.StringComparison.Ordinal);
            Assert.Greater(specAt, 0);
            Assert.Greater(strAt, specAt);
            StringAssert.Contains("\"loading.tip.3\"", zero.EnglishJson);
            var rows = Index(zero.Csv);
            Assert.AreEqual("str", rows["loading.tip.3"]["kind"]);
            Assert.AreEqual("0", rows["loading.tip.3"]["placeholder_count"]);
            Assert.AreEqual("true", rows["fx.shred"]["hasPlaceholderLike"]);
            Assert.AreEqual("1", rows["fx.shred"]["placeholder_count"]);
            Assert.AreEqual("6", rows["dock.shop.free"]["max_chars"]);
            StringAssert.Contains("Loading screen tip", rows["loading.tip.3"]["notes"]);
            Assert.AreEqual("", rows["ORBITAL SALVAGE"]["placeholder_count"]);
            Assert.AreEqual("", rows["ORBITAL SALVAGE"]["max_chars"]);

            string editor = File.ReadAllText(Path.Combine(Application.dataPath, "Scripts", "Editor", "LocExtract.cs"));
            StringAssert.Contains("Orbital Salvage/Tools/Extract Localization Spec Strings", editor);
            StringAssert.Contains("RunBatch", editor);
        }

        static Dictionary<string, Dictionary<string, string>> Index(string csv)
        {
            string[] lines = csv.Split('\n');
            List<string> header = ParseCsvLine(lines[0]);
            var rows = new Dictionary<string, Dictionary<string, string>>(System.StringComparer.Ordinal);
            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].Length == 0) continue;
                List<string> fields = ParseCsvLine(lines[i]);
                Assert.AreEqual(header.Count, fields.Count, lines[i]);
                var row = new Dictionary<string, string>(System.StringComparer.Ordinal);
                for (int c = 0; c < header.Count; c++) row[header[c]] = fields[c];
                rows[row["key"]] = row;
            }
            return rows;
        }

        static List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var sb = new StringBuilder();
            bool quote = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (quote)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            sb.Append('"');
                            i++;
                        }
                        else quote = false;
                    }
                    else sb.Append(c);
                }
                else if (c == '"') quote = true;
                else if (c == ',')
                {
                    fields.Add(sb.ToString());
                    sb.Length = 0;
                }
                else sb.Append(c);
            }
            fields.Add(sb.ToString());
            return fields;
        }

        static string Fixture()
        {
            return "{\"elements\":["
                + "{\"id\":\"gift-banner-desc\",\"c\":\"banner\",\"fz\":\"11px\",\"tt\":\"none\",\"r\":[0,0,200,20],\"runs\":["
                + "{\"s\":\"Claim your free daily +100 \",\"r\":[0,0,120,11]},"
                + "{\"s\":\" allowance.\",\"r\":[0,0,60,11]}]},"
                + "{\"id\":\"credits\",\"fz\":\"15px\",\"tt\":\"none\",\"runs\":["
                + "{\"s\":\"  250  \",\"r\":[0,0,27,20]},"
                + "{\"s\":\" / \",\"r\":[0,0,8,20]},"
                + "{\"s\":\"▶\",\"r\":[0,0,10,10]},"
                + "{\"s\":\"×\",\"r\":[0,0,10,10]}]},"
                + "{\"fz\":\"18px\",\"tt\":\"uppercase\",\"r\":[0,0,80,20],\"runs\":["
                + "{\"s\":\" RESUME PLAY\\n              \",\"r\":[0,0,0,0]}]},"
                + "{\"id\":\"mass\",\"fz\":\"12px\",\"tt\":\"none\",\"runs\":["
                + "{\"s\":\"105 kg\",\"r\":[0,0,40,12]}]},"
                + "{\"id\":\"go\",\"fz\":\"16px\",\"tt\":\"none\",\"runs\":["
                + "{\"s\":\"Go \U0001F680\",\"r\":[0,0,30,16]}]},"
                + "{\"id\":\"hello\",\"fz\":\"14px\",\"tt\":\"none\",\"runs\":["
                + "{\"s\":\"Hello, world\",\"r\":[0,0,90,14]}]}"
                + "]}";
        }
    }
}
