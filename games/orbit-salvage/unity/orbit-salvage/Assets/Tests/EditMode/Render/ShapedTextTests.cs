using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;
using OrbitalSalvage.Loc;
using UnityEngine;

namespace OrbitalSalvage.Render.Tests
{
    using Loc = OrbitalSalvage.Loc.Loc;

    public class ShapedTextTests
    {
        const string OutDir = "/tmp/maze-i18n/shaping-core";

        static string FontDir
        {
            get { return Path.Combine(Application.dataPath, "Fonts"); }
        }

        static string ShapedDir
        {
            get { return Path.Combine(Application.dataPath, "Resources", "Localization", "shaped"); }
        }

        [SetUp]
        public void Reset()
        {
            Loc.Reset(null);
            OutlineFonts.Clear();
        }

        [TearDown]
        public void Restore()
        {
            Loc.Reset(null);
            OutlineFonts.Clear();
        }

        [Test]
        public void Reader_RoundTrip_RebuildsTheTemplate()
        {
            ShapedTable table = Load("ar");
            Assert.IsFalse(table.Refused);
            Assert.AreEqual("Cairo-700", table.FontName);
            Assert.AreEqual(1000, table.UnitsPerEm);
            Assert.AreEqual(700, table.Weight);
            bool saw = false;
            for (int i = 0; i < table.Count; i++)
            {
                ShapedTable.Entry entry = table.EntryAt(i);
                Assert.AreEqual(ShapedTable.Hash(entry.Template), entry.Hash);
                ShapedTable.Resolved resolved;
                Assert.IsTrue(table.TryResolve(entry.Template, out resolved));
                Assert.AreSame(entry, resolved.Entry);
                if (entry.Template == "اختر اللغة") saw = true;
            }
            Assert.IsTrue(saw, "Arabic language label is not in the table");
        }

        [Test]
        public void GlyphOffsets_AreAppliedInCanvasPixels()
        {
            using (var canvas = new Canvas2D(64, 64))
            {
                canvas.Font = "700 40px Fredoka";
                canvas.ShapeCode = "hi";
                canvas.LetterSpacing = 0;
                var marks = new List<Canvas2D.ShapedMark>();
                double width;
                Assert.IsTrue(canvas.LayoutShapedMarks("कंपन", marks, out width));
                Assert.Greater(width, 0);
                bool shifted = false;
                for (int i = 0; i < marks.Count; i++)
                {
                    Assert.AreEqual("Baloo2-700", marks[i].Font);
                    if (Math.Abs(marks[i].Dx) > 0.5 || Math.Abs(marks[i].Dy) > 0.5) shifted = true;
                }
                Assert.IsTrue(shifted, "no HarfBuzz offset reached the laid glyphs");
            }
        }

        [Test]
        public void RtlLine_PutsTheLatinRunOnTheLeft()
        {
            ShapedTable table = Load("ar");
            const string text = "هدف: 100 kg";
            ShapedTable.Resolved resolved;
            Assert.IsTrue(table.TryResolve(text, out resolved));
            ShapedText.Fit fit = ShapedText.Build(resolved, s => s.Length * 8.0, 32, table.UnitsPerEm, 4000, 0, TextFit.Action.Single, 1);
            Assert.IsTrue(fit.RightAlign);
            Assert.Greater(fit.Lines[0].Pieces.Length, 1);
            Assert.That(fit.Lines[0].Pieces[0].Text, Does.Contain("kg"));
            string last = fit.Lines[0].Pieces[fit.Lines[0].Pieces.Length - 1].Text;
            Assert.That(last, Does.Contain("ه"));
        }

        [Test]
        public void KhmerWrap_BreaksOnlyBetweenClusters()
        {
            ShapedTable table = Load("km");
            const string text = "ការលេង កាក់មាស និងឈុត រក្សាទុកលើឧបករណ៍របស់អ្នកតាម localStorage ក្នុងកម្មវិធីរុករក។ គ្មានគណនី គ្មានការតាមដានពីក្រៅ និងគ្មានផ្សាយពាណិជ្ជកម្ម។";
            ShapedTable.Resolved resolved;
            Assert.IsTrue(table.TryResolve(text, out resolved));
            ShapedText.Fit fit = ShapedText.Build(resolved, s => 10, 28, table.UnitsPerEm, 120, 0, TextFit.Action.Wrap, 12);
            Assert.Greater(fit.Lines.Length, 1, "the paragraph did not wrap");
            for (int i = 0; i < fit.Lines.Length; i++)
            {
                string line = fit.Lines[i].Text ?? "";
                Assert.Greater(line.Length, 0);
                int cp = char.ConvertToUtf32(line, 0);
                Assert.AreNotEqual(0x17D2, cp, "line " + i + " starts with a coeng");
                bool vowel = (cp >= 0x17B6 && cp <= 0x17D3) || cp == 0x17DD;
                Assert.IsFalse(vowel, "line " + i + " starts inside a cluster");
            }
        }

        [Test]
        public void Placeholder_SplicesAWesternNumber()
        {
            ShapedTable table = Load("ar");
            ShapedTable.Resolved resolved;
            Assert.IsTrue(table.TryResolve("هدف: 7 kg", out resolved));
            Assert.AreEqual("7", resolved.SlotText(0));
            Assert.IsTrue(table.TryResolve("هدف: XX kg", out resolved));
            Assert.AreEqual("XX", resolved.SlotText(0));
            Assert.IsFalse(table.TryResolve("هدف: كلمة kg", out resolved));
            Assert.IsFalse(table.TryResolve("كلمة20", out resolved));
            Assert.IsTrue(ShapedTable.IsWesternNumber("20"));
            Assert.IsFalse(ShapedTable.IsWesternNumber("٢٠"));
            Assert.IsTrue(table.TryResolve("12 / 20 kg", out resolved));
            Assert.IsFalse(resolved.RightAlign, "a Latin-only quota is not an Arabic paragraph");
            ShapedText.Fit quota = ShapedText.Build(resolved, s => s.Length * 8.0, 28, table.UnitsPerEm, 4000, 0, TextFit.Action.Single, 1);
            Assert.IsTrue(quota.Lines[0].Text.StartsWith("12"), quota.Lines[0].Text);
            Assert.IsTrue(quota.Lines[0].Pieces[0].Text.StartsWith("12"), quota.Lines[0].Pieces[0].Text);
        }

        [Test]
        public void Placeholder_SplicesABakedName()
        {
            ShapedTable table = Load("ar");
            ShapedTable.Resolved resolved;
            const string toast = "🏆 كأس: أول نام نام! (+100 🪙)";
            Assert.IsTrue(table.TryResolve(toast, out resolved));
            Assert.AreEqual("أول نام نام!", resolved.SlotText(0));
            Assert.AreEqual("100", resolved.SlotText(1));
            ShapedText.Fit fit = ShapedText.Build(resolved, s => s.Length * 8.0, 28, table.UnitsPerEm, 4000, 0, TextFit.Action.Single, 1);
            bool named = false;
            for (int i = 0; i < fit.Lines[0].Pieces.Length; i++)
            {
                ShapedText.Piece piece = fit.Lines[0].Pieces[i];
                if (piece.Glyphs != null && piece.Glyphs.Length > 0 && piece.Text != null && piece.Text.Contains("نام"))
                    named = true;
            }
            Assert.IsTrue(named, "the achievement title was not drawn from its baked run");
            Assert.IsTrue(table.TryResolve("نظف أول نام نام!! 🚀", out resolved));
            Assert.AreEqual("أول نام نام!", resolved.SlotText(0));
        }

        [Test]
        public void GameplayWords_UseTheShapedFace()
        {
            using (var canvas = new Canvas2D(420, 96))
            {
                canvas.Font = "700 28px Fredoka";
                canvas.ShapeCode = "ar";
                var marks = new List<Canvas2D.ShapedMark>();
                double width;
                Assert.IsTrue(canvas.LayoutShapedMarks("كوكبي", marks, out width));
                Assert.Greater(marks.Count, 0);
                Assert.AreEqual("Cairo-700", marks[0].Font);
                marks.Clear();
                Assert.IsTrue(canvas.LayoutShapedMarks("×5! فرم ⚡", marks, out width));
                bool cairo = false;
                for (int i = 0; i < marks.Count; i++)
                    if (marks[i].Font == "Cairo-700") cairo = true;
                Assert.IsTrue(cairo, "floating shred text did not use Cairo");
                marks.Clear();
                canvas.ShapeCode = "hi";
                Assert.IsTrue(canvas.LayoutShapedMarks("⏩ तेज़", marks, out width));
                bool baloo = false;
                for (int i = 0; i < marks.Count; i++)
                    if (marks[i].Font == "Baloo2-700") baloo = true;
                Assert.IsTrue(baloo, "boost label did not use Baloo");
                marks.Clear();
                canvas.ShapeCode = "km";
                Assert.IsTrue(canvas.LayoutShapedMarks("ញ៉ាំ! 😋", marks, out width));
                bool khmer = false;
                for (int i = 0; i < marks.Count; i++)
                    if (marks[i].Font == "KantumruyPro-700") khmer = true;
                Assert.IsTrue(khmer, "nom word did not use Kantumruy");
            }
        }

        [Test]
        public void ArabicParagraph_RightAlignsInsideTheSameRect()
        {
            ShapedTable table = Load("ar");
            ShapedTable.Resolved resolved;
            Assert.IsTrue(table.TryResolve("اختر اللغة", out resolved));
            ShapedText.Fit fit = ShapedText.Build(resolved, s => 8, 32, table.UnitsPerEm, 400, 0, TextFit.Action.Single, 1);
            Assert.IsTrue(fit.RightAlign);
            double x = TextFit.PlaceX(TextFit.Anchor.Right, 0, 200, fit.Widths[0], 10, 190);
            Assert.AreEqual(190 - fit.Widths[0], x, 0.01);
            ShapedTable hi = Load("hi");
            Assert.IsTrue(hi.TryResolve("हिन्दी", out resolved));
            ShapedText.Fit hindi = ShapedText.Build(resolved, s => 8, 32, hi.UnitsPerEm, 400, 0, TextFit.Action.Single, 1);
            Assert.IsFalse(hindi.RightAlign);
        }

        [Test]
        public void Uppercase_IsSkippedForBakedScripts()
        {
            const string mixed = "हिन्दी kg";
            Assert.AreEqual(mixed, LocCase.Upper(mixed, "hi"));
            Assert.AreEqual("العربية", LocCase.Upper("العربية", "ar"));
            Assert.AreEqual("ភាសាខ្មែរ", LocCase.Upper("ភាសាខ្មែរ", "km"));
            Assert.AreNotEqual("hello", LocCase.Upper("hello", "de"));
        }

        [Test]
        public void FontHashMismatch_RefusesTheTable()
        {
            byte[] data = File.ReadAllBytes(Path.Combine(ShapedDir, "ar.bytes"));
            byte[] font = File.ReadAllBytes(Path.Combine(FontDir, "Cairo-700.ttf"));
            ShapedTable ok = ShapedTable.Load(data, font);
            Assert.IsFalse(ok.Refused);
            Assert.Greater(ok.Count, 0);
            var bad = (byte[])font.Clone();
            bad[bad.Length / 2] ^= 0x5A;
            ShapedTable refused = ShapedTable.Load(data, bad);
            Assert.IsTrue(refused.Refused);
            ShapedTable.Resolved resolved;
            Assert.IsFalse(refused.TryResolve("اختر اللغة", out resolved));
        }

        [Test]
        public void EveryBakedGlyph_ExistsInTheShippedFont()
        {
            CheckFont("ar", "Cairo-700.ttf");
            CheckFont("hi", "Baloo2-700.ttf");
            CheckFont("km", "KantumruyPro-700.ttf");
        }

        [Test]
        public void MissingBake_PaintsNothing_AndEnglishDoesNotEnter()
        {
            using (var canvas = new Canvas2D(32, 32))
            {
                canvas.Font = "700 16px Fredoka";
                var marks = new List<Canvas2D.ShapedMark>();
                double width;
                Assert.IsFalse(canvas.LayoutShapedMarks("PLAY", marks, out width));
                canvas.ShapeCode = "ar";
                Assert.IsTrue(canvas.LayoutShapedMarks("كلمة ليست هنا", marks, out width));
                Assert.AreEqual(0, width, 0.01);
                Assert.AreEqual(0, marks.Count);
            }
        }

        [Test]
        public void HomeRingLabel_PaintsAfterTheChildEmojiIsStripped()
        {
            Directory.CreateDirectory(OutDir);
            string[] keys = { "home.ring.donut", "home.ring.duckie", "home.ring.candy", "home.ring.kitty" };
            foreach (string lang in new[] { "hi", "km", "ar" })
            {
                ShapedTable table = Load(lang);
                LocTable loc = ReadTable(lang);
                foreach (string key in keys)
                {
                    string full = loc.Str[key];
                    ShapedTable.Resolved whole;
                    Assert.IsTrue(table.TryResolve(full, out whole), key);
                    Assert.AreEqual(ShapedTable.KindEmoji, whole.Entry.Runs[0].Kind, key);
                    string cut = full.Substring(whole.Entry.Runs[0].Text.Length);
                    ShapedTable.Resolved tail;
                    Assert.IsTrue(table.TryResolve(cut, out tail), lang + " " + key + " cut " + cut);
                    Assert.AreNotEqual(ShapedTable.KindEmoji, tail.Entry.Runs[0].Kind, key);
                    using (var canvas = new Canvas2D(64, 64))
                    {
                        canvas.Font = "700 28px Fredoka";
                        canvas.ShapeCode = lang;
                        canvas.LetterSpacing = 0;
                        var marks = new List<Canvas2D.ShapedMark>();
                        double width;
                        Assert.IsTrue(canvas.LayoutShapedMarks(cut, marks, out width), cut);
                        Assert.Greater(width, 8, cut);
                        Assert.Greater(marks.Count, 0, cut);
                        string face = lang == "ar" ? "Cairo-700" : lang == "hi" ? "Baloo2-700" : "KantumruyPro-700";
                        bool script = false;
                        for (int i = 0; i < marks.Count; i++)
                            if (marks[i].Font == face) script = true;
                        Assert.IsTrue(script, lang + " " + key + " did not use " + face);
                    }
                }
                string donut = loc.Str["home.ring.donut"];
                ShapedTable.Resolved donutEntry;
                Assert.IsTrue(table.TryResolve(donut, out donutEntry));
                string pill = donut.Substring(donutEntry.Entry.Runs[0].Text.Length);
                int ink = Paint(lang, pill, lang + "-ring-pill.png", false, false);
                Assert.Greater(ink, 20, lang + " ring pill painted nothing");
            }
        }

        [Test]
        public void TranslatedNames_ResolveInsteadOfReturningEnglish()
        {
            LocTable english = ReadTable("en");
            Loc.SetLoader(new FileLocLoader());
            foreach (LanguageInfo info in LanguageRegistry.All)
            {
                if (info.Code == "en") continue;
                LocTable table = ReadTable(info.Code);
                Assert.IsTrue(Loc.SetLanguage(info.Code), info.Code);
                Assert.AreEqual(info.Code, Loc.CurrentLanguage, info.Code + " table did not load");
                foreach (var kv in english.Spec)
                {
                    string translated;
                    if (!table.Spec.TryGetValue(kv.Key, out translated)) continue;
                    if (translated == null || translated.Trim() == kv.Value.Trim()) continue;
                    Assert.AreEqual(translated.Trim(), Loc.TrName(kv.Key), info.Code + " TrName " + kv.Key);
                }
                if (!ShapedText.IsComplex(info.Code)) continue;
                ShapedTable shaped = Load(info.Code);
                foreach (var kv in table.Spec)
                    AssertResolves(shaped, info.Code, "spec", kv.Key, kv.Value);
                foreach (var kv in table.Str)
                    AssertResolves(shaped, info.Code, "str", kv.Key, kv.Value);
                string line = Loc.Tr("home.level.title", Loc.TrName("LVL 1"), Loc.TrName("Bubblegum Chute 1"));
                AssertResolves(shaped, info.Code, "composed", "home.level.title", line);
                string toast = Loc.Tr("toast.sector.cleanup", Loc.TrName("Bubblegum Chute 1"));
                AssertResolves(shaped, info.Code, "composed", "toast.sector.cleanup", toast);
            }
        }

        static void AssertResolves(ShapedTable table, string lang, string section, string key, string text)
        {
            if (string.IsNullOrEmpty(text) || !ShapedTable.ContainsScript(text, lang)) return;
            ShapedTable.Resolved resolved;
            Assert.IsTrue(table.TryResolve(text, out resolved), lang + " " + section + " " + key + " does not bake: " + text);
            if (resolved.Entry.Runs == null || resolved.Entry.Runs.Length == 0) return;
            if (resolved.Entry.Runs[0].Kind != ShapedTable.KindEmoji) return;
            string cut = text.Substring(resolved.Entry.Runs[0].Text.Length);
            if (!ShapedTable.ContainsScript(cut, lang)) return;
            ShapedTable.Resolved tail;
            Assert.IsTrue(table.TryResolve(cut, out tail), lang + " emoji cut " + key + " : " + cut);
            Assert.AreNotEqual(ShapedTable.KindEmoji, tail.Entry.Runs[0].Kind, key);
        }

        static LocTable ReadTable(string code)
        {
            string path = Path.Combine(Application.dataPath, "Resources", "Localization", code + ".json");
            LocTable table;
            Assert.IsTrue(LocTable.TryParse(File.ReadAllText(path), out table), code);
            return table;
        }

        [Test]
        public void Stills_ShowJoinsConjunctsCoengAndWraps()
        {
            Directory.CreateDirectory(OutDir);
            int latin = Paint(null, "HI", "latin-control.png", false, false);
            int joined = Paint("ar", "اختر اللغة", "ar-joined.png", false, false);
            int mixed = Paint("ar", "هدف: 100 kg", "ar-mixed.png", false, false);
            int right = Paint("ar", "اختر اللغة", "ar-right.png", true, false);
            int wrapAr = Paint("ar", "نحفظ تقدم اللعب وعملات ذهبية والأزياء محليا على جهازك عبر localStorage في المتصفح. بلا حساب، وبلا تتبع خارجي، وبلا إعلانات سلوكية.", "ar-wrap.png", false, true);
            int hi = Paint("hi", "100% ऑफ़लाइन, बच्चे सुरक्षित:", "hi-conjunct.png", false, false);
            int below = Paint("hi", "0 / 3 पूरा", "hi-below.png", false, false);
            int km = Paint("km", "0 / 3 បានបញ្ចប់", "km-coeng.png", false, false);
            int wrapKm = Paint("km", "ការលេង កាក់មាស និងឈុត រក្សាទុកលើឧបករណ៍របស់អ្នកតាម localStorage ក្នុងកម្មវិធីរុករក។ គ្មានគណនី គ្មានការតាមដានពីក្រៅ និងគ្មានផ្សាយពាណិជ្ជកម្ម។", "km-wrap.png", false, true);
            int wrapHi = Paint("hi", "खेल की प्रगति, सोने के सिक्के और कपड़े तुम्हारे डिवाइस पर ब्राउज़र localStorage में सहेजे जाते हैं। कोई खाता नहीं, कोई ट्रैकिंग SDK नहीं, और व्यवहार वाले विज्ञापन नहीं।", "hi-wrap.png", false, true);
            Assert.Greater(latin, 30, "Latin control still is blank");
            Assert.Greater(joined, 80, "Arabic join still is blank");
            Assert.Greater(mixed, 80, "mixed still is blank");
            Assert.Greater(right, 80, "right-align still is blank");
            Assert.Greater(wrapAr, 80);
            Assert.Greater(hi, 40, "Devanagari still is blank");
            Assert.Greater(below, 20, "below-base matra still is blank");
            Assert.Greater(km, 40, "Khmer still is blank");
            Assert.Greater(wrapKm, 80);
            Assert.Greater(wrapHi, 80);
            string joinedNote = File.ReadAllText(Path.Combine(OutDir, "ar-joined.png.txt"));
            Assert.That(joinedNote, Does.Contain("font=Cairo-700"));
            Assert.That(joinedNote, Does.Not.Contain("marks=0"));
        }

        static int Paint(string lang, string text, string file, bool right, bool wrap)
        {
            const int w = 720;
            const int h = 280;
            using (var canvas = new Canvas2D(w, h))
            {
                canvas.BeginFrame();
                canvas.FillStyle = "#101018";
                canvas.FillRect(0, 0, w, h);
                canvas.FillStyle = "#f4f0e6";
                canvas.Font = "700 36px Fredoka";
                canvas.LetterSpacing = 0.4;
                canvas.TextBaseline = "alphabetic";
                canvas.TextAlign = "left";
                canvas.ShapeCode = lang;
                var note = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(lang))
                {
                    var marks = new List<Canvas2D.ShapedMark>();
                    double shapedWidth;
                    bool laid = canvas.LayoutShapedMarks(text, marks, out shapedWidth);
                    note.Append("laid=").Append(laid).Append(" marks=").Append(marks.Count).Append(" width=").Append(shapedWidth.ToString("0.0"));
                    if (marks.Count > 0)
                        note.Append(" dx0=").Append(marks[0].Dx.ToString("0.00")).Append(" font=").Append(marks[0].Font);
                    note.Append('\n');
                }
                if (!wrap)
                {
                    double width = canvas.MeasureText(text);
                    note.Append("measure=").Append(width.ToString("0.0")).Append('\n');
                    double x = right ? w - 24 - width : 24;
                    canvas.FillText(text, x, 96);
                }
                else
                {
                    Assert.IsTrue(ShapedText.TryFit(canvas, text, lang, 640, 36, 0.4, TextFit.Action.Wrap, 8, out ShapedText.Fit fit), lang + " did not resolve");
                    Assert.Greater(fit.Lines.Length, 1, lang + " paragraph did not wrap");
                    note.Append("lines=").Append(fit.Lines.Length).Append('\n');
                    for (int i = 0; i < fit.Lines.Length; i++)
                    {
                        double x = fit.RightAlign ? 24 + (640 - fit.Widths[i]) : 24;
                        canvas.PreparedLine = fit.Lines[i];
                        canvas.FillText(fit.Texts[i], x, 48 + i * 44);
                        canvas.PreparedLine = null;
                    }
                }
                canvas.EndFrame();
                RenderTexture prev = RenderTexture.active;
                RenderTexture source = canvas.Target;
                var copy = new RenderTexture(source.width, source.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
                copy.Create();
                Graphics.CopyTexture(source, copy);
                RenderTexture.active = copy;
                var tex = new Texture2D(copy.width, copy.height, TextureFormat.RGB24, false, true);
                tex.ReadPixels(new Rect(0, 0, copy.width, copy.height), 0, 0, false);
                tex.Apply(false);
                RenderTexture.active = prev;
                byte[] png = tex.EncodeToPNG();
                Color32[] pixels = tex.GetPixels32();
                int ink = 0;
                int dark = 0;
                for (int i = 0; i < pixels.Length; i++)
                {
                    if (pixels[i].r < 40 && pixels[i].g < 40 && pixels[i].b < 50) dark++;
                    else if (pixels[i].r > 180 && pixels[i].g > 160 && pixels[i].b > 140) ink++;
                }
                note.Append("corner=").Append(pixels[0].r).Append(',').Append(pixels[0].g).Append(',').Append(pixels[0].b).Append(',').Append(pixels[0].a);
                note.Append(" ink=").Append(ink).Append(" dark=").Append(dark).Append('\n');
                UnityEngine.Object.DestroyImmediate(tex);
                copy.Release();
                UnityEngine.Object.DestroyImmediate(copy);
                File.WriteAllBytes(Path.Combine(OutDir, file), png);
                File.WriteAllText(Path.Combine(OutDir, file + ".txt"), note.ToString());
                Assert.Greater(dark, 1000, file + " background was not drawn\n" + note);
                Assert.Greater(ink, 20, file + " has no glyphs\n" + note);
                return ink;
            }
        }

        static void CheckFont(string lang, string file)
        {
            ShapedTable table = Load(lang);
            var face = new TrueTypeFont(file, File.ReadAllBytes(Path.Combine(FontDir, file)));
            Assert.IsTrue(ShapedTable.HashEquals(table.FontHash, ShapedTable.Sha256(faceBytes(file))));
            for (int i = 0; i < table.Count; i++)
            {
                ShapedTable.Run[] runs = table.EntryAt(i).Runs;
                for (int r = 0; r < runs.Length; r++)
                {
                    ShapedTable.Glyph[] glyphs = runs[r].Glyphs;
                    if (glyphs == null) continue;
                    for (int g = 0; g < glyphs.Length; g++)
                    {
                        int gid = glyphs[g].Gid;
                        Assert.Greater(gid, 0, lang + " gid 0 in " + table.EntryAt(i).Template);
                        Assert.Less(gid, face.numGlyphs, lang + " gid " + gid);
                    }
                }
            }
        }

        static byte[] faceBytes(string file)
        {
            return File.ReadAllBytes(Path.Combine(FontDir, file));
        }

        static ShapedTable Load(string lang)
        {
            byte[] data = File.ReadAllBytes(Path.Combine(ShapedDir, lang + ".bytes"));
            string font = lang == "ar" ? "Cairo-700.ttf" : lang == "hi" ? "Baloo2-700.ttf" : "KantumruyPro-700.ttf";
            return ShapedTable.Load(data, File.ReadAllBytes(Path.Combine(FontDir, font)));
        }

        sealed class FileLocLoader : ILocTableLoader
        {
            public bool TryLoad(string code, out string json)
            {
                string path = Path.Combine(Application.dataPath, "Resources", "Localization", code + ".json");
                if (!File.Exists(path))
                {
                    json = null;
                    return false;
                }
                json = File.ReadAllText(path);
                return true;
            }
        }
    }
}
