using System;
using System.Diagnostics;
using System.Text;

namespace OrbitalSalvage.Viewer
{
    /// <summary>
    /// On-device frame stats. Callers pass times and counters; nothing here touches the GPU,
    /// <c>JsRandom</c>, or the quality governor. The viewer logs one <c>[Perf]</c> line per second
    /// when PlayerPrefs <see cref="PrefKey"/> is 1, and stays silent otherwise.
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

        public string FormatSummary(in Stats stats, in View view, in Counters counters)
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
