using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;
using OrbitalSalvage.Loc;
using OrbitalSalvage.Render;
using OrbitalSalvage.Viewer.Editor;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Tests
{
    using Loc = OrbitalSalvage.Loc.Loc;

    public class LocFontPickerTests
    {
        const string Dir = "/tmp/maze-i18n/loc-fonts";

        [TearDown]
        public void ResetLanguage()
        {
            Loc.Reset(null);
            LocFonts.Reset();
            OutlineFonts.Clear();
        }

        [Test]
        public void LanguagePicker_DrawsEndonyms_AndSettingsEnglishStaysPut()
        {
            Directory.CreateDirectory(Dir);
            Loc.Reset(new ResourcesLocTableLoader());
            Assert.IsTrue(Loc.SetLanguage("en"));
            byte[] settings = Paint("settings_default", "settings_default__en.png", false);
            byte[] settingsFonts = Paint("settings_default", "settings_default__en-overrides.png", true);
            Assert.AreEqual(Sha(settings), Sha(settingsFonts), "settings_default changed when picker ids were installed");

            byte[] picker = Paint("settings_modal_language", "settings_modal_language__en.png", true);
            Assert.Greater(picker.Length, 8);
            AssertCovered("de", "Deutsch");
            AssertCovered("fr", "Français");
            AssertCovered("vi", "Tiếng Việt");
            AssertCovered("uz", "O\u02BBzbekcha");
            AssertCovered("zh-Hans", "简体中文");
            AssertCovered("zh-Hant", "繁體中文");
            AssertCovered("ja", "日本語");
            AssertCovered("ko", "한국어");
            AssertCovered("km", "ភាសាខ្មែរ");
            AssertCovered("hi", "हिन्दी");
            AssertCovered("ar", "العربية");

            Assert.IsTrue(Loc.SetLanguage("uz"), "uz.json did not load");
            OutlineFonts.Clear();
            Paint("settings_modal_language", "uz-after-picker.png", true);
            Paint("maps_world_1_fresh", "uz-after-play.png", false);
            Paint("shop_skyboxes_default", "uz-after-level.png", false);
            Paint("modal_defeat_12_of_20", "uz-after-void.png", false);

            Assert.IsTrue(Loc.SetDiagnostic("qps"));
            Paint("settings_default", "settings_default__qps.png", false);
            Paint("settings_modal_language", "settings_modal_language__qps.png", true);
        }

        static void AssertCovered(string code, string text)
        {
            TrueTypeFont[] chain = OutlineFonts.ChainFor(code, "Fredoka", 800);
            for (int i = 0; i < text.Length; i++)
            {
                int cp = char.ConvertToUtf32(text, i);
                if (char.IsHighSurrogate(text[i])) i++;
                if (cp == 0x20) continue;
                bool hit = false;
                for (int f = 0; f < chain.Length && !hit; f++)
                    hit = chain[f].GlyphIndex(cp) != 0;
                Assert.IsTrue(hit, code + " U+" + cp.ToString("X4"));
            }
        }

        static void AssertHasHole(string code, string text)
        {
            TrueTypeFont[] chain = OutlineFonts.ChainFor(code, "Fredoka", 800);
            bool hole = false;
            for (int i = 0; i < text.Length && !hole; i++)
            {
                int cp = char.ConvertToUtf32(text, i);
                if (char.IsHighSurrogate(text[i])) i++;
                bool hit = false;
                for (int f = 0; f < chain.Length && !hit; f++)
                    hit = chain[f].GlyphIndex(cp) != 0;
                hole = !hit;
            }
            Assert.IsTrue(hole, code + " was expected to still miss a glyph");
        }

        static byte[] Paint(string id, string file, bool pickerFonts)
        {
            Texture2D tex = null;
            try
            {
                tex = PagePreviewPainter.Render(id, "zero", true, 0, 0, chrome =>
                {
                    if (pickerFonts) chrome.ApplyPickerFontOverrides();
                }, out byte[] png);
                Assert.IsNotNull(png, id);
                File.WriteAllBytes(Path.Combine(Dir, file), png);
                return png;
            }
            finally
            {
                if (tex != null) Object.DestroyImmediate(tex);
            }
        }

        static string Sha(byte[] data)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(data);
                var sb = new System.Text.StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("X2"));
                return sb.ToString();
            }
        }
    }
}
