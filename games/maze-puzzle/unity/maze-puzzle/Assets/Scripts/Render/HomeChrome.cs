using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using OrbitalSalvage.Logic;
using UnityEngine;

namespace OrbitalSalvage.Render
{
    /// <summary>
    /// Draws a painted page from the web's measured layout (<c>Resources/Parity/Spec/&lt;state&gt;__&lt;profile&gt;.json</c>)
    /// with <see cref="Canvas2D"/>: visible elements are <see cref="CssBox"/>es, outline glyph text, and emoji images at the
    /// CSS rects Chromium reported. The spec's viewport scale is the capture device pixel ratio. The shipping player
    /// (<c>ParityViewer</c>) letterboxes that raster into the window safe area; this type does not do window fit.
    /// Infinite CSS animations set <see cref="Loops"/> and are sampled from the draw clock. Strict still parity is not
    /// finished: several pages sit under the 99.0% / 0.985 bar, mostly on text-edge coverage.
    /// </summary>
    public sealed class HomeChrome : IDisposable
    {
        public static bool Applies(string stateId)
        {
            if (string.IsNullOrEmpty(stateId)) return false;
            switch (stateId)
            {
                case "home_default":
                case "maps_world_1_fresh":
                case "daily_default":
                case "shop_rings_default":
                case "shop_singularities_default":
                case "shop_skyboxes_default":
                case "achievements_none":
                case "settings_default":
                case "game_lvl01_t1000ms":
                case "modal_pause":
                case "modal_debrief_exact":
                case "modal_defeat_12_of_20":
                case "loading_t0500":
                case "daily_master_ready":
                    return true;
            }
            if (stateId.IndexOf('/') >= 0 || stateId.IndexOf('\\') >= 0 || stateId.IndexOf('#') >= 0) return false;
            return Resources.Load<TextAsset>("Parity/Spec/" + stateId + "__zero") != null;
        }

        Canvas2D canvas;
        readonly StreamPlayer playfield;
        readonly List<Dictionary<string, object>> elements;
        /// <summary>When set, the playfield canvas element is not replayed and full-viewport boxes under it are left transparent.</summary>
        public bool SkipPlayfield;
        /// <summary>Page already drawn underneath a modal. App pages in this spec are skipped so the overlay blurs this texture.</summary>
        public RenderTexture Underlay;

        public struct ToastPaint
        {
            public string Text;
            public double AgeMs;
        }

        /// <summary>Toasts painted into #toast-container after the page. Empty leaves the capture pixels unchanged.</summary>
        public readonly List<ToastPaint> Toasts = new List<ToastPaint>();
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
        double scale;
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
        Texture[] layerTex;
        CanvasImage[] layerImg;
        double[] baseOpacity;
        bool cacheReady;
        bool cacheOn;
        int paintOnlyLayer = -1;
        int lastBlit = -2;
        CssFrame frameSlot;
        readonly Dictionary<Dictionary<string, object>, double[]> rects = new Dictionary<Dictionary<string, object>, double[]>();
        readonly double[] rectScratch = new double[4];
        readonly double[] runScratch = new double[4];
        sealed class Laid
        {
            public string font, text;
            public double width;
            public string[] lines;
        }

        readonly Dictionary<int, Laid> laid = new Dictionary<int, Laid>();
        readonly Dictionary<Dictionary<string, object>, string> collapsed = new Dictionary<Dictionary<string, object>, string>();
        readonly Dictionary<Dictionary<string, object>, string> collapsedUpper = new Dictionary<Dictionary<string, object>, string>();
        bool[] specHidden;
        bool[] isShine;
        string[] fontOf;
        double[] letterOf;
        Rgba[] colorOf;
        List<CssShadow>[] shadowOf;
        bool[] upperOf;
        double[] effBuf;
        bool[] skipBuf;
        bool[] pageSkip;
        Dictionary<int, int> paintIndex;
        // Graphics.Blit replaced the target, so cached layers used to wipe the frame. BlitLayer now source-overs.
        public static bool UseLayerCache = true;

        /// <summary>Diagnostic only. -1 paints every element. Never set on the parity path.</summary>
        public static int DebugStopAfter = -1;

        /// <summary>Diagnostic only. Build a live frame but do not apply per-element motion.</summary>
        public static bool DebugFreezeMotion;

        /// <summary>Diagnostic only. Force the exact backdrop readback on a live frame.</summary>
        public static bool DebugExactBackdrop;

        /// <summary>Product path only. Skips backdrop-filter blurs at the lowest raster tier. Parity leaves this false.</summary>
        public static bool SimplifyEffects;

        /// <summary>Product path. While true, <see cref="Draw"/> paints without building the layer cache.</summary>
        public static bool DeferLayerCache;
        static HomeChrome()
        {
            var args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, "-parity-no-layer-cache") >= 0) UseLayerCache = false;
        }

        sealed class PreparedBox
        {
            public CssBoxStyle style;
            public string[] rad = new string[4];
            public double w, h;
            public bool any;
        }

        readonly Dictionary<Dictionary<string, object>, PreparedBox> prepared = new Dictionary<Dictionary<string, object>, PreparedBox>();

        /// <summary>True after a live draw has cached static runs.</summary>
        public bool UsesLayerCache => UseLayerCache && cacheReady && staticLayerCount > 0;

        /// <summary>How many times this chrome has baked its static layers.</summary>
        public int CacheBuilds { get; private set; }
        bool cachedSkip;
        bool cachedExt;
        string[] snapText = Array.Empty<string>();
        string[] snapWidth = Array.Empty<string>();
        string[] snapWidthPct = Array.Empty<string>();
        string[] snapOpacity = Array.Empty<string>();
        string[] snapHidden = Array.Empty<string>();
        string[] snapClass = Array.Empty<string>();
        struct ClassOcc { public int element; public string key; }
        ClassOcc[] classOcc;
        int classOccN;
        string[] classSnapK = Array.Empty<string>();
        string[] classSnapV = Array.Empty<string>();
        bool[] isAppPage;
        bool[] isBeam;
        byte[] backdropKind;
        double[] blurSigma;
        readonly Rgba[] borderSave = new Rgba[4];
        readonly List<CssShadow> shadowSave = new List<CssShadow>(4);
        readonly string[] textCandidates = new string[4];
        readonly string[] oneLine = new string[1];
        readonly Dictionary<string, string> upperText = new Dictionary<string, string>();
        readonly Dictionary<string, string> trimmedText = new Dictionary<string, string>();

        public RenderTexture Target => canvas.Target;
        public int PixelWidth => canvas.Width;
        public int PixelHeight => canvas.Height;
        public double RasterScale => scale;

        /// <summary>
        /// Replace the device raster. CSS layout stays at the spec viewport; only the CTM scale changes.
        /// Drops the layer cache because those textures are device-sized. Parity never calls this.
        /// </summary>
        public void ResizeRaster(double newScale)
        {
            if (newScale < 1) newScale = 1;
            if (newScale > 3) newScale = 3;
            if (Math.Abs(newScale - scale) < 0.05) return;
            int w = Math.Max(1, (int)Math.Round(cssW * newScale));
            int h = Math.Max(1, (int)Math.Round(cssH * newScale));
            var next = new Canvas2D(w, h);
            canvas.Dispose();
            canvas = next;
            scale = newScale;
            DropLayerCache();
        }

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

        static Dictionary<string, List<Dictionary<string, object>>> shopTemplates;

        /// <summary>Replace shop cards with the measured locked / owned / equipped sub-templates, stacked at <paramref name="slotX"/>/<paramref name="slotY"/>.</summary>
        public void StampShopCards(string kinds, double[] slotX, double[] slotY)
        {
            if (IsItemStamp(kinds))
            {
                CardStamp.ApplyTrees(elements, ItemTrees(kinds), slotX, slotY);
                return;
            }
            CardStamp.ApplyShop(elements, kinds, ShopTemplates(), slotX, slotY);
        }

        public static double[] ShopCardHeights(string kinds)
        {
            if (!IsItemStamp(kinds)) return CardStamp.Heights(kinds, ShopTemplates());
            var trees = ItemTrees(kinds);
            if (trees == null) return null;
            var heights = new double[trees.Count];
            for (int i = 0; i < trees.Count; i++)
            {
                if (trees[i] == null || trees[i].Count == 0) return null;
                heights[i] = CardStamp.Rect(trees[i][0])[3];
            }
            return heights;
        }

        public string BakedPageId()
        {
            for (int i = 0; i < elements.Count; i++)
            {
                string id = S(elements[i], "id");
                string classes = " " + S(elements[i], "c") + " ";
                if (id.StartsWith("page-", StringComparison.Ordinal) && classes.Contains(" page-active ")) return id;
            }
            return "";
        }

        static bool IsItemStamp(string kinds) => !string.IsNullOrEmpty(kinds) && kinds.IndexOf(':') >= 0;

        static List<List<Dictionary<string, object>>> ItemTrees(string kinds)
        {
            var parts = kinds.Split(',');
            var trees = new List<List<Dictionary<string, object>>>(parts.Length);
            var pool = new Dictionary<string, List<List<Dictionary<string, object>>>>();
            for (int i = 0; i < parts.Length; i++)
            {
                int colon = parts[i].IndexOf(':');
                if (colon <= 0) return null;
                string id = parts[i].Substring(0, colon);
                string kind = parts[i].Substring(colon + 1);
                var item = FindCosmetic(id);
                if (item == null) return null;
                string bucket = SpecBucket(id);
                if (!pool.TryGetValue(bucket, out var cards))
                {
                    cards = new List<List<Dictionary<string, object>>>();
                    var specs = SpecsFor(id);
                    for (int s = 0; s < specs.Length; s++)
                    {
                        var els = LoadElements(specs[s]);
                        if (els == null) continue;
                        cards.AddRange(CardStamp.ExtractAll(els));
                    }
                    pool[bucket] = cards;
                }
                var tree = CardStamp.MatchItem(cards, item.Name, kind);
                if (tree == null) return null;
                trees.Add(tree);
            }
            return trees;
        }

        static CosmeticItem FindCosmetic(string id)
        {
            CosmeticItem[][] tables = { EconomyCatalog.Rings, EconomyCatalog.Singularities, EconomyCatalog.Skyboxes };
            for (int t = 0; t < tables.Length; t++)
            {
                var table = tables[t];
                for (int i = 0; i < table.Length; i++)
                    if (table[i].Id == id) return table[i];
            }
            return null;
        }

        static string SpecBucket(string id)
        {
            if (id.StartsWith("singularity_", StringComparison.Ordinal)) return "singularities";
            if (id.StartsWith("skybox_", StringComparison.Ordinal)) return "skyboxes";
            return "rings";
        }

        static string[] SpecsFor(string id)
        {
            if (id.StartsWith("singularity_", StringComparison.Ordinal))
                return new[] { "shop_singularities_default", "shop_singularities_mixed" };
            if (id.StartsWith("skybox_", StringComparison.Ordinal))
                return new[] { "shop_skyboxes_default", "shop_skyboxes_mixed" };
            return new[] { "shop_rings_default", "shop_rings_mixed", "shop_gift_claimed" };
        }

        static List<Dictionary<string, object>> LoadElements(string spec)
        {
            var asset = Resources.Load<TextAsset>("Parity/Spec/" + spec + "__zero");
            if (asset == null) return null;
            var root = MiniJson.Parse(asset.text) as Dictionary<string, object>;
            if (root == null || !(root["elements"] is List<object> raw)) return null;
            var list = new List<Dictionary<string, object>>(raw.Count);
            for (int i = 0; i < raw.Count; i++)
                if (raw[i] is Dictionary<string, object> el) list.Add(el);
            return list;
        }

        static Dictionary<string, List<Dictionary<string, object>>> ShopTemplates()
        {
            if (shopTemplates != null) return shopTemplates;
            shopTemplates = new Dictionary<string, List<Dictionary<string, object>>>();
            TakeKind(shopTemplates, "shop_rings_default", "equipped");
            TakeKind(shopTemplates, "shop_rings_default", "locked");
            TakeKind(shopTemplates, "shop_rings_mixed", "owned");
            return shopTemplates;
        }

        static void TakeKind(Dictionary<string, List<Dictionary<string, object>>> into, string spec, string kind)
        {
            if (into.ContainsKey(kind)) return;
            var asset = Resources.Load<TextAsset>("Parity/Spec/" + spec + "__zero");
            if (asset == null) return;
            var root = MiniJson.Parse(asset.text) as Dictionary<string, object>;
            if (root == null || !(root["elements"] is List<object> raw)) return;
            var list = new List<Dictionary<string, object>>(raw.Count);
            for (int i = 0; i < raw.Count; i++)
                if (raw[i] is Dictionary<string, object> el) list.Add(el);
            var tree = CardStamp.Extract(list, kind);
            if (tree != null) into[kind] = tree;
        }

        public void Dispose()
        {
            DropLayerCache();
            playfield?.Dispose();
            canvas.Dispose();
        }

        static void DestroyObject(UnityEngine.Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(obj);
            else UnityEngine.Object.DestroyImmediate(obj);
        }

        // ---- spec access ---------------------------------------------------------------------------------

        static string S(Dictionary<string, object> e, string key) => e.TryGetValue(key, out var v) && v is string s ? s : "";
        static double N(Dictionary<string, object> e, string key, double fallback = 0) => e.TryGetValue(key, out var v) && v is double d ? d : fallback;
        bool CoversViewport(Dictionary<string, object> e)
        {
            var r = Rect(e);
            return r[0] <= 1 && r[1] <= 1 && r[2] >= cssW - 1 && r[3] >= cssH - 1;
        }

        double[] Rect(Dictionary<string, object> e)
        {
            if (rects.TryGetValue(e, out var cached)) return cached;
            var l = (List<object>)e["r"];
            var r = new[] { (double)l[0], (double)l[1], (double)l[2], (double)l[3] };
            rects[e] = r;
            return r;
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

        public void InvalidateLayerCache() => DropLayerCache();

        void DropLayerCache()
        {
            if (layerTex != null)
                for (int i = 0; i < layerTex.Length; i++)
                    ReleaseLayer(layerTex[i]);
            layerTex = null;
            layerImg = null;
            elementLayer = null;
            cacheReady = false;
            staticLayerCount = 0;
        }

        static void ReleaseLayer(Texture tex)
        {
            if (tex == null) return;
            if (tex is RenderTexture rt) rt.Release();
            DestroyObject(tex);
        }

        static bool SameKeys<T>(Dictionary<string, T> dict, string[] snap)
        {
            if (snap == null || dict.Count != snap.Length) return false;
            for (int i = 0; i < snap.Length; i++)
                if (!dict.ContainsKey(snap[i])) return false;
            return true;
        }

        static bool SameHidden(HashSet<string> set, string[] snap)
        {
            if (snap == null || set.Count != snap.Length) return false;
            for (int i = 0; i < snap.Length; i++)
                if (!set.Contains(snap[i])) return false;
            return true;
        }

        static string[] CopyKeys<T>(Dictionary<string, T> dict)
        {
            var keys = new string[dict.Count];
            int i = 0;
            foreach (var kv in dict) keys[i++] = kv.Key;
            return keys;
        }

        bool DynamicSetSame()
        {
            return cachedSkip == SkipPlayfield
                && cachedExt == (ExternalPlayfield != null)
                && SameKeys(TextOverrides, snapText)
                && SameKeys(WidthOverrides, snapWidth)
                && SameKeys(WidthPercentOverrides, snapWidthPct)
                && SameKeys(OpacityOverrides, snapOpacity)
                && SameHidden(HiddenIds, snapHidden)
                && SameKeys(ClassTextOverrides, snapClass);
        }

        void RememberDynamicSet()
        {
            cachedSkip = SkipPlayfield;
            cachedExt = ExternalPlayfield != null;
            snapText = CopyKeys(TextOverrides);
            snapWidth = CopyKeys(WidthOverrides);
            snapWidthPct = CopyKeys(WidthPercentOverrides);
            snapOpacity = CopyKeys(OpacityOverrides);
            snapHidden = new string[HiddenIds.Count];
            HiddenIds.CopyTo(snapHidden);
            snapClass = CopyKeys(ClassTextOverrides);
        }

        void EnsureCache()
        {
            // Only the set of overridden ids changes which elements are baked. Values are painted live.
            if (cacheReady && !DynamicSetSame()) DropLayerCache();
            if (cacheReady) return;
            if (DeferLayerCache) return;
            RememberDynamicSet();
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
                string id = S(e, "id");
                if (id.Length > 0 && (WidthPercentOverrides.ContainsKey(id) || OpacityOverrides.ContainsKey(id) || HiddenIds.Contains(id)))
                    dynamic[k] = true;
                if (classText != null && classText.ContainsKey(k)) dynamic[k] = true;
                // A live texture must be sampled by later backdrop-filter overlays every frame.
                if (ExternalPlayfield != null && S(e, "t") == "canvas") dynamic[k] = true;
            }
            for (int k = 0; k < n; k++)
            {
                for (int p = parent[k], hops = 0; p >= 0 && hops < n; p = parent[p], hops++)
                    if (moves[p]) { dynamic[k] = true; break; }
            }
            if (OpacityOverrides.Count > 0 || HiddenIds.Count > 0)
            {
                for (int k = 0; k < n; k++)
                {
                    for (int p = parent[k], hops = 0; p >= 0 && hops < n; p = parent[p], hops++)
                    {
                        string pid = S(elements[p], "id");
                        if (pid.Length > 0 && (OpacityOverrides.ContainsKey(pid) || HiddenIds.Contains(pid)))
                        {
                            dynamic[k] = true;
                            break;
                        }
                    }
                }
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

            var layerLive = new bool[staticLayerCount];
            for (int k = 0; k < n; k++)
            {
                if (elementLayer[k] < 0) continue;
                string kind = S(elements[k], "t");
                // Images and the playfield stream ignore a zero opacity on the element itself.
                if (kind == "canvas" || kind == "img" || baseOpacity[k] > 0) layerLive[elementLayer[k]] = true;
            }

            var saved = frame;
            frame = null;
            layerTex = new Texture[staticLayerCount];
            layerImg = new CanvasImage[staticLayerCount];
            for (int L = 0; L < staticLayerCount; L++)
            {
                if (!layerLive[L]) continue;
                paintOnlyLayer = L;
                canvas.BeginFrame();
                canvas.SetTransform(scale, 0, 0, scale, 0, 0);
                PaintAll();
                canvas.EndFrame();
                var rt = canvas.CopyTarget("HomeChrome.Layer" + L.ToString(CultureInfo.InvariantCulture));
                layerTex[L] = rt;
                layerImg[L] = new CanvasImage(rt);
            }
            paintOnlyLayer = -1;
            frame = saved;
            cacheReady = true;
            CacheBuilds++;
        }

        // ---- drawing ------------------------------------------------------------------------------------

        /// <summary>Capture pose: the phase baked into the spec. No wall clock.</summary>
        public void Draw() => Draw(double.NaN, -1, null, false, false);

        /// <summary>Animation clock in milliseconds since the animations started (page shown), not since the capture.</summary>
        public void Draw(double timeMs) => Draw(timeMs, -1, null, false, false);

        public void Draw(double timeMs, int pressed, string pressCss, bool modalIntro, bool pinPageEnter)
        {
            long prof = PaintProfile.Stamp();
            frame = double.IsNaN(timeMs) ? null : CssFrame.Build(elements, timeMs, pinPageEnter, modalIntro, pressed, pressCss, frameSlot);
            if (frame != null) frameSlot = frame;
            // A live frame used to force the approximate blur. Settled modals (pageEnter already
            // finished) need the exact readback or the dim behind the card shifts by several levels.
            canvas.ExactBackdrop = DebugExactBackdrop || frame == null || !frame.HasMotion;
            bool savedPremul = Canvas2D.GradientInterpolatesPremultiplied;
            Canvas2D.GradientInterpolatesPremultiplied = true; // CSS gradients interpolate premultiplied
            Deferred.Clear(); DrawnBoxes = DrawnRuns = DrawnEmoji = 0;
            try
            {
                BuildClassText();
                bool composite = Underlay != null;
                if (frame != null && UseLayerCache && !composite) { PaintProfile.Begin(PaintProfile.Cache); EnsureCache(); PaintProfile.End(PaintProfile.Cache); }
                bool settled = frame != null && !frame.PageTransform && frame.PageOpacity >= 0.999;
                cacheOn = UseLayerCache && !composite && settled && cacheReady && paintOnlyLayer < 0 && staticLayerCount > 0;
                lastBlit = -2;
                canvas.BeginFrame();
                canvas.SetTransform(scale, 0, 0, scale, 0, 0);
                if (composite)
                {
                    canvas.Save();
                    canvas.SetTransform(scale, 0, 0, scale, 0, 0);
                    canvas.ImageSmoothingEnabled = false;
                    canvas.DrawTexture(Underlay, 0, 0, Underlay.width, Underlay.height, 0, 0, cssW, cssH);
                    canvas.Restore();
                }
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
                PaintAll();
                PaintToasts();
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

        void EnsureClassOcc()
        {
            if (classOcc != null) return;
            int total = 0;
            for (int k = 0; k < elements.Count; k++)
            {
                string classes = S(elements[k], "c");
                for (int i = 0; i < classes.Length; i++)
                    if (classes[i] != ' ' && (i == 0 || classes[i - 1] == ' ')) total++;
            }
            classOcc = new ClassOcc[total];
            var count = new Dictionary<string, int>();
            for (int k = 0; k < elements.Count; k++)
            {
                string classes = S(elements[k], "c");
                int i = 0;
                while (i < classes.Length)
                {
                    while (i < classes.Length && classes[i] == ' ') i++;
                    int start = i;
                    while (i < classes.Length && classes[i] != ' ') i++;
                    if (i <= start) continue;
                    string token = classes.Substring(start, i - start);
                    count.TryGetValue(token, out int n);
                    classOcc[classOccN++] = new ClassOcc { element = k, key = token + "#" + n.ToString(CultureInfo.InvariantCulture) };
                    count[token] = n + 1;
                }
            }
        }

        bool ClassValuesMatch()
        {
            if (classSnapK == null || classSnapK.Length != ClassTextOverrides.Count) return false;
            int i = 0;
            foreach (var kv in ClassTextOverrides)
            {
                if (classSnapK[i] != kv.Key || classSnapV[i] != kv.Value) return false;
                i++;
            }
            return true;
        }

        void BuildClassText()
        {
            if (ClassTextOverrides.Count == 0)
            {
                classText = null;
                classSnapK = Array.Empty<string>();
                classSnapV = Array.Empty<string>();
                return;
            }
            EnsureClassOcc();
            if (classText != null && ClassValuesMatch()) return;
            if (classText == null) classText = new Dictionary<int, string>();
            else classText.Clear();
            for (int i = 0; i < classOccN; i++)
                if (ClassTextOverrides.TryGetValue(classOcc[i].key, out var text)) classText[classOcc[i].element] = text;
            classSnapK = new string[ClassTextOverrides.Count];
            classSnapV = new string[ClassTextOverrides.Count];
            int n = 0;
            foreach (var kv in ClassTextOverrides)
            {
                classSnapK[n] = kv.Key;
                classSnapV[n] = kv.Value;
                n++;
            }
        }

        void EnsurePaintFlags()
        {
            if (isAppPage != null) return;
            int n = elements.Count;
            isAppPage = new bool[n];
            isBeam = new bool[n];
            backdropKind = new byte[n];
            blurSigma = new double[n];
            for (int k = 0; k < n; k++)
            {
                string classes = S(elements[k], "c");
                isAppPage[k] = HasToken(classes, "app-page");
                isBeam[k] = HasToken(classes, "pedestal-glow-beam");
                string bf = S(elements[k], "bf");
                if (bf.Length == 0 || bf == "none") continue;
                int at = bf.IndexOf("blur(", StringComparison.Ordinal);
                if (at < 0) { backdropKind[k] = 2; continue; }
                int start = at + 5;
                int end = bf.IndexOf(')', start);
                if (end <= start) { backdropKind[k] = 2; continue; }
                backdropKind[k] = 1;
                blurSigma[k] = CssBox.ParseLength(bf.Substring(start, end - start));
            }
        }

        static bool HasToken(string classes, string token)
        {
            if (string.IsNullOrEmpty(classes)) return false;
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

        void PaintAll()
        {
            EnsurePaintFlags();
            int n = elements.Count;
            if (effBuf == null || effBuf.Length < n)
            {
                effBuf = new double[n];
                skipBuf = new bool[n];
                paintIndex = new Dictionary<int, int>(n);
                for (int k = 0; k < n; k++) paintIndex[(int)N(elements[k], "i", k)] = k;
            }
            Array.Clear(effBuf, 0, n);
            Array.Clear(skipBuf, 0, n);
            if (pageSkip == null || pageSkip.Length < n) pageSkip = new bool[n];
            Array.Clear(pageSkip, 0, n);
            if (specHidden == null)
            {
                specHidden = new bool[n];
                isShine = new bool[n];
                for (int k = 0; k < n; k++)
                {
                    string classes = " " + S(elements[k], "c") + " ";
                    specHidden[k] = N(elements[k], "v", 1) == 0 || S(elements[k], "vis") == "hidden" || classes.Contains(" page-hidden ");
                    isShine[k] = classes.Contains(" play-btn-glow ");
                }
            }
            var eff = effBuf;
            var skip = skipBuf;
            var byIndex = paintIndex;
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
                if (DebugStopAfter >= 0 && k > DebugStopAfter) break;
                var e = elements[k];
                if (paintOnlyLayer >= 0 && (elementLayer == null || elementLayer[k] != paintOnlyLayer)) continue;
                int parent = (int)N(e, "p", -1);
                int pk = parent >= 0 && byIndex.TryGetValue(parent, out var pp) ? pp : -1;
                bool appPage = isAppPage[k];
                pageSkip[k] = appPage || (pk >= 0 && pageSkip[pk]);
                if (Underlay != null && S(e, "id") == "mobile-frame")
                {
                    // The underlay already includes the plate. Keep it as a visible parent so the modal inherits opacity.
                    eff[k] = 1;
                    skip[k] = false;
                    continue;
                }
                if (Underlay != null && pageSkip[k])
                {
                    eff[k] = 0;
                    skip[k] = true;
                    continue;
                }
                double parentOpacity = pk >= 0 ? eff[pk] : 1;
                bool parentSkipped = pk >= 0 && skip[pk];

                if (SkipPlayfield && playfieldIndex >= 0 && k < playfieldIndex && CoversViewport(e))
                {
                    eff[k] = parentOpacity * N(e, "o", 1);
                    skip[k] = parentSkipped;
                    if (parentSkipped) continue;
                    if (specHidden[k]) { skip[k] = true; continue; }
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

                string eid = S(e, "id");
                double opacity = frame != null && !double.IsNaN(frame.Opacity[k]) ? frame.Opacity[k] : N(e, "o", 1);
                if (eid.Length > 0 && OpacityOverrides.Count > 0 && OpacityOverrides.TryGetValue(eid, out double opacityMul)) opacity *= opacityMul;
                eff[k] = parentOpacity * opacity;
                skip[k] = parentSkipped;
                if (parentSkipped) continue;
                if (specHidden[k]) { skip[k] = true; continue; }
                if (eid.Length > 0 && HiddenIds.Count > 0 && HiddenIds.Contains(eid)) { skip[k] = true; continue; }
                if (OmitCached(k)) continue;
                bool moving = Motion(k);
                if (moving) BeginMotion(k);
                if (isShine[k]) { PaintShine(e, pk, eff[k], k); if (moving) canvas.Restore(); continue; }

                if (!SimplifyEffects && backdropKind[k] == 1)
                {
                    if (eff[k] > 0.001)
                    {
                        var rr = Rect(e);
                        canvas.BackdropBlur(rr[0], rr[1], rr[2], rr[3], blurSigma[k]);
                    }
                }
                else if (!SimplifyEffects && backdropKind[k] == 2) Defer("backdrop-filter");
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

        bool Motion(int k) => !DebugFreezeMotion && frame != null && !frame.Shine[k] && (frame.Tree[k] || frame.Pressed(k));

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
            double[] src;
            if (element && frame != null && frame.Tree[k] && frame.Local[k] != null) src = frame.Local[k];
            else src = Rect(e);
            rectScratch[0] = src[0];
            rectScratch[1] = src[1];
            rectScratch[2] = src[2];
            rectScratch[3] = src[3];
            if (element && frame != null && Math.Abs(frame.ExtraY[k]) > 1e-6) rectScratch[1] += frame.ExtraY[k];
            return rectScratch;
        }

        bool Outside(double[] r, double reach)
        {
            return r[0] > cssW + reach || r[1] > cssH + reach || r[0] + r[2] < -reach || r[1] + r[3] < -reach;
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
            bool moving = frame != null && (frame.PageTransform || (element && frame.Tree[k]));
            if (!moving)
            {
                double reach = 64;
                var shadows = prep.style.shadows;
                for (int i = 0; i < shadows.Count; i++)
                {
                    var sh = shadows[i];
                    double need = sh.blur + Math.Abs(sh.spread) + Math.Abs(sh.dx) + Math.Abs(sh.dy) + 8;
                    if (need > reach) reach = need;
                }
                if (Outside(r, reach)) { PaintProfile.End(PaintProfile.PaintBox); return; }
            }
            var style = prep.style;
            style.x = r[0]; style.y = r[1]; style.w = r[2]; style.h = r[3];
            bool borderOverride = element && frame != null && frame.BorderLive[k];
            bool shadowOverride = element && frame != null && frame.ShadowN[k] > 0;
            if (!prep.any && !borderOverride && !shadowOverride) { PaintProfile.End(PaintProfile.PaintBox); return; }
            if (borderOverride)
            {
                Array.Copy(style.borderC, borderSave, 4);
                var parsed = frame.BorderRgba[k];
                for (int i = 0; i < 4; i++) style.borderC[i] = parsed;
            }
            if (shadowOverride)
            {
                shadowSave.Clear();
                shadowSave.AddRange(style.shadows);
                style.shadows.Clear();
                int baseIx = k * CssFrame.ShadowStride;
                int sn = frame.ShadowN[k];
                for (int i = 0; i < sn; i++) style.shadows.Add(frame.ShadowPool[baseIx + i]);
            }
            if (tilt != 0)
            {
                double cx = style.x + style.w * 0.5, cy = style.y + style.h * 0.5;
                canvas.Save();
                canvas.Translate(cx, cy);
                canvas.Rotate(tilt);
                canvas.Translate(-cx, -cy);
            }
            bool beam = element && isBeam != null && isBeam[k];
            if (beam)
            {
                canvas.Save();
                canvas.BeginPath();
                canvas.MoveTo(style.x + style.w * 0.15, style.y);
                canvas.LineTo(style.x + style.w * 0.85, style.y);
                canvas.LineTo(style.x + style.w, style.y + style.h);
                canvas.LineTo(style.x, style.y + style.h);
                canvas.ClosePath();
                canvas.Clip();
            }
            // matrix3d discs are stored as their foreshortened AABB. Paint the pre-transform ellipse through perspective * rotateX.
            double pox = 0, poy = 0, pcos = 0, psod = 0, phw = 0, phh = 0;
            bool persp = element && CssBox.TryPerspectiveDisc(S(e, "tf"), style.x, style.y, style.w, style.h, out pox, out poy, out pcos, out psod, out phw, out phh);
            if (persp)
            {
                CssBox.DrawProjectedShadows(canvas, style, alpha, style.shadows, pox, poy, pcos, psod, phw, phh);
                canvas.Save();
                canvas.BeginPath();
                CssBox.TracePerspectiveEllipse(canvas, pox, poy, phw, phh, pcos, psod, 0, 0);
                canvas.Clip();
                CssBox.Draw(canvas, style, alpha, false, false);
                CssBox.DrawProjectedBorder(canvas, style, alpha, pox, poy, pcos, psod, phw, phh);
                canvas.Restore();
            }
            else CssBox.Draw(canvas, style, alpha);
            if (beam) canvas.Restore();
            if (tilt != 0) canvas.Restore();
            if (borderOverride) Array.Copy(borderSave, style.borderC, 4);
            if (shadowOverride)
            {
                style.shadows.Clear();
                style.shadows.AddRange(shadowSave);
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
                    style.borderDashed[i] = style.borderW[i] > 0 && i < bs.Count && bs[i] == "dashed";
                    style.borderC[i] = CssColor.Parse(i < bc.Count ? bc[i] : "", Rgba.Transparent);
                    if (style.borderOn[i] || style.borderDashed[i]) prep.any = true;
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

        string Collapse(Dictionary<string, object> run)
        {
            if (collapsed.TryGetValue(run, out var s)) return s;
            s = Spaces.Replace(S(run, "s"), " ");
            collapsed[run] = s;
            return s;
        }

        static string FontFamily(string ff)
        {
            if (string.IsNullOrEmpty(ff)) return "Fredoka";
            int comma = ff.IndexOf(',');
            string first = (comma >= 0 ? ff.Substring(0, comma) : ff).Trim().Trim('"', '\'');
            return first;
        }

        void EnsureRunStyle(Dictionary<string, object> e, int k)
        {
            if (fontOf == null)
            {
                int n = elements.Count;
                fontOf = new string[n];
                letterOf = new double[n];
                colorOf = new Rgba[n];
                shadowOf = new List<CssShadow>[n];
                upperOf = new bool[n];
            }
            if (fontOf[k] != null) return;
            string family = FontFamily(S(e, "ff"));
            fontOf[k] = S(e, "fw") + " " + S(e, "fz") + " " + family;
            letterOf[k] = CssBox.ParseLength(S(e, "ls"));
            colorOf[k] = CssColor.Parse(S(e, "col"), Rgba.Black);
            shadowOf[k] = CssBox.ParseShadows(S(e, "ts"));
            upperOf[k] = S(e, "tt") == "uppercase";
        }

        string CachedUpper(string text)
        {
            if (text == null) return "";
            if (upperText.TryGetValue(text, out var up)) return up;
            up = text.ToUpperInvariant();
            upperText[text] = up;
            return up;
        }

        string ChildEmoji(Dictionary<string, object> e)
        {
            int id = (int)N(e, "i", -1);
            if (id < 0) return "";
            for (int i = 0; i < elements.Count; i++)
            {
                if ((int)N(elements[i], "p", -2) != id) continue;
                string mark = S(elements[i], "emoji");
                if (mark.Length > 0) return mark;
            }
            return "";
        }

        static bool Blank(string s)
        {
            for (int i = 0; i < s.Length; i++) if (!char.IsWhiteSpace(s[i])) return false;
            return true;
        }

        void PaintRuns(Dictionary<string, object> e, int k, double alpha)
        {
            if (!e.TryGetValue("runs", out var rv) || !(rv is List<object> runs) || runs.Count == 0) return;
            PaintProfile.Begin(PaintProfile.PaintRuns);
            EnsureRunStyle(e, k);
            string font = fontOf[k];
            double ls = letterOf[k];
            var color = colorOf[k];
            var shadows = shadowOf[k];
            bool upper = upperOf[k];
            double textReach = 48;
            for (int i = 0; i < shadows.Count; i++)
            {
                var sh = shadows[i];
                double need = sh.blur + Math.Abs(sh.dx) + Math.Abs(sh.dy) + 8;
                if (need > textReach) textReach = need;
            }

            string textId = S(e, "id");
            string textReplacement = null;
            bool replaceText = textId.Length > 0 && TextOverrides.TryGetValue(textId, out textReplacement);
            if (!replaceText && classText != null && classText.TryGetValue(k, out textReplacement)) replaceText = true;
            if (replaceText)
            {
                // The equipped label is "🍩 DONUT FLOAT" while the spec already paints that emoji as a child image.
                string mark = ChildEmoji(e);
                if (mark.Length > 0 && textReplacement.StartsWith(mark, StringComparison.Ordinal))
                {
                    if (!trimmedText.TryGetValue(textReplacement, out var cut))
                    {
                        cut = textReplacement.Substring(mark.Length);
                        trimmedText[textReplacement] = cut;
                    }
                    textReplacement = cut;
                }
            }
            for (int ri = 0; ri < runs.Count; ri++)
            {
                var run = (Dictionary<string, object>)runs[ri];
                string text;
                if (replaceText && ri == 0) text = upper ? CachedUpper(textReplacement) : textReplacement;
                else if (replaceText) continue;
                else if (upper)
                {
                    if (!collapsedUpper.TryGetValue(run, out text))
                    {
                        text = Collapse(run).ToUpperInvariant();
                        collapsedUpper[run] = text;
                    }
                }
                else text = Collapse(run);
                double[] src;
                if (frame != null && frame.Tree[k] && frame.Runs[k] != null && ri < frame.Runs[k].Length && frame.Runs[k][ri] != null)
                    src = frame.Runs[k][ri];
                else src = Rect(run);
                runScratch[0] = src[0];
                runScratch[1] = src[1];
                runScratch[2] = src[2];
                runScratch[3] = src[3];
                var r = runScratch;
                if (frame != null && Math.Abs(frame.ExtraY[k]) > 1e-6) r[1] += frame.ExtraY[k];
                if (Blank(text)) continue;
                bool movingText = frame != null && (frame.PageTransform || frame.Tree[k]);
                if (!movingText && Outside(r, textReach)) continue;
                PaintProfile.Runs++;

                canvas.Save();
                canvas.Font = font;
                canvas.LetterSpacing = ls;
                int slot = k * 16 + ri;
                string[] lines;
                if (!replaceText && laid.TryGetValue(slot, out var lay) && lay.font == font && lay.text == text && Math.Abs(lay.width - r[2]) < 0.05)
                    lines = lay.lines;
                else
                {
                    string best = text;
                    double bestErr = double.MaxValue;
                    textCandidates[0] = text;
                    textCandidates[1] = text.TrimStart();
                    textCandidates[2] = text.TrimEnd();
                    textCandidates[3] = text.Trim();
                    foreach (var cand in textCandidates)
                    {
                        double err = Math.Abs(canvas.MeasureText(cand) - r[2]);
                        if (err < bestErr - 0.01) { bestErr = err; best = cand; }
                    }
                    // A captured run is already one line box. Reflowing it against a slightly
                    // narrower pre-transform width stacks the second word on top of the first.
                    double fontPx = CssBox.ParseLength(S(e, "fz"));
                    bool singleLine = fontPx > 0 && r[3] <= fontPx * 1.5 + 2;
                    if (singleLine && replaceText)
                    {
                        oneLine[0] = best;
                        lines = oneLine;
                    }
                    else if (singleLine) lines = new[] { best };
                    else lines = WrapToWidth(canvas, best, r[2]).ToArray();
                    if (!replaceText)
                        laid[slot] = new Laid { font = font, text = text, width = r[2], lines = lines };
                }
                double lineH = lines.Length > 1 ? r[3] / lines.Length : 0;
                for (int i = 0; i < lines.Length; i++)
                {
                    // The run rect's top is the first em box, the same origin a single line uses.
                    double baseline = r[1] + (lines.Length > 1 ? i * lineH : 0) + canvas.FontAscent();
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
        /// <summary>
        /// Measured rest box of <c>.toast</c> inside <c>#toast-container</c> (top 75, width 360, centered).
        /// cartoonToast is 2.6s; the controller removes the toast at 2.5s.
        /// </summary>
        void PaintToasts()
        {
            if (Toasts.Count == 0) return;
            var keys = CssAnimation.Find("cartoonToast");
            const double boxX = 35, boxW = 360, gap = 8, padY = 13, lineH = 16, contentW = 322;
            double y = 75;
            var border = CssColor.Parse("#1e1b4b", Rgba.Black);
            var ink = border;
            canvas.Font = "800 13px Fredoka";
            for (int i = 0; i < Toasts.Count; i++)
            {
                var toast = Toasts[i];
                string text = toast.Text ?? "";
                var lines = WrapToWidth(canvas, text, contentW);
                double h = padY * 2 + lines.Count * lineH;
                var sample = CssAnimation.Evaluate(toast.AgeMs, 0, 2600, 1, "normal", "forwards", "cubic-bezier(0.34, 1.56, 0.64, 1)", keys, boxW, h, 0);
                double alpha = sample.Opacity ? sample.OpacityValue : 1;
                if (alpha > 1) alpha = 1;
                if (alpha <= 0.001) { y += h + gap; continue; }
                canvas.Save();
                if (sample.Transform)
                {
                    double cx = boxX + boxW * 0.5, cy = y + h * 0.5;
                    var m = sample.Matrix;
                    canvas.Translate(cx, cy);
                    canvas.Transform(m.a, m.b, m.c, m.d, m.e, m.f);
                    canvas.Translate(-cx, -cy);
                }
                var style = new CssBoxStyle
                {
                    x = boxX, y = y, w = boxW, h = h, hasBg = true,
                    bg = CssColor.Parse("#ffffff", Rgba.Black)
                };
                for (int c = 0; c < 4; c++) style.radii[c] = new Vec2d(18, 18);
                for (int c = 0; c < 4; c++) { style.borderW[c] = 3; style.borderC[c] = border; style.borderOn[c] = true; }
                style.shadows.AddRange(CssBox.ParseShadows("0 6px 0 #1e1b4b"));
                CssBox.Draw(canvas, style, alpha);
                canvas.Font = "800 13px Fredoka";
                double block = lines.Count * lineH;
                double top = y + (h - block) * 0.5;
                for (int li = 0; li < lines.Count; li++)
                {
                    double tw = canvas.MeasureText(lines[li]);
                    double baseline = top + li * lineH + canvas.FontAscent();
                    CssBox.DrawText(canvas, lines[li], boxX + (boxW - tw) * 0.5, baseline, ink, null, alpha);
                }
                canvas.Restore();
                y += h + gap;
                DrawnBoxes++;
            }
        }

        public bool PaintPlayfield(Canvas2D dest)
        {
            if (playfield == null || dest == null) return false;
            dest.BeginFrame();
            playfield.Play(dest);
            dest.EndFrame();
            return true;
        }

        sealed class HintBake
        {
            public Dictionary<string, object> Hint, Hand, Text, Emoji, Run;
            public int TextIndex = -1, EmojiIndex = -1;
            public double Opacity;
            public string HintTf, HandTf, TextFz, Shadow;
            public List<string> Border, Radius;
            public double[] HintR, HandR, TextR, EmojiR, RunR;
        }

        HintBake hintBake;
        bool hintDirty;

        /// <summary>
        /// Paint #gesture-hint at <paramref name="ageMs"/> since the game page was shown.
        /// The spec stores the dismissed pose. Age below 0 leaves that pose alone.
        /// </summary>
        public void PoseGestureHint(double ageMs)
        {
            if (hintBake == null) hintBake = CaptureHint();
            if (hintBake == null) return;
            if (ageMs < 0 || !TryMatrix(hintBake.HintTf, out double a, out _, out _, out _, out double tx, out double ty))
            {
                RestoreHint();
                return;
            }
            if (!TryMatrix(hintBake.HandTf, out double ha, out double hb, out _, out _, out double htx, out _))
            {
                RestoreHint();
                return;
            }
            var pose = GestureHintPose.At(
                ageMs, cssW, cssH,
                new GestureHintPose.Box(hintBake.HintR[0], hintBake.HintR[1], hintBake.HintR[2], hintBake.HintR[3]),
                a, tx, ty,
                new GestureHintPose.Box(ReadBr(hintBake.Hand)[0], ReadBr(hintBake.Hand)[1], ReadBr(hintBake.Hand)[2], ReadBr(hintBake.Hand)[3]),
                ha, hb, htx,
                new GestureHintPose.Box(hintBake.TextR[0], hintBake.TextR[1], hintBake.TextR[2], hintBake.TextR[3]),
                new GestureHintPose.Box(hintBake.EmojiR[0], hintBake.EmojiR[1], hintBake.EmojiR[2], hintBake.EmojiR[3]));
            if (!pose.Live)
            {
                RestoreHint();
                return;
            }
            SetRect(hintBake.Hint, pose.Card.X, pose.Card.Y, pose.Card.W, pose.Card.H);
            SetRect(hintBake.Hand, pose.Hand.X, pose.Hand.Y, pose.Hand.W, pose.Hand.H);
            SetRect(hintBake.Text, pose.Text.X, pose.Text.Y, pose.Text.W, pose.Text.H);
            SetRect(hintBake.Run, pose.Text.X, pose.Text.Y, pose.Text.W, pose.Text.H);
            SetRect(hintBake.Emoji, pose.Emoji.X, pose.Emoji.Y, pose.Emoji.W, pose.Emoji.H);
            hintBake.Hint["o"] = pose.Opacity;
            hintBake.Hint["tf"] = GestureHintPose.Matrix(pose.Scale, 0, 0, pose.Scale, pose.CardTx, pose.CardTy);
            hintBake.Hand["tf"] = GestureHintPose.Matrix(pose.HandA, pose.HandB, pose.HandC, pose.HandD, pose.HandTx, pose.HandTy);
            double font = ParsePx(hintBake.TextFz) * pose.Scale;
            hintBake.Text["fz"] = font.ToString("0.####", CultureInfo.InvariantCulture) + "px";
            WritePxList(hintBake.Hint, "bw", hintBake.Border, pose.Scale);
            WritePxList(hintBake.Hint, "rad", hintBake.Radius, pose.Scale);
            hintBake.Hint["sh"] = ScaleShadow(hintBake.Shadow, pose.Scale);
            if (fontOf != null && hintBake.TextIndex >= 0 && hintBake.TextIndex < fontOf.Length) fontOf[hintBake.TextIndex] = null;
            prepared.Remove(hintBake.Hint);
            hintDirty = true;
            DropLayerCache();
        }

        HintBake CaptureHint()
        {
            Dictionary<string, object> hint = null;
            int hintI = -1;
            for (int k = 0; k < elements.Count; k++)
            {
                if (S(elements[k], "id") != "gesture-hint") continue;
                hint = elements[k];
                hintI = (int)N(hint, "i", -1);
                break;
            }
            if (hint == null || hintI < 0) return null;
            Dictionary<string, object> hand = null, text = null;
            int textIndex = -1;
            for (int k = 0; k < elements.Count; k++)
            {
                if ((int)N(elements[k], "p", -1) != hintI) continue;
                string cls = S(elements[k], "c");
                if (cls.IndexOf("hand-swipe-anim", StringComparison.Ordinal) >= 0) hand = elements[k];
                else if (cls.IndexOf("hint-text", StringComparison.Ordinal) >= 0) { text = elements[k]; textIndex = k; }
            }
            if (hand == null || text == null || !text.TryGetValue("runs", out var rv) || !(rv is List<object> runs) || runs.Count == 0)
                return null;
            var run = runs[0] as Dictionary<string, object>;
            if (run == null) return null;
            int handI = (int)N(hand, "i", -1);
            Dictionary<string, object> emoji = null;
            int emojiIndex = -1;
            for (int k = 0; k < elements.Count; k++)
            {
                if ((int)N(elements[k], "p", -1) != handI) continue;
                if (!elements[k].ContainsKey("emoji")) continue;
                emoji = elements[k];
                emojiIndex = k;
                break;
            }
            if (emoji == null) return null;
            return new HintBake
            {
                Hint = hint, Hand = hand, Text = text, Emoji = emoji, Run = run,
                TextIndex = textIndex, EmojiIndex = emojiIndex,
                Opacity = N(hint, "o", 1),
                HintTf = S(hint, "tf"),
                HandTf = S(hand, "tf"),
                TextFz = S(text, "fz"),
                Shadow = S(hint, "sh"),
                Border = Strs(hint, "bw"),
                Radius = Strs(hint, "rad"),
                HintR = CopyBox(hint), HandR = CopyBox(hand), TextR = CopyBox(text), EmojiR = CopyBox(emoji), RunR = CopyBox(run),
            };
        }

        void RestoreHint()
        {
            if (!hintDirty || hintBake == null || hintBake.HintR == null) return;
            SetRect(hintBake.Hint, hintBake.HintR[0], hintBake.HintR[1], hintBake.HintR[2], hintBake.HintR[3]);
            SetRect(hintBake.Hand, hintBake.HandR[0], hintBake.HandR[1], hintBake.HandR[2], hintBake.HandR[3]);
            SetRect(hintBake.Text, hintBake.TextR[0], hintBake.TextR[1], hintBake.TextR[2], hintBake.TextR[3]);
            SetRect(hintBake.Run, hintBake.RunR[0], hintBake.RunR[1], hintBake.RunR[2], hintBake.RunR[3]);
            SetRect(hintBake.Emoji, hintBake.EmojiR[0], hintBake.EmojiR[1], hintBake.EmojiR[2], hintBake.EmojiR[3]);
            hintBake.Hint["o"] = hintBake.Opacity;
            hintBake.Hint["tf"] = hintBake.HintTf;
            hintBake.Hand["tf"] = hintBake.HandTf;
            hintBake.Text["fz"] = hintBake.TextFz;
            WritePxList(hintBake.Hint, "bw", hintBake.Border, 1);
            WritePxList(hintBake.Hint, "rad", hintBake.Radius, 1);
            hintBake.Hint["sh"] = hintBake.Shadow;
            if (fontOf != null && hintBake.TextIndex >= 0 && hintBake.TextIndex < fontOf.Length) fontOf[hintBake.TextIndex] = null;
            prepared.Remove(hintBake.Hint);
            hintDirty = false;
            DropLayerCache();
        }

        static double[] ReadBr(Dictionary<string, object> e)
        {
            if (e != null && e.TryGetValue("br", out var v) && v is List<object> l && l.Count >= 4)
                return new[] { (double)l[0], (double)l[1], (double)l[2], (double)l[3] };
            return CopyBox(e);
        }

        static double[] CopyBox(Dictionary<string, object> e)
        {
            var l = (List<object>)e["r"];
            return new[] { (double)l[0], (double)l[1], (double)l[2], (double)l[3] };
        }

        void SetRect(Dictionary<string, object> e, double x, double y, double w, double h)
        {
            if (e == null || !e.TryGetValue("r", out var v) || !(v is List<object> l) || l.Count < 4) return;
            l[0] = x; l[1] = y; l[2] = w; l[3] = h;
            if (rects.TryGetValue(e, out var cached))
            {
                cached[0] = x; cached[1] = y; cached[2] = w; cached[3] = h;
            }
        }

        static double ParsePx(string css)
        {
            if (string.IsNullOrEmpty(css) || !css.EndsWith("px", StringComparison.Ordinal)) return 0;
            return double.TryParse(css.Substring(0, css.Length - 2), NumberStyles.Float, CultureInfo.InvariantCulture, out var n) ? n : 0;
        }

        static void WritePxList(Dictionary<string, object> e, string key, List<string> original, double scale)
        {
            if (original == null || !e.TryGetValue(key, out var v) || !(v is List<object> l)) return;
            for (int i = 0; i < l.Count && i < original.Count; i++)
            {
                double n = ParsePx(original[i]);
                l[i] = (n * scale).ToString("0.####", CultureInfo.InvariantCulture) + "px";
            }
        }

        static string ScaleShadow(string shadow, double scale)
        {
            if (string.IsNullOrEmpty(shadow) || Math.Abs(scale - 1) < 1e-9) return shadow;
            return Regex.Replace(shadow, @"-?\d*\.?\d+px", m =>
            {
                double n = ParsePx(m.Value);
                return (n * scale).ToString("0.####", CultureInfo.InvariantCulture) + "px";
            });
        }

        void DrawEmoji(Dictionary<string, object> e, int k, double alpha)
        {
            var r = ElementRect(e, k, true);
            if (r[2] <= 0 || r[3] <= 0) return;
            var img = CanvasEmoji.Get(S(e, "emoji"));
            if (img == null) return;
            canvas.Save();
            double tilt = 0;
            for (var cur = e; cur != null; cur = ParentOf(cur))
            {
                if (S(cur, "c").IndexOf("hand-swipe-anim", StringComparison.Ordinal) < 0) continue;
                tilt = InheritedTilt(e, r);
                break;
            }
            if (tilt != 0)
            {
                double cx = r[0] + r[2] * 0.5, cy = r[1] + r[3] * 0.5;
                canvas.Translate(cx, cy);
                canvas.Rotate(tilt);
                canvas.Translate(-cx, -cy);
            }
            canvas.GlobalAlpha = Math.Min(1, alpha);
            canvas.DrawImage(img, r[0], r[1], r[2], r[3]);
            canvas.Restore();
            DrawnEmoji++;
        }
    }
}
