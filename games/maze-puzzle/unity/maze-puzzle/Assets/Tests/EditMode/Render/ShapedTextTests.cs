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
                if (entry.Template == "إنقاذ مداري") saw = true;
            }
            Assert.IsTrue(saw, "fixture Arabic title is not in the table");
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
                Assert.IsTrue(canvas.LayoutShapedMarks("कु", marks, out width));
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
            const string text = "المرحلة 12 من Orbital Salvage";
            ShapedTable.Resolved resolved;
            Assert.IsTrue(table.TryResolve(text, out resolved));
            ShapedText.Fit fit = ShapedText.Build(resolved, s => s.Length * 8.0, 32, table.UnitsPerEm, 4000, 0, TextFit.Action.Single, 1);
            Assert.IsTrue(fit.RightAlign);
            Assert.Greater(fit.Lines[0].Pieces.Length, 1);
            Assert.That(fit.Lines[0].Pieces[0].Text, Does.Contain("Orbital Salvage"));
            string last = fit.Lines[0].Pieces[fit.Lines[0].Pieces.Length - 1].Text;
            Assert.That(last, Does.Contain("ا"));
        }

        [Test]
        public void KhmerWrap_BreaksOnlyBetweenClusters()
        {
            ShapedTable table = Load("km");
            const string text = "តើអ្នកប្រាកដទេ? វានឹងកំណត់ផែនទីដែលបានបើក សម្លៀកបំពាក់ផ្ទាល់ខ្លួន និងកាក់មាសដែលរកបានឡើងវិញ។ មិនអាចធ្វើវិញបានទេ!";
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
            Assert.IsTrue(table.TryResolve("الهدف: 20 كغ", out resolved));
            Assert.AreEqual("20", resolved.SlotText(0));
            Assert.IsFalse(table.TryResolve("الهدف: XX كغ", out resolved));
            Assert.IsFalse(table.TryResolve("كلمة20", out resolved));
            Assert.IsTrue(ShapedTable.IsWesternNumber("20"));
            Assert.IsFalse(ShapedTable.IsWesternNumber("٢٠"));
        }

        [Test]
        public void ArabicParagraph_RightAlignsInsideTheSameRect()
        {
            ShapedTable table = Load("ar");
            ShapedTable.Resolved resolved;
            Assert.IsTrue(table.TryResolve("إنقاذ مداري", out resolved));
            ShapedText.Fit fit = ShapedText.Build(resolved, s => 8, 32, table.UnitsPerEm, 400, 0, TextFit.Action.Single, 1);
            Assert.IsTrue(fit.RightAlign);
            double x = TextFit.PlaceX(TextFit.Anchor.Right, 0, 200, fit.Widths[0], 10, 190);
            Assert.AreEqual(190 - fit.Widths[0], x, 0.01);
            ShapedTable hi = Load("hi");
            Assert.IsTrue(hi.TryResolve("कु", out resolved));
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
            Assert.IsFalse(refused.TryResolve("إنقاذ مداري", out resolved));
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
        public void Stills_ShowJoinsConjunctsCoengAndWraps()
        {
            Directory.CreateDirectory(OutDir);
            int latin = Paint(null, "HI", "latin-control.png", false, false);
            int joined = Paint("ar", "إنقاذ مداري", "ar-joined.png", false, false);
            int mixed = Paint("ar", "المرحلة 12 من Orbital Salvage", "ar-mixed.png", false, false);
            int right = Paint("ar", "إنقاذ مداري", "ar-right.png", true, false);
            int wrapAr = Paint("ar", "هل أنت متأكد؟ سيؤدي هذا إلى إعادة ضبط كل الخرائط المفتوحة والأزياء المخصصة والعملات الذهبية المكتسبة. لا يمكن التراجع عن هذا الإجراء!", "ar-wrap.png", false, true);
            int hi = Paint("hi", "कक्षीय सफ़ाई", "hi-conjunct.png", false, false);
            int below = Paint("hi", "कु", "hi-below.png", false, false);
            int km = Paint("km", "ការសម្អាតសំរាមអវកាស", "km-coeng.png", false, false);
            int wrapKm = Paint("km", "តើអ្នកប្រាកដទេ? វានឹងកំណត់ផែនទីដែលបានបើក សម្លៀកបំពាក់ផ្ទាល់ខ្លួន និងកាក់មាសដែលរកបានឡើងវិញ។ មិនអាចធ្វើវិញបានទេ!", "km-wrap.png", false, true);
            int wrapHi = Paint("hi", "क्या आप सुनिश्चित हैं? कृपया प्रतीक्षा करें। यह क्रिया सभी अनलॉक किए गए नक्शे, कस्टम पोशाक और अर्जित स्वर्ण सिक्के रीसेट कर देगी। इसे वापस नहीं लिया जा सकता!", "hi-wrap.png", false, true);
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
            Assert.That(joinedNote, Does.Contain("marks=11"));
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
    }
}
