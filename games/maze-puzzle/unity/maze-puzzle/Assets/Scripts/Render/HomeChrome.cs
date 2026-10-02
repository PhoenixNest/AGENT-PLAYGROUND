using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace OrbitalSalvage.Render
{
    /// <summary>
    /// Draws the home screen from the web's measured layout (<c>Resources/Parity/Spec/home__&lt;profile&gt;.json</c>, distilled from
    /// <c>tools/parity/web/dump-spec.mjs</c> by <c>web/distill-spec.mjs</c>) with <see cref="Canvas2D"/>: every visible element is a
    /// <see cref="CssBox"/> plus its text runs (outline glyph text) and emoji images, at the rect Chromium reports in CSS px.
    /// The spec carries a rect set per safe-area profile, so zero and iphone differ exactly as measured.
    /// Deferred (not drawn, listed in <see cref="Deferred"/>): filters, inset shadows, the pedestal diorama,
    /// the play-button shine, and CSS animations (elements sit at their measured rect only). Backdrop blur is drawn.
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
                if (classes.Contains(" stage-pedestal-3d ")) { skip[k] = true; Defer("3D pedestal diorama (matrix3d, glow beam, rim pulse)"); continue; }
                if (classes.Contains(" play-btn-glow ")) { skip[k] = true; Defer("play button shine sweep (animated gradient, overflow clip)"); continue; }
                string tf = S(e, "tf");
                if (tf.Contains("matrix3d")) { skip[k] = true; Defer("3D transform"); continue; }

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
            {
                if (sh.inset) Defer("inset box-shadow");
                else any = true;
            }
            if (!any) return;
            CssBox.Draw(canvas, style, alpha);
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
                double baseline = r[1] + canvas.FontAscent();
                CssBox.DrawText(canvas, best, r[0], baseline, color, shadows, alpha);
                canvas.Restore();
                DrawnRuns++;
            }
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
