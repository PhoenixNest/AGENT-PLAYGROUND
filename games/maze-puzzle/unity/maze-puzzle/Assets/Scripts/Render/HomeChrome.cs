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
    /// Deferred (not drawn, listed in <see cref="Deferred"/>): filters and CSS animations
    /// other than the play-button shine (elements sit at their measured rect; the shine uses its sampled matrix).
    /// Blurred inset shadows are drawn (cycle 15).
    /// </summary>
    public sealed class HomeChrome : IDisposable
    {
        public static bool Applies(string stateId) =>
            stateId == "home_default" || stateId == "maps_world_1_fresh" || stateId == "daily_default"
            || stateId == "shop_rings_default" || stateId == "shop_singularities_default" || stateId == "shop_skyboxes_default"
            || stateId == "achievements_none" || stateId == "settings_default" || stateId == "game_lvl01_t1000ms"
            || stateId == "modal_pause" || stateId == "modal_debrief_exact" || stateId == "modal_defeat_12_of_20"
            || stateId == "loading_t0500" || stateId == "daily_master_ready";

        readonly Canvas2D canvas;
        readonly StreamPlayer playfield;
        readonly List<Dictionary<string, object>> elements;
        /// <summary>When set, the playfield canvas element is not replayed and full-viewport boxes under it are left transparent.</summary>
        public bool SkipPlayfield;
        /// <summary>
        /// Live playfield drawn at the canvas element, before later backdrop-filter overlays.
        /// Null keeps the spec's stored stream (or the hole, when <see cref="SkipPlayfield"/> is set).
        /// </summary>
        public RenderTexture ExternalPlayfield;
        public readonly Dictionary<string, string> TextOverrides = new Dictionary<string, string>();
        public readonly Dictionary<string, double> WidthOverrides = new Dictionary<string, double>();
        /// <summary>Element id to a width as a percent of its parent. Applied only when the key is present.</summary>
        public readonly Dictionary<string, double> WidthPercentOverrides = new Dictionary<string, double>();
        /// <summary>Element id to an extra opacity multiplier.</summary>
        public readonly Dictionary<string, double> OpacityOverrides = new Dictionary<string, double>();
        /// <summary>Element ids skipped, with their descendants.</summary>
        public readonly HashSet<string> HiddenIds = new HashSet<string>();
        /// <summary>Class token plus document ordinal, <c>btn-buy-item#0</c>, replaces that element's first text run.</summary>
        public readonly Dictionary<string, string> ClassTextOverrides = new Dictionary<string, string>();
        Dictionary<int, string> classText;
        readonly double scale;
        readonly int cssW, cssH;
        public readonly string Profile;
        public readonly Dictionary<string, int> Deferred = new Dictionary<string, int>();
        public int DrawnBoxes, DrawnRuns, DrawnEmoji;
        public double CaptureTimeMs { get; private set; }
        public bool Loops { get; private set; }
        Dictionary<int, Dictionary<string, object>> byId;
        CssFrame frame;
        int[] elementLayer;
        int staticLayerCount;
        Texture2D[] layerTex;
        CanvasImage[] layerImg;
        double[] baseOpacity;
        bool cacheReady;
        bool cacheOn;
        int paintOnlyLayer = -1;
        int lastBlit = -2;
        // Blit of cached layers replaced the frame with empty gray. Keep the builder, do not composite.
        sealed class PreparedBox
        {
            public CssBoxStyle style;
            public string[] rad = new string[4];
            public double w, h;
            public bool any;
        }

        readonly Dictionary<Dictionary<string, object>, PreparedBox> prepared = new Dictionary<Dictionary<string, object>, PreparedBox>();

        static readonly bool UseLayerCache = false;
        /// <summary>True after a live draw has cached static runs. Full home paint measured about 800 ms.</summary>
        public bool UsesLayerCache => UseLayerCache && cacheReady && staticLayerCount > 0;
        bool cachedSkip;
        string cachedOverrides = "";

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
            var chrome = new HomeChrome(profile, canvas, field, list, w, h, scale);
            if (root.TryGetValue("t0", out var t0) && t0 is double td) chrome.CaptureTimeMs = td;
            foreach (var e in list)
            {
                if (!e.TryGetValue("am", out var amv) || !(amv is Dictionary<string, object> am)) continue;
                if (N(e, "v", 1) == 0 || S(e, "vis") == "hidden" || (" " + S(e, "c") + " ").Contains(" page-hidden ")) continue;
                if (N(am, "it", 1) <= 0) chrome.Loops = true;
            }
            return chrome;
        }

        public void Dispose()
        {
            if (layerTex != null)
                for (int i = 0; i < layerTex.Length; i++)
                    if (layerTex[i] != null) UnityEngine.Object.Destroy(layerTex[i]);
            playfield?.Dispose();
            canvas.Dispose();
        }

        // ---- spec access ---------------------------------------------------------------------------------

        static string S(Dictionary<string, object> e, string key) => e.TryGetValue(key, out var v) && v is string s ? s : "";
        static double N(Dictionary<string, object> e, string key, double fallback = 0) => e.TryGetValue(key, out var v) && v is double d ? d : fallback;
        bool CoversViewport(Dictionary<string, object> e)
        {
            var r = Rect(e);
            return r[0] <= 1 && r[1] <= 1 && r[2] >= cssW - 1 && r[3] >= cssH - 1;
        }

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

        bool OmitCached(int k)
        {
            if (!cacheOn || elementLayer == null || elementLayer[k] < 0) return false;
            int layer = elementLayer[k];
            if (layer != lastBlit && layerTex != null && layer < layerTex.Length && layerTex[layer] != null)
            {
                canvas.BlitLayer(layerTex[layer]);
                lastBlit = layer;
            }
            return true;
        }

        void DropLayerCache()
        {
            if (layerTex != null)
                for (int i = 0; i < layerTex.Length; i++)
                    if (layerTex[i] != null) UnityEngine.Object.Destroy(layerTex[i]);
            layerTex = null;
            layerImg = null;
            elementLayer = null;
            cacheReady = false;
            staticLayerCount = 0;
        }

        string OverrideKey()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var kv in TextOverrides) sb.Append('\n').Append(kv.Key).Append('=').Append(kv.Value);
            foreach (var kv in WidthOverrides) sb.Append('\n').Append(kv.Key).Append('#').Append(kv.Value.ToString(CultureInfo.InvariantCulture));
            foreach (var kv in WidthPercentOverrides) sb.Append('\n').Append(kv.Key).Append('%').Append(kv.Value.ToString(CultureInfo.InvariantCulture));
            foreach (var kv in OpacityOverrides) sb.Append('\n').Append(kv.Key).Append('@').Append(kv.Value.ToString(CultureInfo.InvariantCulture));
            foreach (var id in HiddenIds) sb.Append('\n').Append('!').Append(id);
            foreach (var kv in ClassTextOverrides) sb.Append('\n').Append(kv.Key).Append('=').Append(kv.Value);
            return sb.ToString();
        }

        void EnsureCache()
        {
            // Quota text/width and the live playfield hole change which elements may be baked.
            string key = OverrideKey();
            if (cacheReady && (cachedSkip != SkipPlayfield || cachedOverrides != key)) DropLayerCache();
            if (cacheReady) return;
            cachedSkip = SkipPlayfield;
            cachedOverrides = key;
            int n = elements.Count;
            elementLayer = new int[n];
            var dynamic = new bool[n];
            var moves = new bool[n];
            var parent = new int[n];
            var byIndex = new Dictionary<int, int>(n);
            for (int k = 0; k < n; k++) byIndex[(int)N(elements[k], "i", k)] = k;
            for (int k = 0; k < n; k++)
            {
                var e = elements[k];
                int pi = (int)N(e, "p", -1);
                parent[k] = pi >= 0 && byIndex.TryGetValue(pi, out var pk) ? pk : -1;
                string classes = S(e, "c");
                bool shine = classes.IndexOf("play-btn-glow", StringComparison.Ordinal) >= 0;
                bool backdrop = S(e, "bf") != "" && S(e, "bf") != "none";
                bool anim = false;
                if (e.TryGetValue("am", out var amv) && amv is Dictionary<string, object> am)
                {
                    string name = S(am, "n");
                    if (name != "" && name != "pageEnter")
                    {
                        anim = true;
                        moves[k] = shine || N(am, "xf", 0) >= 1;
                    }
                }
                if (shine) moves[k] = true;
                dynamic[k] = shine || backdrop || anim;
            }
            for (int k = 0; k < n; k++)
            {
                for (int p = parent[k]; p >= 0; p = parent[p])
                    if (moves[p]) { dynamic[k] = true; break; }
            }
            bool seenDynamic = false;
            for (int k = 0; k < n; k++)
            {
                bool backdrop = S(elements[k], "bf") != "" && S(elements[k], "bf") != "none";
                if (backdrop && !seenDynamic && !moves[k]) dynamic[k] = false;
                if (dynamic[k]) seenDynamic = true;
            }
            int hole = -1;
            if (SkipPlayfield)
                for (int k = 0; k < n; k++)
                    if (S(elements[k], "t") == "canvas") { hole = k; break; }
            for (int k = 0; k < n; k++)
            {
                var e = elements[k];
                // The live playfield is composited underneath every frame. Never bake the canvas
                // or the opaque full-viewport boxes that would cover it.
                if (hole >= 0 && (k == hole || (k < hole && CoversViewport(e)))) dynamic[k] = true;
                string id = S(e, "id");
                if (id.Length > 0 && (TextOverrides.ContainsKey(id) || WidthOverrides.ContainsKey(id))) dynamic[k] = true;
            }
            baseOpacity = new double[n];
            for (int k = 0; k < n; k++)
            {
                var e = elements[k];
                int pk = parent[k];
                double parentO = pk >= 0 ? baseOpacity[pk] : 1;
                bool hidden = N(e, "v", 1) == 0 || S(e, "vis") == "hidden" || (" " + S(e, "c") + " ").Contains(" page-hidden ");
                baseOpacity[k] = hidden || parentO <= 0 ? 0 : parentO * N(e, "o", 1);
            }
            int layer = 0;
            bool open = false;
            for (int k = 0; k < n; k++)
            {
                if (dynamic[k])
                {
                    elementLayer[k] = -1;
                    if (open) { layer++; open = false; }
                }
                else
                {
                    elementLayer[k] = layer;
                    open = true;
                }
            }
            staticLayerCount = open ? layer + 1 : layer;
            if (staticLayerCount <= 0) { cacheReady = true; return; }

            var saved = frame;
            frame = null;
            layerTex = new Texture2D[staticLayerCount];
            layerImg = new CanvasImage[staticLayerCount];
            for (int L = 0; L < staticLayerCount; L++)
            {
                paintOnlyLayer = L;
                canvas.BeginFrame();
                canvas.SetTransform(scale, 0, 0, scale, 0, 0);
                PaintAll();
                canvas.EndFrame();
                var tex = new Texture2D(canvas.Width, canvas.Height, TextureFormat.RGBA32, false, true)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.HideAndDontSave
                };
                var prev = RenderTexture.active;
                RenderTexture.active = canvas.Target;
                tex.ReadPixels(new Rect(0, 0, canvas.Width, canvas.Height), 0, 0, false);
                tex.Apply(false, false);
                RenderTexture.active = prev;
                layerTex[L] = tex;
                layerImg[L] = new CanvasImage(tex);
            }
            paintOnlyLayer = -1;
            frame = saved;
            cacheReady = true;
        }

        // ---- drawing ------------------------------------------------------------------------------------

        /// <summary>Capture pose: the phase baked into the spec. No wall clock.</summary>
        public void Draw() => Draw(double.NaN, -1, null, false, false);

        /// <summary>Animation clock in milliseconds since the animations started (page shown), not since the capture.</summary>
        public void Draw(double timeMs) => Draw(timeMs, -1, null, false, false);

        public void Draw(double timeMs, int pressed, string pressCss, bool modalIntro, bool pinPageEnter)
        {
            long prof = PaintProfile.Stamp();
            frame = double.IsNaN(timeMs) ? null : CssFrame.Build(elements, timeMs, pinPageEnter, modalIntro, pressed, pressCss);
            canvas.ExactBackdrop = frame == null;
            bool savedPremul = Canvas2D.GradientInterpolatesPremultiplied;
            Canvas2D.GradientInterpolatesPremultiplied = true; // CSS gradients interpolate premultiplied
            Deferred.Clear(); DrawnBoxes = DrawnRuns = DrawnEmoji = 0;
            try
            {
                if (frame != null && UseLayerCache) { PaintProfile.Begin(PaintProfile.Cache); EnsureCache(); PaintProfile.End(PaintProfile.Cache); }
                cacheOn = frame != null && cacheReady && paintOnlyLayer < 0;
                lastBlit = -2;
                canvas.BeginFrame();
                canvas.SetTransform(scale, 0, 0, scale, 0, 0);
                if (frame != null && frame.PageTransform && frame.PageRect != null)
                {
                    var pr = frame.PageRect;
                    var m = frame.PageMatrix;
                    double cx = pr[0] + pr[2] * 0.5, cy = pr[1] + pr[3] * 0.5;
                    canvas.Translate(cx, cy);
                    canvas.Transform(m.a, m.b, m.c, m.d, m.e, m.f);
                    canvas.Translate(-cx, -cy);
                }
                if (frame != null && frame.PageOpacity < 0.999) canvas.GlobalAlpha = frame.PageOpacity;
                BuildClassText();
                PaintAll();
                canvas.EndFrame();
            }
            finally
            {
                cacheOn = false;
                Canvas2D.GradientInterpolatesPremultiplied = savedPremul;
                frame = null;
                PaintProfile.AddWall(PaintProfile.Stamp() - prof);
            }
        }

        void BuildClassText()
        {
            classText = null;
            if (ClassTextOverrides.Count == 0) return;
            classText = new Dictionary<int, string>();
            var count = new Dictionary<string, int>();
            for (int k = 0; k < elements.Count; k++)
            {
                string classes = S(elements[k], "c");
                if (classes.Length == 0) continue;
                int i = 0;
                while (i < classes.Length)
                {
                    while (i < classes.Length && classes[i] == ' ') i++;
                    int start = i;
                    while (i < classes.Length && classes[i] != ' ') i++;
                    if (i <= start) continue;
                    string token = classes.Substring(start, i - start);
                    int n;
                    count.TryGetValue(token, out n);
                    string key = token + "#" + n.ToString(CultureInfo.InvariantCulture);
                    count[token] = n + 1;
                    if (ClassTextOverrides.TryGetValue(key, out var text)) classText[k] = text;
                }
            }
        }

        void PaintAll()
        {
            int n = elements.Count;
            var eff = new double[n];
            var skip = new bool[n];
            var byIndex = new Dictionary<int, int>();
            for (int k = 0; k < n; k++) byIndex[(int)N(elements[k], "i", k)] = k;
            if (paintOnlyLayer >= 0 && baseOpacity != null)
                for (int k = 0; k < n; k++) eff[k] = baseOpacity[k];
            int playfieldIndex = -1;
            if (SkipPlayfield)
            {
                for (int k = 0; k < n; k++)
                    if (S(elements[k], "t") == "canvas") { playfieldIndex = k; break; }
            }

            for (int k = 0; k < n; k++)
            {
                var e = elements[k];
                if (paintOnlyLayer >= 0 && (elementLayer == null || elementLayer[k] != paintOnlyLayer)) continue;
                int parent = (int)N(e, "p", -1);
                int pk = parent >= 0 && byIndex.TryGetValue(parent, out var pp) ? pp : -1;
                double parentOpacity = pk >= 0 ? eff[pk] : 1;
                bool parentSkipped = pk >= 0 && skip[pk];

                if (SkipPlayfield && playfieldIndex >= 0 && k < playfieldIndex && CoversViewport(e))
                {
                    string coverClasses = " " + S(e, "c") + " ";
                    eff[k] = parentOpacity * N(e, "o", 1);
                    skip[k] = parentSkipped;
                    if (parentSkipped) continue;
                    if (N(e, "v", 1) == 0 || S(e, "vis") == "hidden" || coverClasses.Contains(" page-hidden ")) { skip[k] = true; continue; }
                    continue;
                }

            if (S(e, "t") == "canvas")
            {
                if (ExternalPlayfield != null)
                {
                    canvas.Save();
                    canvas.SetTransform(scale, 0, 0, scale, 0, 0);
                    canvas.ImageSmoothingEnabled = false;
                    canvas.DrawTexture(ExternalPlayfield, 0, 0, ExternalPlayfield.width, ExternalPlayfield.height, 0, 0, cssW, cssH);
                    canvas.Restore();
                    continue;
                }
                if (SkipPlayfield) continue;
                if (OmitCached(k)) continue;
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
                    if (OmitCached(k)) continue;
                    bool move = Motion(k);
                    if (move) BeginMotion(k);
                    DrawEmoji(e, k, eff[k]);
                    if (move) canvas.Restore();
                    continue;
                }

                string classes = " " + S(e, "c") + " ";
                string eid = S(e, "id");
                double opacity = frame != null && !double.IsNaN(frame.Opacity[k]) ? frame.Opacity[k] : N(e, "o", 1);
                if (eid.Length > 0 && OpacityOverrides.Count > 0 && OpacityOverrides.TryGetValue(eid, out double opacityMul)) opacity *= opacityMul;
                eff[k] = parentOpacity * opacity;
                skip[k] = parentSkipped;
                if (parentSkipped) continue;
                if (N(e, "v", 1) == 0 || S(e, "vis") == "hidden" || classes.Contains(" page-hidden ")) { skip[k] = true; continue; }
                if (eid.Length > 0 && HiddenIds.Count > 0 && HiddenIds.Contains(eid)) { skip[k] = true; continue; }
                if (OmitCached(k)) continue;
                bool moving = Motion(k);
                if (moving) BeginMotion(k);
                if (classes.Contains(" play-btn-glow ")) { PaintShine(e, pk, eff[k], k); if (moving) canvas.Restore(); continue; }
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
                if (S(e, "an") != "" && S(e, "an") != "none" && !e.ContainsKey("am"))
                    Defer("animation (drawn at the measured rect only): " + S(e, "an"));

                PaintBox(e, k, eff[k], true);
                PaintRuns(e, k, eff[k]);
                if (e.TryGetValue("bef", out var bef) && bef is Dictionary<string, object> bd) PaintBox(bd, k, eff[k], false);
                if (e.TryGetValue("aft", out var aft) && aft is Dictionary<string, object> ad) PaintBox(ad, k, eff[k], false);
                if (e.TryGetValue("th", out var th) && th is Dictionary<string, object> td) PaintBox(td, k, eff[k], false);
                if (moving) canvas.Restore();
            }
        }

        bool Motion(int k) => frame != null && !frame.Shine[k] && (frame.Tree[k] || frame.Pressed(k));

        void BeginMotion(int k)
        {
            canvas.Save();
            if (frame.Tree[k]) frame.ApplyMotion(canvas, k);
            frame.ApplyPress(canvas, k);
        }

        static Vec2d Radius(string css, double w, double h)
        {
            if (string.IsNullOrEmpty(css) || css == "0px") return new Vec2d(0, 0);
            var parts = css.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            double rx = CssBox.ParseLength(parts[0], w);
            double ry = parts.Length > 1 ? CssBox.ParseLength(parts[1], h) : CssBox.ParseLength(parts[0], h);
            return new Vec2d(rx, ry);
        }

        double[] ElementRect(Dictionary<string, object> e, int k, bool element)
        {
            double[] r;
            if (element && frame != null && frame.Tree[k] && frame.Local[k] != null)
                r = new[] { frame.Local[k][0], frame.Local[k][1], frame.Local[k][2], frame.Local[k][3] };
            else r = Rect(e);
            if (element && frame != null && Math.Abs(frame.ExtraY[k]) > 1e-6) r[1] += frame.ExtraY[k];
            return r;
        }

        void PaintBox(Dictionary<string, object> e, int k, double alpha, bool element)
        {
            PaintProfile.Begin(PaintProfile.PaintBox);
            PaintProfile.Elements++;
            var r = ElementRect(e, k, element);
            bool flat = element && frame != null && frame.Tree[k];
            // A rotated square's measured box is the AABB, which is larger than the circle drawn inside it.
            if (!flat) UnrotateCircle(e, r);
            string widthId = S(e, "id");
            if (widthId.Length > 0 && WidthOverrides.TryGetValue(widthId, out double widthOverride)) r[2] = widthOverride;
            else if (widthId.Length > 0 && WidthPercentOverrides.Count > 0 && WidthPercentOverrides.TryGetValue(widthId, out double widthPct))
            {
                var parent = ParentOf(e);
                if (parent != null)
                {
                    var pr = Rect(parent);
                    r[2] = pr[2] * widthPct / 100.0;
                }
            }
            double tilt = flat ? 0 : InheritedTilt(e, r);
            if (r[2] <= 0 || r[3] <= 0) { PaintProfile.End(PaintProfile.PaintBox); return; }
            var prep = PrepareBox(e, r[2], r[3]);
            var style = prep.style;
            style.x = r[0]; style.y = r[1]; style.w = r[2]; style.h = r[3];
            bool borderOverride = element && frame != null && frame.Border[k] != null;
            bool shadowOverride = element && frame != null && frame.Shadow[k] != null;
            if (!prep.any && !borderOverride && !shadowOverride) { PaintProfile.End(PaintProfile.PaintBox); return; }
            Rgba[] savedBorder = null;
            CssShadow[] savedShadow = null;
            if (borderOverride)
            {
                savedBorder = (Rgba[])style.borderC.Clone();
                var parsed = CssColor.Parse(frame.Border[k], Rgba.Transparent);
                for (int i = 0; i < 4; i++) style.borderC[i] = parsed;
            }
            if (shadowOverride)
            {
                savedShadow = style.shadows.ToArray();
                style.shadows.Clear();
                style.shadows.AddRange(CssBox.ParseShadows(frame.Shadow[k]));
            }
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
            if (savedBorder != null) Array.Copy(savedBorder, style.borderC, 4);
            if (savedShadow != null)
            {
                style.shadows.Clear();
                style.shadows.AddRange(savedShadow);
            }
            DrawnBoxes++;
            PaintProfile.End(PaintProfile.PaintBox);
        }

        PreparedBox PrepareBox(Dictionary<string, object> e, double w, double h)
        {
            if (!prepared.TryGetValue(e, out var prep))
            {
                prep = new PreparedBox { style = new CssBoxStyle() };
                var style = prep.style;
                if (CssColor.TryParse(S(e, "bg"), out var bg) && bg.a > 0) { style.hasBg = true; style.bg = bg; prep.any = true; }
                string bi = S(e, "bi");
                if (bi != "" && bi != "none") { style.bgImage = bi; prep.any = true; }
                var rad = Strs(e, "rad");
                for (int i = 0; i < 4 && i < rad.Count; i++) prep.rad[i] = rad[i];
                var bw = Strs(e, "bw"); var bc = Strs(e, "bc"); var bs = Strs(e, "bs");
                for (int i = 0; i < 4 && i < bw.Count; i++)
                {
                    style.borderW[i] = CssBox.ParseLength(bw[i]);
                    style.borderOn[i] = style.borderW[i] > 0 && i < bs.Count && bs[i] == "solid";
                    style.borderC[i] = CssColor.Parse(i < bc.Count ? bc[i] : "", Rgba.Transparent);
                    if (style.borderOn[i]) prep.any = true;
                }
                style.shadows.AddRange(CssBox.ParseShadows(S(e, "sh")));
                foreach (var sh in style.shadows)
                    if (sh.color.a > 0) prep.any = true;
                prep.w = double.NaN;
                prepared[e] = prep;
            }
            if (double.IsNaN(prep.w) || Math.Abs(prep.w - w) > 0.02 || Math.Abs(prep.h - h) > 0.02)
            {
                for (int i = 0; i < 4; i++) prep.style.radii[i] = Radius(prep.rad[i], w, h);
                prep.w = w;
                prep.h = h;
            }
            return prep;
        }

        static readonly Regex Spaces = new Regex(@"\s+", RegexOptions.Compiled);

        static string FontFamily(string ff)
        {
            if (string.IsNullOrEmpty(ff)) return "Fredoka";
            int comma = ff.IndexOf(',');
            string first = (comma >= 0 ? ff.Substring(0, comma) : ff).Trim().Trim('"', '\'');
            return first;
        }

        void PaintRuns(Dictionary<string, object> e, int k, double alpha)
        {
            if (!e.TryGetValue("runs", out var rv) || !(rv is List<object> runs) || runs.Count == 0) return;
            PaintProfile.Begin(PaintProfile.PaintRuns);
            string family = FontFamily(S(e, "ff"));
            string font = S(e, "fw") + " " + S(e, "fz") + " " + family;
            double ls = CssBox.ParseLength(S(e, "ls"));
            var color = CssColor.Parse(S(e, "col"), Rgba.Black);
            var shadows = CssBox.ParseShadows(S(e, "ts"));
            bool upper = S(e, "tt") == "uppercase";

            string textId = S(e, "id");
            string textReplacement = null;
            bool replaceText = textId.Length > 0 && TextOverrides.TryGetValue(textId, out textReplacement);
            if (!replaceText && classText != null && classText.TryGetValue(k, out textReplacement)) replaceText = true;
            for (int ri = 0; ri < runs.Count; ri++)
            {
                var run = (Dictionary<string, object>)runs[ri];
                string text = replaceText && ri == 0 ? textReplacement : Spaces.Replace(S(run, "s"), " ");
                if (replaceText && ri > 0) continue;
                if (upper) text = text.ToUpperInvariant();
                double[] r;
                if (frame != null && frame.Tree[k] && frame.Runs[k] != null && ri < frame.Runs[k].Length && frame.Runs[k][ri] != null)
                    r = new[] { frame.Runs[k][ri][0], frame.Runs[k][ri][1], frame.Runs[k][ri][2], frame.Runs[k][ri][3] };
                else r = Rect(run);
                if (frame != null && Math.Abs(frame.ExtraY[k]) > 1e-6) r[1] += frame.ExtraY[k];
                if (text.Trim().Length == 0) continue;
                PaintProfile.Runs++;

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
            PaintProfile.End(PaintProfile.PaintRuns);
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
        void PaintShine(Dictionary<string, object> e, int pk, double alpha, int k)
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

            string tf = frame != null && frame.Own[k] ? CssTransform.Format(frame.Matrix[k]) : S(e, "tf");
            if (TryMatrix(tf, out var a, out var b, out var c, out var d, out var ee, out var f))
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

        /// <summary>
        /// Replay the Chromium canvas dump for this spec into <paramref name="dest"/>.
        /// That dump is the playfield baked into the still; the modal chrome leaves it unpainted when <see cref="SkipPlayfield"/> is set.
        /// </summary>
        public bool PaintPlayfield(Canvas2D dest)
        {
            if (playfield == null || dest == null) return false;
            dest.BeginFrame();
            playfield.Play(dest);
            dest.EndFrame();
            return true;
        }

        void DrawEmoji(Dictionary<string, object> e, int k, double alpha)
        {
            var r = ElementRect(e, k, true);
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
