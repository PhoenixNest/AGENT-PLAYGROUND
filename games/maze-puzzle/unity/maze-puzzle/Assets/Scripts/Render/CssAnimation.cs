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
            public string ShadowValue;
            public bool Border;
            public string BorderValue;
            public bool Transform;
            public Matrix2D Matrix;
        }

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
                sample.Shadow = true;
                sample.ShadowValue = LerpShadows(sf, st, su);
            }
            if (set.TryBorder(directed, ease, out var bf, out var bt, out var bu))
            {
                sample.Border = true;
                sample.BorderValue = Format(LerpColor(CssColor.Parse(bf, Rgba.Transparent), CssColor.Parse(bt, Rgba.Transparent), bu));
            }
            if (set.TryTransform(directed, ease, out var tf, out var tt, out var tu))
            {
                sample.Transform = true;
                sample.Matrix = CssTransform.Interpolate(tf, tt, tu, width, height, dx);
            }
            return sample;
        }

        static string LerpShadows(string a, string b, double u)
        {
            var left = CssBox.ParseShadows(a);
            var right = CssBox.ParseShadows(b);
            int n = Math.Max(left.Count, right.Count);
            if (n == 0) return "none";
            var parts = new List<string>(n);
            for (int i = 0; i < n; i++)
            {
                var sa = i < left.Count ? left[i] : default(CssShadow);
                var sb = i < right.Count ? right[i] : default(CssShadow);
                if (i >= left.Count) sa.color = Rgba.Transparent;
                if (i >= right.Count) sb.color = Rgba.Transparent;
                var sh = new CssShadow
                {
                    dx = sa.dx + (sb.dx - sa.dx) * u,
                    dy = sa.dy + (sb.dy - sa.dy) * u,
                    blur = sa.blur + (sb.blur - sa.blur) * u,
                    spread = sa.spread + (sb.spread - sa.spread) * u,
                    color = LerpColor(sa.color, sb.color, u),
                    inset = u < 0.5 ? sa.inset : sb.inset
                };
                parts.Add(string.Format(CultureInfo.InvariantCulture, "{0} {1}px {2}px {3}px {4}px",
                    Format(sh.color), sh.dx, sh.dy, sh.blur, sh.spread) + (sh.inset ? " inset" : ""));
            }
            return string.Join(", ", parts);
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
        public readonly double[] Opacity;
        public readonly string[] Shadow;
        public readonly string[] Border;
        public readonly bool[] Own;
        public readonly bool[] Live;
        public readonly bool[] Tree;
        public readonly bool[] Shine;
        public readonly Matrix2D[] Matrix;
        public readonly double[][] Local;
        public readonly double[][][] Runs;
        public readonly int[] Parent;
        public readonly double[] ExtraY;
        public int PressRoot = -1;
        public Matrix2D PressMatrix = Matrix2D.Identity;
        public bool HasMotion;
        public bool PageTransform;
        public double PageOpacity = 1;
        public Matrix2D PageMatrix = Matrix2D.Identity;
        public double[] PageRect;

        CssFrame(int n)
        {
            Opacity = new double[n];
            Shadow = new string[n];
            Border = new string[n];
            Own = new bool[n];
            Live = new bool[n];
            Tree = new bool[n];
            Shine = new bool[n];
            Matrix = new Matrix2D[n];
            Local = new double[n][];
            Runs = new double[n][][];
            Parent = new int[n];
            ExtraY = new double[n];
            for (int i = 0; i < n; i++) Opacity[i] = double.NaN;
        }

        public static CssFrame Build(IList<Dictionary<string, object>> elements, double timeMs, bool pinPageEnter, bool modalIntro, int pressIndex, string pressCss)
        {
            int n = elements.Count;
            var frame = new CssFrame(n);
            var byIndex = new Dictionary<int, int>();
            for (int k = 0; k < n; k++) byIndex[SpecInt(elements[k], "i", k)] = k;

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
                if (sample.Shadow) frame.Shadow[k] = sample.ShadowValue;
                if (sample.Border) frame.Border[k] = sample.BorderValue;
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
                for (int p = k; p >= 0; p = frame.Parent[p])
                {
                    if (frame.Live[p]) { under = true; break; }
                }
                frame.Tree[k] = under;
                if (!under) continue;
                frame.Local[k] = CopyRect(elements[k], elements[k].ContainsKey("br") ? "br" : "r");
                frame.Runs[k] = CopyRuns(elements[k], true);
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
                        for (int p = k; p >= 0; p = frame.Parent[p]) if (p == card) { inside = true; break; }
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
            for (int p = index; p >= 0; p = Parent[p]) if (p == PressRoot) return true;
            return false;
        }

        public void ApplyMotion(Canvas2D canvas, int index)
        {
            var chain = new List<int>();
            for (int p = index; p >= 0; p = Parent[p])
                if (Live[p]) chain.Add(p);
            for (int i = chain.Count - 1; i >= 0; i--)
            {
                var r = Local[chain[i]];
                if (r == null) continue;
                double cx = r[0] + r[2] * 0.5, cy = r[1] + r[3] * 0.5 + ExtraY[chain[i]];
                var m = Matrix[chain[i]];
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

        static bool NearlyIdentity(Matrix2D m)
        {
            return Math.Abs(m.a - 1) < 1e-3 && Math.Abs(m.b) < 1e-3 && Math.Abs(m.c) < 1e-3
                && Math.Abs(m.d - 1) < 1e-3 && Math.Abs(m.e) < 0.05 && Math.Abs(m.f) < 0.05;
        }

        static double Clamp(double t) => t < 0 ? 0 : t > 1 ? 1 : t;

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
