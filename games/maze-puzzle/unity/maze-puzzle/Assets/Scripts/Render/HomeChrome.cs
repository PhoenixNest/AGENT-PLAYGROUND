using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace OrbitalSalvage.Render
{
    /// <summary>
    /// Draws the home screen from the web's measured layout (<c>Resources/Parity/Spec/home__&lt;profile&gt;.json</c>, distilled from
    /// <c>tools/parity/web/dump-spec.mjs</c> by <c>web/distill-spec.mjs</c>) with <see cref="Canvas2D"/>: every visible element is a
    /// <see cref="CssBox"/> plus its text runs (outline glyph text) and emoji images, at the rect Chromium reports in CSS px.
    /// The spec carries a rect set per safe-area profile, so zero and iphone differ exactly as measured.
    /// Deferred (not drawn, listed in <see cref="Deferred"/>): filters, blurred inset shadows, and CSS animations
    /// other than the play-button shine (elements sit at their measured rect; the shine uses its sampled matrix).
    /// </summary>
    public sealed class HomeChrome : IDisposable
    {
        public static bool Applies(string stateId) =>
            stateId == "home_default" || stateId == "maps_world_1_fresh" || stateId == "daily_default"
            || stateId == "shop_rings_default" || stateId == "shop_singularities_default" || stateId == "shop_skyboxes_default"
            || stateId == "achievements_none" || stateId == "settings_default" || stateId == "game_lvl01_t1000ms"
            || stateId == "modal_pause" || stateId == "modal_debrief_exact" || stateId == "modal_defeat_12_of_20"
            || stateId == "loading_t0500";

        readonly Canvas2D canvas;
        readonly StreamPlayer playfield;
        readonly List<Dictionary<string, object>> elements;
        readonly double scale;
        readonly int cssW, cssH;
        public readonly string Profile;
        public readonly Dictionary<string, int> Deferred = new Dictionary<string, int>();
        public int DrawnBoxes, DrawnRuns, DrawnEmoji;
        Dictionary<int, Dictionary<string, object>> byId;

        public RenderTexture Target => canvas.Target;
        public int PixelWidth => canvas.Width;
        public int PixelHeight => canvas.Height;

        HomeChrome(string profile, Canvas2D canvas, StreamPlayer playfield, List<Dictionary<string, object>> elements, int cssW, int cssH, double scale)
        {
            Profile = profile; this.canvas = canvas; this.playfield = playfield; this.elements = elements; this.cssW = cssW; this.cssH = cssH; this.scale = scale;
        }

        public static HomeChrome Load(string profile) => Load("home", profile);

        public static HomeChrome Load(string spec, string profile)
        {
            var asset = Resources.Load<TextAsset>("Parity/Spec/" + spec + "__" + profile);
            if (asset == null) throw new InvalidOperationException("missing home spec for profile " + profile);
            var root = (Dictionary<string, object>)MiniJson.Parse(asset.text);
            var vp = (Dictionary<string, object>)root["viewport"];
            int w = (int)(double)vp["w"], h = (int)(double)vp["h"];
            double scale = (double)vp["scale"];
            var list = new List<Dictionary<string, object>>();
            foreach (var o in (List<object>)root["elements"]) list.Add((Dictionary<string, object>)o);
            var canvas = new Canvas2D((int)Math.Round(w * scale), (int)Math.Round(h * scale));
            var fieldAsset = Resources.Load<TextAsset>("Parity/Playfield/" + spec);
            StreamPlayer field = fieldAsset != null ? StreamPlayer.Load(fieldAsset.text) : null;
            return new HomeChrome(profile, canvas, field, list, w, h, scale);
        }

        public void Dispose() { playfield?.Dispose(); canvas.Dispose(); }

        // ---- spec access ---------------------------------------------------------------------------------

        static string S(Dictionary<string, object> e, string key) => e.TryGetValue(key, out var v) && v is string s ? s : "";
        static double N(Dictionary<string, object> e, string key, double fallback = 0) => e.TryGetValue(key, out var v) && v is double d ? d : fallback;
        static double[] Rect(Dictionary<string, object> e)
        {
            var l = (List<object>)e["r"];
            return new[] { (double)l[0], (double)l[1], (double)l[2], (double)l[3] };
        }
        static List<string> Strs(Dictionary<string, object> e, string key)
        {
            var r = new List<string>();
            if (e.TryGetValue(key, out var v) && v is List<object> l) foreach (var o in l) r.Add(o as string ?? "");
            return r;
        }

        void Defer(string what) { Deferred.TryGetValue(what, out int n); Deferred[what] = n + 1; }

        // ---- drawing ------------------------------------------------------------------------------------

        public void Draw()
        {
            bool savedPremul = Canvas2D.GradientInterpolatesPremultiplied;
            Canvas2D.GradientInterpolatesPremultiplied = true; // CSS gradients interpolate premultiplied
            Deferred.Clear(); DrawnBoxes = DrawnRuns = DrawnEmoji = 0;
            try
            {
                canvas.BeginFrame();
                canvas.SetTransform(scale, 0, 0, scale, 0, 0);
                PaintAll();
                canvas.EndFrame();
            }
            finally { Canvas2D.GradientInterpolatesPremultiplied = savedPremul; }
        }

        void PaintAll()
        {
            int n = elements.Count;
            var eff = new double[n];
            var skip = new bool[n];
            var byIndex = new Dictionary<int, int>();
            for (int k = 0; k < n; k++) byIndex[(int)N(elements[k], "i", k)] = k;

            for (int k = 0; k < n; k++)
            {
                var e = elements[k];
                int parent = (int)N(e, "p", -1);
                int pk = parent >= 0 && byIndex.TryGetValue(parent, out var pp) ? pp : -1;
                double parentOpacity = pk >= 0 ? eff[pk] : 1;
                bool parentSkipped = pk >= 0 && skip[pk];

            if (S(e, "t") == "canvas")
            {
                if (playfield != null)
                {
                    playfield.Play(canvas);
                    canvas.SetTransform(scale, 0, 0, scale, 0, 0);
                }
                continue;
            }

            if (S(e, "t") == "img")
                {
                    eff[k] = parentOpacity; skip[k] = parentSkipped;
                    if (parentSkipped) continue;
                    DrawEmoji(e, eff[k]);
                    continue;
                }

                string classes = " " + S(e, "c") + " ";
                eff[k] = parentOpacity * N(e, "o", 1);
                skip[k] = parentSkipped;
                if (parentSkipped) continue;
                if (N(e, "v", 1) == 0 || S(e, "vis") == "hidden" || classes.Contains(" page-hidden ")) { skip[k] = true; continue; }
                if (classes.Contains(" play-btn-glow ")) { PaintShine(e, pk, eff[k]); continue; }
                // matrix3d discs are painted in their measured (already foreshortened) box.

                if (S(e, "bf") != "" && S(e, "bf") != "none")
                {
                    var m = System.Text.RegularExpressions.Regex.Match(S(e, "bf"), @"blur\(([^)]+)\)");
                    if (m.Success && eff[k] > 0.001)
                    {
                        var rr = Rect(e);
                        canvas.BackdropBlur(rr[0], rr[1], rr[2], rr[3], CssBox.ParseLength(m.Groups[1].Value));
                    }
                    else Defer("backdrop-filter");
                }
                if (S(e, "flt") != "" && S(e, "flt") != "none") Defer("filter (drop-shadow / blur)");
                if (S(e, "an") != "" && S(e, "an") != "none") Defer("animation (drawn at the measured rect only): " + S(e, "an"));

                PaintBox(e, eff[k]);
                PaintRuns(e, eff[k]);
                if (e.TryGetValue("bef", out var bef) && bef is Dictionary<string, object> bd) PaintBox(bd, eff[k]);
                if (e.TryGetValue("aft", out var aft) && aft is Dictionary<string, object> ad) PaintBox(ad, eff[k]);
                if (e.TryGetValue("th", out var th) && th is Dictionary<string, object> td) PaintBox(td, eff[k]);
            }
        }

        static Vec2d Radius(string css, double w, double h)
        {
            if (string.IsNullOrEmpty(css) || css == "0px") return new Vec2d(0, 0);
            var parts = css.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            double rx = CssBox.ParseLength(parts[0], w);
            double ry = parts.Length > 1 ? CssBox.ParseLength(parts[1], h) : CssBox.ParseLength(parts[0], h);
            return new Vec2d(rx, ry);
        }

        void PaintBox(Dictionary<string, object> e, double alpha)
        {
            var r = Rect(e);
            // A rotated square's measured box is the AABB, which is larger than the circle drawn inside it.
            UnrotateCircle(e, r);
            double tilt = InheritedTilt(e, r);
            if (r[2] <= 0 || r[3] <= 0) return;
            var style = new CssBoxStyle { x = r[0], y = r[1], w = r[2], h = r[3] };
            bool any = false;

            if (CssColor.TryParse(S(e, "bg"), out var bg) && bg.a > 0) { style.hasBg = true; style.bg = bg; any = true; }
            string bi = S(e, "bi");
            if (bi != "" && bi != "none") { style.bgImage = bi; any = true; }

            var rad = Strs(e, "rad");
            for (int i = 0; i < 4 && i < rad.Count; i++) style.radii[i] = Radius(rad[i], r[2], r[3]);

            var bw = Strs(e, "bw"); var bc = Strs(e, "bc"); var bs = Strs(e, "bs");
            for (int i = 0; i < 4 && i < bw.Count; i++)
            {
                style.borderW[i] = CssBox.ParseLength(bw[i]);
                style.borderOn[i] = style.borderW[i] > 0 && i < bs.Count && bs[i] == "solid";
                style.borderC[i] = i < bc.Count ? CssColor.Parse(bc[i], Rgba.Transparent) : Rgba.Transparent;
                if (style.borderOn[i]) any = true;
            }

            style.shadows.AddRange(CssBox.ParseShadows(S(e, "sh")));
            foreach (var sh in style.shadows)
                if (sh.color.a > 0) any = true;
            if (!any) return;
            if (tilt != 0)
            {
                double cx = style.x + style.w * 0.5, cy = style.y + style.h * 0.5;
                canvas.Save();
                canvas.Translate(cx, cy);
                canvas.Rotate(tilt);
                canvas.Translate(-cx, -cy);
            }
            CssBox.Draw(canvas, style, alpha);
            if (tilt != 0) canvas.Restore();
            DrawnBoxes++;
        }

        static readonly Regex Spaces = new Regex(@"\s+", RegexOptions.Compiled);

        static string FontFamily(string ff)
        {
            if (string.IsNullOrEmpty(ff)) return "Fredoka";
            int comma = ff.IndexOf(',');
            string first = (comma >= 0 ? ff.Substring(0, comma) : ff).Trim().Trim('"', '\'');
            return first;
        }

        void PaintRuns(Dictionary<string, object> e, double alpha)
        {
            if (!e.TryGetValue("runs", out var rv) || !(rv is List<object> runs) || runs.Count == 0) return;
            string family = FontFamily(S(e, "ff"));
            string font = S(e, "fw") + " " + S(e, "fz") + " " + family;
            double ls = CssBox.ParseLength(S(e, "ls"));
            var color = CssColor.Parse(S(e, "col"), Rgba.Black);
            var shadows = CssBox.ParseShadows(S(e, "ts"));
            bool upper = S(e, "tt") == "uppercase";

            foreach (var ro in runs)
            {
                var run = (Dictionary<string, object>)ro;
                string text = Spaces.Replace(S(run, "s"), " ");
                if (upper) text = text.ToUpperInvariant();
                var r = Rect(run);
                if (text.Trim().Length == 0) continue;

                canvas.Save();
                canvas.Font = font;
                canvas.LetterSpacing = ls;
                // The range rect only covers the laid-out characters: collapsed edge whitespace is not part of it.
                string best = text; double bestErr = double.MaxValue;
                string[] candidates = { text, text.TrimStart(), text.TrimEnd(), text.Trim() };
                foreach (var cand in candidates)
                {
                    double err = Math.Abs(canvas.MeasureText(cand) - r[2]);
                    if (err < bestErr - 0.01) { bestErr = err; best = cand; }
                }
                var lines = WrapToWidth(canvas, best, r[2]);
                double lineH = lines.Count > 1 ? r[3] / lines.Count : 0;
                for (int i = 0; i < lines.Count; i++)
                {
                    // The run rect's top is the first em box, the same origin a single line uses.
                    double baseline = r[1] + (lines.Count > 1 ? i * lineH : 0) + canvas.FontAscent();
                    CssBox.DrawText(canvas, lines[i], r[0], baseline, color, shadows, alpha);
                    DrawnRuns++;
                }
                canvas.Restore();
            }
        }

        static List<string> WrapToWidth(Canvas2D canvas, string text, double width)
        {
            var lines = new List<string>();
            if (canvas.MeasureText(text) <= width + Math.Max(8, width * 0.08)) { lines.Add(text); return lines; }
            string cur = "";
            foreach (var word in text.Split(' '))
            {
                if (word.Length == 0) continue;
                string trial = cur.Length == 0 ? word : cur + " " + word;
                if (cur.Length > 0 && canvas.MeasureText(trial) > width + 0.5)
                {
                    lines.Add(cur);
                    cur = word;
                }
                else cur = trial;
            }
            if (cur.Length > 0) lines.Add(cur);
            if (lines.Count == 0) lines.Add(text);
            return lines;
        }

        /// <summary>
        /// The play-button shine is a 200% strip at -50%/-50%, rotated about its center by the sampled
        /// <c>matrix()</c>, and clipped to the button. Painting that local box (not the measured AABB) puts the
        /// white band where Chromium sampled <c>shineSweep</c>.
        /// </summary>
        void PaintShine(Dictionary<string, object> e, int pk, double alpha)
        {
            if (pk < 0 || alpha <= 0.001) return;
            var parent = elements[pk];
            var pr = Rect(parent);
            if (pr[2] <= 0 || pr[3] <= 0) return;
            double lx = pr[0] - pr[2] * 0.5, ly = pr[1] - pr[3] * 0.5, lw = pr[2] * 2, lh = pr[3] * 2;
            double ox = lx + lw * 0.5, oy = ly + lh * 0.5;

            canvas.Save();
            canvas.BeginPath();
            var rad = Strs(parent, "rad");
            var radii = new Vec2d[4];
            for (int i = 0; i < 4; i++) radii[i] = i < rad.Count ? Radius(rad[i], pr[2], pr[3]) : new Vec2d(0, 0);
            canvas.RoundRect(pr[0], pr[1], pr[2], pr[3], radii);
            canvas.Clip();

            if (TryMatrix(S(e, "tf"), out var a, out var b, out var c, out var d, out var ee, out var f))
            {
                canvas.Translate(ox, oy);
                canvas.Transform(a, b, c, d, ee, f);
                canvas.Translate(-ox, -oy);
            }

            var style = new CssBoxStyle { x = lx, y = ly, w = lw, h = lh, bgImage = S(e, "bi") };
            CssBox.Draw(canvas, style, alpha);
            canvas.Restore();
            DrawnBoxes++;
        }

        /// <summary>
        /// A child of a rotated element is measured as an axis-aligned box. Circles ignore that rotation.
        /// Anything else is drawn at its pre-rotation size and rotated about the box centre by the accumulated angle.
        /// </summary>
        double InheritedTilt(Dictionary<string, object> e, double[] r)
        {
            var rad = Strs(e, "rad");
            if (rad.Count >= 4)
            {
                bool circle = true;
                for (int i = 0; i < 4; i++) if (rad[i] != "50%") circle = false;
                if (circle) return 0;
            }
            double a = 1, b = 0, c = 0, d = 1;
            var cur = e;
            for (int guard = 0; cur != null && guard < 16; guard++)
            {
                if (TryMatrix(S(cur, "tf"), out var ma, out var mb, out var mc, out var md, out _, out _))
                {
                    double na = ma * a + mc * b, nb = mb * a + md * b, nc = ma * c + mc * d, nd = mb * c + md * d;
                    a = na; b = nb; c = nc; d = nd;
                }
                cur = ParentOf(cur);
            }
            double ang = Math.Atan2(b, a);
            if (Math.Abs(ang) < 0.02) return 0;
            double cos = Math.Abs(Math.Cos(ang)), sin = Math.Abs(Math.Sin(ang));
            double det = cos * cos - sin * sin;
            if (Math.Abs(det) < 1e-4) return 0;
            double visW = (r[2] * cos - r[3] * sin) / det;
            double visH = (r[3] * cos - r[2] * sin) / det;
            if (visW <= 1 || visH <= 1 || visW > r[2] + 1 || visH > r[3] + 1) return 0;
            double cx = r[0] + r[2] * 0.5, cy = r[1] + r[3] * 0.5;
            r[0] = cx - visW * 0.5;
            r[1] = cy - visH * 0.5;
            r[2] = visW;
            r[3] = visH;
            return ang;
        }

        Dictionary<string, object> ParentOf(Dictionary<string, object> e)
        {
            if (byId == null)
            {
                byId = new Dictionary<int, Dictionary<string, object>>();
                foreach (var el in elements) byId[(int)N(el, "i", -1)] = el;
            }
            int p = (int)N(e, "p", -1);
            return p >= 0 && byId.TryGetValue(p, out var par) ? par : null;
        }

        /// <summary>
        /// <c>getBoundingClientRect</c> is the axis-aligned bounds of the border box after rotation. A circle
        /// (50% radius on a square) does not grow when it rotates, so the painted box is the pre-rotation size,
        /// still centered on that AABB. Translation in the matrix is already included in the measured rect.
        /// </summary>
        static void UnrotateCircle(Dictionary<string, object> e, double[] r)
        {
            if (!TryMatrix(S(e, "tf"), out var a, out var b, out _, out _, out _, out _)) return;
            var rad = Strs(e, "rad");
            if (rad.Count < 4) return;
            for (int i = 0; i < 4; i++) if (rad[i] != "50%") return;
            double s = Math.Sqrt(a * a + b * b);
            if (s < 1e-6) return;
            double cos = Math.Abs(a / s), sin = Math.Abs(b / s);
            if (sin < 0.01) return;
            double det = cos * cos - sin * sin;
            if (Math.Abs(det) < 1e-4) return;
            double visW = (r[2] * cos - r[3] * sin) / det;
            double visH = (r[3] * cos - r[2] * sin) / det;
            if (visW <= 1 || visH <= 1 || visW > r[2] + 1 || visH > r[3] + 1) return;
            double cx = r[0] + r[2] * 0.5, cy = r[1] + r[3] * 0.5;
            r[0] = cx - visW * 0.5;
            r[1] = cy - visH * 0.5;
            r[2] = visW;
            r[3] = visH;
        }

        static bool TryMatrix(string tf, out double a, out double b, out double c, out double d, out double e, out double f)
        {
            a = b = c = d = e = f = 0;
            if (string.IsNullOrEmpty(tf) || !tf.StartsWith("matrix(", StringComparison.Ordinal) || !tf.EndsWith(")", StringComparison.Ordinal)) return false;
            var parts = tf.Substring(7, tf.Length - 8).Split(',');
            if (parts.Length != 6) return false;
            return double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out a)
                && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out b)
                && double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out c)
                && double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out d)
                && double.TryParse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture, out e)
                && double.TryParse(parts[5], NumberStyles.Float, CultureInfo.InvariantCulture, out f);
        }

        void DrawEmoji(Dictionary<string, object> e, double alpha)
        {
            var r = Rect(e);
            if (r[2] <= 0 || r[3] <= 0) return;
            var img = CanvasEmoji.Get(S(e, "emoji"));
            if (img == null) return;
            canvas.Save();
            canvas.GlobalAlpha = Math.Min(1, alpha);
            canvas.DrawImage(img, r[0], r[1], r[2], r[3]);
            canvas.Restore();
            DrawnEmoji++;
        }
    }
}
