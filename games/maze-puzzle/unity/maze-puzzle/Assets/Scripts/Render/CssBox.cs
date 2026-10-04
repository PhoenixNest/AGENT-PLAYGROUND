using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace OrbitalSalvage.Render
{
    /// <summary>One parsed CSS box-shadow or text-shadow layer (lengths in CSS px).</summary>
    public struct CssShadow
    {
        public double dx, dy, blur, spread;
        public Rgba color;
        public bool inset;
    }

    /// <summary>The paintable part of an element's computed style: border box in CSS px, background, radii, border and shadows.</summary>
    public sealed class CssBoxStyle
    {
        public double x, y, w, h;
        public double opacity = 1;
        public bool hasBg;
        public Rgba bg;
        public string bgImage;
        /// <summary>Per-corner radii (top-left, top-right, bottom-right, bottom-left), resolved to CSS px.</summary>
        public Vec2d[] radii = new Vec2d[4];
        public double[] borderW = new double[4];   // top, right, bottom, left
        public Rgba[] borderC = new Rgba[4];
        public bool[] borderOn = new bool[4];
        public bool[] borderDashed = new bool[4];
        public readonly List<CssShadow> shadows = new List<CssShadow>();
        internal CanvasGradient[] cachedLayers;
        internal string cachedCss;
        internal double cx, cy, cw, ch;
    }

    /// <summary>
    /// Draws one CSS box with <see cref="Canvas2D"/>: background colour or gradient, per-corner radius, border, and box shadows.
    /// Solid extrusion shadows (<c>0 Npx 0 color</c>) are an offset rounded rect behind the box; blurred shadows use the canvas
    /// shadow of an off-canvas copy of the shape so that only the shadow lands on the target. Not implemented (reported by the
    /// caller as deferred): inset shadows, backdrop-filter, transforms, filters, animations, per-side border styles other than solid.
    /// </summary>
    public static class CssBox
    {
        const double OffCanvas = 100000.0;

        // ---- parsing ------------------------------------------------------------------------------------

        public static double ParseLength(string s, double basis = 0)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            s = s.Trim();
            if (s == "normal" || s == "auto" || s == "none") return 0;
            double scale = 1;
            if (s.EndsWith("%", StringComparison.Ordinal)) { s = s.Substring(0, s.Length - 1); scale = basis / 100.0; }
            else if (s.EndsWith("px", StringComparison.Ordinal)) s = s.Substring(0, s.Length - 2);
            return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v * scale : 0;
        }

        /// <summary>Splits on commas that are not inside parentheses.</summary>
        public static List<string> SplitTop(string s)
        {
            var parts = new List<string>();
            int depth = 0, start = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                if (ch == '(') depth++;
                else if (ch == ')') depth--;
                else if (ch == ',' && depth == 0) { parts.Add(s.Substring(start, i - start).Trim()); start = i + 1; }
            }
            if (start < s.Length) parts.Add(s.Substring(start).Trim());
            return parts;
        }

        /// <summary>Splits on spaces that are not inside parentheses.</summary>
        static List<string> Tokens(string s)
        {
            var parts = new List<string>();
            int depth = 0, start = -1;
            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                if (ch == '(') depth++;
                else if (ch == ')') depth--;
                if (ch == ' ' && depth == 0) { if (start >= 0) { parts.Add(s.Substring(start, i - start)); start = -1; } }
                else if (start < 0) start = i;
            }
            if (start >= 0) parts.Add(s.Substring(start));
            return parts;
        }

        static bool IsColorToken(string t) => t.StartsWith("rgb", StringComparison.OrdinalIgnoreCase) || t.StartsWith("#") || t == "transparent" || char.IsLetter(t[0]) && CssColor.TryParse(t, out _);

        static readonly Dictionary<string, List<CssShadow>> shadowCache = new Dictionary<string, List<CssShadow>>();
        static readonly List<CssShadow> NoShadows = new List<CssShadow>();

        /// <summary>Parses <c>color dx dy [blur [spread]] [inset]</c> layers (box-shadow) or <c>color dx dy [blur]</c> (text-shadow). The returned list is shared and must not be mutated.</summary>
        public static List<CssShadow> ParseShadows(string css)
        {
            PaintProfile.Begin(PaintProfile.ParseShadows);
            if (string.IsNullOrWhiteSpace(css) || css == "none") { PaintProfile.End(PaintProfile.ParseShadows); return NoShadows; }
            if (shadowCache.TryGetValue(css, out var cached)) { PaintProfile.End(PaintProfile.ParseShadows); return cached; }
            var list = new List<CssShadow>();
            foreach (var layer in SplitTop(css))
            {
                var sh = new CssShadow { color = Rgba.Black };
                var nums = new List<double>();
                foreach (var tok in Tokens(layer))
                {
                    if (tok == "inset") sh.inset = true;
                    else if (IsColorToken(tok) && !char.IsDigit(tok[0]) && tok[0] != '-' && tok[0] != '.') sh.color = CssColor.Parse(tok, Rgba.Black);
                    else nums.Add(ParseLength(tok));
                }
                if (nums.Count > 0) sh.dx = nums[0];
                if (nums.Count > 1) sh.dy = nums[1];
                if (nums.Count > 2) sh.blur = nums[2];
                if (nums.Count > 3) sh.spread = nums[3];
                list.Add(sh);
            }
            shadowCache[css] = list;
            PaintProfile.End(PaintProfile.ParseShadows);
            return list;
        }

        // ---- box drawing ----------------------------------------------------------------------------------

        static Vec2d[] ResolveRadii(CssBoxStyle s, double grow)
        {
            var r = new Vec2d[4];
            for (int i = 0; i < 4; i++) r[i] = new Vec2d(Math.Max(0, s.radii[i].x + grow), Math.Max(0, s.radii[i].y + grow));
            return r;
        }

        /// <summary>
        /// Device-pixel shadow offset that cancels a local-space shift of <paramref name="offX"/>/<paramref name="offY"/>
        /// and then applies a CSS offset. The off-canvas trick shifts the shape by a local X and must be undone with the
        /// full linear transform: using only <c>ctm.a</c> drops the shadow by <c>OffCanvas * ctm.b</c> once the element rotates.
        /// </summary>
        static void ShadowDevice(Canvas2D c, double cssDx, double cssDy, double offX, double offY, out double devX, out double devY, out double blurScale)
        {
            var m = c.CurrentTransform;
            double lx = cssDx + offX, ly = cssDy + offY;
            devX = m.a * lx + m.c * ly;
            devY = m.b * lx + m.d * ly;
            blurScale = Math.Sqrt(m.a * m.a + m.b * m.b);
            if (blurScale < 1e-9) blurScale = Math.Abs(m.a);
        }

        static void RRect(Canvas2D c, double x, double y, double w, double h, Vec2d[] r)
        {
            if (w <= 0 || h <= 0) return;
            bool square = true;
            foreach (var v in r) if (v.x > 0 || v.y > 0) square = false;
            if (square) c.Rect(x, y, w, h); else c.RoundRect(x, y, w, h, r);
        }

        /// <summary>
        /// Recover the pre-transform ellipse for a computed <c>perspective(d) rotateX(a)</c>.
        /// The spec stores only the foreshortened AABB; the layout size is the unique ellipse whose projection has that box.
        /// </summary>
        public static bool TryPerspectiveDisc(string tf, double x, double y, double w, double h,
            out double ox, out double oy, out double cos, out double sod, out double halfW, out double halfH)
        {
            ox = oy = cos = sod = halfW = halfH = 0;
            if (string.IsNullOrEmpty(tf) || !tf.StartsWith("matrix3d(", StringComparison.Ordinal) || tf[tf.Length - 1] != ')') return false;
            var parts = tf.Substring(9, tf.Length - 10).Split(',');
            if (parts.Length < 16) return false;
            var m = new double[16];
            for (int i = 0; i < 16; i++)
                if (!double.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out m[i])) return false;
            // Column-major. perspective(d) * rotateX(a) has no translation and no X scale.
            if (Math.Abs(m[0] - 1) > 1e-3 || Math.Abs(m[1]) > 1e-3 || Math.Abs(m[4]) > 1e-3) return false;
            if (Math.Abs(m[12]) > 1e-3 || Math.Abs(m[13]) > 1e-3) return false;
            cos = m[5];
            double sin = m[6];
            sod = -m[7];
            if (!(sod > 1e-6) || cos < 0.2 || Math.Abs(m[9] + sin) > 2e-2) return false;
            double A = h * sod;
            double B = 2 * cos;
            double disc = B * B + 4 * A * A;
            if (!(A > 0) || disc <= 0) return false;
            double u = (-B + Math.Sqrt(disc)) / (2 * A);
            if (!(u > 0) || u >= 0.9) return false;
            halfH = u / sod;
            halfW = w * 0.5 * (1 - u);
            ox = x + w * 0.5;
            oy = y + h - halfH * cos / (1 - u);
            return halfW > 1 && halfH > 1;
        }

        public static void TracePerspectiveEllipse(Canvas2D c, double ox, double oy, double halfW, double halfH, double cos, double sod, double shiftX, double shiftY)
        {
            const int n = 96;
            for (int i = 0; i < n; i++)
            {
                double t = Math.PI * 2 * i / n;
                double lx = halfW * Math.Cos(t) + shiftX;
                double ly = halfH * Math.Sin(t) + shiftY;
                double clipW = 1 - ly * sod;
                if (clipW < 1e-4) clipW = 1e-4;
                double sx = ox + lx / clipW;
                double sy = oy + ly * cos / clipW;
                if (i == 0) c.MoveTo(sx, sy);
                else c.LineTo(sx, sy);
            }
            c.ClosePath();
        }

        /// <summary>Outer box-shadows of a perspective disc. The blur stays in screen space; the silhouette is the projected ellipse.</summary>
        public static void DrawProjectedShadows(Canvas2D c, CssBoxStyle s, double alpha, List<CssShadow> shadows,
            double ox, double oy, double cos, double sod, double halfW, double halfH)
        {
            if (shadows == null || shadows.Count == 0 || alpha <= 0) return;
            c.Save();
            c.GlobalAlpha = Math.Min(1, alpha);
            for (int i = shadows.Count - 1; i >= 0; i--)
            {
                var sh = shadows[i];
                if (sh.inset || sh.color.a <= 0) continue;
                double pad = sh.blur + sh.spread + Math.Abs(sh.dx) + Math.Abs(sh.dy) + 4;
                c.Save();
                c.BeginPath();
                c.Rect(s.x - pad, s.y - pad, s.w + pad * 2, s.h + pad * 2);
                TracePerspectiveEllipse(c, ox, oy, halfW, halfH, cos, sod, 0, 0);
                c.Clip("evenodd");
                c.FillStyle = "rgba(0,0,0,1)";
                if (sh.blur <= 0)
                {
                    c.SetFill(sh.color);
                    c.BeginPath();
                    TracePerspectiveEllipse(c, ox, oy, halfW + sh.spread, halfH + sh.spread, cos, sod, sh.dx, sh.dy);
                    c.Fill();
                }
                else
                {
                    c.SetShadow(sh.color);
                    ShadowDevice(c, sh.dx, sh.dy, OffCanvas, 0, out var devX, out var devY, out var blurScale);
                    c.ShadowBlur = sh.blur * blurScale;
                    c.ShadowOffsetX = devX;
                    c.ShadowOffsetY = devY;
                    c.BeginPath();
                    TracePerspectiveEllipse(c, ox - OffCanvas, oy, halfW + sh.spread, halfH + sh.spread, cos, sod, 0, 0);
                    c.Fill();
                }
                c.Restore();
            }
            c.Restore();
        }

        public static void DrawProjectedBorder(Canvas2D c, CssBoxStyle s, double alpha,
            double ox, double oy, double cos, double sod, double halfW, double halfH)
        {
            double bw = s.borderW[0];
            if (bw <= 0 || alpha <= 0 || s.borderC[0].a <= 0) return;
            bool dashed = s.borderDashed[0] && s.borderDashed[1] && s.borderDashed[2] && s.borderDashed[3];
            c.Save();
            c.GlobalAlpha = Math.Min(1, alpha);
            if (dashed)
            {
                c.StrokeStyle = ToCss(s.borderC[0]);
                c.LineWidth = bw;
                c.LineCap = "butt";
                c.SetLineDash(new[] { bw * 3, bw });
                c.BeginPath();
                TracePerspectiveEllipse(c, ox, oy, Math.Max(0.5, halfW - bw * 0.5), Math.Max(0.5, halfH - bw * 0.5), cos, sod, 0, 0);
                c.Stroke();
                c.SetLineDash(null);
            }
            else if (s.borderOn[0])
            {
                c.SetFill(s.borderC[0]);
                c.BeginPath();
                TracePerspectiveEllipse(c, ox, oy, halfW, halfH, cos, sod, 0, 0);
                TracePerspectiveEllipse(c, ox, oy, Math.Max(0.5, halfW - bw), Math.Max(0.5, halfH - bw), cos, sod, 0, 0);
                c.Fill("evenodd");
            }
            c.Restore();
        }

        /// <summary>Draws the box. <paramref name="alpha"/> is the accumulated opacity of the element and its ancestors.</summary>
        public static void Draw(Canvas2D c, CssBoxStyle s, double alpha)
        {
            Draw(c, s, alpha, true, true);
        }

        public static void Draw(Canvas2D c, CssBoxStyle s, double alpha, bool outerShadows, bool border)
        {
            PaintProfile.Begin(PaintProfile.CssBox);
            if (s.w <= 0 || s.h <= 0 || alpha <= 0) { PaintProfile.End(PaintProfile.CssBox); return; }
            c.Save();
            c.GlobalAlpha = Math.Min(1, alpha);

            // Outer shadows are visible only outside the border box. A fill, even a translucent one, composites
            // against the page rather than against this element's own shadow.
            var outer = ResolveRadii(s, 0);
            if (outerShadows)
            for (int i = s.shadows.Count - 1; i >= 0; i--)
            {
                var sh = s.shadows[i];
                if (sh.inset || sh.color.a <= 0) continue;
                double sx = s.x - sh.spread, sy = s.y - sh.spread, sw = s.w + 2 * sh.spread, shh = s.h + 2 * sh.spread;
                var radii = ResolveRadii(s, sh.spread);
                double pad = sh.blur + sh.spread + Math.Abs(sh.dx) + Math.Abs(sh.dy) + 4;
                c.Save();
                c.BeginPath();
                c.Rect(s.x - pad, s.y - pad, s.w + pad * 2, s.h + pad * 2);
                RRect(c, s.x, s.y, s.w, s.h, outer);
                c.Clip("evenodd");
                c.Save();
                c.FillStyle = "rgba(0,0,0,1)";
                if (sh.blur <= 0)
                {
                    c.SetFill(sh.color);
                    c.BeginPath();
                    RRect(c, sx + sh.dx, sy + sh.dy, sw, shh, radii);
                    c.Fill();
                }
                else
                {
                    c.SetShadow(sh.color);
                    ShadowDevice(c, sh.dx, sh.dy, OffCanvas, 0, out var devX, out var devY, out var blurScale);
                    c.ShadowBlur = sh.blur * blurScale;
                    c.ShadowOffsetX = devX;
                    c.ShadowOffsetY = devY;
                    c.BeginPath();
                    RRect(c, sx - OffCanvas, sy, sw, shh, radii);
                    c.Fill();
                }
                c.Restore();
                c.Restore();
            }
            if (s.hasBg && s.bg.a > 0)
            {
                c.SetFill(s.bg);
                c.BeginPath();
                RRect(c, s.x, s.y, s.w, s.h, outer);
                c.Fill();
            }
            if (!string.IsNullOrEmpty(s.bgImage) && s.bgImage != "none")
            {
                var split = SplitTop(s.bgImage);
                for (int li = split.Count - 1; li >= 0; li--)
                {
                    if (CssGradient.IsEllipse(split[li]))
                    {
                        CssGradient.FillEllipse(c, split[li], s.x, s.y, s.w, s.h, outer);
                        continue;
                    }
                    var g = CssGradient.Create(c, split[li], s.x, s.y, s.w, s.h);
                    if (g == null) continue;
                    c.FillStyle = g;
                    c.BeginPath();
                    RRect(c, s.x, s.y, s.w, s.h, outer);
                    c.Fill();
                }
            }

            DrawHardInsets(c, s, outer);
            DrawBlurredInsets(c, s, outer);
            if (border) DrawBorder(c, s, outer);
            c.Restore();
            PaintProfile.End(PaintProfile.CssBox);
        }

        /// <summary>Zero-blur inset shadows: the part of the padding box not covered by that box shifted by the offset.</summary>
        static void DrawHardInsets(Canvas2D c, CssBoxStyle s, Vec2d[] outer)
        {
            double bt = s.borderOn[0] ? s.borderW[0] : 0, br = s.borderOn[1] ? s.borderW[1] : 0;
            double bb = s.borderOn[2] ? s.borderW[2] : 0, bl = s.borderOn[3] ? s.borderW[3] : 0;
            foreach (var sh in s.shadows)
            {
                if (!sh.inset || sh.color.a <= 0 || sh.blur > 0) continue;
                double ix = s.x + bl, iy = s.y + bt, iw = s.w - bl - br, ih = s.h - bt - bb;
                if (iw <= 0 || ih <= 0) continue;
                var inner = new Vec2d[4];
                for (int i = 0; i < 4; i++) inner[i] = new Vec2d(Math.Max(0, outer[i].x - bl), Math.Max(0, outer[i].y - bt));
                c.Save();
                c.BeginPath();
                RRect(c, ix, iy, iw, ih, inner);
                c.Clip();
                c.SetFill(sh.color);
                c.BeginPath();
                RRect(c, ix, iy, iw, ih, inner);
                RRect(c, ix + sh.dx, iy + sh.dy, iw, ih, inner);
                c.Fill("evenodd");
                c.Restore();
            }
        }

        /// <summary>
        /// Blurred inset shadows. The shadow is the blur of everything outside the padding box (shifted by the
        /// offset), clipped to the inside, so the centre stays clear and the edge darkens.
        /// </summary>
        static void DrawBlurredInsets(Canvas2D c, CssBoxStyle s, Vec2d[] outer)
        {
            double bt = s.borderOn[0] ? s.borderW[0] : 0, br = s.borderOn[1] ? s.borderW[1] : 0;
            double bb = s.borderOn[2] ? s.borderW[2] : 0, bl = s.borderOn[3] ? s.borderW[3] : 0;
            for (int i = s.shadows.Count - 1; i >= 0; i--)
            {
                var sh = s.shadows[i];
                if (!sh.inset || sh.color.a <= 0 || sh.blur <= 0) continue;
                double ix = s.x + bl, iy = s.y + bt, iw = s.w - bl - br, ih = s.h - bt - bb;
                if (iw <= 0 || ih <= 0) continue;
                var inner = new Vec2d[4];
                for (int k = 0; k < 4; k++) inner[k] = new Vec2d(Math.Max(0, outer[k].x - bl), Math.Max(0, outer[k].y - bt));
                double spread = sh.spread;
                double hx = ix + sh.dx + spread, hy = iy + sh.dy + spread, hw = iw - spread * 2, hh = ih - spread * 2;
                var hole = inner;
                if (hw > 0 && hh > 0)
                {
                    hole = new Vec2d[4];
                    for (int k = 0; k < 4; k++) hole[k] = new Vec2d(Math.Max(0, inner[k].x - spread), Math.Max(0, inner[k].y - spread));
                }
                else { hx = ix + sh.dx; hy = iy + sh.dy; hw = iw; hh = ih; }
                double pad = sh.blur + Math.Abs(sh.dx) + Math.Abs(sh.dy) + Math.Abs(sh.spread) + 4;
                c.Save();
                c.BeginPath();
                RRect(c, ix, iy, iw, ih, inner);
                c.Clip();
                c.FillStyle = "rgba(0,0,0,1)";
                c.SetShadow(sh.color);
                ShadowDevice(c, 0, 0, OffCanvas, 0, out var devX, out var devY, out var blurScale);
                c.ShadowBlur = sh.blur * blurScale;
                c.ShadowOffsetX = devX;
                c.ShadowOffsetY = devY;
                c.BeginPath();
                c.Rect(ix - pad - OffCanvas, iy - pad, iw + pad * 2, ih + pad * 2);
                RRect(c, hx - OffCanvas, hy, hw, hh, hole);
                c.Fill("evenodd");
                c.Restore();
            }
        }

        static void DrawBorder(Canvas2D c, CssBoxStyle s, Vec2d[] outer)
        {
            double bt = s.borderOn[0] || s.borderDashed[0] ? s.borderW[0] : 0;
            double br = s.borderOn[1] || s.borderDashed[1] ? s.borderW[1] : 0;
            double bb = s.borderOn[2] || s.borderDashed[2] ? s.borderW[2] : 0;
            double bl = s.borderOn[3] || s.borderDashed[3] ? s.borderW[3] : 0;
            if (bt <= 0 && br <= 0 && bb <= 0 && bl <= 0) return;
            if (s.borderDashed[0] && s.borderDashed[1] && s.borderDashed[2] && s.borderDashed[3]
                && bt == br && br == bb && bb == bl && s.borderC[0].a > 0)
            {
                // Chromium's dashed stroke is a 3:1 dash/gap in border-width units, centered on the border midline.
                double bw = bt;
                c.Save();
                c.StrokeStyle = ToCss(s.borderC[0]);
                c.LineWidth = bw;
                c.LineCap = "butt";
                c.SetLineDash(new[] { bw * 3, bw });
                c.BeginPath();
                c.Ellipse(s.x + s.w * 0.5, s.y + s.h * 0.5, Math.Max(0.5, s.w * 0.5 - bw * 0.5), Math.Max(0.5, s.h * 0.5 - bw * 0.5), 0, 0, Math.PI * 2);
                c.Stroke();
                c.SetLineDash(null);
                c.Restore();
                return;
            }
            bool uniform = bt == br && br == bb && bb == bl &&
                s.borderC[0].r == s.borderC[1].r && s.borderC[0].a == s.borderC[1].a && s.borderC[0].r == s.borderC[2].r && s.borderC[0].r == s.borderC[3].r;
            if (uniform)
            {
                if (s.borderC[0].a <= 0) return;
                var inner = new Vec2d[4];
                for (int i = 0; i < 4; i++) inner[i] = new Vec2d(Math.Max(0, outer[i].x - bl), Math.Max(0, outer[i].y - bt));
                c.SetFill(s.borderC[0]);
                c.BeginPath();
                RRect(c, s.x, s.y, s.w, s.h, outer);
                RRect(c, s.x + bl, s.y + bt, s.w - bl - br, s.h - bt - bb, inner);
                c.Fill("evenodd");
                return;
            }
            // Mixed sides: straight strips (no corner mitres); the pages here only use these on square boxes.
            void Strip(int side, double x, double y, double w, double h)
            {
                if (w <= 0 || h <= 0 || s.borderC[side].a <= 0) return;
                c.SetFill(s.borderC[side]);
                c.FillRect(x, y, w, h);
            }
            Strip(0, s.x, s.y, s.w, bt);
            Strip(2, s.x, s.y + s.h - bb, s.w, bb);
            Strip(3, s.x, s.y + bt, bl, s.h - bt - bb);
            Strip(1, s.x + s.w - br, s.y + bt, br, s.h - bt - bb);
        }

        public static string ToCss(Rgba c)
        {
            PaintProfile.Begin(PaintProfile.ToCss);
            string s = "rgba(" + Math.Round(c.r * 255).ToString(CultureInfo.InvariantCulture) + "," + Math.Round(c.g * 255).ToString(CultureInfo.InvariantCulture) + "," +
                Math.Round(c.b * 255).ToString(CultureInfo.InvariantCulture) + "," + c.a.ToString("0.#####", CultureInfo.InvariantCulture) + ")";
            PaintProfile.End(PaintProfile.ToCss);
            return s;
        }

        // ---- text -------------------------------------------------------------------------------------

        /// <summary>
        /// Draws <paramref name="text"/> with its left edge at <paramref name="x"/> and its alphabetic baseline at <paramref name="baseline"/>,
        /// with the CSS text-shadow layers behind it (last layer first). The canvas font, fill colour and letter spacing must be set by the caller.
        /// </summary>
        public static void DrawText(Canvas2D c, string text, double x, double baseline, Rgba color, List<CssShadow> shadows, double alpha)
        {
            if (string.IsNullOrEmpty(text) || alpha <= 0) return;
            c.Save();
            c.GlobalAlpha = Math.Min(1, alpha);
            c.TextAlign = "start";
            c.TextBaseline = "alphabetic";
            if (shadows != null)
                for (int i = shadows.Count - 1; i >= 0; i--)
                {
                    var sh = shadows[i];
                    if (sh.color.a <= 0) continue;
                    c.Save();
                    if (sh.blur <= 0)
                    {
                        c.SetFill(sh.color);
                        c.FillText(text, x + sh.dx, baseline + sh.dy);
                    }
                    else
                    {
                        c.FillStyle = "rgba(0,0,0,1)";
                        c.SetShadow(sh.color);
                        ShadowDevice(c, sh.dx, sh.dy, OffCanvas, 0, out var devX, out var devY, out var blurScale);
                        c.ShadowBlur = sh.blur * blurScale;
                        c.ShadowOffsetX = devX;
                        c.ShadowOffsetY = devY;
                        c.FillText(text, x - OffCanvas, baseline);
                    }
                    c.Restore();
                }
            c.SetFill(color);
            c.FillText(text, x, baseline);
            c.Restore();
        }
    }

    /// <summary>CSS <c>linear-gradient</c> / <c>radial-gradient</c> (circle) to <see cref="CanvasGradient"/>.</summary>
    public static class CssGradient
    {
        struct Stop { public Rgba color; public double pos; public bool hasPos; }

        public static CanvasGradient Create(Canvas2D c, string css, double x, double y, double w, double h)
        {
            css = css.Trim();
            int open = css.IndexOf('(');
            if (open < 0 || !css.EndsWith(")", StringComparison.Ordinal)) return null;
            string fn = css.Substring(0, open).Trim();
            var args = CssBox.SplitTop(css.Substring(open + 1, css.Length - open - 2));
            if (args.Count < 2) return null;

            bool radial = fn == "radial-gradient";
            if (!radial && fn != "linear-gradient") return null;

            string first = args[0];
            bool firstIsStop = first.StartsWith("rgb", StringComparison.OrdinalIgnoreCase) || first.StartsWith("#");
            int stopStart = firstIsStop ? 0 : 1;
            var stops = new List<Stop>();
            for (int i = stopStart; i < args.Count; i++)
            {
                string a = args[i];
                var st = new Stop();
                int sp = LastTopSpace(a);
                string colorText = a, posText = null;
                if (sp > 0 && (a.EndsWith("%", StringComparison.Ordinal) || a.EndsWith("px", StringComparison.Ordinal)))
                {
                    colorText = a.Substring(0, sp); posText = a.Substring(sp + 1);
                }
                st.color = CssColor.Parse(colorText, Rgba.Transparent);
                if (posText != null) { st.hasPos = true; st.pos = posText.EndsWith("%", StringComparison.Ordinal) ? CssBox.ParseLength(posText, 100) / 100.0 : double.NaN; }
                stops.Add(st);
            }
            if (stops.Count < 2) return null;

            CanvasGradient g;
            double lineLen;
            if (!radial)
            {
                double deg = 180;
                if (!firstIsStop)
                {
                    string t = first.Trim();
                    if (t.EndsWith("deg", StringComparison.Ordinal)) deg = CssBox.ParseLength(t.Substring(0, t.Length - 3));
                    else if (t == "to top") deg = 0;
                    else if (t == "to right") deg = 90;
                    else if (t == "to bottom") deg = 180;
                    else if (t == "to left") deg = 270;
                }
                double rad = deg * Math.PI / 180.0;
                double dx = Math.Sin(rad), dy = -Math.Cos(rad);
                lineLen = Math.Abs(w * dx) + Math.Abs(h * dy);
                double cx = x + w / 2, cy = y + h / 2;
                g = c.CreateLinearGradient(cx - dx * lineLen / 2, cy - dy * lineLen / 2, cx + dx * lineLen / 2, cy + dy * lineLen / 2);
            }
            else
            {
                double cx = x + w / 2, cy = y + h / 2;
                string t = firstIsStop ? "" : first;
                int at = t.IndexOf(" at ", StringComparison.Ordinal);
                if (t.StartsWith("at ", StringComparison.Ordinal)) at = -1 + 0;
                string atText = null;
                if (at >= 0) atText = t.Substring(at + 4);
                else if (t.StartsWith("at ", StringComparison.Ordinal)) atText = t.Substring(3);
                if (atText != null)
                {
                    var pr = atText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (pr.Length >= 1) cx = x + Position(pr[0], w);
                    if (pr.Length >= 2) cy = y + Position(pr[1], h);
                    else cy = y + h / 2;
                }
                // farthest-corner (the default) for a circle
                double fx = Math.Max(Math.Abs(cx - x), Math.Abs(cx - (x + w))), fy = Math.Max(Math.Abs(cy - y), Math.Abs(cy - (y + h)));
                double r = Math.Sqrt(fx * fx + fy * fy);
                if (t.Contains("closest-side")) r = Math.Min(Math.Min(Math.Abs(cx - x), Math.Abs(cx - x - w)), Math.Min(Math.Abs(cy - y), Math.Abs(cy - y - h)));
                if (t.Contains("farthest-side")) r = Math.Max(fx, fy);
                lineLen = r;
                g = c.CreateRadialGradient(cx, cy, 0, cx, cy, r);
            }

            // Resolve missing stop positions per CSS Images 3.
            if (!stops[0].hasPos) { var s0 = stops[0]; s0.pos = 0; s0.hasPos = true; stops[0] = s0; }
            if (!stops[stops.Count - 1].hasPos) { var sl = stops[stops.Count - 1]; sl.pos = 1; sl.hasPos = true; stops[stops.Count - 1] = sl; }
            double prev = 0;
            for (int i = 0; i < stops.Count; i++)
            {
                if (stops[i].hasPos) { var s = stops[i]; if (double.IsNaN(s.pos)) s.pos = 0; s.pos = Math.Max(s.pos, prev); stops[i] = s; prev = s.pos; }
            }
            for (int i = 1; i < stops.Count - 1; i++)
            {
                if (stops[i].hasPos) continue;
                int j = i;
                while (!stops[j].hasPos) j++;
                double a0 = stops[i - 1].pos, a1 = stops[j].pos;
                int n = j - (i - 1);
                for (int k = i; k < j; k++) { var s = stops[k]; s.pos = a0 + (a1 - a0) * (k - (i - 1)) / n; s.hasPos = true; stops[k] = s; }
            }
            foreach (var s in stops) g.AddColorStop(Math.Min(1, Math.Max(0, s.pos)), s.color);
            return g;
        }

        /// <summary>A radial gradient whose ending shape is an ellipse (the CSS default). <c>circle</c> stays circular.</summary>
        public static bool IsEllipse(string css)
        {
            if (string.IsNullOrEmpty(css)) return false;
            css = css.Trim();
            if (!css.StartsWith("radial-gradient(", StringComparison.Ordinal)) return false;
            int open = css.IndexOf('(');
            if (open < 0) return false;
            string body = css.Substring(open + 1);
            int comma = body.IndexOf(',');
            string head = comma >= 0 ? body.Substring(0, comma) : body;
            if (head.IndexOf("circle", StringComparison.Ordinal) >= 0) return false;
            // A leading color means the shape was omitted, which defaults to ellipse.
            if (head.TrimStart().StartsWith("rgb", StringComparison.OrdinalIgnoreCase) || head.TrimStart().StartsWith("#", StringComparison.Ordinal))
                return true;
            return head.IndexOf("ellipse", StringComparison.Ordinal) >= 0 || head.IndexOf("at ", StringComparison.Ordinal) >= 0 || head.Trim().Length == 0;
        }

        /// <summary>
        /// Fills <paramref name="outer"/> with an elliptical radial gradient. The CTM is scaled to the box so a
        /// circular gradient becomes the ellipse CSS paints (farthest-corner unless closest-side is named).
        /// </summary>
        public static void FillEllipse(Canvas2D c, string css, double x, double y, double w, double h, Vec2d[] outer)
        {
            if (w <= 0 || h <= 0) return;
            int open = css.IndexOf('(');
            if (open < 0) return;
            var args = CssBox.SplitTop(css.Substring(open + 1, css.Length - open - 2));
            if (args.Count < 2) return;
            string first = args[0];
            bool firstIsStop = first.StartsWith("rgb", StringComparison.OrdinalIgnoreCase) || first.StartsWith("#");
            int stopStart = firstIsStop ? 0 : 1;
            var stops = new List<Stop>();
            for (int i = stopStart; i < args.Count; i++)
            {
                string a = args[i];
                var st = new Stop();
                int sp = LastTopSpace(a);
                string colorText = a, posText = null;
                if (sp > 0 && (a.EndsWith("%", StringComparison.Ordinal) || a.EndsWith("px", StringComparison.Ordinal)))
                {
                    colorText = a.Substring(0, sp); posText = a.Substring(sp + 1);
                }
                st.color = CssColor.Parse(colorText, Rgba.Transparent);
                if (posText != null)
                {
                    st.hasPos = true;
                    st.pos = posText.EndsWith("%", StringComparison.Ordinal) ? CssBox.ParseLength(posText, 100) / 100.0 : double.NaN;
                }
                stops.Add(st);
            }
            if (stops.Count < 2) return;
            PlaceStops(stops);

            string head = firstIsStop ? "" : first;
            // Unit box: the border box is [-0.5, 0.5]. Farthest-corner is the corner radius sqrt(0.5).
            double radius = head.IndexOf("closest-side", StringComparison.Ordinal) >= 0 ? 0.5 : Math.Sqrt(0.5);
            double atx = 0, aty = 0;
            int at = head.IndexOf(" at ", StringComparison.Ordinal);
            if (at >= 0)
            {
                var pr = head.Substring(at + 4).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (pr.Length >= 1) atx = Position(pr[0], 1) - 0.5;
                if (pr.Length >= 2) aty = Position(pr[1], 1) - 0.5;
            }

            c.Save();
            c.Translate(x + w * 0.5, y + h * 0.5);
            c.Scale(w, h);
            var g = c.CreateRadialGradient(atx, aty, 0, atx, aty, radius);
            foreach (var s in stops) g.AddColorStop(Math.Min(1, Math.Max(0, s.pos)), s.color);
            c.FillStyle = g;
            c.BeginPath();
            if (outer != null && outer.Length >= 4)
            {
                var scaled = new Vec2d[4];
                for (int i = 0; i < 4; i++) scaled[i] = new Vec2d(outer[i].x / w, outer[i].y / h);
                c.RoundRect(-0.5, -0.5, 1, 1, scaled);
            }
            else c.Rect(-0.5, -0.5, 1, 1);
            c.Fill();
            c.Restore();
        }

        static void PlaceStops(List<Stop> stops)
        {
            if (!stops[0].hasPos) { var s0 = stops[0]; s0.pos = 0; s0.hasPos = true; stops[0] = s0; }
            if (!stops[stops.Count - 1].hasPos) { var sl = stops[stops.Count - 1]; sl.pos = 1; sl.hasPos = true; stops[stops.Count - 1] = sl; }
            double prev = 0;
            for (int i = 0; i < stops.Count; i++)
            {
                if (stops[i].hasPos) { var s = stops[i]; if (double.IsNaN(s.pos)) s.pos = 0; s.pos = Math.Max(s.pos, prev); stops[i] = s; prev = s.pos; }
            }
            for (int i = 1; i < stops.Count - 1; i++)
            {
                if (stops[i].hasPos) continue;
                int j = i;
                while (!stops[j].hasPos) j++;
                double a0 = stops[i - 1].pos, a1 = stops[j].pos;
                int n = j - (i - 1);
                for (int k = i; k < j; k++) { var s = stops[k]; s.pos = a0 + (a1 - a0) * (k - (i - 1)) / n; s.hasPos = true; stops[k] = s; }
            }
        }

        static double Position(string tok, double basis)
        {
            if (tok == "center") return basis / 2;
            if (tok == "left" || tok == "top") return 0;
            if (tok == "right" || tok == "bottom") return basis;
            return CssBox.ParseLength(tok, basis);
        }

        static int LastTopSpace(string s)
        {
            int depth = 0, last = -1;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '(') depth++;
                else if (s[i] == ')') depth--;
                else if (s[i] == ' ' && depth == 0) last = i;
            }
            return last;
        }
    }
}
