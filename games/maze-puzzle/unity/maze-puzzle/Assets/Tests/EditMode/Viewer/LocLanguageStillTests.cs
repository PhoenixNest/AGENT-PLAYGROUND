using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using OrbitalSalvage.Loc;
using OrbitalSalvage.Render;
using OrbitalSalvage.Viewer.Editor;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Tests
{
    using Loc = OrbitalSalvage.Loc.Loc;

    /// <summary>
    /// Every zero and iphone spec, painted the same way <see cref="LocPseudoStillTests"/> paints qps.
    /// One extra language-picker still moves the current-language check onto that language.
    /// The 90-spec pass does not: <see cref="PagePreviewPainter"/> keeps the captured English mark
    /// unless the caller passes text overrides.
    /// </summary>
    public class LocLanguageStillTests
    {
        const string Root = "/tmp/maze-i18n/audit-all";

        [TearDown]
        public void RestoreEnglish()
        {
            Loc.SetLoader(new ResourcesLocTableLoader());
            Loc.SetLanguage("en");
            Loc.Reset(null);
            OutlineFonts.Clear();
            HomeChrome.RecordRuns = false;
        }

        [TestCase("de")]
        [TestCase("fr")]
        [TestCase("es")]
        [TestCase("it")]
        [TestCase("pt-PT")]
        [TestCase("vi")]
        [TestCase("fil")]
        [TestCase("id")]
        [TestCase("uz")]
        [TestCase("zh-Hans")]
        [TestCase("zh-Hant")]
        [TestCase("ja")]
        [TestCase("ko")]
        [Timeout(1500000)]
        public void Language_PaintsEveryZeroAndIphonePage(string code)
        {
            string table = Path.Combine(Application.dataPath, "Resources", "Localization", code + ".json");
            if (!File.Exists(table))
                Assert.Ignore(code + ".json is not in Resources yet");
            string dir = Path.Combine(Root, code, "stills");
            Directory.CreateDirectory(dir);
            Loc.SetLoader(new ResourcesLocTableLoader());
            Assert.IsTrue(Loc.SetLanguage("en"));
            byte[] englishHome = Paint("home_default", "zero");

            Assert.IsTrue(Loc.SetLanguage(code), code + ".json did not load from Resources");
            Assert.AreEqual(code, Loc.CurrentLanguage);
            OutlineFonts.Clear();

            string specDir = Path.Combine(Application.dataPath, "Resources", "Parity", "Spec");
            string[] files = Directory.GetFiles(specDir, "*.json");
            Array.Sort(files, StringComparer.Ordinal);
            int painted = 0;
            var names = new List<string>();
            for (int i = 0; i < files.Length; i++)
            {
                string name = Path.GetFileName(files[i]);
                string profile = null;
                if (name.EndsWith("__zero.json", StringComparison.Ordinal)) profile = "zero";
                else if (name.EndsWith("__iphone.json", StringComparison.Ordinal)) profile = "iphone";
                if (profile == null) continue;
                string id = name.Substring(0, name.Length - ("__" + profile + ".json").Length);
                byte[] png = Paint(id, profile);
                string file = id + "__" + profile + ".png";
                File.WriteAllBytes(Path.Combine(dir, file), png);
                Assert.Greater(png.Length, 8, file);
                names.Add(file + "\t" + png.Length.ToString());
                painted++;
            }
            Assert.AreEqual(90, painted, code);
            byte[] home = File.ReadAllBytes(Path.Combine(dir, "home_default__zero.png"));
            Assert.AreNotEqual(Sha(englishHome), Sha(home), code + " home_default stayed on the English pixels");

            byte[] marked = Paint("settings_modal_language", "zero", chrome => MarkLanguage(chrome, code));
            File.WriteAllBytes(Path.Combine(dir, "settings_modal_language__marked.png"), marked);
            names.Add("settings_modal_language__marked.png\t" + marked.Length.ToString());
            File.WriteAllLines(Path.Combine(dir, "index.txt"), names);

            string captured = CheckMarks(code, null);
            string moved = CheckMarks(code, chrome => MarkLanguage(chrome, code));
            var note = new StringBuilder();
            note.Append("language=").Append(code).Append('\n');
            note.Append("default-painter keeps the captured mark. Lines are id, then drawn text, for the check and the two buttons we care about.\n");
            note.Append("# default\n").Append(captured);
            note.Append("# marked\n").Append(moved);
            note.Append("default-check\t").Append(CheckOwner(captured)).Append('\n');
            note.Append("marked-check\t").Append(CheckOwner(moved)).Append('\n');
            File.WriteAllText(Path.Combine(Root, code, "picker-marker.txt"), note.ToString());
            Assert.That(captured, Does.Contain("\u2713"), code + " default picker lost the check mark");
            Assert.That(CheckOwner(captured), Does.Contain("btn-lang-en"),
                code + " default painter did not keep the captured English check\n" + captured);
            Assert.That(CheckOwner(moved), Does.Contain("btn-lang-" + code),
                code + " marked picker did not place the check\n" + moved);
        }

        static void MarkLanguage(HomeChrome chrome, string code)
        {
            LanguageInfo info;
            Assert.IsTrue(LanguageRegistry.TryGet(code, out info), code);
            chrome.TextOverrides["settings-language-value"] = info.Endonym;
            chrome.TextOverrides[LanguageMarker.ButtonId("en")] = "English";
            chrome.TextOverrides[LanguageMarker.ButtonId(code)] = LanguageMarker.Mark(info.Endonym);
        }

        static string CheckMarks(string code, Action<HomeChrome> prepare)
        {
            HomeChrome.RecordRuns = true;
            HomeChrome chrome = HomeChrome.Load("settings_modal_language", "zero");
            try
            {
                chrome.ApplyPickerFontOverrides();
                if (prepare != null) prepare(chrome);
                chrome.Draw();
                string current = "btn-lang-" + code;
                var sb = new StringBuilder();
                for (int i = 0; i < chrome.RecordedRuns.Count; i++)
                {
                    HomeChrome.PaintedRun run = chrome.RecordedRuns[i];
                    string id = run.Id ?? "";
                    string text = run.Text ?? "";
                    bool check = text.IndexOf('\u2713') >= 0;
                    if (!check && id != "btn-lang-en" && id != current) continue;
                    sb.Append(id).Append('\t').Append(text).Append('\n');
                }
                return sb.ToString();
            }
            finally
            {
                chrome.Dispose();
                HomeChrome.RecordRuns = false;
            }
        }

        static string CheckOwner(string recorded)
        {
            var owners = new List<string>();
            string[] lines = recorded.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                int tab = lines[i].IndexOf('\t');
                if (tab <= 0) continue;
                string text = lines[i].Substring(tab + 1);
                if (text.StartsWith("\u2713", StringComparison.Ordinal)) owners.Add(lines[i].Substring(0, tab));
            }
            return owners.Count == 0 ? "(none)" : string.Join(",", owners);
        }

        static byte[] Paint(string id, string profile)
        {
            return Paint(id, profile, null);
        }

        static byte[] Paint(string id, string profile, Action<HomeChrome> prepare)
        {
            Texture2D tex = null;
            try
            {
                tex = PagePreviewPainter.Render(id, profile, true, 0, 0, prepare, out byte[] png);
                Assert.IsNotNull(png, id + " " + profile);
                return png;
            }
            finally
            {
                if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
            }
        }

        static string Sha(byte[] data)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(data);
                var sb = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("X2"));
                return sb.ToString();
            }
        }
    }
}
