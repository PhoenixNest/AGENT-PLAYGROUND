using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;

namespace OrbitalSalvage.Render
{
    public sealed partial class Canvas2D
    {
        public static double SdfRange = 12.0;
        public static double TextGamma = 1.0;
        public static double TextBias = 0.25;
        public static bool SnapGlyphs = true;
        public static bool EmBoxBaselines = true;

        static readonly int IdAtlas = Shader.PropertyToID("_C2D_Atlas");
        static readonly int IdGlyphs = Shader.PropertyToID("_C2D_Glyphs");
        static readonly Regex FontRegex = new Regex(@"(\d+(?:\.\d+)?)px", RegexOptions.Compiled);

        struct FontSpec
        {
            public string family;
            public int weight;
            public double px;
        }

        struct GlyphQuad
        {
            public float lx0, ly0, lx1, ly1, u0, v0, u1, v1;
        }

        sealed class PositionedGlyph
        {
            public TMP_FontAsset font;
            public GlyphQuad quad;
            public double pen;
        }

        sealed class KernTable
        {
            public int count;
            public readonly Dictionary<uint, float> pairs = new Dictionary<uint, float>();
        }

        readonly Dictionary<string, FontSpec> fontCache = new Dictionary<string, FontSpec>();
        readonly Dictionary<TMP_FontAsset, KernTable> kernTables = new Dictionary<TMP_FontAsset, KernTable>();
        readonly List<KeyValuePair<string, bool>> runs = new List<KeyValuePair<string, bool>>();
        readonly List<PositionedGlyph> glyphScratch = new List<PositionedGlyph>();
        readonly List<double> emojiPens = new List<double>();
        readonly List<CanvasImage> emojiImages = new List<CanvasImage>();
        Vector4[] glyphData = new Vector4[3 * 2048];
        int glyphRecords;
        GraphicsBuffer glyphBuffer;
        Material matText;

        public string Font { set { if (!string.IsNullOrWhiteSpace(value)) state.font = value; } }
        public string TextAlign { set { state.textAlign = value; } }
        public string TextBaseline { set { state.textBaseline = value; } }

        void InitText() { }
        void DisposeText() { glyphBuffer?.Dispose(); }
        void BeginFrameText() { glyphRecords = 0; }

        void UploadText()
        {
            if (glyphRecords == 0) return;
            if (glyphBuffer == null || glyphBuffer.count < glyphRecords * 3)
            {
                glyphBuffer?.Dispose();
                int n = 3072;
                while (n < glyphRecords * 3) n *= 2;
                glyphBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, n, 16);
            }
            glyphBuffer.SetData(glyphData, 0, 0, glyphRecords * 3);
            Shader.SetGlobalBuffer(IdGlyphs, glyphBuffer);
        }

        FontSpec ParseFont(string css)
        {
            if (fontCache.TryGetValue(css, out var cached)) return cached;
            var spec = new FontSpec { family = ParityFonts.Nunito, weight = 400, px = 10 };
            var m = FontRegex.Match(css);
            if (m.Success)
            {
                spec.px = double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                foreach (var tok in css.Substring(0, m.Index).Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string t = tok.ToLowerInvariant();
                    if (t == "normal" || t == "italic" || t == "oblique" || t == "small-caps") { if (t == "normal") spec.weight = 400; continue; }
                    if (t == "bold") spec.weight = 700;
                    else if (t == "bolder") spec.weight = 700;
                    else if (t == "lighter") spec.weight = 300;
                    else if (int.TryParse(t, out int w)) spec.weight = w;
                }
                foreach (var fam in css.Substring(m.Index + m.Length).Split(','))
                {
                    string f = fam.Trim().Trim('"', '\'');
                    if (string.Equals(f, ParityFonts.Fredoka, StringComparison.OrdinalIgnoreCase)) { spec.family = ParityFonts.Fredoka; break; }
                    if (string.Equals(f, ParityFonts.Nunito, StringComparison.OrdinalIgnoreCase)) { spec.family = ParityFonts.Nunito; break; }
                }
            }
            fontCache[css] = spec;
            return spec;
        }

        float Kern(TMP_FontAsset font, uint first, uint second)
        {
            if (!kernTables.TryGetValue(font, out var kt)) { kt = new KernTable { count = -1 }; kernTables[font] = kt; }
            var recs = font.fontFeatureTable?.glyphPairAdjustmentRecords;
            if (recs == null) return 0;
            if (kt.count != recs.Count)
            {
                kt.pairs.Clear();
                foreach (var r in recs)
                {
                    uint key = (r.secondAdjustmentRecord.glyphIndex << 16) | r.firstAdjustmentRecord.glyphIndex;
                    kt.pairs[key] = r.firstAdjustmentRecord.glyphValueRecord.xAdvance;
                }
                kt.count = recs.Count;
            }
            return kt.pairs.TryGetValue((second << 16) | first, out float v) ? v : 0;
        }

        bool FindGlyph(TMP_FontAsset primary, uint cp, out TMP_FontAsset owner, out TMP_Character ch)
        {
            owner = primary;
            if (TryGet(primary, cp, out ch)) return true;
            var table = primary.fallbackFontAssetTable;
            if (table != null)
                foreach (var fb in table)
                {
                    if (fb == null) continue;
                    if (TryGet(fb, cp, out ch)) { owner = fb; return true; }
                }
            ch = null;
            return false;
        }

        static readonly HashSet<long> missingGlyphs = new HashSet<long>();
        static readonly uint[] oneChar = new uint[1];

        static bool TryGet(TMP_FontAsset font, uint cp, out TMP_Character ch)
        {
            if (font.characterLookupTable.TryGetValue(cp, out ch)) return true;
            long key = ((long)font.GetInstanceID() << 32) | cp;
            if (missingGlyphs.Contains(key)) { ch = null; return false; }
            oneChar[0] = cp;
            if (font.TryAddCharacters(oneChar, true) && font.characterLookupTable.TryGetValue(cp, out ch)) return true;
            missingGlyphs.Add(key);
            ch = null;
            return false;
        }

        static float Round(double v) => (float)Math.Floor(v + 0.5);

        public void FillText(string text, double x, double y, double maxWidth = double.NaN) { DrawText(text, x, y, false); }
        public void StrokeText(string text, double x, double y, double maxWidth = double.NaN) { DrawText(text, x, y, true); }

        /// <summary>Legacy TMP SDF atlas path (SPEC 6.4 v1.6: not the parity path). Default off.</summary>
        public static bool UseSdfText = false;

        void DrawTextSdf(string text, double x, double y, bool stroke)
        {
            if (string.IsNullOrEmpty(text) || !Finite(x, y)) return;
            var st = state;
            if (st.globalAlpha <= 0 || !st.ctm.IsInvertible) return;
            var spec = ParseFont(st.font);
            var asset = ParityFonts.Resolve(spec.family, spec.weight);
            if (asset == null) return;
            double px = spec.px;
            double s = px / asset.faceInfo.pointSize;
            double pad = asset.atlasPadding;
            var atlas = asset.atlasTexture;
            double atlasW = atlas.width, atlasH = atlas.height;

            CanvasEmoji.SplitRuns(text, runs);
            glyphScratch.Clear();
            emojiPens.Clear();
            emojiImages.Clear();
            double pen = 0;
            foreach (var run in runs)
            {
                if (run.Value)
                {
                    emojiPens.Add(pen);
                    emojiImages.Add(CanvasEmoji.Get(run.Key));
                    pen += px;
                    continue;
                }
                TMP_FontAsset prevFont = null;
                uint prevIndex = 0;
                string str = run.Key;
                for (int i = 0; i < str.Length; i++)
                {
                    uint cp = char.IsHighSurrogate(str[i]) && i + 1 < str.Length ? (uint)char.ConvertToUtf32(str[i], str[i + 1]) : str[i];
                    if (cp > 0xFFFF) i++;
                    if (!FindGlyph(asset, cp, out var owner, out var ch)) { if (!FindGlyph(asset, 0x25A1, out owner, out ch)) continue; }
                    var g = ch.glyph;
                    if (prevFont == owner) pen += Kern(owner, prevIndex, ch.glyphIndex) * s;
                    var r = g.glyphRect;
                    var mt = g.metrics;
                    double ownerScale = px / owner.faceInfo.pointSize;
                    double ownerPad = owner.atlasPadding;
                    double aw = owner.atlasTexture.width, ah = owner.atlasTexture.height;
                    var q = new GlyphQuad
                    {
                        lx0 = (float)((mt.horizontalBearingX - ownerPad) * ownerScale),
                        lx1 = (float)((mt.horizontalBearingX + mt.width + ownerPad) * ownerScale),
                        ly0 = (float)(-(mt.horizontalBearingY + ownerPad) * ownerScale),
                        ly1 = (float)((mt.height - mt.horizontalBearingY + ownerPad) * ownerScale),
                        u0 = (float)((r.x - ownerPad) / aw), u1 = (float)((r.x + r.width + ownerPad) / aw),
                        v0 = (float)((r.y - ownerPad) / ah), v1 = (float)((r.y + r.height + ownerPad) / ah),
                    };
                    glyphScratch.Add(new PositionedGlyph { font = owner, quad = q, pen = pen });
                    pen += mt.horizontalAdvance * ownerScale;
                    prevFont = owner;
                    prevIndex = ch.glyphIndex;
                }
            }
            double total = pen;
            double startX = x;
            string align = st.textAlign;
            if (align == "center") startX = x - total / 2;
            else if (align == "right" || align == "end") startX = x - total;

            double ascentRaw = asset.faceInfo.ascentLine, descentRaw = -asset.faceInfo.descentLine;
            double ascent = Math.Round(ascentRaw * s);
            double descent = Math.Round(descentRaw * s);
            double emAscent = px * ascentRaw / (ascentRaw + descentRaw);
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
            if (glyphScratch.Count > 0) EmitGlyphs(glyphScratch, startX, baseline, px, asset.faceInfo.pointSize, stroke);

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
        }

        void EmitGlyphs(List<PositionedGlyph> glyphs, double startX, double baseline, double px, double pointSize, bool stroke)
        {
            var st = state;
            if (matText == null) matText = MakeMaterial("Hidden/Canvas2D/Text");
            var ctm = st.ctm;
            bool axisAligned = ctm.b == 0 && ctm.c == 0;
            double devScale = ctm.MaxScale();
            double hw = st.lineWidth * 0.5 * devScale;
            float alphaMul = 1f;

            int i = 0;
            while (i < glyphs.Count)
            {
                var font = glyphs[i].font;
                int j = i;
                while (j < glyphs.Count && glyphs[j].font == font) j++;

                int pb = NewParams();
                if (!WritePaint(pb, stroke, (float)(st.globalAlpha * alphaMul))) return;
                prm[pb + IdxFlags] = new Vector4(0, 0, 0, 0);
                BindClip(pb);
                int recBase = glyphRecords;
                int n = j - i;
                if ((glyphRecords + n) * 3 > glyphData.Length) Array.Resize(ref glyphData, Math.Max(glyphData.Length * 2, (glyphRecords + n) * 3));
                double ownerScale = px / font.faceInfo.pointSize;
                double devPerTexel = ownerScale * devScale;
                double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
                for (int k = i; k < j; k++)
                {
                    var pg = glyphs[k];
                    var o = ctm.Apply(startX + pg.pen, baseline);
                    double ox = o.x, oy = o.y;
                    if (SnapGlyphs && axisAligned) { ox = Math.Floor(ox * 4 + 0.5) / 4; oy = Math.Floor(oy + 0.5); }
                    var q = pg.quad;
                    int d = (glyphRecords++) * 3;
                    glyphData[d] = new Vector4((float)ox, (float)oy, q.lx0, q.ly0);
                    glyphData[d + 1] = new Vector4(q.lx1, q.ly1, q.u0, q.v0);
                    glyphData[d + 2] = new Vector4(q.u1, q.v1, 0, 0);
                    double[] xs = { q.lx0, q.lx1 }, ys = { q.ly0, q.ly1 };
                    foreach (double lx in xs)
                        foreach (double ly in ys)
                        {
                            double px2 = ox + ctm.a * lx + ctm.c * ly, py2 = oy + ctm.b * lx + ctm.d * ly;
                            if (px2 < minX) minX = px2;
                            if (px2 > maxX) maxX = px2;
                            if (py2 < minY) minY = py2;
                            if (py2 > maxY) maxY = py2;
                        }
                }
                prm[pb + IdxEdge] = new Vector4(recBase, 0, 0, 0);
                prm[pb + IdxImgA] = new Vector4((float)ctm.a, (float)ctm.b, (float)ctm.c, (float)ctm.d);
                prm[pb + IdxImgC] = new Vector4((float)TextBias, 0, 0, 0);
                prm[pb + IdxText] = new Vector4(stroke ? 1f : 0f, (float)hw, (float)TextGamma, (float)(2 * SdfRange * devPerTexel));
                SetRect(pb, 0, 0, Width, Height);
                cmd.SetRenderTarget(target);
                cmd.SetGlobalTexture(IdAtlas, font.atlasTexture);
                Draw(matText, 0, 6 * n, pb);
                i = j;
            }
        }
    }
}
