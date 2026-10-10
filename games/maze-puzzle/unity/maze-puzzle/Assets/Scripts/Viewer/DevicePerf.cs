using System;
using System.Diagnostics;
using System.Text;

namespace OrbitalSalvage.Viewer
{
    /// <summary>
    /// On-device frame stats. Callers pass times and counters; nothing here touches the GPU,
    /// <c>JsRandom</c>, or the quality governor. The viewer logs one <c>[Perf]</c> line per second
    /// when PlayerPrefs <see cref="PrefKey"/> is 1, and stays silent otherwise.
    /// When frame timing is available the line adds <c>gpu_ms=avg/max</c>; otherwise <c>gpu_ms=na</c>.
    /// Paint counters on that line are per-second deltas: <c>submits</c>, <c>cmds</c> (draws executed),
    /// <c>draws</c> (draws recorded), <c>backdrop=exact/gpu</c>, <c>shadows</c>, <c>blits</c>, <c>cachebuilds</c>.
    ///
    /// Experiment prefs, read once at product boot. Absent prefs leave the product path unchanged.
    /// <list type="table">
    /// <item><term>os.parity.exp</term><description>Comma-separated flags, default empty (all off). Unknown tokens are ignored.</description></item>
    /// <item><term>noshadow</term><description>Skip blurred outer and inset shadows. Zero-blur shadows still draw.</description></item>
    /// <item><term>nobackdrop</term><description>HomeChrome treats this as SimplifyEffects, so BackdropBlur is skipped.</description></item>
    /// <item><term>noloops</term><description>Freeze CSS ambient loops. The page paints on change (enter, press, toast, quota).</description></item>
    /// <item><term>nocache</term><description>HomeChrome.UseLayerCache = false.</description></item>
    /// <item><term>nogamehud</term><description>Game page skips the HUD HomeChrome draw. The playfield still renders. Pause modals still draw.</description></item>
    /// <item><term>nogamefx</term><description>Game canvas skips blurred ShadowBlur / glow passes. HUD shadows stay.</description></item>
    /// <item><term>noimgui</term><description>Game page skips the HUD GUI.DrawTexture and blits the playfield only.</description></item>
    /// <item><term>scale1 / scale1.5 / scale2 / scale3</term><description>Force that raster scale. The governor does not step or save over it. Last scale token wins.</description></item>
    /// <item><term>f30 / f15</term><description>Application.targetFrameRate. Last of the two wins.</description></item>
    /// <item><term>amb30 / amb20 / amb15 / amb10</term><description>Cap ambient CSS-loop repaints at that many frames per second. The governor does not overwrite it. Last of the four wins. noloops still freezes the loops entirely.</description></item>
    /// <item><term>rt16</term><description>Shadow accum is ARGBHalf instead of RFloat. os.parity.exp or os.parity.boot.exp applies it before the first canvas, including while a bench is set. A bench cell that flips it rebuilds the canvases.</description></item>
    /// <item><term>os.parity.boot.exp</term><description>Creation-time flags, read once. rt16 here stays on for the whole process, including bench cells that do not name it.</description></item>
    /// <item><term>os.parity.bench</term><description>auto, or one of home, maps, daily, shop, achievements, game, pause, tabs, gamestart, pauseopen. Each cell closes modals, returns home, and reloads level 1 on a throwaway profile. The governor does not read or write the scale pref.</description></item>
    /// <item><term>os.parity.bench.secs</term><description>Measured window length. Default 12. The first 2 seconds of each idle window are warm-up and are not counted. Tabs and gamestart dwell 3 seconds. pauseopen dwells 2 seconds.</description></item>
    /// <item><term>os.parity.bench.exps</term><description>Semicolon-separated flag sets, such as none;rt16;noloops;amb15. Replaces the default config list for every scenario in the run.</description></item>
    /// </list>
    /// Bench matrix: home, game, tabs, and gamestart use none, noshadow, nobackdrop, noloops, nocache, scale1.5, scale1, f30.
    /// Game also runs nogamehud, noimgui, nogamefx. Maps, pause, pauseopen, daily, shop, and achievements use none, nobackdrop, scale1.
    /// auto runs home, maps, game, pause, tabs, gamestart, pauseopen. A single mode runs that scenario's configs.
    /// Every cell checks the chrome page id before measuring. Pause requires an id that starts with modal_pause.
    /// A mismatch logs [Bench] state mismatch and retries once from a clean home. Game, pause, gamestart, and pauseopen keep the sector from opening a debrief or defeat modal.
    /// gamestart logs the Home to game click. pauseopen logs the pause click. Tabs logs each dock click from Home.
    /// Each window logs <c>[Bench] scenario= exp= fps= avg_ms= p50= p95= max= over50= gpu_ms= submits= backdrop= shadows= cachebuilds=</c>.
    /// <c>gpu_ms</c> is <c>avg/max</c> or <c>na</c>. <c>backdrop</c> is <c>exact/gpu</c>.
    /// Tabs, gamestart, and pauseopen also log <c>[Bench] switch scenario= exp= from= to= ms= over100= worst=</c>.
    /// <c>ms</c> is the time from the click to the next frame. <c>over100</c> and <c>worst</c> cover that present frame plus the following dwell. The return to Home between tab stops is not a measured switch.
    /// The run ends with <c>[Bench] done</c>.
    /// </summary>
    public sealed class DevicePerf
    {
        public const string PrefKey = "os.parity.log";
        public const float ReportIntervalSeconds = 1f;
        /// <summary>Same threshold as <see cref="QualityGovernor.SlowSeconds"/>.</summary>
        public const float SlowSeconds = 0.022f;
        public const float JankSeconds = 0.050f;
        /// <summary>A single frame longer than this is a hitch. Not the governor's 1.5 s ignore window.</summary>
        public const float FrameHitchSeconds = 0.080f;
        public const int Capacity = 240;

        public enum Phase
        {
            Update = 0,
            Prewarm = 1,
            Draw = 2,
            Sim = 3,
            Submit = 4,
            Gui = 5
        }

        public const int PhaseCount = 6;

        public struct Stats
        {
            public int Frames;
            public float ElapsedSeconds;
            public float Fps;
            public float AvgMs;
            public float P50;
            public float P95;
            public float P99;
            public float MaxMs;
            public int Over22;
            public int Over50;
        }

        public struct View
        {
            public string Page;
            public double Scale;
            public double Cap;
            public double Fit;
        }

        public struct Counters
        {
            public int Gc0;
            public long GcMemory;
            public long Allocated;
            public long Mono;
            public long Reserved;
            public long Resident;
            public long Budget;
            public int MemoryMb;
            public string Thermal;
        }

        public struct Gpu
        {
            public bool Available;
            public float Avg;
            public float Max;
        }

        public struct Paint
        {
            public int Submits;
            public int Commands;
            public int Draws;
            public int BackdropExact;
            public int BackdropGpu;
            public int Shadows;
            public int Blits;
            public int CacheBuilds;
            public int Edges;
            public int ShadowScopeHit;
            public int ShadowLowHit;
            public int ShadowCacheHit;
            public int ShadowMissClip;
            public int ShadowMissFmt;
            public int ShadowMissSmall;
            public int BackdropCacheHit;
            public int BackdropCacheMiss;
            public int RoundShadow;
            public int DirtyFrames;
            public int DirtyFull;
            public int DirtyFullCold;
            public int DirtyFullPage;
            public int DirtyFullArea;
            public int DirtyAreaSum;
            public int ShapeRect, ShapeCirc, ShapeEll, ShapeRr, ShapeStroke, ShapeOther;
            public int EdgeRect, EdgeCirc, EdgeEll, EdgeRr, EdgeStroke, EdgeOther;
            public int GameRect, GameCirc, GameEll, GameRr, GameStroke, GameOther;
            public int GameEdgeRect, GameEdgeCirc, GameEdgeEll, GameEdgeRr, GameEdgeStroke, GameEdgeOther;
            public int FastRect, FastCirc, FastEll, FastRr, FastLine, FastArc;
            public int FastBatch, FastBatchInst;
            public int GpuAlloc, ReadPx;
            public int[] SecDraw, SecEdge;
            public int RoundMissXform, RoundMissInset, RoundMissText, RoundMissShader;
            public int PrewarmBake;
            public string Wide;
            public int WidePct;
        }

        public struct Hitch
        {
            public bool Emit;
            public float Ms;
            public string Page;
            public float UpdateMs;
            public float PrewarmMs;
            public float DrawMs;
            public float SimMs;
            public float SubmitMs;
            public float GuiMs;
            public int Alloc;
            public int ReadPx;
            public int Gc0;
            public int Bake;
        }

        readonly float[] samples = new float[Capacity];
        readonly float[] sort = new float[Capacity];
        readonly long[] frameTicks = new long[PhaseCount];
        readonly StringBuilder line = new StringBuilder(512);

        bool enabled;
        bool haveFrame;
        int count;
        int total;
        int over22;
        int over50;
        double sumMs;
        float maxMs;
        int gpuN;
        double gpuSum;
        float gpuMax;
        float windowStart = float.NaN;
        string framePage = "-";

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                if (enabled == value) return;
                enabled = value;
                if (!value) Reset();
                else windowStart = float.NaN;
            }
        }

        public static bool IsEnabled(int pref)
        {
            return pref == 1;
        }

        public static bool IsHitch(float unscaledDeltaSeconds)
        {
            return unscaledDeltaSeconds > FrameHitchSeconds;
        }

        public static bool ShouldLogGovernor(bool steppedDown, bool steppedUp)
        {
            return steppedDown || steppedUp;
        }

        public static string GovernorReason(bool steppedDown)
        {
            return steppedDown ? "sustained" : "comfort";
        }

        /// <summary>
        /// Product page id. A live <paramref name="currentPage"/> wins (home, maps, game).
        /// A <c>modal_</c> template stays intact so a pause sheet is not reported as the game tab.
        /// </summary>
        public static string PageId(string currentPage, string templateId)
        {
            if (!string.IsNullOrEmpty(templateId) && templateId.StartsWith("modal_", StringComparison.Ordinal))
                return templateId;
            if (!string.IsNullOrEmpty(currentPage)) return currentPage;
            if (string.IsNullOrEmpty(templateId)) return "-";
            int us = templateId.IndexOf('_');
            if (us <= 0) return templateId;
            return templateId.Substring(0, us);
        }

        public static float Percentile(float[] sortedAscending, int n, double p)
        {
            if (sortedAscending == null || n <= 0) return 0f;
            if (n > sortedAscending.Length) n = sortedAscending.Length;
            int rank = (int)Math.Ceiling(n * p) - 1;
            if (rank < 0) rank = 0;
            if (rank >= n) rank = n - 1;
            return sortedAscending[rank];
        }

        public static float TicksToMs(long ticks)
        {
            if (ticks <= 0) return 0f;
            return (float)(ticks * 1000.0 / Stopwatch.Frequency);
        }

        public static void SwitchDurations(long startTicks, long nowTicks, long bakeAtStart, long bakeNow, long loadTicks, out float ms, out float bakeMs)
        {
            long wall = nowTicks - startTicks;
            if (wall < 0) wall = 0;
            long bake = (bakeNow - bakeAtStart) + loadTicks;
            if (bake < 0) bake = 0;
            ms = TicksToMs(wall);
            bakeMs = TicksToMs(bake);
        }

        public void Reset()
        {
            enabled = false;
            haveFrame = false;
            count = 0;
            total = 0;
            over22 = 0;
            over50 = 0;
            sumMs = 0;
            maxMs = 0f;
            gpuN = 0;
            gpuSum = 0;
            gpuMax = 0f;
            windowStart = float.NaN;
            framePage = "-";
            for (int i = 0; i < PhaseCount; i++) frameTicks[i] = 0;
        }

        public void SetPage(string page)
        {
            framePage = string.IsNullOrEmpty(page) ? "-" : page;
        }

        public float AvgMs()
        {
            if (!enabled || total <= 0) return 0f;
            return (float)(sumMs / total);
        }

        /// <summary>
        /// Start of a frame. <paramref name="unscaledDeltaSeconds"/> is the gap that just ended
        /// (the previous frame). A hitch uses that gap plus the phases recorded for that frame.
        /// </summary>
        public Hitch BeginFrame(float unscaledDeltaSeconds, float now)
        {
            if (!enabled) return default;
            if (float.IsNaN(windowStart)) windowStart = now;
            var hitch = new Hitch();
            if (haveFrame)
            {
                if (unscaledDeltaSeconds > FrameHitchSeconds)
                {
                    hitch.Emit = true;
                    float ms = unscaledDeltaSeconds > 0f ? unscaledDeltaSeconds * 1000f : 0f;
                    hitch.Ms = ms;
                    hitch.Page = framePage ?? "-";
                    hitch.UpdateMs = TicksToMs(frameTicks[(int)Phase.Update]);
                    hitch.PrewarmMs = TicksToMs(frameTicks[(int)Phase.Prewarm]);
                    hitch.DrawMs = TicksToMs(frameTicks[(int)Phase.Draw]);
                    hitch.SimMs = TicksToMs(frameTicks[(int)Phase.Sim]);
                    hitch.SubmitMs = TicksToMs(frameTicks[(int)Phase.Submit]);
                    hitch.GuiMs = TicksToMs(frameTicks[(int)Phase.Gui]);
                }
                Push(unscaledDeltaSeconds);
            }
            for (int i = 0; i < PhaseCount; i++) frameTicks[i] = 0;
            haveFrame = true;
            return hitch;
        }

        public void AddGpu(float ms)
        {
            if (!enabled || !(ms > 0f)) return;
            gpuN++;
            gpuSum += ms;
            if (ms > gpuMax) gpuMax = ms;
        }

        public Gpu TakeGpu()
        {
            var gpu = new Gpu();
            if (gpuN > 0)
            {
                gpu.Available = true;
                gpu.Avg = (float)(gpuSum / gpuN);
                gpu.Max = gpuMax;
            }
            gpuN = 0;
            gpuSum = 0;
            gpuMax = 0f;
            return gpu;
        }

        public void AddTicks(Phase phase, long ticks)
        {
            if (!enabled || ticks == 0) return;
            int i = (int)phase;
            if (i < 0 || i >= PhaseCount) return;
            frameTicks[i] += ticks;
        }

        public bool TrySnapshot(float now, out Stats stats)
        {
            stats = default;
            if (!enabled) return false;
            if (float.IsNaN(windowStart))
            {
                windowStart = now;
                return false;
            }
            float elapsed = now - windowStart;
            if (elapsed < ReportIntervalSeconds) return false;
            int n = count;
            float p50 = 0f, p95 = 0f, p99 = 0f;
            if (n > 0)
            {
                for (int i = 0; i < n; i++) sort[i] = samples[i];
                Array.Sort(sort, 0, n);
                p50 = Percentile(sort, n, 0.50);
                p95 = Percentile(sort, n, 0.95);
                p99 = Percentile(sort, n, 0.99);
            }
            if (elapsed < 1e-4f) elapsed = 1e-4f;
            stats.Frames = total;
            stats.ElapsedSeconds = elapsed;
            stats.Fps = total > 0 ? total / elapsed : 0f;
            stats.AvgMs = total > 0 ? (float)(sumMs / total) : 0f;
            stats.P50 = p50;
            stats.P95 = p95;
            stats.P99 = p99;
            stats.MaxMs = maxMs;
            stats.Over22 = over22;
            stats.Over50 = over50;
            count = 0;
            total = 0;
            over22 = 0;
            over50 = 0;
            sumMs = 0;
            maxMs = 0f;
            windowStart = now;
            return true;
        }

        public string FormatSummary(in Stats stats, in View view, in Counters counters, in Gpu gpu, in Paint paint)
        {
            line.Length = 0;
            line.Append("[Perf] page=");
            line.Append(string.IsNullOrEmpty(view.Page) ? "-" : view.Page);
            line.Append(" scale=");
            AppendTenths(line, view.Scale);
            line.Append(" cap=");
            AppendTenths(line, view.Cap);
            line.Append(" fit=");
            AppendTenths(line, view.Fit);
            line.Append(" frames=");
            line.Append(stats.Frames);
            line.Append(" fps=");
            AppendTenths(line, stats.Fps);
            line.Append(" avg=");
            AppendTenths(line, stats.AvgMs);
            line.Append(" p50=");
            AppendTenths(line, stats.P50);
            line.Append(" p95=");
            AppendTenths(line, stats.P95);
            line.Append(" p99=");
            AppendTenths(line, stats.P99);
            line.Append(" max=");
            AppendTenths(line, stats.MaxMs);
            line.Append(" over22=");
            line.Append(stats.Over22);
            line.Append(" over50=");
            line.Append(stats.Over50);
            line.Append(" gc0=");
            line.Append(counters.Gc0);
            line.Append(" gcm=");
            line.Append(counters.GcMemory);
            line.Append(" alloc=");
            line.Append(counters.Allocated);
            line.Append(" mono=");
            line.Append(counters.Mono);
            line.Append(" reserved=");
            line.Append(counters.Reserved);
            line.Append(" resident=");
            line.Append(counters.Resident);
            line.Append(" budget=");
            line.Append(counters.Budget);
            line.Append(" mem=");
            line.Append(counters.MemoryMb);
            if (!string.IsNullOrEmpty(counters.Thermal))
            {
                line.Append(" thermal=");
                line.Append(counters.Thermal);
            }
            line.Append(" gpu_ms=");
            if (!gpu.Available) line.Append("na");
            else
            {
                AppendTenths(line, gpu.Avg);
                line.Append('/');
                AppendTenths(line, gpu.Max);
            }
            line.Append(" submits=");
            line.Append(paint.Submits);
            line.Append(" cmds=");
            line.Append(paint.Commands);
            line.Append(" draws=");
            line.Append(paint.Draws);
            line.Append(" backdrop=");
            line.Append(paint.BackdropExact);
            line.Append('/');
            line.Append(paint.BackdropGpu);
            line.Append(" shadows=");
            line.Append(paint.Shadows);
            line.Append(" blits=");
            line.Append(paint.Blits);
            line.Append(" cachebuilds=");
            line.Append(paint.CacheBuilds);
            line.Append(" edges=");
            line.Append(paint.Edges);
            line.Append(" opt ss=");
            line.Append(paint.ShadowScopeHit);
            line.Append(" sl=");
            line.Append(paint.ShadowLowHit);
            line.Append(" scache=");
            line.Append(paint.ShadowCacheHit);
            line.Append(" missclip=");
            line.Append(paint.ShadowMissClip);
            line.Append(" missfmt=");
            line.Append(paint.ShadowMissFmt);
            line.Append(" misssmall=");
            line.Append(paint.ShadowMissSmall);
            line.Append(" bd=");
            line.Append(paint.BackdropCacheHit);
            line.Append('/');
            line.Append(paint.BackdropCacheMiss);
            line.Append(" rr=");
            line.Append(paint.RoundShadow);
            line.Append(" dirty=");
            line.Append(paint.DirtyFrames);
            line.Append('/');
            line.Append(paint.DirtyFull);
            line.Append(" cold=");
            line.Append(paint.DirtyFullCold);
            line.Append(" fullpage=");
            line.Append(paint.DirtyFullPage);
            line.Append(" area=");
            line.Append(paint.DirtyFullArea);
            if (paint.DirtyFrames > 0)
            {
                line.Append(" areapct=");
                line.Append(paint.DirtyAreaSum / paint.DirtyFrames);
            }
            line.Append(" shp=");
            Append6(line, paint.ShapeRect, paint.ShapeCirc, paint.ShapeEll, paint.ShapeRr, paint.ShapeStroke, paint.ShapeOther);
            line.Append(" se=");
            Append6(line, paint.EdgeRect, paint.EdgeCirc, paint.EdgeEll, paint.EdgeRr, paint.EdgeStroke, paint.EdgeOther);
            line.Append(" gshp=");
            Append6(line, paint.GameRect, paint.GameCirc, paint.GameEll, paint.GameRr, paint.GameStroke, paint.GameOther);
            line.Append(" gse=");
            Append6(line, paint.GameEdgeRect, paint.GameEdgeCirc, paint.GameEdgeEll, paint.GameEdgeRr, paint.GameEdgeStroke, paint.GameEdgeOther);
            line.Append(" fast=");
            Append6(line, paint.FastRect, paint.FastCirc, paint.FastEll, paint.FastRr, paint.FastLine, paint.FastArc);
            line.Append(" sec=");
            AppendSec(line, paint.SecDraw, paint.SecEdge);
            line.Append(" batch=");
            line.Append(paint.FastBatch);
            line.Append('/');
            line.Append(paint.FastBatchInst);
            line.Append(" alloc=");
            line.Append(paint.GpuAlloc);
            line.Append(" readpx=");
            line.Append(paint.ReadPx);
            line.Append(" rrwhy=");
            line.Append(paint.RoundMissXform);
            line.Append('/');
            line.Append(paint.RoundMissInset);
            line.Append('/');
            line.Append(paint.RoundMissText);
            line.Append('/');
            line.Append(paint.RoundMissShader);
            line.Append(" bake=");
            line.Append(paint.PrewarmBake);
            line.Append(" wide=");
            if (string.IsNullOrEmpty(paint.Wide)) line.Append('-');
            else
            {
                line.Append(paint.Wide);
                line.Append('@');
                line.Append(paint.WidePct);
            }
            return line.ToString();
        }

        public string FormatHitch(in Hitch hitch)
        {
            line.Length = 0;
            line.Append("[Perf] hitch ms=");
            AppendTenths(line, hitch.Ms);
            line.Append(" page=");
            line.Append(string.IsNullOrEmpty(hitch.Page) ? "-" : hitch.Page);
            line.Append(" update=");
            AppendTenths(line, hitch.UpdateMs);
            line.Append(" prewarm=");
            AppendTenths(line, hitch.PrewarmMs);
            line.Append(" draw=");
            AppendTenths(line, hitch.DrawMs);
            line.Append(" sim=");
            AppendTenths(line, hitch.SimMs);
            line.Append(" submit=");
            AppendTenths(line, hitch.SubmitMs);
            line.Append(" gui=");
            AppendTenths(line, hitch.GuiMs);
            line.Append(" alloc=");
            line.Append(hitch.Alloc);
            line.Append(" readpx=");
            line.Append(hitch.ReadPx);
            line.Append(" gc=");
            line.Append(hitch.Gc0);
            line.Append(" bake=");
            line.Append(hitch.Bake);
            return line.ToString();
        }

        public string FormatPage(string from, string to, float ms, float bakeMs)
        {
            line.Length = 0;
            line.Append("[Perf] page ");
            line.Append(string.IsNullOrEmpty(from) ? "-" : from);
            line.Append(" -> ");
            line.Append(string.IsNullOrEmpty(to) ? "-" : to);
            line.Append(" ms=");
            AppendTenths(line, ms);
            line.Append(" bake_ms=");
            AppendTenths(line, bakeMs);
            return line.ToString();
        }

        public string FormatGovernor(bool steppedDown, double fromScale, double toScale, float p95Ms, float avgMs, string page)
        {
            line.Length = 0;
            line.Append("[Perf] governor step=");
            line.Append(steppedDown ? "down" : "up");
            line.Append(" from=");
            AppendTenths(line, fromScale);
            line.Append(" to=");
            AppendTenths(line, toScale);
            line.Append(" scale=");
            AppendTenths(line, toScale);
            line.Append(" reason=");
            line.Append(GovernorReason(steppedDown));
            line.Append(" page=");
            line.Append(string.IsNullOrEmpty(page) ? "-" : page);
            line.Append(" p95=");
            AppendTenths(line, p95Ms);
            line.Append(" avg=");
            AppendTenths(line, avgMs);
            return line.ToString();
        }

        void Push(float unscaledDeltaSeconds)
        {
            float ms = unscaledDeltaSeconds > 0f ? unscaledDeltaSeconds * 1000f : 0f;
            total++;
            if (unscaledDeltaSeconds > SlowSeconds) over22++;
            if (unscaledDeltaSeconds > JankSeconds) over50++;
            if (ms > maxMs) maxMs = ms;
            sumMs += ms;
            if (count >= Capacity) return;
            samples[count++] = ms;
        }

        static void AppendSec(StringBuilder line, int[] draws, int[] edges)
        {
            for (int i = 0; i < 8; i++)
            {
                if (i > 0) line.Append(',');
                int d = draws != null && i < draws.Length ? draws[i] : 0;
                int e = edges != null && i < edges.Length ? edges[i] : 0;
                line.Append(d);
                line.Append('/');
                line.Append(e);
            }
        }

        static void Append6(StringBuilder line, int a, int b, int c, int d, int e, int f)
        {
            line.Append(a);
            line.Append('/');
            line.Append(b);
            line.Append('/');
            line.Append(c);
            line.Append('/');
            line.Append(d);
            line.Append('/');
            line.Append(e);
            line.Append('/');
            line.Append(f);
        }

        static void AppendTenths(StringBuilder sb, double value)
        {
            if (value < 0d || double.IsNaN(value) || double.IsInfinity(value)) value = 0d;
            long tenths = (long)Math.Floor(value * 10.0 + 0.5);
            long whole = tenths / 10;
            long frac = tenths % 10;
            sb.Append(whole);
            sb.Append('.');
            sb.Append((char)('0' + (int)frac));
        }
    }
}
