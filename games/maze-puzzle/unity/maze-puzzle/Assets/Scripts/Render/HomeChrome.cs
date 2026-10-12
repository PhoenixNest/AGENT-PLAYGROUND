using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using OrbitalSalvage.Loc;
using OrbitalSalvage.Logic;
using UnityEngine;

namespace OrbitalSalvage.Render
{
    using Loc = OrbitalSalvage.Loc.Loc;

    /// <summary>
    /// Draws a painted page from the web's measured layout (<c>Resources/Parity/Spec/&lt;state&gt;__&lt;profile&gt;.json</c>)
    /// with <see cref="Canvas2D"/>: visible elements are <see cref="CssBox"/>es, outline glyph text, and emoji images at the
    /// CSS rects Chromium reported. The spec's viewport scale is the capture device pixel ratio. The shipping player
    /// (<c>ParityViewer</c>) letterboxes that raster into the window safe area; this type does not do window fit.
    /// Infinite CSS animations set <see cref="Loops"/> and are sampled from the draw clock. Strict still parity is not
    /// finished: several pages sit under the 99.0% / 0.985 bar, mostly on text-edge coverage.
    /// </summary>
    public sealed partial class HomeChrome : IDisposable
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
            if (Resources.Load<TextAsset>("Parity/Spec/" + stateId + "__zero") != null) return true;
            return Resources.Load<TextAsset>("Parity/Spec/" + stateId + "__iphone") != null;
        }

        Canvas2D canvas;
        readonly StreamPlayer playfield;
        readonly List<Dictionary<string, object>> elements;
        /// <summary>When set, the playfield canvas element is not replayed and full-viewport boxes under it are left transparent.</summary>
        public bool SkipPlayfield;
        /// <summary>Page already drawn underneath a modal. App pages in this spec are skipped so the overlay blurs this texture.</summary>
        public RenderTexture Underlay;

        /// <summary>How many times the paused playfield was blurred for a modal. A size change counts again.</summary>
        public int PlayfieldBlurBuilds { get; private set; }

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
        bool compositeValid;
        bool dirtyActive;
        bool[] dirtyHits;
        bool[] dirtyLive;
        bool[] dirtyLayer;
        bool[] moverMark;
        const int DirtyCap = 24;
        int dirtyN;
        readonly int[] dirtyX0 = new int[DirtyCap];
        readonly int[] dirtyY0 = new int[DirtyCap];
        readonly int[] dirtyX1 = new int[DirtyCap];
        readonly int[] dirtyY1 = new int[DirtyCap];
        readonly double[] hitX0 = new double[DirtyCap];
        readonly double[] hitY0 = new double[DirtyCap];
        readonly double[] hitX1 = new double[DirtyCap];
        readonly double[] hitY1 = new double[DirtyCap];
        int prevN;
        readonly double[] prevX0 = new double[DirtyCap];
        readonly double[] prevY0 = new double[DirtyCap];
        readonly double[] prevX1 = new double[DirtyCap];
        readonly double[] prevY1 = new double[DirtyCap];
        int curN;
        bool curOverflow;
        readonly double[] curX0 = new double[DirtyCap];
        readonly double[] curY0 = new double[DirtyCap];
        readonly double[] curX1 = new double[DirtyCap];
        readonly double[] curY1 = new double[DirtyCap];
        readonly int[] curK = new int[DirtyCap];
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
        int locSeen;
        struct LocMem
        {
            public int Version;
            public byte State;
            public string Text;
        }
        LocMem[] locMem;
        sealed class FitLay
        {
            public int Version;
            public string Source;
            public string Font;
            public double ElW, RunW;
            public string[] Lines;
            public double[] Widths;
            public double FontPx, Letter, LinePitch;
            public string DrawFont;
            public TextFit.Anchor Anchor;
            public TextFit.Action Action;
            public double LeftOff, RightOff;
            public double IconShift;
            public int IconIndex;
            public double SpecElX;
            public double SingleX;
            public double YShift, Incoming, Ceiling;
            public bool Ellipsis, ExtraWrap, SoftShrink, KeepX;
        }
        FitLay[] fitLay;
        double[] locIconShift;
        readonly List<TextFit.Icon> iconScratch = new List<TextFit.Icon>(4);
        readonly List<string> fitLineScratch = new List<string>(4);
        Func<string, double, double, double> fitMeasure;
        string fitFontBase;
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

        /// <summary>True once <see cref="EnsureCache"/> has finished, including a page with no static layers.</summary>
        public bool LayerCacheReady => cacheReady;

        /// <summary>Static layer slots from the last bake. Empty slots are layers that were fully skipped.</summary>
        public int BakedLayerSlots => layerTex == null ? 0 : staticLayerCount;

        /// <summary>Layer textures that were actually allocated.</summary>
        public int BakedLayerCount
        {
            get
            {
                if (layerTex == null) return 0;
                int n = 0;
                for (int i = 0; i < layerTex.Length; i++)
                    if (layerTex[i] != null) n++;
                return n;
            }
        }

        /// <summary>Sum of baked layer RenderTextures, width × height × bytes per pixel of each format.</summary>
        public long BakedLayerBytes
        {
            get
            {
                if (layerTex == null) return 0;
                long n = 0;
                for (int i = 0; i < layerTex.Length; i++) n += TextureBytes(layerTex[i]);
                return n;
            }
        }

        /// <summary>
        /// The four permanent Canvas2D buffers (ARGB32 + RFloat + RHalf + RHalf = 12 bytes per pixel).
        /// Blur scratch and clip textures allocated during a paint are not included.
        /// </summary>
        public long CanvasResidentBytes => (long)PixelWidth * PixelHeight * 12;

        /// <summary>Canvas buffers plus baked layer textures.</summary>
        public long ResidentBytes => CanvasResidentBytes + BakedLayerBytes;

        /// <summary>Layer slots that were not allocated. Those layers had nothing visible to bake.</summary>
        public int SkippedLayerCount => BakedLayerSlots - BakedLayerCount;

        /// <summary>One-shot readback of the baked layers. Does not change pixels or release textures.</summary>
        public struct LayerAudit
        {
            public int Slots;
            public int Allocated;
            public int Skipped;
            public int FullyClear;
            public int IdenticalToAnother;
            public int Unreadable;
        }

        /// <summary>
        /// Hashes each allocated layer. <see cref="LayerAudit.Skipped"/> counts slots that were never allocated.
        /// <see cref="LayerAudit.FullyClear"/> counts allocated layers whose pixels are all zero.
        /// <see cref="LayerAudit.IdenticalToAnother"/> counts allocated layers that share a SHA-256 with another layer.
        /// Callers report this; baking does not drop or share layers from it.
        /// </summary>
        public LayerAudit AuditLayerIdentity()
        {
            var audit = new LayerAudit
            {
                Slots = BakedLayerSlots,
                Allocated = BakedLayerCount
            };
            audit.Skipped = audit.Slots - audit.Allocated;
            if (layerTex == null) return audit;
            var seen = new Dictionary<string, int>();
            for (int i = 0; i < layerTex.Length; i++)
            {
                if (!(layerTex[i] is RenderTexture rt) || !rt.IsCreated()) continue;
                if (!TryHashLayer(rt, out string hash, out bool clear))
                {
                    audit.Unreadable++;
                    continue;
                }
                if (clear) audit.FullyClear++;
                if (seen.TryGetValue(hash, out int n))
                {
                    if (n == 1) audit.IdenticalToAnother++;
                    audit.IdenticalToAnother++;
                    seen[hash] = n + 1;
                }
                else seen[hash] = 1;
            }
            return audit;
        }

        static bool TryHashLayer(RenderTexture rt, out string hash, out bool clear)
        {
            hash = null;
            clear = false;
            var prev = RenderTexture.active;
            Texture2D tex = null;
            try
            {
                RenderTexture.active = rt;
                tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false, true);
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
                RenderStats.ReadPx++;
                tex.Apply(false, false);
                byte[] raw = tex.GetRawTextureData();
                clear = true;
                for (int i = 0; i < raw.Length; i++)
                {
                    if (raw[i] != 0) { clear = false; break; }
                }
                using (var sha = SHA256.Create())
                    hash = BitConverter.ToString(sha.ComputeHash(raw)).Replace("-", "");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                RenderTexture.active = prev;
                if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
            }
        }

        /// <summary>How many times this chrome has baked its static layers.</summary>
        public int CacheBuilds { get; private set; }

        /// <summary>When true, <see cref="EnsureCache"/> records its duration in <see cref="ProbeBakeTicks"/>. Stays false unless a device-perf page switch is open.</summary>
        public static bool ProbeBake;
        public static long ProbeBakeTicks;

        /// <summary>Build the static layer cache the product draw uses. No-op while baking is deferred.</summary>
        public void BakeStaticLayers()
        {
            if (!UseLayerCache || DeferLayerCache) return;
            Draw(4000);
        }

        public static long TextureBytes(Texture tex)
        {
            if (tex == null) return 0;
            int bpp = 4;
            if (tex is RenderTexture rt)
            {
                switch (rt.format)
                {
                    case RenderTextureFormat.R8: bpp = 1; break;
                    case RenderTextureFormat.RHalf: bpp = 2; break;
                    case RenderTextureFormat.RGHalf: bpp = 4; break;
                    case RenderTextureFormat.ARGBHalf: bpp = 8; break;
                    case RenderTextureFormat.RFloat:
                    case RenderTextureFormat.ARGB32:
                    case RenderTextureFormat.BGRA32: bpp = 4; break;
                    default: bpp = 4; break;
                }
            }
            return (long)tex.width * tex.height * bpp;
        }

        /// <summary>
        /// Paint the daily-gift banner from <c>shop_gift_claimed</c>.
        /// Specs that already carry the claimed class are left unchanged.
        /// </summary>
        public void UseClaimedGiftBanner(bool claimed)
        {
            if (claimed)
            {
                if (bannerApplied || BannerAlreadyClaimed()) return;
                if (!ApplyClaimedBanner()) return;
                bannerApplied = true;
            }
            else if (bannerApplied)
            {
                RestoreClaimedBanner();
                bannerApplied = false;
            }
        }

        readonly HashSet<string> styleOverrideIds = new HashSet<string>();
        string[] snapStyle = Array.Empty<string>();
        bool bannerApplied;
        List<BannerSnap> bannerSnaps;
        static readonly Dictionary<string, List<Dictionary<string, object>>> claimedBannerByProfile =
            new Dictionary<string, List<Dictionary<string, object>>>();

        sealed class BannerSnap
        {
            public Dictionary<string, object> Element;
            public Dictionary<string, object> Fields;
            public bool Inserted;
        }

        bool cachedSkip;
        bool cachedExt;
        string[] snapText = Array.Empty<string>();
        string[] snapWidth = Array.Empty<string>();
        string[] snapWidthPct = Array.Empty<string>();
        string[] snapOpacity = Array.Empty<string>();
        string[] snapHidden = Array.Empty<string>();
        string[] snapClass = Array.Empty<string>();
        int cachedReflow;
        int reflowSig;
        double[] reflowY;
        double[] reflowH;
        int[] reflowLines;
        byte[] reflowAlign;
        double[] reflowPitch;
        string[] reflowSource;
        List<string>[] reflowLineText;
        byte[] blockCenter;
        int[] parentOf;
        Dictionary<string, int> indexById;
        string[] reflowKeys = Array.Empty<string>();
        readonly List<string> wrapScratch = new List<string>(4);

        struct WrapHit
        {
            public int Index;
            public int Card;
            public double Extra;
            public double Y;
        }

        readonly WrapHit[] wrapHits = new WrapHit[8];
        int wrapN;

        /// <summary>True when a centered block override wrapped and shifted the card upward.</summary>
        public bool BlockReflowActive => reflowSig != 0;

        public struct PaintedRun
        {
            public string Id;
            public string Text;
            public double X, Y, W, H;
        }

        /// <summary>Runs translated inside an element that already had two or more runs. Per-run until a later task joins them.</summary>
        public int LocFragmentRuns;

        /// <summary>Test hook. Records each drawn run. Leave false on the product path.</summary>
        public static bool RecordRuns;
        public readonly List<PaintedRun> RecordedRuns = new List<PaintedRun>();
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

        /// <summary>New shadow accum at the same raster size, and a dropped layer cache. Bench uses this when rt16 flips.</summary>
        public void RebuildRasterInPlace()
        {
            var next = new Canvas2D(Math.Max(1, canvas.Width), Math.Max(1, canvas.Height));
            canvas.Dispose();
            canvas = next;
            DropLayerCache();
        }

        /// <summary>
        /// Drop baked layers when <paramref name="liveScale"/> is a different raster step.
        /// Returns false when the chrome is already at that step, so a matching page is left untouched.
        /// </summary>
        public bool InvalidateIfScaleDiffers(double liveScale)
        {
            double before = scale;
            ResizeRaster(liveScale);
            return Math.Abs(scale - before) >= 0.05;
        }

        HomeChrome(string profile, Canvas2D canvas, StreamPlayer playfield, List<Dictionary<string, object>> elements, int cssW, int cssH, double scale)
        {
            Profile = profile; this.canvas = canvas; this.playfield = playfield; this.elements = elements; this.cssW = cssW; this.cssH = cssH; this.scale = scale;
        }

        public static HomeChrome Load(string profile) => Load("home", profile);

        public static HomeChrome Load(string spec, string profile)
        {
            if (string.IsNullOrEmpty(profile)) profile = "zero";
            var asset = Resources.Load<TextAsset>("Parity/Spec/" + spec + "__" + profile);
            if (asset == null && profile != "zero")
            {
                profile = "zero";
                asset = Resources.Load<TextAsset>("Parity/Spec/" + spec + "__zero");
            }
            if (asset == null) throw new InvalidOperationException("missing home spec for profile " + profile);
            var root = (Dictionary<string, object>)MiniJson.Parse(asset.text);
            var vp = (Dictionary<string, object>)root["viewport"];
            int w = (int)(double)vp["w"], h = (int)(double)vp["h"];
            double scale = (double)vp["scale"];
            var list = new List<Dictionary<string, object>>();
            foreach (var o in (List<object>)root["elements"]) list.Add((Dictionary<string, object>)o);
            var canvas = new Canvas2D((int)Math.Round(w * scale), (int)Math.Round(h * scale));
            // Playfield recordings are canvas-space frames. The web canvas fills #mobile-frame and
            // initLayout uses the frame size, not safe-area insets, so the zero recording is reused.
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

        static readonly Dictionary<string, Dictionary<string, List<Dictionary<string, object>>>> shopTemplatesByProfile =
            new Dictionary<string, Dictionary<string, List<Dictionary<string, object>>>>();

        /// <summary>First element whose class is exactly <paramref name="exactClass"/>. Used to stack cards on this page's grid.</summary>
        public bool TryGridOrigin(string exactClass, out double x, out double y)
        {
            x = y = 0;
            if (string.IsNullOrEmpty(exactClass)) return false;
            for (int i = 0; i < elements.Count; i++)
            {
                if (S(elements[i], "c") != exactClass) continue;
                if (!elements[i].TryGetValue("r", out var rv) || !(rv is List<object> rect) || rect.Count < 2) continue;
                x = (double)rect[0];
                y = (double)rect[1];
                return true;
            }
            return false;
        }

        /// <summary>Replace shop cards with the measured locked / owned / equipped sub-templates, stacked at <paramref name="slotX"/>/<paramref name="slotY"/>.</summary>
        public void StampShopCards(string kinds, double[] slotX, double[] slotY)
        {
            if (IsItemStamp(kinds))
            {
                CardStamp.ApplyTrees(elements, ItemTrees(kinds, Profile), slotX, slotY);
                return;
            }
            CardStamp.ApplyShop(elements, kinds, ShopTemplates(Profile), slotX, slotY);
        }

        public static double[] ShopCardHeights(string kinds) => ShopCardHeights(kinds, "zero");

        public static double[] ShopCardHeights(string kinds, string profile)
        {
            if (!IsItemStamp(kinds)) return CardStamp.Heights(kinds, ShopTemplates(profile));
            var trees = ItemTrees(kinds, profile);
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

        static List<List<Dictionary<string, object>>> ItemTrees(string kinds, string profile)
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
                        var els = LoadElements(specs[s], profile);
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

        static TextAsset LoadSpecAsset(string spec, string profile)
        {
            if (string.IsNullOrEmpty(profile)) profile = "zero";
            var asset = Resources.Load<TextAsset>("Parity/Spec/" + spec + "__" + profile);
            if (asset == null && profile != "zero")
                asset = Resources.Load<TextAsset>("Parity/Spec/" + spec + "__zero");
            return asset;
        }

        static List<Dictionary<string, object>> LoadElements(string spec, string profile)
        {
            var asset = LoadSpecAsset(spec, profile);
            if (asset == null) return null;
            var root = MiniJson.Parse(asset.text) as Dictionary<string, object>;
            if (root == null || !(root["elements"] is List<object> raw)) return null;
            var list = new List<Dictionary<string, object>>(raw.Count);
            for (int i = 0; i < raw.Count; i++)
                if (raw[i] is Dictionary<string, object> el) list.Add(el);
            return list;
        }

        static Dictionary<string, List<Dictionary<string, object>>> ShopTemplates(string profile)
        {
            if (string.IsNullOrEmpty(profile)) profile = "zero";
            if (shopTemplatesByProfile.TryGetValue(profile, out var cached)) return cached;
            var map = new Dictionary<string, List<Dictionary<string, object>>>();
            TakeKind(map, "shop_rings_default", "equipped", profile);
            TakeKind(map, "shop_rings_default", "locked", profile);
            TakeKind(map, "shop_rings_mixed", "owned", profile);
            shopTemplatesByProfile[profile] = map;
            return map;
        }

        static void TakeKind(Dictionary<string, List<Dictionary<string, object>>> into, string spec, string kind, string profile)
        {
            if (into.ContainsKey(kind)) return;
            var asset = LoadSpecAsset(spec, profile);
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
            ReleasePlayfieldBlur();
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
            // A baked layer is the capture pose. Descendants of a live backdrop must paint after that blur.
            if (dirtyActive && dirtyLive != null && (uint)k < (uint)dirtyLive.Length && dirtyLive[k]) return false;
            int layer = elementLayer[k];
            if (dirtyActive && dirtyLayer != null && layer < dirtyLayer.Length && !dirtyLayer[layer])
            {
                lastBlit = layer;
                return true;
            }
            if (layer != lastBlit && layerTex != null && layer < layerTex.Length && layerTex[layer] != null)
            {
                if (dirtyActive && dirtyN > 0)
                {
                    for (int i = 0; i < dirtyN; i++)
                    {
                        canvas.SetScissorPx(dirtyX0[i], dirtyY0[i], dirtyX1[i], dirtyY1[i]);
                        canvas.BlitLayer(layerTex[layer]);
                    }
                    canvas.SetScissorPx(0, 0, 0, 0);
                }
                else canvas.BlitLayer(layerTex[layer]);
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
            compositeValid = false;
            prevN = 0;
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
                && cachedReflow == reflowSig
                && SameKeys(TextOverrides, snapText)
                && SameKeys(WidthOverrides, snapWidth)
                && SameKeys(WidthPercentOverrides, snapWidthPct)
                && SameKeys(OpacityOverrides, snapOpacity)
                && SameHidden(HiddenIds, snapHidden)
                && SameKeys(ClassTextOverrides, snapClass)
                && SameHidden(styleOverrideIds, snapStyle);
        }

        void RememberDynamicSet()
        {
            cachedSkip = SkipPlayfield;
            cachedExt = ExternalPlayfield != null;
            cachedReflow = reflowSig;
            snapText = CopyKeys(TextOverrides);
            snapWidth = CopyKeys(WidthOverrides);
            snapWidthPct = CopyKeys(WidthPercentOverrides);
            snapOpacity = CopyKeys(OpacityOverrides);
            snapHidden = new string[HiddenIds.Count];
            HiddenIds.CopyTo(snapHidden);
            snapClass = CopyKeys(ClassTextOverrides);
            snapStyle = new string[styleOverrideIds.Count];
            styleOverrideIds.CopyTo(snapStyle);
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
                if (reflowSig != 0 && reflowY != null && (Math.Abs(reflowY[k]) > 0.1 || Math.Abs(reflowH[k]) > 0.1))
                    dynamic[k] = true;
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
            RenderStats.CacheBuilds++;
        }

        bool BannerAlreadyClaimed()
        {
            var root = FindById("daily-gift-banner");
            if (root == null) return true;
            return (" " + S(root, "c") + " ").Contains(" claimed ");
        }

        Dictionary<string, object> FindById(string id)
        {
            for (int k = 0; k < elements.Count; k++)
                if (S(elements[k], "id") == id) return elements[k];
            return null;
        }

        bool ApplyClaimedBanner()
        {
            var source = ClaimedBannerNodes(Profile);
            if (source == null || source.Count == 0) return false;
            var hostRoot = FindById("daily-gift-banner");
            if (hostRoot == null) return false;
            int hostRootI = (int)N(hostRoot, "i", -1);
            int sourceRootI = -1;
            for (int i = 0; i < source.Count; i++)
                if (S(source[i], "id") == "daily-gift-banner") sourceRootI = (int)N(source[i], "i", -1);
            if (sourceRootI < 0) return false;

            var hostByI = IndexByI(elements);
            var hostMap = new Dictionary<string, Dictionary<string, object>>();
            var hostNodes = CollectBanner(elements);
            if (hostNodes == null) return false;
            for (int i = 0; i < hostNodes.Count; i++)
            {
                string path = RelPath(hostNodes[i], hostByI, hostRootI);
                if (!hostMap.ContainsKey(path)) hostMap[path] = hostNodes[i];
            }

            var sourceByI = IndexByI(source);
            bannerSnaps = new List<BannerSnap>();
            Dictionary<string, object> previous = null;
            for (int i = 0; i < source.Count; i++)
            {
                var src = source[i];
                string path = RelPath(src, sourceByI, sourceRootI);
                if (hostMap.TryGetValue(path, out var host))
                {
                    bannerSnaps.Add(new BannerSnap { Element = host, Fields = Snapshot(host) });
                    CopyVisual(host, src);
                    previous = host;
                    continue;
                }
                int parentI = (int)N(src, "p", -1);
                if (!sourceByI.TryGetValue(parentI, out var srcParent)) continue;
                string parentPath = RelPath(srcParent, sourceByI, sourceRootI);
                if (!hostMap.TryGetValue(parentPath, out var hostParent)) continue;
                var clone = (Dictionary<string, object>)CloneJson(src);
                clone["i"] = (double)(MaxElementI() + 1);
                clone["p"] = hostParent["i"];
                int at = previous != null ? elements.IndexOf(previous) + 1 : elements.IndexOf(hostParent) + 1;
                if (at < 1) at = elements.Count;
                elements.Insert(at, clone);
                hostMap[path] = clone;
                bannerSnaps.Add(new BannerSnap { Element = clone, Inserted = true });
                previous = clone;
            }
            RememberBannerIds();
            InvalidateBannerCaches();
            return true;
        }

        void RestoreClaimedBanner()
        {
            if (bannerSnaps != null)
            {
                for (int i = bannerSnaps.Count - 1; i >= 0; i--)
                    if (bannerSnaps[i].Inserted) elements.Remove(bannerSnaps[i].Element);
                for (int i = 0; i < bannerSnaps.Count; i++)
                {
                    var snap = bannerSnaps[i];
                    if (snap.Inserted || snap.Fields == null) continue;
                    snap.Element.Clear();
                    foreach (var kv in snap.Fields) snap.Element[kv.Key] = CloneJson(kv.Value);
                }
            }
            bannerSnaps = null;
            styleOverrideIds.Clear();
            InvalidateBannerCaches();
        }

        void RememberBannerIds()
        {
            styleOverrideIds.Clear();
            var root = FindById("daily-gift-banner");
            if (root == null) return;
            int rootI = (int)N(root, "i", -1);
            var byI = IndexByI(elements);
            for (int k = 0; k < elements.Count; k++)
            {
                bool hit = false;
                var cur = elements[k];
                for (int guard = 0; cur != null && guard < 24; guard++)
                {
                    if ((int)N(cur, "i", -2) == rootI) { hit = true; break; }
                    int p = (int)N(cur, "p", -1);
                    if (p < 0 || !byI.TryGetValue(p, out cur)) break;
                }
                if (!hit) continue;
                string id = S(elements[k], "id");
                if (id.Length > 0) styleOverrideIds.Add(id);
            }
        }

        void InvalidateBannerCaches()
        {
            prepared.Clear();
            rects.Clear();
            laid.Clear();
            collapsed.Clear();
            collapsedUpper.Clear();
            fontOf = null;
            specHidden = null;
            isShine = null;
            isAppPage = null;
            classOcc = null;
            classText = null;
            classSnapK = Array.Empty<string>();
            classSnapV = Array.Empty<string>();
            byId = null;
            parentOf = null;
            indexById = null;
            effBuf = null;
            reflowY = null;
            frameSlot = null;
            DropLayerCache();
        }

        int MaxElementI()
        {
            int max = 0;
            for (int k = 0; k < elements.Count; k++)
            {
                int id = (int)N(elements[k], "i", 0);
                if (id > max) max = id;
            }
            return max;
        }

        static List<Dictionary<string, object>> ClaimedBannerNodes(string profile)
        {
            if (string.IsNullOrEmpty(profile)) profile = "zero";
            if (claimedBannerByProfile.TryGetValue(profile, out var cached)) return cached;
            var asset = LoadSpecAsset("shop_gift_claimed", profile);
            if (asset == null) return null;
            var root = MiniJson.Parse(asset.text) as Dictionary<string, object>;
            if (root == null || !(root["elements"] is List<object> raw)) return null;
            var list = new List<Dictionary<string, object>>(raw.Count);
            for (int i = 0; i < raw.Count; i++)
                if (raw[i] is Dictionary<string, object> el) list.Add(el);
            var nodes = CollectBanner(list);
            if (nodes == null) return null;
            var copy = new List<Dictionary<string, object>>(nodes.Count);
            for (int i = 0; i < nodes.Count; i++)
                copy.Add((Dictionary<string, object>)CloneJson(nodes[i]));
            claimedBannerByProfile[profile] = copy;
            return copy;
        }

        static List<Dictionary<string, object>> CollectBanner(List<Dictionary<string, object>> els)
        {
            int rootI = -1;
            for (int k = 0; k < els.Count; k++)
                if (S(els[k], "id") == "daily-gift-banner") { rootI = (int)N(els[k], "i", -1); break; }
            if (rootI < 0) return null;
            var ids = new HashSet<int> { rootI };
            bool grew = true;
            while (grew)
            {
                grew = false;
                for (int k = 0; k < els.Count; k++)
                {
                    int id = (int)N(els[k], "i", -1);
                    int p = (int)N(els[k], "p", -2);
                    if (ids.Contains(p) && ids.Add(id)) grew = true;
                }
            }
            var list = new List<Dictionary<string, object>>();
            for (int k = 0; k < els.Count; k++)
                if (ids.Contains((int)N(els[k], "i", -1))) list.Add(els[k]);
            return list;
        }

        static Dictionary<int, Dictionary<string, object>> IndexByI(List<Dictionary<string, object>> els)
        {
            var map = new Dictionary<int, Dictionary<string, object>>(els.Count);
            for (int k = 0; k < els.Count; k++) map[(int)N(els[k], "i", k)] = els[k];
            return map;
        }

        static string RelPath(Dictionary<string, object> e, Dictionary<int, Dictionary<string, object>> byI, int rootI)
        {
            var bits = new List<string>(8);
            var cur = e;
            for (int guard = 0; cur != null && guard < 16; guard++)
            {
                string id = S(cur, "id");
                bits.Add(id.Length > 0 ? "#" + id : "." + S(cur, "c"));
                if ((int)N(cur, "i", -2) == rootI) break;
                int p = (int)N(cur, "p", -1);
                if (p < 0 || !byI.TryGetValue(p, out cur)) break;
            }
            bits.Reverse();
            return string.Join("/", bits);
        }

        static Dictionary<string, object> Snapshot(Dictionary<string, object> host)
        {
            var copy = new Dictionary<string, object>(host.Count);
            foreach (var kv in host) copy[kv.Key] = CloneJson(kv.Value);
            return copy;
        }

        static void CopyVisual(Dictionary<string, object> host, Dictionary<string, object> source)
        {
            object keepI = host["i"];
            object keepP = host.ContainsKey("p") ? host["p"] : null;
            host.Clear();
            foreach (var kv in source)
            {
                if (kv.Key == "i" || kv.Key == "p") continue;
                host[kv.Key] = CloneJson(kv.Value);
            }
            host["i"] = keepI;
            if (keepP != null) host["p"] = keepP;
        }

        static object CloneJson(object value)
        {
            var dict = value as Dictionary<string, object>;
            if (dict != null)
            {
                var copy = new Dictionary<string, object>(dict.Count);
                foreach (var kv in dict) copy[kv.Key] = CloneJson(kv.Value);
                return copy;
            }
            var list = value as List<object>;
            if (list != null)
            {
                var copy = new List<object>(list.Count);
                for (int i = 0; i < list.Count; i++) copy.Add(CloneJson(list[i]));
                return copy;
            }
            return value;
        }

        // ---- drawing ------------------------------------------------------------------------------------

        /// <summary>Capture pose: the phase baked into the spec. No wall clock.</summary>
        public void Draw() => Draw(double.NaN, -1, null, false, false);

        /// <summary>Animation clock in milliseconds since the animations started (page shown), not since the capture.</summary>
        public void Draw(double timeMs) => Draw(timeMs, -1, null, false, false);

        public void Draw(double timeMs, int pressed, string pressCss, bool modalIntro, bool pinPageEnter)
        {
            if (locSeen != Loc.Version) InvalidateForLanguage();
            int caller = RenderStats.Caller;
            RenderStats.Caller = 0;
            long prof = PaintProfile.Stamp();
            frame = double.IsNaN(timeMs) ? null : CssFrame.Build(elements, timeMs, pinPageEnter, modalIntro, pressed, pressCss, frameSlot);
            if (frame != null) frameSlot = frame;
            if (RecordRuns) RecordedRuns.Clear();
            ApplyBlockReflow();
            // A live frame used to force the approximate blur. Settled modals (pageEnter already
            // finished) need the exact readback or the dim behind the card shifts by several levels.
            // The sheet intro is motion too; the approximate blur of an empty hole flashes gray.
            canvas.ExactBackdrop = DebugExactBackdrop || modalIntro || frame == null || !frame.HasMotion;
            bool savedPremul = Canvas2D.GradientInterpolatesPremultiplied;
            Canvas2D.GradientInterpolatesPremultiplied = true; // CSS gradients interpolate premultiplied
            Deferred.Clear(); DrawnBoxes = DrawnRuns = DrawnEmoji = 0;
            if (ExternalPlayfield == null) ReleasePlayfieldBlur();
            try
            {
                BuildClassText();
                bool composite = Underlay != null;
                // The sheet intro moves the card. A baked layer is the settled pose, so blitting it
                // pops the sheet open on frame 0.
                if (frame != null && UseLayerCache && !composite && !modalIntro)
                {
                    PaintProfile.Begin(PaintProfile.Cache);
                    if (ProbeBake)
                    {
                        long bakeStamp = System.Diagnostics.Stopwatch.GetTimestamp();
                        EnsureCache();
                        ProbeBakeTicks += System.Diagnostics.Stopwatch.GetTimestamp() - bakeStamp;
                    }
                    else
                        EnsureCache();
                    PaintProfile.End(PaintProfile.Cache);
                }
                bool settled = frame != null && !frame.PageTransform && frame.PageOpacity >= 0.999;
                cacheOn = UseLayerCache && !composite && settled && !modalIntro && cacheReady && paintOnlyLayer < 0 && staticLayerCount > 0;
                lastBlit = -2;
                BuildDirty();
                canvas.BeginFrame();
                if (dirtyActive)
                    for (int i = 0; i < dirtyN; i++)
                        canvas.ClearDirty(dirtyX0[i], dirtyY0[i], dirtyX1[i], dirtyY1[i]);
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
                if (!cacheOn) compositeValid = false;
                else compositeValid = true;
                dirtyActive = false;
                canvas.ScissorOn = false;
                cacheOn = false;
                Canvas2D.GradientInterpolatesPremultiplied = savedPremul;
                frame = null;
                PaintProfile.AddWall(PaintProfile.Stamp() - prof);
                RenderStats.Caller = caller;
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

        bool DirtyMiss(int k)
        {
            return dirtyActive && dirtyHits != null && (uint)k < (uint)dirtyHits.Length && !dirtyHits[k];
        }

        void BoundsOf(int k, out double x0, out double y0, out double x1, out double y1)
        {
            var e = elements[k];
            var r = Rect(e);
            x0 = r[0];
            y0 = r[1];
            x1 = r[0] + r[2];
            y1 = r[1] + r[3];
            string id = S(e, "id");
            if (id.Length > 0 && WidthOverrides.TryGetValue(id, out double widthOverride)) x1 = x0 + widthOverride;
            if (reflowY != null && k < reflowY.Length)
            {
                y0 += reflowY[k];
                y1 += reflowY[k] + reflowH[k];
            }
            if (frame != null && Math.Abs(frame.ExtraY[k]) > 1e-6)
            {
                y0 += frame.ExtraY[k];
                y1 += frame.ExtraY[k];
            }
            if (frame != null && (frame.Tree[k] || frame.Own[k]) && frame.Local[k] != null)
            {
                var lr = frame.Local[k];
                double ax0 = double.MaxValue, ay0 = double.MaxValue, ax1 = double.MinValue, ay1 = double.MinValue;
                for (int corner = 0; corner < 4; corner++)
                {
                    double x = lr[0] + ((corner & 1) == 0 ? 0 : lr[2]);
                    double y = lr[1] + ((corner & 2) == 0 ? 0 : lr[3]);
                    if (Math.Abs(frame.ExtraY[k]) > 1e-6) y += frame.ExtraY[k];
                    var p = TransformChain(k, x, y);
                    if (p.x < ax0) ax0 = p.x;
                    if (p.y < ay0) ay0 = p.y;
                    if (p.x > ax1) ax1 = p.x;
                    if (p.y > ay1) ay1 = p.y;
                }
                x0 = ax0;
                y0 = ay0;
                x1 = ax1;
                y1 = ay1;
            }
            double reach = 48;
            if (prepared.TryGetValue(e, out var prep) && prep.style != null && prep.style.shadows != null)
            {
                var shadows = prep.style.shadows;
                for (int i = 0; i < shadows.Count; i++)
                {
                    var sh = shadows[i];
                    double need = sh.blur + Math.Abs(sh.spread) + Math.Abs(sh.dx) + Math.Abs(sh.dy) + 8;
                    if (need > reach) reach = need;
                }
            }
            x0 -= reach;
            y0 -= reach;
            x1 += reach;
            y1 += reach;
        }

        Vec2d TransformChain(int index, double x, double y)
        {
            frame.Chain.Clear();
            for (int p = index, hops = 0; p >= 0 && hops < frame.Parent.Length; p = frame.Parent[p], hops++)
                if (frame.Live[p]) frame.Chain.Add(p);
            var point = new Vec2d(x, y);
            for (int i = 0; i < frame.Chain.Count; i++)
            {
                int a = frame.Chain[i];
                var lr = frame.Local[a];
                if (lr == null) continue;
                double ox = frame.OriginX[a], oy = frame.OriginY[a];
                if (double.IsNaN(ox)) ox = lr[2] * 0.5;
                if (double.IsNaN(oy)) oy = lr[3] * 0.5;
                double cx = lr[0] + ox, cy = lr[1] + oy + frame.ExtraY[a];
                double lx = point.x - cx, ly = point.y - cy;
                var m = frame.Matrix[a];
                point = new Vec2d(m.a * lx + m.c * ly + m.e + cx, m.b * lx + m.d * ly + m.f + cy);
            }
            return point;
        }

        bool BackdropHitsMotion(double[] rr)
        {
            if (frame == null || moverMark == null) return true;
            int n = elements.Count;
            double rx1 = rr[0] + rr[2], ry1 = rr[1] + rr[3];
            for (int k = 0; k < n; k++)
            {
                if (!moverMark[k]) continue;
                BoundsOf(k, out double a, out double b, out double c, out double d);
                if (c > rr[0] && d > rr[1] && a < rx1 && b < ry1) return true;
            }
            return false;
        }

        bool MotionSource(int k)
        {
            if (frame == null) return false;
            if (frame.Shine[k] || frame.Live[k] || frame.Own[k]) return true;
            if (frame.ShadowN[k] > 0 || frame.BorderLive[k]) return true;
            if (!double.IsNaN(frame.Opacity[k]) || Math.Abs(frame.ExtraY[k]) > 1e-4 || frame.Pressed(k)) return true;
            string id = S(elements[k], "id");
            if (id.Length == 0) return false;
            return TextOverrides.ContainsKey(id) || WidthOverrides.ContainsKey(id)
                || WidthPercentOverrides.ContainsKey(id) || OpacityOverrides.ContainsKey(id) || HiddenIds.Contains(id);
        }

        void NoteCur(int k, double a, double b, double c, double d)
        {
            if (curN >= DirtyCap) { curOverflow = true; return; }
            curK[curN] = k;
            curX0[curN] = a;
            curY0[curN] = b;
            curX1[curN] = c;
            curY1[curN] = d;
            curN++;
        }

        bool TryShineClip(int k, out double x0, out double y0, out double x1, out double y1)
        {
            x0 = y0 = x1 = y1 = 0;
            if (S(elements[k], "c").IndexOf("play-btn-glow", StringComparison.Ordinal) < 0) return false;
            if (frame == null) return false;
            int p = frame.Parent[k];
            if (p < 0 || p >= elements.Count) return false;
            var r = Rect(elements[p]);
            if (r[2] <= 1 || r[3] <= 1) return false;
            // The swept band is clipped to the button, but the clip mask and the soft edge
            // land a little outside the border box. The unclipped AABB is most of the page.
            x0 = r[0] - 36;
            y0 = r[1] - 36;
            x1 = r[0] + r[2] + 36;
            y1 = r[1] + r[3] + 36;
            return true;
        }

        void NoteWide(int k, double x, double y, double w, double h, double pct)
        {
            var e = elements[k];
            string id = S(e, "id");
            string label = id.Length > 0 ? id : S(e, "c");
            if (label.Length == 0) label = "k" + k.ToString();
            if (label.Length > 48) label = label.Substring(0, 48);
            int ip = (int)Math.Round(pct * 100);
            RenderStats.DirtyWide = label;
            RenderStats.DirtyWidePct = ip;
            if (RenderStats.DirtyWideLogged == label) return;
            RenderStats.DirtyWideLogged = label;
            Debug.Log("[Perf] wide " + label + " pct=" + ip.ToString()
                + " xy=" + x.ToString("0") + "," + y.ToString("0")
                + " wh=" + w.ToString("0") + "x" + h.ToString("0"));
        }

        bool OverlapsHits(double a, double b, double c, double d)
        {
            for (int i = 0; i < dirtyN; i++)
                if (c > hitX0[i] && d > hitY0[i] && a < hitX1[i] && b < hitY1[i]) return true;
            return false;
        }

        void ClipToHits(int k)
        {
            if (!dirtyActive) return;
            BoundsOf(k, out double a, out double b, out double c, out double d);
            int x0 = int.MaxValue, y0 = int.MaxValue, x1 = int.MinValue, y1 = int.MinValue;
            for (int i = 0; i < dirtyN; i++)
            {
                if (!(c > hitX0[i] && d > hitY0[i] && a < hitX1[i] && b < hitY1[i])) continue;
                if (dirtyX0[i] < x0) x0 = dirtyX0[i];
                if (dirtyY0[i] < y0) y0 = dirtyY0[i];
                if (dirtyX1[i] > x1) x1 = dirtyX1[i];
                if (dirtyY1[i] > y1) y1 = dirtyY1[i];
            }
            if (x1 <= x0) canvas.SetScissorPx(0, 0, 0, 0);
            else canvas.SetScissorPx(x0, y0, x1, y1);
        }

        void RememberMovers()
        {
            prevN = curN;
            for (int i = 0; i < curN; i++)
            {
                prevX0[i] = curX0[i];
                prevY0[i] = curY0[i];
                prevX1[i] = curX1[i];
                prevY1[i] = curY1[i];
            }
        }

        bool HiddenBox(int k)
        {
            for (int p = k, hops = 0; p >= 0 && hops < 16; hops++)
            {
                var e = elements[p];
                string classes = " " + S(e, "c") + " ";
                if (N(e, "v", 1) == 0 || S(e, "vis") == "hidden" || classes.IndexOf(" page-hidden ", StringComparison.Ordinal) >= 0)
                    return true;
                if (frame == null || frame.Parent == null) break;
                p = frame.Parent[p];
            }
            return false;
        }

        void InheritBackdropHits(int n)
        {
            if (frame == null || frame.Parent == null || backdropKind == null) return;
            if (dirtyLive == null || dirtyLive.Length < n) dirtyLive = new bool[n];
            Array.Clear(dirtyLive, 0, n);
            for (int k = 0; k < n; k++)
            {
                int p = frame.Parent[k];
                if (p < 0) continue;
                bool fromBackdrop = dirtyLive[p] || (dirtyHits[p] && backdropKind[p] == 1);
                if (!fromBackdrop) continue;
                dirtyHits[k] = true;
                dirtyLive[k] = true;
                if (elementLayer != null && elementLayer[k] >= 0 && (uint)elementLayer[k] < (uint)dirtyLayer.Length)
                    dirtyLayer[elementLayer[k]] = true;
            }
        }

        void BuildDirty()
        {
            dirtyActive = false;
            dirtyN = 0;
            canvas.ScissorOn = false;
            canvas.KeepTarget = false;
            canvas.DirtyW = 0;
            bool want = RenderOpts.Dirty || RenderOpts.DirtyClip || (RenderOpts.GameOne && SkipPlayfield && ExternalPlayfield == null);
            bool classify = want || RenderOpts.BackdropLow || RenderOpts.BackdropScope;
            int n = elements.Count;
            if (moverMark == null || moverMark.Length < n) moverMark = new bool[n];
            else Array.Clear(moverMark, 0, n);
            curN = 0;
            curOverflow = false;
            if (!classify || frame == null) return;
            double page = cssW * cssH;
            for (int k = 0; k < n; k++)
            {
                if (!MotionSource(k)) continue;
                moverMark[k] = true;
                double a, b, c, d;
                if (!(RenderOpts.DirtyClip && TryShineClip(k, out a, out b, out c, out d)))
                    BoundsOf(k, out a, out b, out c, out d);
                NoteCur(k, a, b, c, d);
            }
            if (!want)
            {
                RememberMovers();
                return;
            }
            if (!cacheOn || !compositeValid)
            {
                RenderStats.DirtyFull++;
                RenderStats.DirtyFullCold++;
                RememberMovers();
                return;
            }
            if (!frame.HasMotion || frame.PageTransform || frame.PageOpacity < 0.999)
            {
                RenderStats.DirtyFull++;
                RenderStats.DirtyFullPage++;
                RememberMovers();
                return;
            }
            if (curN == 0 || curOverflow)
            {
                RenderStats.DirtyFull++;
                RenderStats.DirtyFullArea++;
                RememberMovers();
                return;
            }
            for (int i = 0; i < curN; i++)
            {
                double aw = Math.Max(0, curX1[i] - curX0[i]);
                double ah = Math.Max(0, curY1[i] - curY0[i]);
                if (aw * ah > page * 0.85)
                {
                    NoteWide(curK[i], curX0[i], curY0[i], aw, ah, aw * ah / page);
                    RenderStats.DirtyFull++;
                    RenderStats.DirtyFullArea++;
                    RememberMovers();
                    return;
                }
            }
            dirtyN = 0;
            void Push(double a, double b, double c, double d)
            {
                if (dirtyN >= DirtyCap) return;
                int x0 = (int)Math.Floor(a * scale) - 2;
                int y0 = (int)Math.Floor(b * scale) - 2;
                int x1 = (int)Math.Ceiling(c * scale) + 2;
                int y1 = (int)Math.Ceiling(d * scale) + 2;
                if (x0 < 0) x0 = 0;
                if (y0 < 0) y0 = 0;
                if (x1 > canvas.Width) x1 = canvas.Width;
                if (y1 > canvas.Height) y1 = canvas.Height;
                if (x1 <= x0 || y1 <= y0) return;
                hitX0[dirtyN] = a;
                hitY0[dirtyN] = b;
                hitX1[dirtyN] = c;
                hitY1[dirtyN] = d;
                dirtyX0[dirtyN] = x0;
                dirtyY0[dirtyN] = y0;
                dirtyX1[dirtyN] = x1;
                dirtyY1[dirtyN] = y1;
                dirtyN++;
            }
            for (int i = 0; i < curN; i++) Push(curX0[i], curY0[i], curX1[i], curY1[i]);
            if (RenderOpts.DirtyClip) PushBackdropHoles(page);
            for (int i = 0; i < prevN; i++) Push(prevX0[i], prevY0[i], prevX1[i], prevY1[i]);

            void PushBackdropHoles(double pageArea)
            {
                if (backdropKind == null) return;
                int movers = dirtyN;
                int count = elements.Count;
                for (int k = 0; k < count; k++)
                {
                    if (backdropKind[k] != 1 || HiddenBox(k)) continue;
                    var r = Rect(elements[k]);
                    if (r[2] < 1 || r[3] < 1) continue;
                    double pad = blurSigma != null && k < blurSigma.Length ? blurSigma[k] * 2 : 16;
                    if (pad < 8) pad = 8;
                    double a = r[0] - pad, b = r[1] - pad, c = r[0] + r[2] + pad, d = r[1] + r[3] + pad;
                    if (Math.Max(0, c - a) * Math.Max(0, d - b) > pageArea * 0.45) continue;
                    bool overlap = false;
                    for (int i = 0; i < movers; i++)
                        if (c > hitX0[i] && d > hitY0[i] && a < hitX1[i] && b < hitY1[i]) { overlap = true; break; }
                    if (overlap) Push(a, b, c, d);
                }
            }
            RememberMovers();
            if (dirtyN == 0)
            {
                RenderStats.DirtyFull++;
                RenderStats.DirtyFullArea++;
                return;
            }
            if (dirtyHits == null || dirtyHits.Length < n) dirtyHits = new bool[n];
            Array.Clear(dirtyHits, 0, n);
            if (dirtyLayer == null || dirtyLayer.Length < staticLayerCount) dirtyLayer = new bool[Math.Max(1, staticLayerCount)];
            Array.Clear(dirtyLayer, 0, dirtyLayer.Length);
            for (int k = 0; k < n; k++)
            {
                BoundsOf(k, out double a, out double b, out double c, out double d);
                bool hit = OverlapsHits(a, b, c, d);
                dirtyHits[k] = hit;
                if (hit && elementLayer != null && (uint)elementLayer[k] < (uint)dirtyLayer.Length)
                    dirtyLayer[elementLayer[k]] = true;
            }
            if (RenderOpts.DirtyClip) InheritBackdropHits(n);
            long area = 0;
            for (int i = 0; i < dirtyN; i++) area += (long)(dirtyX1[i] - dirtyX0[i]) * (dirtyY1[i] - dirtyY0[i]);
            long pagePx = (long)canvas.Width * canvas.Height;
            RenderStats.DirtyFrames++;
            RenderStats.DirtyAreaSum += pagePx > 0 ? (int)(area * 100 / pagePx) : 0;
            dirtyActive = true;
            canvas.KeepTarget = true;
        }

        void PaintAll()
        {
            if (locIconShift != null) Array.Clear(locIconShift, 0, locIconShift.Length);
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

            overflowClipN = 0;
            for (int k = 0; k < n; k++)
            {
                if (DebugStopAfter >= 0 && k > DebugStopAfter) break;
                var e = elements[k];
                if (paintOnlyLayer >= 0 && (elementLayer == null || elementLayer[k] != paintOnlyLayer)) continue;
                SyncOverflowClips(k);
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
                    eff[k] = parentOpacity;
                    skip[k] = parentSkipped;
                    if (DirtyMiss(k)) continue;
                    ClipToHits(k);
                    canvas.Save();
                    canvas.SetTransform(scale, 0, 0, scale, 0, 0);
                    canvas.ImageSmoothingEnabled = false;
                    canvas.DrawTexture(ExternalPlayfield, 0, 0, ExternalPlayfield.width, ExternalPlayfield.height, 0, 0, cssW, cssH);
                    canvas.Restore();
                    continue;
                }
                if (SkipPlayfield) continue;
                if (OmitCached(k)) continue;
                if (DirtyMiss(k)) continue;
                ClipToHits(k);
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
                    if (DirtyMiss(k)) continue;
                    ClipToHits(k);
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
                // Captures store hud-quota-bar at width 0 (v:0). A live width above 0 is the fill.
                if (specHidden[k] && !PositiveWidthOverride(eid)) { skip[k] = true; continue; }
                if (eid.Length > 0 && HiddenIds.Count > 0 && HiddenIds.Contains(eid)) { skip[k] = true; continue; }
                if (OmitCached(k)) continue;
                if (DirtyMiss(k)) continue;
                ClipToHits(k);
                bool moving = Motion(k);
                if (moving) BeginMotion(k);
                if (isShine[k])
                {
                    // The button clip is the painted bound. A dirty scissor would also clip the
                    // offscreen clip mask, which drops the band.
                    bool heldScissor = canvas.ScissorOn;
                    if (heldScissor) canvas.SetScissorPx(0, 0, 0, 0);
                    PaintShine(e, pk, eff[k], k);
                    if (moving) canvas.Restore();
                    if (heldScissor) ClipToHits(k);
                    continue;
                }

                if (!SimplifyEffects && backdropKind[k] == 1)
                {
                    if (eff[k] > 0.001)
                    {
                        var rr = Rect(e);
                        // The live playfield is composited under this texture, so a backdrop readback cannot see it.
                        bool overlay = HasToken(S(e, "c"), "modal-overlay");
                        bool covered = overlay && ExternalPlayfield != null
                            && TryDrawPlayfieldBlur(rr[0], rr[1], rr[2], rr[3], blurSigma[k], eff[k]);
                        if (!covered)
                        {
                            bool backdropOpt = RenderOpts.BackdropLow || RenderOpts.BackdropScope || RenderOpts.BackdropHold;
                            if (backdropOpt && cacheOn && (RenderOpts.BackdropHold || !BackdropHitsMotion(rr)))
                                canvas.BackdropReuseId = k + 1;
                            canvas.BackdropBlur(rr[0], rr[1], rr[2], rr[3], blurSigma[k]);
                        }
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
                PushModalCardClip(e, k);
            }
            ClearOverflowClips();
        }

        bool PositiveWidthOverride(string id)
        {
            return id.Length > 0 && WidthOverrides.TryGetValue(id, out double width) && width > 0;
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
            if (element && reflowY != null && k < reflowY.Length)
            {
                rectScratch[1] += reflowY[k];
                rectScratch[3] += reflowH[k];
            }
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

        void NoteRun(string id, string text, double x, double y, double w, double h)
        {
            RecordedRuns.Add(new PaintedRun { Id = id, Text = text, X = x, Y = y, W = w, H = h });
        }

        void EnsureReflowBuffers(int n)
        {
            if (reflowY != null && reflowY.Length == n) return;
            reflowY = new double[n];
            reflowH = new double[n];
            reflowLines = new int[n];
            reflowAlign = new byte[n];
            reflowPitch = new double[n];
            reflowSource = new string[n];
            reflowLineText = new List<string>[n];
            blockCenter = new byte[n];
        }

        bool ReflowKeySetSame()
        {
            if (reflowKeys.Length != TextOverrides.Count) return false;
            for (int i = 0; i < reflowKeys.Length; i++)
                if (!TextOverrides.ContainsKey(reflowKeys[i])) return false;
            return true;
        }

        void EnsureIndexById()
        {
            if (indexById != null) return;
            indexById = new Dictionary<string, int>();
            for (int k = 0; k < elements.Count; k++)
            {
                string id = S(elements[k], "id");
                if (id.Length > 0 && !indexById.ContainsKey(id)) indexById[id] = k;
            }
        }

        void EnsureParents()
        {
            if (parentOf != null) return;
            int n = elements.Count;
            parentOf = new int[n];
            var byIndex = new Dictionary<int, int>(n);
            for (int k = 0; k < n; k++) byIndex[(int)N(elements[k], "i", k)] = k;
            for (int k = 0; k < n; k++)
            {
                int pi = (int)N(elements[k], "p", -1);
                parentOf[k] = pi >= 0 && byIndex.TryGetValue(pi, out var pk) ? pk : -1;
            }
        }

        /// <summary>
        /// A centered block is a div whose captured run has matching left and right gaps inside the
        /// element rect (the sample is centered, so the rect is the wrap width, not a shrink-wrapped line).
        /// One line keeps the captured run x. Extra lines grow the modal-card upward because the overlay
        /// is align-items:flex-end: elements at or above the text move up, elements below stay.
        /// </summary>
        bool IsCenteredBlock(int k)
        {
            if (blockCenter[k] != 0) return blockCenter[k] == 1;
            var e = elements[k];
            bool yes = false;
            if (S(e, "t") == "div" && e.TryGetValue("runs", out var rv) && rv is List<object> runs && runs.Count > 0 && runs[0] is Dictionary<string, object> run)
            {
                var rr = Rect(run);
                var er = Rect(e);
                if (rr != null && er != null && er[2] > 0 && rr[2] > 0)
                {
                    double left = rr[0] - er[0];
                    double right = (er[0] + er[2]) - (rr[0] + rr[2]);
                    yes = left >= 1 && right >= 1 && Math.Abs(left - right) <= 1.5;
                }
            }
            blockCenter[k] = (byte)(yes ? 1 : 2);
            return yes;
        }

        /// <summary>
        /// A div whose box is already taller than one captured line and whose text starts at the box's left edge.
        /// The web wraps that block (gift-banner-desc). Nowrap pills and shrink-wrapped spans are not this.
        /// </summary>
        bool IsLeftWrapBox(int k, out double pitch)
        {
            pitch = 0;
            var e = elements[k];
            if (S(e, "t") != "div") return false;
            if (!e.TryGetValue("runs", out var rv) || !(rv is List<object> runs) || runs.Count < 2) return false;
            if (!(runs[0] is Dictionary<string, object> run)) return false;
            var rr = Rect(run);
            var er = Rect(e);
            if (rr == null || er == null || er[2] <= 0 || rr[3] <= 0) return false;
            double left = rr[0] - er[0];
            if (left > 1.5) return false;
            if (er[3] < rr[3] * 1.5) return false;
            pitch = er[3] / runs.Count;
            return pitch > 0;
        }

        int CardOf(int k)
        {
            for (int p = k, hops = 0; p >= 0 && hops < elements.Count; p = parentOf[p], hops++)
                if (HasToken(S(elements[p], "c"), "modal-card")) return p;
            return -1;
        }

        int CountWrapped(int k, Dictionary<string, object> e, string text)
        {
            if (reflowSource[k] == text && reflowLineText[k] != null) return reflowLineText[k].Count;
            var er = Rect(e);
            canvas.Font = fontOf[k];
            canvas.LetterSpacing = letterOf[k];
            if (reflowLineText[k] == null) reflowLineText[k] = new List<string>(4);
            WrapWords(canvas, text, er[2], 0.5, reflowLineText[k]);
            reflowSource[k] = text;
            return reflowLineText[k].Count;
        }

        void ApplyBlockReflow()
        {
            // Foreign copy fits the captured boxes. The card-grow path stays English-only.
            if (!Loc.EnglishActive)
            {
                if (reflowSig != 0)
                {
                    reflowSig = 0;
                    wrapN = 0;
                    if (reflowLines != null) Array.Clear(reflowLines, 0, reflowLines.Length);
                    if (reflowY != null) Array.Clear(reflowY, 0, reflowY.Length);
                    if (reflowH != null) Array.Clear(reflowH, 0, reflowH.Length);
                }
                return;
            }
            int n = elements.Count;
            EnsureReflowBuffers(n);
            Array.Clear(reflowY, 0, n);
            Array.Clear(reflowH, 0, n);
            Array.Clear(reflowLines, 0, n);
            Array.Clear(reflowAlign, 0, n);
            Array.Clear(reflowPitch, 0, n);
            reflowSig = 0;
            wrapN = 0;
            if (TextOverrides.Count == 0)
            {
                if (reflowKeys.Length != 0) reflowKeys = Array.Empty<string>();
                return;
            }
            if (!ReflowKeySetSame())
            {
                reflowKeys = new string[TextOverrides.Count];
                int i = 0;
                foreach (var kv in TextOverrides) reflowKeys[i++] = kv.Key;
            }
            EnsureIndexById();
            for (int i = 0; i < reflowKeys.Length; i++)
            {
                string id = reflowKeys[i];
                if (!indexById.TryGetValue(id, out int k)) continue;
                if (!TextOverrides.TryGetValue(id, out string text) || string.IsNullOrEmpty(text)) continue;
                var e = elements[k];
                EnsureRunStyle(e, k);
                if (upperOf[k]) text = CachedUpper(text);
                if (!IsCenteredBlock(k))
                {
                    if (!IsLeftWrapBox(k, out double pitch)) continue;
                    var box = Rect(e);
                    int wrapped = CountWrapped(k, e, text);
                    if (wrapped <= 1) continue;
                    reflowLines[k] = wrapped;
                    reflowAlign[k] = 2;
                    reflowPitch[k] = pitch;
                    reflowSig = reflowSig * 31 + (k + 1) * 17 + wrapped;
                    if (wrapped * pitch > box[3] + 1) reflowH[k] = wrapped * pitch - box[3];
                    continue;
                }
                int lines = CountWrapped(k, e, text);
                if (lines <= 1) continue;
                EnsureParents();
                int card = CardOf(k);
                if (card < 0 || wrapN >= wrapHits.Length) continue;
                var er = Rect(e);
                wrapHits[wrapN++] = new WrapHit { Index = k, Card = card, Extra = (lines - 1) * er[3], Y = er[1] };
                reflowLines[k] = lines;
                reflowAlign[k] = 1;
                reflowPitch[k] = er[3];
                reflowSig = reflowSig * 31 + (k + 1) * 17 + lines;
            }
            if (wrapN == 0) return;
            EnsureParents();
            for (int k = 0; k < n; k++)
            {
                int card = CardOf(k);
                if (card < 0) continue;
                var er = Rect(elements[k]);
                double dy = 0, dh = 0;
                for (int w = 0; w < wrapN; w++)
                {
                    if (wrapHits[w].Card != card) continue;
                    if (wrapHits[w].Y + 0.01 >= er[1]) dy += wrapHits[w].Extra;
                    bool inside = wrapHits[w].Index == k || (wrapHits[w].Y >= er[1] - 0.01 && wrapHits[w].Y < er[1] + er[3] - 0.01);
                    if (inside) dh += wrapHits[w].Extra;
                }
                reflowY[k] = -dy;
                reflowH[k] = dh;
            }
        }

        void PaintReflowLines(Dictionary<string, object> e, int k, double alpha)
        {
            var el = Rect(e);
            double top = el[1] + (reflowY != null && k < reflowY.Length ? reflowY[k] : 0);
            double lineH = reflowPitch != null && k < reflowPitch.Length && reflowPitch[k] > 0 ? reflowPitch[k] : el[3];
            bool left = reflowAlign != null && k < reflowAlign.Length && reflowAlign[k] == 2;
            if (left && e.TryGetValue("runs", out var rv) && rv is List<object> runs && runs.Count > 0 && runs[0] is Dictionary<string, object> first)
            {
                var fr = Rect(first);
                if (fr != null) top = fr[1] + (reflowY != null && k < reflowY.Length ? reflowY[k] : 0);
            }
            var lines = reflowLineText[k];
            string id = S(e, "id");
            canvas.Save();
            canvas.Font = fontOf[k];
            canvas.LetterSpacing = letterOf[k];
            for (int i = 0; i < lines.Count; i++)
            {
                double tw = canvas.MeasureText(lines[i]);
                double x = left ? el[0] : el[0] + (el[2] - tw) * 0.5;
                double baseline = top + i * lineH + canvas.FontAscent();
                CssBox.DrawText(canvas, lines[i], x, baseline, colorOf[k], shadowOf[k], alpha);
                if (RecordRuns) NoteRun(id, lines[i], x, top + i * lineH, tw, lineH);
                DrawnRuns++;
            }
            canvas.Restore();
        }

        void PaintRuns(Dictionary<string, object> e, int k, double alpha)
        {
            if (!e.TryGetValue("runs", out var rv) || !(rv is List<object> runs) || runs.Count == 0) return;
            PaintProfile.Begin(PaintProfile.PaintRuns);
            EnsureRunStyle(e, k);
            if (reflowLines != null && k < reflowLines.Length && reflowLines[k] > 1 && reflowLineText != null && reflowLineText[k] != null)
            {
                PaintReflowLines(e, k, alpha);
                PaintProfile.End(PaintProfile.PaintRuns);
                return;
            }
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
            // EnglishActive is one bool. When it is set, the run loop does not allocate or take another branch.
            bool english = Loc.EnglishActive;
            if (!english && runs.Count > 1)
            {
                int[] order = RunOrderByY(runs);
                for (int n = 0; n < order.Length; n++)
                    PaintLocalisedRun(e, k, order[n], runs[order[n]] as Dictionary<string, object>, runs, runs.Count, replaceText, textReplacement, upper, font, ls, color, shadows, alpha, textId, textReach, false);
            }
            for (int ri = 0; ri < runs.Count; ri++)
            {
                var run = (Dictionary<string, object>)runs[ri];
                if (!english)
                {
                    PaintLocalisedRun(e, k, ri, run, runs, runs.Count, replaceText, textReplacement, upper, font, ls, color, shadows, alpha, textId, textReach, true);
                    continue;
                }
                PaintCapturedRun(e, k, ri, run, replaceText, textReplacement, upper, font, ls, color, shadows, alpha, textId, textReach, 0);
            }
            PaintProfile.End(PaintProfile.PaintRuns);
        }

        void PaintCapturedRun(Dictionary<string, object> e, int k, int ri, Dictionary<string, object> run,
            bool replaceText, string textReplacement, bool upper, string font, double ls, Rgba color, List<CssShadow> shadows,
            double alpha, string textId, double textReach, double shiftX)
        {
            string text;
            if (replaceText && ri == 0) text = upper ? CachedUpper(textReplacement) : textReplacement;
            else if (replaceText) return;
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
            if (reflowY != null && k < reflowY.Length) r[1] += reflowY[k];
            r[0] += shiftX;
            if (Blank(text)) return;
            bool movingText = frame != null && (frame.PageTransform || frame.Tree[k]);
            if (!movingText && Outside(r, textReach)) return;
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
                    double tw = canvas.MeasureText(best);
                    if (tw > r[2] + 0.5)
                    {
                        var er = Rect(e);
                        double rightGap = (er[0] + er[2]) - (r[0] + r[2]);
                        double leftGap = r[0] - er[0];
                        if (er != null && rightGap <= 1.5 && leftGap >= 8)
                            r[0] = er[0] + er[2] - tw;
                    }
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
                if (RecordRuns) NoteRun(textId, lines[i], r[0], r[1] + (lines.Length > 1 ? i * lineH : 0), canvas.MeasureText(lines[i]), lines.Length > 1 ? lineH : r[3]);
                DrawnRuns++;
            }
            canvas.Restore();
        }

        /// <summary>
        /// Non-English run. A miss falls through to <see cref="PaintCapturedRun"/> so untranslated
        /// symbols keep the English boxes. Hits use <see cref="TextFit"/> and never the baked-box trim.
        /// </summary>
        void PaintLocalisedRun(Dictionary<string, object> e, int k, int ri, Dictionary<string, object> run, List<object> runs, int runCount,
            bool replaceText, string textReplacement, bool upper, string font, double ls, Rgba color, List<CssShadow> shadows,
            double alpha, string textId, double textReach, bool draw)
        {
            if (run == null) return;
            double shift = IconShiftOf(k);
            if (replaceText && ri != 0) return;
            string display = null;
            bool fitted = false;
            if (replaceText)
            {
                display = upper ? LocCase.Upper(textReplacement, Loc.CurrentLanguage) : textReplacement;
                fitted = true;
            }
            else if (TryLocRun(k, ri, run, out string translated))
            {
                // Per-run translation. A group key is not in the table yet. Extra lines push the next run down.
                if (draw && runCount >= 2) LocFragmentRuns++;
                display = upper ? LocCase.Upper(translated, Loc.CurrentLanguage) : translated;
                fitted = true;
            }
            if (!fitted)
            {
                if (draw) PaintCapturedRun(e, k, ri, run, false, null, upper, font, ls, color, shadows, alpha, textId, textReach, shift);
                return;
            }
            if (Blank(display)) return;
            double[] src = Rect(run);
            var er = Rect(e);
            if (src == null || er == null) return;
            double runX = src[0], specY = src[1], runW = src[2], runH = src[3];
            if (frame != null && Math.Abs(frame.ExtraY[k]) > 1e-6) specY += frame.ExtraY[k];
            if (reflowY != null && k < reflowY.Length) specY += reflowY[k];
            double incoming = runCount > 1 ? StackShift(runs, k, ri) : 0;
            double ceiling = RunCeiling(e, k, runs, specY, er);
            double runY = specY + incoming;
            int slot = k * 16 + ri;
            int version = Loc.Version;
            FitLay lay = fitLay != null && slot < fitLay.Length ? fitLay[slot] : null;
            if (lay == null || lay.Version != version || lay.Source != display || lay.Font != font || Math.Abs(lay.ElW - er[2]) > 0.05 || Math.Abs(lay.RunW - runW) > 0.05
                || Math.Abs(lay.Incoming - incoming) > 0.05 || Math.Abs(lay.Ceiling - ceiling) > 0.05)
            {
                lay = BuildFit(e, k, run, display, font, ls, er, runX, runY, runW, runH, version, incoming, ceiling);
                if (fitLay == null || slot >= fitLay.Length) fitLay = new FitLay[Math.Max(slot + 16, elements.Count * 16)];
                fitLay[slot] = lay;
            }
            if (!draw) return;
            if (lay.IconIndex >= 0) SetIconShift(lay.IconIndex, lay.IconShift);
            if (lay.Lines == null || lay.Lines.Length == 0) return;
            canvas.Save();
            canvas.Font = lay.DrawFont;
            canvas.LetterSpacing = lay.Letter;
            double left = er[0] + lay.LeftOff;
            double right = er[0] + lay.RightOff;
            double moveX = er[0] - lay.SpecElX;
            for (int i = 0; i < lay.Lines.Length; i++)
            {
                double tw = lay.Widths != null && i < lay.Widths.Length ? lay.Widths[i] : 0;
                double x = lay.Lines.Length == 1 && (lay.IconIndex >= 0 || lay.KeepX)
                    ? lay.SingleX + moveX
                    : TextFit.PlaceX(lay.Anchor, er[0], er[2], tw, left, right);
                double top = runY + i * lay.LinePitch;
                double baseline = top + canvas.FontAscent();
                CssBox.DrawText(canvas, lay.Lines[i], x, baseline, color, shadows, alpha);
                if (RecordRuns) NoteRun(textId, lay.Lines[i], x, top, tw, lay.Lines.Length > 1 ? lay.LinePitch : runH);
                DrawnRuns++;
            }
            canvas.Restore();
        }

        FitLay BuildFit(Dictionary<string, object> e, int k, Dictionary<string, object> run, string display, string font, double ls,
            double[] er, double runX, double runY, double runW, double runH, int version, double incoming, double ceiling)
        {
            EnsureFitMeasure();
            fitFontBase = font;
            CollectIcons(k, runY - incoming, runH, runX, runW);
            var icons = iconScratch.Count == 0 ? null : iconScratch.ToArray();
            double fontPx = CssBox.ParseLength(S(e, "fz"));
            bool buttonLike = TextFit.LooksLikeControl(S(e, "t"), S(e, "id"), S(e, "c"));
            var input = new TextFit.Input
            {
                RunX = runX, RunY = runY, RunW = runW, RunH = runH,
                ElX = er[0], ElY = er[1], ElW = er[2], ElH = er[3],
                CenteredBlock = false,
                IsButton = S(e, "t") == "button",
                ButtonLike = buttonLike,
                FontPx = fontPx,
                LetterSpacing = ls,
                OriginalMultiLine = fontPx > 0 && runH > TextFit.SingleLineFactor * fontPx + TextFit.SingleLineSlack,
                SidePad = TextFit.SidePadding,
                Icons = icons,
                BandY = Profile == "iphone" ? TextFit.HomeIndicatorY : 0,
                CeilingY = ceiling
            };
            // IsCenteredBlock needs blockCenter, which foreign reflow leaves at 0. Compute the same gaps.
            input.CenteredBlock = CenteredGaps(runX, runW, er);
            TextFit.Layout plan = TextFit.Plan(input, (px, spacing) =>
            {
                fitFontBase = font;
                return fitMeasure(display, px, spacing);
            });
            var lay = new FitLay
            {
                Version = version,
                Source = display,
                Font = font,
                ElW = er[2],
                RunW = runW,
                FontPx = plan.FontPx,
                Letter = plan.LetterSpacing,
                LinePitch = plan.LinePitch > 0 ? plan.LinePitch : Math.Max(1, fontPx),
                DrawFont = FontSized(font, plan.FontPx),
                Anchor = plan.Anchor,
                Action = plan.Action,
                LeftOff = plan.Left - er[0],
                RightOff = plan.Right - er[0],
                IconShift = plan.IconShift,
                IconIndex = plan.IconIndex,
                SpecElX = er[0],
                SingleX = plan.X,
                YShift = incoming,
                Incoming = incoming,
                Ceiling = ceiling,
                SoftShrink = plan.SoftShrink,
                KeepX = plan.KeepX
            };
            canvas.Font = lay.DrawFont;
            canvas.LetterSpacing = lay.Letter;
            Func<string, double> measureLine = s =>
            {
                canvas.Font = lay.DrawFont;
                canvas.LetterSpacing = lay.Letter;
                return canvas.MeasureText(s);
            };
            if (plan.Action == TextFit.Action.Wrap)
            {
                fitLineScratch.Clear();
                LineBreaker.For(Loc.CurrentLanguage).Wrap(display, plan.Available, 0.5, measureLine, fitLineScratch);
                int max = plan.MaxLines < 1 ? 1 : plan.MaxLines;
                if (fitLineScratch.Count > max)
                {
                    lay.ExtraWrap = true;
                    lay.Ellipsis = true;
                    while (fitLineScratch.Count > max) fitLineScratch.RemoveAt(fitLineScratch.Count - 1);
                    if (fitLineScratch.Count > 0)
                        fitLineScratch[fitLineScratch.Count - 1] = TextFit.Ellipsize(fitLineScratch[fitLineScratch.Count - 1], plan.Available, measureLine);
                }
                for (int i = 0; i < fitLineScratch.Count; i++)
                {
                    if (measureLine(fitLineScratch[i]) <= plan.Available + 0.5) continue;
                    fitLineScratch[i] = TextFit.Ellipsize(fitLineScratch[i], plan.Available, measureLine);
                    lay.Ellipsis = true;
                }
                lay.Lines = fitLineScratch.ToArray();
            }
            else if (plan.Action == TextFit.Action.Ellipsis)
            {
                lay.Ellipsis = true;
                lay.Lines = new[] { TextFit.Ellipsize(display, plan.Available, measureLine) };
            }
            else lay.Lines = new[] { display };
            lay.Widths = new double[lay.Lines.Length];
            for (int i = 0; i < lay.Lines.Length; i++)
            {
                canvas.Font = lay.DrawFont;
                canvas.LetterSpacing = lay.Letter;
                lay.Widths[i] = canvas.MeasureText(lay.Lines[i]);
            }
            if (lay.Lines.Length == 1 && plan.IconIndex < 0 && !plan.KeepX)
                lay.SingleX = TextFit.PlaceX(plan.Anchor, er[0], er[2], lay.Widths[0], plan.Left, plan.Right);
            return lay;
        }

        static bool CenteredGaps(double runX, double runW, double[] er)
        {
            if (er == null || er[2] <= 0 || runW <= 0) return false;
            double left = runX - er[0];
            double right = (er[0] + er[2]) - (runX + runW);
            return left >= 1 && right >= 1 && Math.Abs(left - right) <= 1.5;
        }

        void EnsureFitMeasure()
        {
            if (fitMeasure != null) return;
            fitMeasure = (text, px, spacing) =>
            {
                canvas.Font = FontSized(fitFontBase, px);
                canvas.LetterSpacing = spacing;
                return canvas.MeasureText(text);
            };
        }

        static string FontSized(string font, double px)
        {
            if (string.IsNullOrEmpty(font)) return font;
            int pxAt = font.IndexOf("px", StringComparison.Ordinal);
            if (pxAt < 0) return font;
            int start = pxAt;
            while (start > 0 && (char.IsDigit(font[start - 1]) || font[start - 1] == '.')) start--;
            string num = px.ToString("0.##", CultureInfo.InvariantCulture);
            return font.Substring(0, start) + num + font.Substring(pxAt);
        }

        int[] RunOrderByY(List<object> runs)
        {
            var order = new int[runs.Count];
            for (int i = 0; i < order.Length; i++) order[i] = i;
            Array.Sort(order, (a, b) => RunTop(runs, a).CompareTo(RunTop(runs, b)));
            return order;
        }

        double RunTop(List<object> runs, int index)
        {
            if (runs == null || index < 0 || index >= runs.Count || !(runs[index] is Dictionary<string, object> run)) return 0;
            var r = Rect(run);
            return r == null ? 0 : r[1];
        }

        double StackShift(List<object> runs, int k, int ri)
        {
            if (runs == null || ri < 0 || ri >= runs.Count) return 0;
            double mine = RunTop(runs, ri);
            double extra = 0;
            for (int i = 0; i < runs.Count; i++)
            {
                if (i == ri) continue;
                double top = RunTop(runs, i);
                if (top >= mine - 0.5) continue;
                if (!(runs[i] is Dictionary<string, object> run)) continue;
                var r = Rect(run);
                if (r == null) continue;
                int slot = k * 16 + i;
                FitLay lay = fitLay != null && slot < fitLay.Length ? fitLay[slot] : null;
                if (lay == null || lay.Lines == null || lay.Lines.Length == 0) continue;
                double pitch = lay.LinePitch > 0 ? lay.LinePitch : r[3];
                double used = lay.Lines.Length * pitch;
                if (used > r[3]) extra += used - r[3];
            }
            return extra;
        }

        double RunCeiling(Dictionary<string, object> e, int k, List<object> runs, double specY, double[] er)
        {
            double bottom = er[1] + er[3];
            if (Profile == "iphone") bottom = Math.Min(bottom, TextFit.HomeIndicatorY);
            EnsureParents();
            if (parentOf != null && k >= 0 && k < parentOf.Length && parentOf[k] >= 0)
            {
                int parent = parentOf[k];
                for (int i = 0; i < elements.Count; i++)
                {
                    if (i == k || parentOf[i] != parent) continue;
                    var sib = Rect(elements[i]);
                    if (sib == null || sib[2] <= 0 || sib[3] <= 0 || sib[1] >= 932) continue;
                    if (sib[1] <= er[1] + 1) continue;
                    if (sib[1] < bottom) bottom = sib[1];
                }
            }
            double reserved = 0;
            if (runs != null)
            {
                for (int i = 0; i < runs.Count; i++)
                {
                    if (!(runs[i] is Dictionary<string, object> run)) continue;
                    var r = Rect(run);
                    if (r == null || r[3] <= 0) continue;
                    if (r[1] > specY + 0.5) reserved += r[3];
                }
            }
            double ceiling = bottom - reserved;
            if (ceiling < specY + 1) ceiling = specY + 1;
            return ceiling;
        }

        void CollectIcons(int k, double runY, double runH, double runX, double runW)
        {
            iconScratch.Clear();
            int id = (int)N(elements[k], "i", -1);
            if (id < 0) return;
            for (int i = 0; i < elements.Count; i++)
            {
                if ((int)N(elements[i], "p", -2) != id) continue;
                var child = elements[i];
                double x, y, w, h;
                int iconIndex = i;
                if (!IconBox(child, out x, out y, out w, out h)
                    && !NestedEmoji(child, out iconIndex, out x, out y, out w, out h)) continue;
                if (y + h < runY - 2 || y > runY + runH + 2) continue;
                double iconRight = x + w;
                double runRight = runX + runW;
                bool lead = iconRight <= runX + 2 && runX - iconRight <= TextFit.IconGapSlop;
                bool trail = x >= runRight - 2 && x - runRight <= TextFit.IconGapSlop;
                bool overlap = x < runRight - 0.5 && iconRight > runX + 0.5;
                if (!lead && !trail && !overlap) continue;
                iconScratch.Add(new TextFit.Icon { Index = iconIndex, X = x, Y = y, W = w, H = h });
            }
        }

        bool IconBox(Dictionary<string, object> child, out double x, out double y, out double w, out double h)
        {
            x = y = w = h = 0;
            var r = Rect(child);
            if (r == null || r[2] <= 0 || r[3] <= 0) return false;
            bool emoji = S(child, "emoji").Length > 0;
            bool symbol = false;
            if (!emoji && child.TryGetValue("runs", out var rv) && rv is List<object> runs && runs.Count == 1 && runs[0] is Dictionary<string, object> run)
            {
                string s = Collapse(run).Trim();
                symbol = s.Length > 0 && s.Length <= 2 && !LocText.HasLetter(s) && r[2] <= 28;
            }
            if (!emoji && !symbol) return false;
            x = r[0]; y = r[1]; w = r[2]; h = r[3];
            return true;
        }

        /// <summary>
        /// A wrapper span whose only job is to hold an emoji image. The image overlaps the
        /// parent's run (the settings title gear). The box reserved is the image, not the wrapper.
        /// </summary>
        bool NestedEmoji(Dictionary<string, object> child, out int index, out double x, out double y, out double w, out double h)
        {
            index = -1;
            x = y = w = h = 0;
            if (child.TryGetValue("runs", out var rv) && rv is List<object> runs)
            {
                for (int n = 0; n < runs.Count; n++)
                {
                    if (runs[n] is Dictionary<string, object> run && LocText.HasLetter(LocText.Normalize(Collapse(run))))
                        return false;
                }
            }
            int id = (int)N(child, "i", -1);
            if (id < 0) return false;
            for (int i = 0; i < elements.Count; i++)
            {
                if ((int)N(elements[i], "p", -2) != id) continue;
                if (S(elements[i], "emoji").Length == 0) continue;
                var r = Rect(elements[i]);
                if (r == null || r[2] <= 0 || r[3] <= 0) continue;
                index = i;
                x = r[0]; y = r[1]; w = r[2]; h = r[3];
                return true;
            }
            return false;
        }

        bool TryLocRun(int k, int ri, Dictionary<string, object> run, out string text)
        {
            text = null;
            if (ri >= 16) return Loc.TryTranslateSpecText(Collapse(run), out text);
            int slot = k * 16 + ri;
            int version = Loc.Version;
            if (locMem == null || slot >= locMem.Length)
            {
                var grown = new LocMem[Math.Max(slot + 16, elements.Count * 16)];
                if (locMem != null) Array.Copy(locMem, grown, locMem.Length);
                locMem = grown;
            }
            if (locMem[slot].Version == version && locMem[slot].State != 0)
            {
                text = locMem[slot].Text;
                return locMem[slot].State == 1;
            }
            string source = Collapse(run);
            bool hit = LocText.HasLetter(LocText.Normalize(source)) && Loc.TryTranslateSpecText(source, out text);
            locMem[slot].Version = version;
            locMem[slot].State = (byte)(hit ? 1 : 2);
            locMem[slot].Text = hit ? text : null;
            return hit;
        }

        double IconShiftOf(int k)
        {
            if (locIconShift == null || k < 0 || k >= locIconShift.Length) return 0;
            return locIconShift[k];
        }

        void SetIconShift(int index, double shift)
        {
            if (index < 0 || Math.Abs(shift) < 0.01) return;
            if (locIconShift == null || index >= locIconShift.Length)
            {
                var grown = new double[Math.Max(index + 1, elements.Count)];
                if (locIconShift != null) Array.Copy(locIconShift, grown, locIconShift.Length);
                locIconShift = grown;
            }
            locIconShift[index] = shift;
        }

        public void InvalidateForLanguage()
        {
            locSeen = Loc.Version;
            laid.Clear();
            collapsedUpper.Clear();
            trimmedText.Clear();
            upperText.Clear();
            locMem = null;
            fitLay = null;
            locIconShift = null;
            LocFragmentRuns = 0;
            reflowSig = 0;
            wrapN = 0;
            if (reflowLines != null) Array.Clear(reflowLines, 0, reflowLines.Length);
            if (reflowY != null) Array.Clear(reflowY, 0, reflowY.Length);
            if (reflowH != null) Array.Clear(reflowH, 0, reflowH.Length);
            if (reflowSource != null)
                for (int i = 0; i < reflowSource.Length; i++) reflowSource[i] = null;
            if (canvas != null) canvas.ClearTextLayoutCache();
            DropLayerCache();
        }

        static List<string> WrapToWidth(Canvas2D canvas, string text, double width)
        {
            var lines = new List<string>();
            WrapWords(canvas, text, width, Math.Max(8, width * 0.08), lines);
            return lines;
        }

        static void WrapWords(Canvas2D canvas, string text, double width, double slack, List<string> lines)
        {
            lines.Clear();
            if (canvas.MeasureText(text) <= width + slack) { lines.Add(text); return; }
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
            if (locIconShift != null && k < locIconShift.Length) r[0] += locIconShift[k];
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
