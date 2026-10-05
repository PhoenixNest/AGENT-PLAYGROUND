using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace OrbitalSalvage.Render
{
    /// <summary>
    /// CSS animation timing and one painted frame of overrides. <c>iterations &lt;= 0</c> means infinite.
    /// <see cref="TryDirected"/> is the linear (pre-easing) progress after delay, iteration, direction and fill.
    /// </summary>
    public static class CssAnimation
    {
        static Dictionary<string, KeyframeSet> library;

        /// <summary>
        /// When set, only these animation names are sampled, each at its own currentTime.
        /// Names absent from the map keep the captured pose. Null samples every animation at the frame clock.
        /// </summary>
        public static Dictionary<string, double> Running;

        public static void SetLibrary(Dictionary<string, KeyframeSet> sets) { library = sets; }

        public static KeyframeSet Find(string name)
        {
            EnsureLibrary();
            if (library != null && name != null && library.TryGetValue(name, out var set)) return set;
            return null;
        }

        public static void EnsureLibrary()
        {
            if (library != null) return;
            var asset = Resources.Load<TextAsset>("Parity/Spec/_keyframes");
            library = asset != null ? KeyframeSet.ParseLibrary(asset.text) : new Dictionary<string, KeyframeSet>();
        }

        public static bool TryDirected(double timeMs, double delayMs, double durationMs, double iterations, string direction, string fill, out double directed)
        {
            directed = 0;
            bool infinite = iterations <= 0;
            if (timeMs < delayMs)
            {
                if (fill != "backwards" && fill != "both") return false;
                directed = Direct(0, 0, direction);
                return true;
            }
            if (!infinite && timeMs >= delayMs + durationMs * iterations)
            {
                if (fill != "forwards" && fill != "both") return false;
                int last = (int)Math.Ceiling(iterations) - 1;
                if (last < 0) last = 0;
                double end = Math.Abs(iterations - Math.Floor(iterations)) < 1e-9 ? 1.0 : iterations - Math.Floor(iterations);
                if (end <= 0) end = 1;
                directed = Direct(end, last, direction);
                return true;
            }
            if (durationMs <= 1e-9)
            {
                directed = Direct(1, 0, direction);
                return true;
            }
            double active = timeMs - delayMs;
            int index = (int)Math.Floor(active / durationMs);
            if (index < 0) index = 0;
            double progress = (active - index * durationMs) / durationMs;
            if (progress < 0) progress = 0;
            if (progress > 1) progress = 1;
            directed = Direct(progress, index, direction);
            return true;
        }

        public static double Direct(double progress, int index, string direction)
        {
            bool reverse = direction == "reverse" || direction == "alternate-reverse";
            if ((direction == "alternate" || direction == "alternate-reverse") && (index & 1) == 1)
                reverse = !reverse;
            return reverse ? 1 - progress : progress;
        }

        public struct Sample
        {
            public bool Opacity;
            public double OpacityValue;
            public bool Shadow;
            public int ShadowCount;
            public bool Border;
            public Rgba BorderColor;
            public bool Transform;
            public Matrix2D Matrix;
        }

        /// <summary>Filled by <see cref="Evaluate"/> when <see cref="Sample.Shadow"/> is set. Copied out before the next call.</summary>
        public static readonly List<CssShadow> ShadowScratch = new List<CssShadow>(4);

        public static Sample Evaluate(double timeMs, double delayMs, double durationMs, double iterations, string direction, string fill, string easing, KeyframeSet set, double width, double height, double dx)
        {
            var sample = new Sample();
            if (set == null) return sample;
            if (!TryDirected(timeMs, delayMs, durationMs, iterations, direction, fill, out var directed)) return sample;
            string ease = string.IsNullOrEmpty(easing) ? "ease" : easing;
            if (set.TryOpacity(directed, ease, out var of, out var ot, out var ou))
            {
                double a = CssBox.ParseLength(of);
                double b = CssBox.ParseLength(ot);
                sample.Opacity = true;
                sample.OpacityValue = a + (b - a) * ou;
            }
            if (set.TryShadow(directed, ease, out var sf, out var st, out var su))
            {
                LerpShadowsInto(sf, st, su, ShadowScratch);
                sample.Shadow = ShadowScratch.Count > 0;
                sample.ShadowCount = ShadowScratch.Count;
            }
            if (set.TryBorder(directed, ease, out var bf, out var bt, out var bu))
            {
                sample.Border = true;
                sample.BorderColor = QuantizeCss(LerpColor(CssColor.Parse(bf, Rgba.Transparent), CssColor.Parse(bt, Rgba.Transparent), bu));
            }
            if (set.TryTransform(directed, ease, out var tf, out var tt, out var tu))
            {
                sample.Transform = true;
                sample.Matrix = CssTransform.Interpolate(tf, tt, tu, width, height, dx);
            }
            return sample;
        }

        static void LerpShadowsInto(string a, string b, double u, List<CssShadow> dst)
        {
            dst.Clear();
            var left = CssBox.ParseShadows(a);
            var right = CssBox.ParseShadows(b);
            int n = Math.Max(left.Count, right.Count);
            for (int i = 0; i < n; i++)
            {
                var sa = i < left.Count ? left[i] : default(CssShadow);
                var sb = i < right.Count ? right[i] : default(CssShadow);
                if (i >= left.Count) sa.color = Rgba.Transparent;
                if (i >= right.Count) sb.color = Rgba.Transparent;
                dst.Add(new CssShadow
                {
                    dx = sa.dx + (sb.dx - sa.dx) * u,
                    dy = sa.dy + (sb.dy - sa.dy) * u,
                    blur = sa.blur + (sb.blur - sa.blur) * u,
                    spread = sa.spread + (sb.spread - sa.spread) * u,
                    color = QuantizeCss(LerpColor(sa.color, sb.color, u)),
                    inset = u < 0.5 ? sa.inset : sb.inset
                });
            }
        }

        /// <summary>Same 8-bit channels and 4-decimal alpha as <see cref="Format"/> then <see cref="CssColor.Parse"/>.</summary>
        public static Rgba QuantizeCss(Rgba c)
        {
            double r = Math.Max(0, Math.Min(255, Math.Round(c.r * 255.0)));
            double g = Math.Max(0, Math.Min(255, Math.Round(c.g * 255.0)));
            double b = Math.Max(0, Math.Min(255, Math.Round(c.b * 255.0)));
            double a = Math.Round(c.a, 4, MidpointRounding.AwayFromZero);
            if (a < 0) a = 0;
            if (a > 1) a = 1;
            return new Rgba((float)(r / 255.0), (float)(g / 255.0), (float)(b / 255.0), (float)a);
        }

        public static Rgba LerpColor(Rgba a, Rgba b, double u)
        {
            float t = (float)u;
            float ar = a.r * a.a, ag = a.g * a.a, ab = a.b * a.a;
            float br = b.r * b.a, bg = b.g * b.a, bb = b.b * b.a;
            float aa = a.a + (b.a - a.a) * t;
            if (aa <= 1e-6f) return Rgba.Transparent;
            return new Rgba((ar + (br - ar) * t) / aa, (ag + (bg - ag) * t) / aa, (ab + (bb - ab) * t) / aa, aa);
        }

        public static string Format(Rgba c)
        {
            return string.Format(CultureInfo.InvariantCulture, "rgba({0},{1},{2},{3})",
                Math.Round(c.r * 255), Math.Round(c.g * 255), Math.Round(c.b * 255), c.a.ToString("0.####", CultureInfo.InvariantCulture));
        }

        public static bool TryPress(string classes, out string transform)
        {
            transform = null;
            if (string.IsNullOrEmpty(classes)) return false;
            if (Has(classes, "btn-hero-play-3d")) { transform = "translateY(5px)"; return true; }
            if (Has(classes, "btn-hud-circle") || Has(classes, "btn-sound-mini")
                || Has(classes, "btn-cartoon-primary") || Has(classes, "btn-cartoon-secondary"))
            { transform = "translateY(3px)"; return true; }
            if (Has(classes, "btn-header-back") || Has(classes, "btn-close")) { transform = "translateY(2px)"; return true; }
            if (Has(classes, "dock-tab-btn") || Has(classes, "tab-nav-btn")) { transform = "scale(0.92)"; return true; }
            if (Has(classes, "level-tile")) { transform = "scale(0.96)"; return true; }
            return false;
        }

        static bool Has(string classes, string token)
        {
            int i = 0;
            while (i < classes.Length)
            {
                while (i < classes.Length && classes[i] == ' ') i++;
                int start = i;
                while (i < classes.Length && classes[i] != ' ') i++;
                if (i - start == token.Length && string.CompareOrdinal(classes, start, token, 0, token.Length) == 0)
                    return true;
            }
            return false;
        }
    }

    /// <summary>Per-element overrides for one clock time. Index-parallel with the spec element list.</summary>
    public sealed class CssFrame
    {
        public const int ShadowStride = 4;
        public readonly double[] Opacity;
        public readonly string[] Shadow;
        public readonly string[] Border;
        public readonly int[] ShadowN;
        public readonly CssShadow[] ShadowPool;
        public readonly bool[] BorderLive;
        public readonly Rgba[] BorderRgba;
        public readonly bool[] Own;
        public readonly bool[] Live;
        public readonly bool[] Tree;
        public readonly bool[] Shine;
        public readonly Matrix2D[] Matrix;
        public readonly double[][] Local;
        public readonly double[][][] Runs;
        public readonly int[] Parent;
        public readonly double[] ExtraY;
        public readonly double[] OriginX;
        public readonly double[] OriginY;
        public int PressRoot = -1;
        public Matrix2D PressMatrix = Matrix2D.Identity;
        public bool HasMotion;
        public bool PageTransform;
        public double PageOpacity = 1;
        public Matrix2D PageMatrix = Matrix2D.Identity;
        public double[] PageRect;
        public Dictionary<int, int> IndexMap;
        public readonly List<int> Chain = new List<int>(8);

        public void ResetMotion()
        {
            HasMotion = false;
            PageTransform = false;
            PageOpacity = 1;
            PageMatrix = Matrix2D.Identity;
            PageRect = null;
            PressRoot = -1;
            PressMatrix = Matrix2D.Identity;
            for (int i = 0; i < Opacity.Length; i++)
            {
                Opacity[i] = double.NaN;
                Shadow[i] = null;
                Border[i] = null;
                ShadowN[i] = 0;
                BorderLive[i] = false;
                Own[i] = false;
                Live[i] = false;
                Tree[i] = false;
                Shine[i] = false;
                ExtraY[i] = 0;
            }
        }

        CssFrame(int n)
        {
            Opacity = new double[n];
            Shadow = new string[n];
            Border = new string[n];
            ShadowN = new int[n];
            ShadowPool = new CssShadow[n * ShadowStride];
            BorderLive = new bool[n];
            BorderRgba = new Rgba[n];
            Own = new bool[n];
            Live = new bool[n];
            Tree = new bool[n];
            Shine = new bool[n];
            Matrix = new Matrix2D[n];
            Local = new double[n][];
            Runs = new double[n][][];
            Parent = new int[n];
            ExtraY = new double[n];
            OriginX = new double[n];
            OriginY = new double[n];
            for (int i = 0; i < n; i++) { Opacity[i] = double.NaN; OriginX[i] = double.NaN; OriginY[i] = double.NaN; }
        }

        public static CssFrame Build(IList<Dictionary<string, object>> elements, double timeMs, bool pinPageEnter, bool modalIntro, int pressIndex, string pressCss, CssFrame reuse = null)
        {
            int n = elements.Count;
            CssFrame frame;
            if (reuse != null && reuse.Opacity != null && reuse.Opacity.Length == n)
            {
                frame = reuse;
                frame.ResetMotion();
            }
            else frame = new CssFrame(n);
            if (frame.IndexMap == null)
            {
                frame.IndexMap = new Dictionary<int, int>(n);
                for (int k = 0; k < n; k++) frame.IndexMap[SpecInt(elements[k], "i", k)] = k;
            }
            var byIndex = frame.IndexMap;

            for (int k = 0; k < n; k++)
            {
                var e = elements[k];
                int parentI = SpecInt(e, "p", -1);
                frame.Parent[k] = parentI >= 0 && byIndex.TryGetValue(parentI, out var pk) ? pk : -1;
                string classes = SpecStr(e, "c");
                frame.Shine[k] = classes.IndexOf("play-btn-glow", StringComparison.Ordinal) >= 0;
                if (!e.TryGetValue("am", out var amv) || !(amv is Dictionary<string, object> am)) continue;
                string name = SpecStr(am, "n");
                var set = CssAnimation.Find(name);
                double when = timeMs;
                if (CssAnimation.Running != null)
                {
                    if (name.Length == 0 || !CssAnimation.Running.TryGetValue(name, out when)) continue;
                }
                if (pinPageEnter && name == "pageEnter")
                    when = SpecNum(am, "dl") + Math.Max(1, SpecNum(am, "du"));
                double w = 0, h = 0;
                RectOf(e, "br", ref w, ref h);
                if (w <= 0) RectOf(e, "r", ref w, ref h);
                double dx = SpecNum(e, "dx");
                var sample = CssAnimation.Evaluate(when, SpecNum(am, "dl"), SpecNum(am, "du"), SpecNum(am, "it"),
                    SpecStr(am, "di"), SpecStr(am, "fi"), SpecStr(am, "ea"), set, w, h, dx);
                if (name == "pageEnter")
                {
                    // One wrapper, not a live subtree: the settled pose is already the captured layout.
                    bool hiddenEnter = SpecInt(e, "v", 1) == 0 || SpecStr(e, "vis") == "hidden" || classes.IndexOf("page-hidden", StringComparison.Ordinal) >= 0;
                    if (!hiddenEnter)
                    {
                        if (sample.Opacity) frame.PageOpacity = sample.OpacityValue;
                        if (sample.Transform)
                        {
                            frame.PageMatrix = sample.Matrix;
                            frame.PageRect = CopyRect(e, "r");
                            frame.PageTransform = !NearlyIdentity(sample.Matrix);
                        }
                        if (frame.PageTransform || frame.PageOpacity < 0.999) frame.HasMotion = true;
                    }
                    continue;
                }
                if (sample.Opacity) frame.Opacity[k] = sample.OpacityValue;
                if (sample.Shadow)
                {
                    int nSh = sample.ShadowCount;
                    if (nSh > ShadowStride) nSh = ShadowStride;
                    frame.ShadowN[k] = nSh;
                    int dst = k * ShadowStride;
                    for (int s = 0; s < nSh; s++) frame.ShadowPool[dst + s] = CssAnimation.ShadowScratch[s];
                }
                if (sample.Border)
                {
                    frame.BorderLive[k] = true;
                    frame.BorderRgba[k] = sample.BorderColor;
                }
                bool hidden = SpecInt(e, "v", 1) == 0 || SpecStr(e, "vis") == "hidden" || classes.IndexOf("page-hidden", StringComparison.Ordinal) >= 0;
                if (sample.Transform)
                {
                    frame.Own[k] = true;
                    frame.Matrix[k] = sample.Matrix;
                    frame.Live[k] = !frame.Shine[k];
                    if (!hidden && frame.Live[k]) frame.HasMotion = true;
                }
                if (!hidden && SpecNum(am, "it") <= 0 && set != null) frame.HasMotion = true;
            }

            for (int k = 0; k < n; k++)
            {
                bool under = false;
                for (int p = k, hops = 0; p >= 0 && hops < n; p = frame.Parent[p], hops++)
                {
                    if (frame.Live[p]) { under = true; break; }
                }
                frame.Tree[k] = under;
                if (!under) continue;
                bool hasBase = elements[k].ContainsKey("br");
                // Local is kept across frames. Unbake once, from the captured rect. Doing it again
                // walks the same inverse and the child drifts (the hero face leaves the sphere,
                // and a debris sprite's box grows until it covers the page).
                if (frame.Local[k] == null)
                {
                    frame.Local[k] = CopyRect(elements[k], hasBase ? "br" : "r");
                    if (!hasBase && frame.Local[k] != null) UnbakeCaptured(elements, frame, k);
                }
                if (frame.Local[k] != null && double.IsNaN(frame.OriginX[k]))
                    OriginOf(elements[k], frame.Local[k][2], frame.Local[k][3], out frame.OriginX[k], out frame.OriginY[k]);
                if (frame.Runs[k] == null) frame.Runs[k] = CopyRuns(elements[k], true);
                frame.HasMotion = true;
            }

            if (modalIntro)
            {
                double uo = CssEasing.Ease("ease", Clamp(timeMs / 250.0));
                double us = CssEasing.CubicBezier(0.34, 1.56, 0.64, 1, Clamp(timeMs / 300.0));
                int card = -1;
                for (int k = 0; k < n; k++)
                {
                    string c = SpecStr(elements[k], "c");
                    if (c.IndexOf("modal-overlay", StringComparison.Ordinal) >= 0 && c.IndexOf("modal-card", StringComparison.Ordinal) < 0)
                        frame.Opacity[k] = uo;
                    if (card < 0 && c.IndexOf("modal-card", StringComparison.Ordinal) >= 0) card = k;
                }
                if (card >= 0)
                {
                    var r = CopyRect(elements[card], "r");
                    double dy = (1 - us) * (r != null ? r[3] : 0);
                    for (int k = 0; k < n; k++)
                    {
                        bool inside = false;
                        for (int p = k, hops = 0; p >= 0 && hops < n; p = frame.Parent[p], hops++) if (p == card) { inside = true; break; }
                        if (inside) frame.ExtraY[k] = dy;
                    }
                    frame.HasMotion = Math.Abs(dy) > 0.2 || uo < 0.999;
                }
            }

            if (pressIndex >= 0 && pressIndex < n && !string.IsNullOrEmpty(pressCss))
            {
                var r = frame.Local[pressIndex] ?? CopyRect(elements[pressIndex], "r");
                if (frame.Local[pressIndex] == null) frame.Local[pressIndex] = r;
                double w = r != null ? r[2] : 0, h = r != null ? r[3] : 0;
                frame.PressRoot = pressIndex;
                frame.PressMatrix = CssTransform.ToMatrix(pressCss, w, h, 0);
            }
            return frame;
        }

        public bool Pressed(int index)
        {
            if (PressRoot < 0) return false;
            for (int p = index, hops = 0; p >= 0 && hops < Parent.Length; p = Parent[p], hops++) if (p == PressRoot) return true;
            return false;
        }

        public void ApplyMotion(Canvas2D canvas, int index)
        {
            Chain.Clear();
            for (int p = index, hops = 0; p >= 0 && hops < Parent.Length; p = Parent[p], hops++)
                if (Live[p]) Chain.Add(p);
            for (int i = Chain.Count - 1; i >= 0; i--)
            {
                var r = Local[Chain[i]];
                if (r == null) continue;
                double ox = OriginX[Chain[i]], oy = OriginY[Chain[i]];
                if (double.IsNaN(ox)) ox = r[2] * 0.5;
                if (double.IsNaN(oy)) oy = r[3] * 0.5;
                double cx = r[0] + ox, cy = r[1] + oy + ExtraY[Chain[i]];
                var m = Matrix[Chain[i]];
                canvas.Translate(cx, cy);
                canvas.Transform(m.a, m.b, m.c, m.d, m.e, m.f);
                canvas.Translate(-cx, -cy);
            }
        }

        public void ApplyPress(Canvas2D canvas, int index)
        {
            if (!Pressed(index)) return;
            var r = Local[PressRoot];
            if (r == null) return;
            double cx = r[0] + r[2] * 0.5, cy = r[1] + r[3] * 0.5 + ExtraY[PressRoot];
            canvas.Translate(cx, cy);
            canvas.Transform(PressMatrix.a, PressMatrix.b, PressMatrix.c, PressMatrix.d, PressMatrix.e, PressMatrix.f);
            canvas.Translate(-cx, -cy);
        }

        static void UnbakeCaptured(IList<Dictionary<string, object>> elements, CssFrame frame, int index)
        {
            frame.Chain.Clear();
            for (int p = frame.Parent[index], hops = 0; p >= 0 && hops < frame.Parent.Length; p = frame.Parent[p], hops++)
                if (frame.Live[p]) frame.Chain.Add(p);
            var rect = frame.Local[index];
            for (int i = frame.Chain.Count - 1; i >= 0; i--)
            {
                int a = frame.Chain[i];
                var box = frame.Local[a];
                if (box == null) box = CopyRect(elements[a], elements[a].ContainsKey("br") ? "br" : "r");
                if (box == null || box[2] <= 0 || box[3] <= 0) continue;
                var m = CssTransform.ToMatrix(SpecStr(elements[a], "tf"), box[2], box[3], SpecNum(elements[a], "dx"));
                if (!m.IsInvertible || NearlyIdentity(m)) continue;
                OriginOf(elements[a], box[2], box[3], out var ox, out var oy);
                ox += box[0];
                oy += box[1];
                var inv = m.Inverse();
                double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
                double x0 = rect[0], y0 = rect[1], x1 = x0 + rect[2], y1 = y0 + rect[3];
                for (int c = 0; c < 4; c++)
                {
                    double x = (c & 1) == 0 ? x0 : x1;
                    double y = (c & 2) == 0 ? y0 : y1;
                    var pt = inv.Apply(x - ox, y - oy);
                    double px = pt.x + ox, py = pt.y + oy;
                    if (px < minX) minX = px;
                    if (py < minY) minY = py;
                    if (px > maxX) maxX = px;
                    if (py > maxY) maxY = py;
                }
                rect[0] = minX;
                rect[1] = minY;
                rect[2] = maxX - minX;
                rect[3] = maxY - minY;
            }
        }

        static bool NearlyIdentity(Matrix2D m)
        {
            return Math.Abs(m.a - 1) < 1e-3 && Math.Abs(m.b) < 1e-3 && Math.Abs(m.c) < 1e-3
                && Math.Abs(m.d - 1) < 1e-3 && Math.Abs(m.e) < 0.05 && Math.Abs(m.f) < 0.05;
        }

        static double Clamp(double t) => t < 0 ? 0 : t > 1 ? 1 : t;

        static void OriginOf(Dictionary<string, object> e, double w, double h, out double ox, out double oy)
        {
            ox = w * 0.5;
            oy = h * 0.5;
            if (!e.TryGetValue("to", out var v) || !(v is string s) || s.Length == 0) return;
            var parts = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 1) ox = CssBox.ParseLength(parts[0], w);
            if (parts.Length >= 2) oy = CssBox.ParseLength(parts[1], h);
        }

        static void RectOf(Dictionary<string, object> e, string key, ref double w, ref double h)
        {
            if (!(e.TryGetValue(key, out var v) && v is List<object> l) || l.Count < 4) return;
            w = (double)l[2];
            h = (double)l[3];
        }

        static double[] CopyRect(Dictionary<string, object> e, string key)
        {
            if (!(e.TryGetValue(key, out var v) && v is List<object> l) || l.Count < 4) return null;
            return new[] { (double)l[0], (double)l[1], (double)l[2], (double)l[3] };
        }

        static double[][] CopyRuns(Dictionary<string, object> e, bool preferBase)
        {
            if (!e.TryGetValue("runs", out var rv) || !(rv is List<object> runs)) return null;
            var list = new double[runs.Count][];
            for (int i = 0; i < runs.Count; i++)
            {
                var run = (Dictionary<string, object>)runs[i];
                string key = preferBase && run.ContainsKey("br") ? "br" : "r";
                list[i] = CopyRect(run, key);
            }
            return list;
        }

        static string SpecStr(Dictionary<string, object> e, string key)
        {
            return e.TryGetValue(key, out var v) && v is string s ? s : "";
        }

        static double SpecNum(Dictionary<string, object> e, string key)
        {
            if (!e.TryGetValue(key, out var v) || v == null) return 0;
            if (v is double d) return d;
            return double.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture, out var n) ? n : 0;
        }

        static int SpecInt(Dictionary<string, object> e, string key, int fallback)
        {
            return e.TryGetValue(key, out var v) && v is double d ? (int)d : fallback;
        }
    }
}
