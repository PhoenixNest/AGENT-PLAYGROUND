using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitalSalvage.Render
{
    public sealed partial class Canvas2D
    {
        struct LaidGlyph
        {
            public TrueTypeFont font;
            public int gid;
            public double pen; // CSS px from the start of the string
        }

        readonly struct LayoutKey : IEquatable<LayoutKey>
        {
            public readonly string text;
            public readonly TrueTypeFont font;
            public readonly int px, ls, chain;
            public LayoutKey(string text, TrueTypeFont font, int px, int ls, int chain)
            {
                this.text = text; this.font = font; this.px = px; this.ls = ls; this.chain = chain;
            }
            public bool Equals(LayoutKey o) => px == o.px && ls == o.ls && chain == o.chain && font == o.font && text == o.text;
            public override bool Equals(object obj) => obj is LayoutKey o && Equals(o);
            public override int GetHashCode()
            {
                int h = text == null ? 0 : text.GetHashCode();
                h = (h * 397) ^ (font == null ? 0 : font.GetHashCode());
                h = (h * 397) ^ px;
                h = (h * 397) ^ ls;
                if (chain != 0) h = (h * 397) ^ chain;
                return h;
            }
        }

        /// <summary>When set, outline layout uses this chain instead of <see cref="OutlineFonts.Chain(TrueTypeFont)"/>. Null keeps the English path.</summary>
        public TrueTypeFont[] TextChain;
        public int TextChainStamp;

        sealed class LayoutHit
        {
            public LaidGlyph[] glyphs = Array.Empty<LaidGlyph>();
            public double[] emojiPens = Array.Empty<double>();
            public CanvasImage[] emoji = Array.Empty<CanvasImage>();
            public double width;
        }

        readonly Dictionary<LayoutKey, LayoutHit> layoutCache = new Dictionary<LayoutKey, LayoutHit>();

        /// <summary>Drops measured runs after a language change. Glyph outlines stay cached.</summary>
        public void ClearTextLayoutCache()
        {
            layoutCache.Clear();
        }

        readonly List<LaidGlyph> laid = new List<LaidGlyph>(128);
        readonly List<int> segOut = new List<int>(64);
        readonly List<TrueTypeFont> cpFonts = new List<TrueTypeFont>(64);
        readonly List<int> cpGids = new List<int>(64);

        void DrawText(string text, double x, double y, bool stroke)
        {
            if (UseSdfText) { DrawTextSdf(text, x, y, stroke); return; }
            DrawTextOutline(text, x, y, stroke);
        }

        /// <summary>Width of <paramref name="text"/> as the web wrapper computes it (SPEC 6.2 step 2): text runs measured, emoji = fontPx.</summary>
        public double MeasureText(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            var spec = ParseFont(state.font);
            if (TextChain == null || TextChain.Length == 0)
            {
                var primary = OutlineFonts.Primary(spec.family, spec.weight);
                return Layout(text, spec.px, primary, null, true);
            }
            return Layout(text, spec.px, TextChain[0], TextChain, true);
        }

        /// <summary>
        /// Lays the string out into <see cref="laid"/> / <see cref="emojiPens"/> / <see cref="emojiImages"/> and returns the total advance.
        /// </summary>
        double Layout(string text, double px, TrueTypeFont primary, TrueTypeFont[] chainOverride, bool measureOnly)
        {
            PaintProfile.Begin(PaintProfile.Layout);
            int stamp = chainOverride == null ? 0 : (TextChainStamp == 0 ? 1 : TextChainStamp);
            var key = new LayoutKey(text, primary, (int)Math.Round(px * 64.0), (int)Math.Round(state.letterSpacing * 64.0), stamp);
            if (layoutCache.TryGetValue(key, out var hit))
            {
                if (!measureOnly) ApplyLayoutHit(hit);
                PaintProfile.End(PaintProfile.Layout);
                return hit.width;
            }
            var chain = chainOverride != null && chainOverride.Length > 0 ? chainOverride : OutlineFonts.Chain(primary);
            CanvasEmoji.SplitRuns(text, runs);
            laid.Clear(); emojiPens.Clear(); emojiImages.Clear();
            double pen = 0;
            double k = px / primary.unitsPerEm;
            foreach (var run in runs)
            {
                if (run.Value)
                {
                    emojiPens.Add(pen); emojiImages.Add(CanvasEmoji.Get(run.Key));
                    pen += px;
                    continue;
                }
                string str = run.Key;
                cpFonts.Clear(); cpGids.Clear();
                for (int i = 0; i < str.Length; i++)
                {
                    int cp = char.IsHighSurrogate(str[i]) && i + 1 < str.Length ? char.ConvertToUtf32(str[i], str[i + 1]) : str[i];
                    if (cp > 0xFFFF) i++;
                    TrueTypeFont f = chain[0];
                    int gid = 0;
                    foreach (var cand in chain)
                    {
                        gid = cand.GlyphIndex(cp);
                        if (gid != 0) { f = cand; break; }
                    }
                    cpFonts.Add(f); cpGids.Add(gid);
                }
                int s = 0;
                while (s < cpFonts.Count)
                {
                    var font = cpFonts[s];
                    int e = s;
                    while (e < cpFonts.Count && cpFonts[e] == font) e++;
                    segOut.Clear();
                    for (int i = s; i < e; i++) segOut.Add(cpGids[i]);
                    // Blink shapes word by word; the GSUB lookups never see a space.
                    if (state.letterSpacing == 0) font.Substitute(segOut); // Blink disables ligatures when letter-spacing is set
                    double fk = px / font.unitsPerEm;
                    int spaceGid = font.GlyphIndex(0x20);
                    for (int i = 0; i < segOut.Count; i++)
                    {
                        laid.Add(new LaidGlyph { font = font, gid = segOut[i], pen = pen });
                        double adv = font.Advance(segOut[i]);
                        // Blink shapes word by word: no kerning pair involving a space applies (measured against Chromium).
                        if (i + 1 < segOut.Count && segOut[i] != spaceGid && segOut[i + 1] != spaceGid) adv += font.Kerning(segOut[i], segOut[i + 1]);
                        pen += adv * fk + state.letterSpacing;
                    }
                    s = e;
                }
            }
            var stored = new LayoutHit
            {
                width = pen,
                glyphs = laid.ToArray(),
                emojiPens = emojiPens.ToArray(),
                emoji = emojiImages.ToArray()
            };
            layoutCache[key] = stored;
            if (measureOnly) { laid.Clear(); emojiPens.Clear(); emojiImages.Clear(); }
            PaintProfile.End(PaintProfile.Layout);
            return pen;
        }

        void ApplyLayoutHit(LayoutHit hit)
        {
            laid.Clear();
            for (int i = 0; i < hit.glyphs.Length; i++) laid.Add(hit.glyphs[i]);
            emojiPens.Clear();
            emojiImages.Clear();
            for (int i = 0; i < hit.emojiPens.Length; i++) emojiPens.Add(hit.emojiPens[i]);
            for (int i = 0; i < hit.emoji.Length; i++) emojiImages.Add(hit.emoji[i]);
        }

        /// <summary>Rounded hhea ascent in CSS px of the current font (Blink's text-fragment ascent): baseline = top of the line box's text rect + this.</summary>
        public double FontAscent()
        {
            var spec = ParseFont(state.font);
            var primary = AscentFace(spec.family, spec.weight);
            return RoundHalfUp(primary.ascender * spec.px / primary.unitsPerEm);
        }

        /// <summary>
        /// The captured line box was measured with Fredoka or Nunito. A CJK primary has a
        /// different em box, so the baseline stays on the Latin face and the ideograph
        /// sits on the same alphabetic line as the English caps. Fredoka, Nunito (including
        /// the Uzbek patch), and the symbol fonts keep their own ascent.
        /// </summary>
        TrueTypeFont AscentFace(string family, int weight)
        {
            if (TextChain != null && TextChain.Length > 0 && !LatinStrut(TextChain[0]))
                return OutlineFonts.Primary(family, weight);
            if (TextChain != null && TextChain.Length > 0) return TextChain[0];
            return OutlineFonts.Primary(family, weight);
        }

        static bool LatinStrut(TrueTypeFont font)
        {
            if (font == null || string.IsNullOrEmpty(font.name)) return false;
            return font.name.StartsWith("Fredoka", StringComparison.Ordinal)
                || font.name.StartsWith("Nunito", StringComparison.Ordinal)
                || font.name.StartsWith("NotoSans", StringComparison.Ordinal);
        }

        static double RoundHalfUp(double v) => Math.Floor(v + 0.5);

        void DrawTextOutline(string text, double x, double y, bool stroke)
        {
            if (string.IsNullOrEmpty(text) || !Finite(x, y)) return;
            PaintProfile.Begin(PaintProfile.Layout);
            var st = state;
            if (st.globalAlpha <= 0 || !st.ctm.IsInvertible) { PaintProfile.End(PaintProfile.Layout); return; }
            var spec = ParseFont(st.font);
            TrueTypeFont primary;
            double px = spec.px;
            double total;
            if (TextChain == null || TextChain.Length == 0)
            {
                primary = OutlineFonts.Primary(spec.family, spec.weight);
                total = Layout(text, px, primary, null, false);
            }
            else
            {
                primary = TextChain[0];
                total = Layout(text, px, primary, TextChain, false);
            }

            double startX = x;
            string align = st.textAlign;
            if (align == "center") startX = x - total / 2;
            else if (align == "right" || align == "end") startX = x - total;

            // Blink: ascent/descent are the rounded hhea metrics; top/middle/bottom use the OS/2 typo metrics normalised to 1 em.
            double ascent = RoundHalfUp(primary.ascender * px / primary.unitsPerEm);
            double descent = RoundHalfUp(-primary.descender * px / primary.unitsPerEm);
            double ta = primary.typoAscender, td = -primary.typoDescender;
            double emAscent = ta + td > 0 ? px * ta / (ta + td) : ascent;
            double emDescent = px - emAscent;
            double baseline = y;
            switch (st.textBaseline)
            {
                case "top": baseline = y + emAscent; break;
                case "middle": baseline = y + (emAscent - emDescent) / 2; break;
                case "bottom": baseline = y - emDescent; break;
                case "hanging": baseline = y + 0.8 * ascent; break;
                case "ideographic": baseline = y - descent; break;
            }

            if (laid.Count > 0) EmitOutlineGlyphs(startX, baseline, px, stroke);

            if (!stroke)
            {
                for (int i = 0; i < emojiImages.Count; i++)
                {
                    if (emojiImages[i] == null) continue;
                    double top;
                    if (st.textBaseline == "top") top = y;
                    else if (st.textBaseline == "middle") top = y - px / 2;
                    else top = y + 0.125 * px - px;
                    DrawImage(emojiImages[i], startX + emojiPens[i], top, px, px);
                }
            }
            PaintProfile.End(PaintProfile.Layout);
        }

        void EmitOutlineGlyphs(double startX, double baseline, double px, bool stroke)
        {
            PaintProfile.Begin(PaintProfile.GlyphEmit);
            var ctm = state.ctm;
            bool axisAligned = ctm.b == 0 && ctm.c == 0;
            double devScale = ctm.MaxScale();
            if (stroke) tmpPath.Reset(); else polys.Clear();
            foreach (var g in laid)
            {
                PaintProfile.Glyphs++;
                var fg = OutlineFonts.Flat(g.font, g.gid, px, devScale);
                if (fg.contourCount == 0) continue;
                var o = ctm.Apply(startX + g.pen, baseline);
                double ox = o.x, oy = o.y;
                if (SnapGlyphs && axisAligned)
                {
                    ox = Math.Floor(ox * GlyphSubpixel + GlyphRound) / GlyphSubpixel + GlyphDx;
                    double yUnits = oy * GlyphSubpixelY;
                    double yFrac = yUnits - Math.Floor(yUnits);
                    oy = Math.Floor(yUnits + 0.5) / GlyphSubpixelY + GlyphDy;
                    // A baseline halfway between device pixels lands one pixel high versus Chromium.
                    if (GlyphSubpixelY <= 1.0 && yFrac > 0.45 && yFrac < 0.55) oy += 1.0;
                }
                int total = fg.pts.Length / 2;
                for (int c = 0; c < fg.contourCount; c++)
                {
                    int a = fg.starts[c], b = c + 1 < fg.contourCount ? fg.starts[c + 1] : total;
                    if (!stroke) polys.Begin();
                    for (int i = a; i < b; i++)
                    {
                        double rx = fg.pts[i * 2], ry = fg.pts[i * 2 + 1];
                        double dx = ox + ctm.a * rx + ctm.c * ry, dy = oy + ctm.b * rx + ctm.d * ry;
                        if (stroke)
                        {
                            if (i == a) tmpPath.MoveTo(Matrix2D.Identity, dx, dy); else tmpPath.LineTo(Matrix2D.Identity, dx, dy);
                        }
                        else polys.pts.Add(new Vec2d(dx, dy));
                    }
                    if (stroke) tmpPath.ClosePath();
                }
            }
            if (stroke) StrokePath(tmpPath);
            else
            {
                glyphMaskFill = GlyphPreblend;
                RenderShape(polys, false, false, 1f, false);
                glyphMaskFill = false;
            }
            PaintProfile.End(PaintProfile.GlyphEmit);
        }

        // ---- Skia glyph-mask preblend (SkMaskGamma with sRGB luminance) --------------------------------

        /// <summary>Apply Skia's text coverage preblend (measured against Chromium: sRGB, SK_GAMMA_CONTRAST).</summary>
        public static bool GlyphPreblend = true;
        /// <summary>Glyph origin quantisation in device px (Skia subpixel positioning): steps per px, rounding bias, extra offsets (calibration knobs).</summary>
        public static double GlyphSubpixel = 4, GlyphSubpixelY = 1, GlyphRound = 0.5, GlyphDx = 0, GlyphDy = 0;
        public static double GlyphContrast = 0.5;
        bool glyphMaskFill;

        static double SrgbToLin(double c) => c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        static double LinToSrgb(double c) => c <= 0.0031308 ? c * 12.92 : 1.055 * Math.Pow(c, 1 / 2.4) - 0.055;

        void WriteGlyphPreblend(int pb)
        {
            var c = state.fillColor;
            // SkComputeLuminance: (54 r + 183 g + 19 b) >> 8 on the 8-bit sRGB channels, then the top 3 bits.
            int r8 = (int)Math.Floor(c.r * 255 + 0.5), g8 = (int)Math.Floor(c.g * 255 + 0.5), b8 = (int)Math.Floor(c.b * 255 + 0.5);
            int lum = (r8 * 54 + g8 * 183 + b8 * 19) >> 8;
            int idx = Math.Min(7, Math.Max(0, lum >> 5));
            double src = ((idx << 5) | (idx << 2) | (idx >> 1)) / 255.0;
            double dst = 1 - src;
            if (Math.Abs(src - dst) < 1.0 / 256) return;
            double linSrc = SrgbToLin(src), linDst = SrgbToLin(dst);
            double cc = GlyphContrast * linDst;
            prm[pb + IdxText] = new Vector4(1f, (float)cc, (float)src, (float)dst);
            prm[pb + IdxImgC] = new Vector4((float)linSrc, (float)linDst, 0, 0);
        }
    }
}
