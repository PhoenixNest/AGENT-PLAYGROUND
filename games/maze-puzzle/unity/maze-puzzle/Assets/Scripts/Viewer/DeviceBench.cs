using System;
using System.Collections.Generic;
using System.Text;

namespace OrbitalSalvage.Viewer
{
    /// <summary>
    /// Pure bench schedule and window maths. The viewer feeds time and page ids; nothing here
    /// touches Unity, the GPU, or <c>JsRandom</c>.
    /// </summary>
    public static class DeviceBench
    {
        public const string PrefKey = "os.parity.bench";
        public const string SecsPrefKey = "os.parity.bench.secs";
        public const float DefaultWindowSeconds = 12f;
        public const float WarmupSeconds = 2f;
        public const float TabDwellSeconds = 3f;
        public const float NavTimeoutSeconds = 8f;
        public const float Over100Seconds = 0.1f;
        public const int SampleCapacity = 4096;

        static readonly string[] Heavy =
        {
            "none", "noshadow", "nobackdrop", "noloops", "nocache", "scale1.5", "scale1", "f30"
        };
        static readonly string[] GameExtra = { "nogamehud", "noimgui", "nogamefx" };
        static readonly string[] Light = { "none", "nobackdrop", "scale1" };
        static readonly string[] AutoScenarios = { "home", "maps", "game", "pause", "tabs" };

        public enum Op
        {
            Apply = 0,
            Click = 1,
            Warmup = 2,
            Sample = 3,
            Dwell = 4,
            Report = 5
        }

        /// <summary>
        /// <see cref="A"/> is the exp, click id, or scenario.
        /// <see cref="B"/> is the page a click expects, or the exp on a report.
        /// <see cref="C"/> is the page a measured click leaves.
        /// A measured click is followed by a <see cref="Op.Dwell"/> of <see cref="Seconds"/>
        /// (zero closes the switch on the arrival frame).
        /// </summary>
        public struct Step
        {
            public Op Op;
            public string A;
            public string B;
            public string C;
            public float Seconds;
            public bool Switch;
        }

        public struct TickIn
        {
            public float Now;
            public string Page;
            public float Dt;
            /// <summary>Negative when the viewer has no click outstanding.</summary>
            public float PresentMs;
            /// <summary>Negative when this frame has no GPU time.</summary>
            public float GpuMs;
            public int Submits;
            public int BackdropExact;
            public int BackdropGpu;
            public int Shadows;
            public int CacheBuilds;
        }

        public struct Action
        {
            public bool Apply;
            public string Exp;
            public string Scenario;
            public bool ResetGovernor;
            public bool Click;
            public string ClickId;
            public bool Done;
        }

        public static bool TryMode(string pref, out string mode)
        {
            mode = null;
            if (string.IsNullOrEmpty(pref)) return false;
            string trimmed = pref.Trim();
            if (trimmed.Length == 0) return false;
            if (string.Equals(trimmed, "auto", StringComparison.OrdinalIgnoreCase))
            {
                mode = "auto";
                return true;
            }
            if (IsScenario(trimmed))
            {
                mode = trimmed.ToLowerInvariant();
                return true;
            }
            return false;
        }

        public static bool IsScenario(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            switch (name.Trim().ToLowerInvariant())
            {
                case "home":
                case "maps":
                case "daily":
                case "shop":
                case "achievements":
                case "game":
                case "pause":
                case "tabs":
                    return true;
                default:
                    return false;
            }
        }

        public static string[] Configs(string scenario)
        {
            switch (scenario)
            {
                case "maps":
                case "pause":
                case "daily":
                case "shop":
                case "achievements":
                    return Copy(Light);
                case "game":
                    var both = new string[Heavy.Length + GameExtra.Length];
                    Array.Copy(Heavy, both, Heavy.Length);
                    Array.Copy(GameExtra, 0, both, Heavy.Length, GameExtra.Length);
                    return both;
                default:
                    return Copy(Heavy);
            }
        }

        public static Step[] Build(string mode)
        {
            return Build(mode, DefaultWindowSeconds, WarmupSeconds, TabDwellSeconds);
        }

        public static Step[] Build(string mode, float windowSeconds, float warmupSeconds, float tabDwellSeconds)
        {
            if (windowSeconds < 0.05f) windowSeconds = 0.05f;
            if (warmupSeconds < 0f) warmupSeconds = 0f;
            if (tabDwellSeconds < 0f) tabDwellSeconds = 0f;
            var steps = new List<Step>(128);
            string cursor = "home";
            if (mode == "auto")
            {
                for (int i = 0; i < AutoScenarios.Length; i++)
                    cursor = AddScenario(steps, AutoScenarios[i], cursor, windowSeconds, warmupSeconds, tabDwellSeconds);
            }
            else
                AddScenario(steps, mode, cursor, windowSeconds, warmupSeconds, tabDwellSeconds);
            return steps.ToArray();
        }

        /// <summary>Page a navigation click lands on. Empty when <paramref name="clickId"/> is unknown.</summary>
        public static string PageAfterClick(string clickId)
        {
            switch (clickId)
            {
                case "btn-open-maps-page": return "maps";
                case "btn-open-daily-shift": return "daily";
                case "btn-open-shop-page": return "shop";
                case "btn-open-trophies-page": return "achievements";
                case "btn-back-maps":
                case "btn-back-daily":
                case "btn-back-shop":
                case "btn-back-achievements":
                case "btn-pause-home":
                    return "home";
                case "btn-home-play":
                case "btn-pause-resume":
                    return "game";
                case "btn-game-pause": return "pause";
                default: return "";
            }
        }

        static string AddScenario(List<Step> steps, string scenario, string cursor, float window, float warmup, float dwell)
        {
            string[] configs = Configs(scenario);
            string entry = scenario == "tabs" ? "home" : scenario;
            for (int c = 0; c < configs.Length; c++)
            {
                steps.Add(new Step { Op = Op.Apply, A = configs[c], B = scenario });
                cursor = AddRoute(steps, cursor, entry, false, 0f);
                steps.Add(new Step { Op = Op.Warmup, Seconds = warmup });
                if (scenario == "tabs")
                    cursor = AddTabs(steps, dwell);
                else
                {
                    steps.Add(new Step { Op = Op.Sample, Seconds = window });
                    cursor = entry;
                }
                steps.Add(new Step { Op = Op.Report, A = scenario, B = configs[c] });
            }
            return cursor;
        }

        static string AddTabs(List<Step> steps, float dwell)
        {
            AddRoute(steps, "home", "maps", true, dwell);
            AddRoute(steps, "maps", "home", true, 0f);
            AddRoute(steps, "home", "daily", true, dwell);
            AddRoute(steps, "daily", "home", true, 0f);
            AddRoute(steps, "home", "shop", true, dwell);
            AddRoute(steps, "shop", "home", true, 0f);
            AddRoute(steps, "home", "achievements", true, dwell);
            AddRoute(steps, "achievements", "home", true, dwell);
            return "home";
        }

        static string AddRoute(List<Step> steps, string from, string to, bool measureLast, float dwellLast)
        {
            if (from == to) return to;
            var ids = new List<string>(4);
            var pages = new List<string>(4);
            string origin = from;
            if (from == "game" && to == "pause")
            {
                ids.Add("btn-game-pause");
                pages.Add("pause");
            }
            else if (from == "pause" && to == "game")
            {
                ids.Add("btn-pause-resume");
                pages.Add("game");
            }
            else if (to == "home")
                AppendHome(ids, pages, from);
            else
            {
                if (from != "home")
                    AppendHome(ids, pages, from);
                AppendFromHome(ids, pages, to);
            }
            for (int i = 0; i < ids.Count; i++)
            {
                bool last = i == ids.Count - 1;
                bool measure = measureLast && last;
                steps.Add(new Step
                {
                    Op = Op.Click,
                    A = ids[i],
                    B = pages[i],
                    C = i == 0 ? origin : pages[i - 1],
                    Switch = measure
                });
                if (measure)
                {
                    steps.Add(new Step
                    {
                        Op = Op.Dwell,
                        Seconds = dwellLast,
                        Switch = true,
                        A = i == 0 ? from : pages[i - 1],
                        B = pages[i]
                    });
                }
            }
            return pages.Count == 0 ? from : pages[pages.Count - 1];
        }

        static void AppendHome(List<string> ids, List<string> pages, string from)
        {
            switch (from)
            {
                case "maps":
                    ids.Add("btn-back-maps");
                    pages.Add("home");
                    break;
                case "daily":
                    ids.Add("btn-back-daily");
                    pages.Add("home");
                    break;
                case "shop":
                    ids.Add("btn-back-shop");
                    pages.Add("home");
                    break;
                case "achievements":
                    ids.Add("btn-back-achievements");
                    pages.Add("home");
                    break;
                case "game":
                    ids.Add("btn-game-pause");
                    pages.Add("pause");
                    ids.Add("btn-pause-home");
                    pages.Add("home");
                    break;
                case "pause":
                    ids.Add("btn-pause-home");
                    pages.Add("home");
                    break;
            }
        }

        static void AppendFromHome(List<string> ids, List<string> pages, string to)
        {
            switch (to)
            {
                case "maps":
                    ids.Add("btn-open-maps-page");
                    pages.Add("maps");
                    break;
                case "daily":
                    ids.Add("btn-open-daily-shift");
                    pages.Add("daily");
                    break;
                case "shop":
                    ids.Add("btn-open-shop-page");
                    pages.Add("shop");
                    break;
                case "achievements":
                    ids.Add("btn-open-trophies-page");
                    pages.Add("achievements");
                    break;
                case "game":
                    ids.Add("btn-home-play");
                    pages.Add("game");
                    break;
                case "pause":
                    ids.Add("btn-home-play");
                    pages.Add("game");
                    ids.Add("btn-game-pause");
                    pages.Add("pause");
                    break;
            }
        }

        static string[] Copy(string[] src)
        {
            var dst = new string[src.Length];
            Array.Copy(src, dst, src.Length);
            return dst;
        }
    }

    /// <summary>Fixed-capacity frame window. Allocated with the player, not per frame.</summary>
    public sealed class BenchWindow
    {
        readonly float[] samples = new float[DeviceBench.SampleCapacity];
        readonly float[] sort = new float[DeviceBench.SampleCapacity];
        int count;
        int frames;
        double sumMs;
        float maxMs;
        int over50;
        int gpuN;
        double gpuSum;
        float gpuMax;
        int submits;
        int backdropExact;
        int backdropGpu;
        int shadows;
        int cacheBuilds;

        public int Frames => frames;

        public void Clear()
        {
            count = 0;
            frames = 0;
            sumMs = 0;
            maxMs = 0f;
            over50 = 0;
            gpuN = 0;
            gpuSum = 0;
            gpuMax = 0f;
            submits = 0;
            backdropExact = 0;
            backdropGpu = 0;
            shadows = 0;
            cacheBuilds = 0;
        }

        public void Push(float dtSeconds, float gpuMs, int frameSubmits, int exact, int gpu, int frameShadows, int frameCaches)
        {
            if (dtSeconds <= 0f) return;
            float ms = dtSeconds * 1000f;
            frames++;
            sumMs += ms;
            if (ms > maxMs) maxMs = ms;
            if (dtSeconds > DevicePerf.JankSeconds) over50++;
            if (count < samples.Length) samples[count++] = ms;
            if (gpuMs > 0f)
            {
                gpuN++;
                gpuSum += gpuMs;
                if (gpuMs > gpuMax) gpuMax = gpuMs;
            }
            submits += frameSubmits;
            backdropExact += exact;
            backdropGpu += gpu;
            shadows += frameShadows;
            cacheBuilds += frameCaches;
        }

        public string Format(string scenario, string exp)
        {
            float elapsed = frames > 0 ? (float)(sumMs / 1000.0) : 0f;
            if (elapsed < 1e-4f) elapsed = frames > 0 ? 1e-4f : 1f;
            float fps = frames > 0 ? frames / elapsed : 0f;
            float avg = frames > 0 ? (float)(sumMs / frames) : 0f;
            float p50 = 0f, p95 = 0f;
            if (count > 0)
            {
                for (int i = 0; i < count; i++) sort[i] = samples[i];
                Array.Sort(sort, 0, count);
                p50 = DevicePerf.Percentile(sort, count, 0.50);
                p95 = DevicePerf.Percentile(sort, count, 0.95);
            }
            var sb = new StringBuilder(256);
            sb.Append("[Bench] scenario=");
            sb.Append(string.IsNullOrEmpty(scenario) ? "-" : scenario);
            sb.Append(" exp=");
            sb.Append(string.IsNullOrEmpty(exp) ? "none" : exp);
            sb.Append(" fps=");
            AppendTenths(sb, fps);
            sb.Append(" avg_ms=");
            AppendTenths(sb, avg);
            sb.Append(" p50=");
            AppendTenths(sb, p50);
            sb.Append(" p95=");
            AppendTenths(sb, p95);
            sb.Append(" max=");
            AppendTenths(sb, maxMs);
            sb.Append(" over50=");
            sb.Append(over50);
            sb.Append(" gpu_ms=");
            if (gpuN <= 0) sb.Append("na");
            else
            {
                AppendTenths(sb, gpuSum / gpuN);
                sb.Append('/');
                AppendTenths(sb, gpuMax);
            }
            sb.Append(" submits=");
            sb.Append(submits);
            sb.Append(" backdrop=");
            sb.Append(backdropExact);
            sb.Append('/');
            sb.Append(backdropGpu);
            sb.Append(" shadows=");
            sb.Append(shadows);
            sb.Append(" cachebuilds=");
            sb.Append(cacheBuilds);
            return sb.ToString();
        }

        public static string FormatSwitch(string scenario, string exp, string from, string to, float presentMs, int over100, float worstMs)
        {
            var sb = new StringBuilder(160);
            sb.Append("[Bench] switch scenario=");
            sb.Append(string.IsNullOrEmpty(scenario) ? "-" : scenario);
            sb.Append(" exp=");
            sb.Append(string.IsNullOrEmpty(exp) ? "none" : exp);
            sb.Append(" from=");
            sb.Append(string.IsNullOrEmpty(from) ? "-" : from);
            sb.Append(" to=");
            sb.Append(string.IsNullOrEmpty(to) ? "-" : to);
            sb.Append(" ms=");
            AppendTenths(sb, presentMs);
            sb.Append(" over100=");
            sb.Append(over100);
            sb.Append(" worst=");
            AppendTenths(sb, worstMs);
            return sb.ToString();
        }

        public static string FormatTimeout(string scenario, string exp, string clickId, string page)
        {
            var sb = new StringBuilder(128);
            sb.Append("[Bench] nav timeout scenario=");
            sb.Append(string.IsNullOrEmpty(scenario) ? "-" : scenario);
            sb.Append(" exp=");
            sb.Append(string.IsNullOrEmpty(exp) ? "none" : exp);
            sb.Append(" click=");
            sb.Append(string.IsNullOrEmpty(clickId) ? "-" : clickId);
            sb.Append(" page=");
            sb.Append(string.IsNullOrEmpty(page) ? "-" : page);
            return sb.ToString();
        }

        public static string DoneLine => "[Bench] done";

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

    /// <summary>
    /// Walks a <see cref="DeviceBench.Step"/> list. One click is issued per call.
    /// A page change is observed on a later call, after the viewer has presented it.
    /// </summary>
    public sealed class BenchPlayer
    {
        readonly DeviceBench.Step[] steps;
        readonly float timeout;
        readonly BenchWindow window = new BenchWindow();
        readonly string[] logs = new string[4];

        int index;
        float phaseStart;
        bool clickSent;
        float clickStart;
        bool collect;
        bool switchOpen;
        int over100;
        float worstMs;
        float presentMs;
        string switchFrom = "";
        string switchTo = "";
        string cellExp = "none";
        string cellScenario = "";
        bool finished;
        int logCount;

        public BenchPlayer(DeviceBench.Step[] steps, float navTimeoutSeconds)
        {
            this.steps = steps ?? Array.Empty<DeviceBench.Step>();
            timeout = navTimeoutSeconds > 0f ? navTimeoutSeconds : DeviceBench.NavTimeoutSeconds;
        }

        public bool Done => finished;
        public int LogCount => logCount;
        public string Log(int i) => logs[i];

        public DeviceBench.Action Tick(DeviceBench.TickIn inn)
        {
            logCount = 0;
            var act = new DeviceBench.Action();
            if (finished || steps.Length == 0)
            {
                Finish(ref act);
                return act;
            }

            string page = inn.Page ?? "";
            if (collect && inn.Dt > 0f)
            {
                window.Push(inn.Dt, inn.GpuMs, inn.Submits, inn.BackdropExact, inn.BackdropGpu, inn.Shadows, inn.CacheBuilds);
            }
            if (switchOpen && inn.Dt > 0f) Acc(inn.Dt);

            for (int guard = 0; guard < 16; guard++)
            {
                if (index >= steps.Length)
                {
                    Finish(ref act);
                    return act;
                }
                var step = steps[index];
                switch (step.Op)
                {
                    case DeviceBench.Op.Apply:
                        cellExp = string.IsNullOrEmpty(step.A) ? "none" : step.A;
                        cellScenario = step.B ?? "";
                        act.Apply = true;
                        act.Exp = cellExp;
                        act.Scenario = cellScenario;
                        act.ResetGovernor = true;
                        index++;
                        phaseStart = inn.Now;
                        continue;
                    case DeviceBench.Op.Click:
                        if (!clickSent)
                        {
                            act.Click = true;
                            act.ClickId = step.A;
                            clickSent = true;
                            clickStart = inn.Now;
                            return act;
                        }
                        if (page == step.B)
                        {
                            float ms = inn.PresentMs >= 0f ? inn.PresentMs : (inn.Now - clickStart) * 1000f;
                            clickSent = false;
                            index++;
                            phaseStart = inn.Now;
                            if (step.Switch)
                            {
                                switchOpen = true;
                                switchFrom = step.C ?? "";
                                switchTo = step.B ?? "";
                                presentMs = ms;
                                over100 = 0;
                                worstMs = 0f;
                                Acc(inn.Dt);
                            }
                            if (index < steps.Length && steps[index].Op == DeviceBench.Op.Click)
                                return act;
                            continue;
                        }
                        if (inn.Now - clickStart >= timeout)
                        {
                            Add(BenchWindow.FormatTimeout(cellScenario, cellExp, step.A, page));
                            clickSent = false;
                            SkipCell();
                            return act;
                        }
                        return act;
                    case DeviceBench.Op.Warmup:
                        if (inn.Now - phaseStart < step.Seconds) return act;
                        collect = true;
                        index++;
                        phaseStart = inn.Now;
                        continue;
                    case DeviceBench.Op.Sample:
                        if (inn.Now - phaseStart < step.Seconds) return act;
                        index++;
                        phaseStart = inn.Now;
                        continue;
                    case DeviceBench.Op.Dwell:
                        if (step.Seconds > 0f && inn.Now - phaseStart < step.Seconds) return act;
                        if (step.Switch) CloseSwitch();
                        index++;
                        phaseStart = inn.Now;
                        if (index < steps.Length && steps[index].Op == DeviceBench.Op.Click)
                            return act;
                        continue;
                    case DeviceBench.Op.Report:
                        Add(window.Format(step.A, step.B));
                        window.Clear();
                        collect = false;
                        index++;
                        phaseStart = inn.Now;
                        continue;
                    default:
                        index++;
                        continue;
                }
            }
            return act;
        }

        void Acc(float dt)
        {
            if (dt > DeviceBench.Over100Seconds) over100++;
            float ms = dt > 0f ? dt * 1000f : 0f;
            if (ms > worstMs) worstMs = ms;
        }

        void CloseSwitch()
        {
            if (!switchOpen) return;
            switchOpen = false;
            Add(BenchWindow.FormatSwitch(cellScenario, cellExp, switchFrom, switchTo, presentMs, over100, worstMs));
        }

        void SkipCell()
        {
            switchOpen = false;
            collect = false;
            window.Clear();
            index++;
            while (index < steps.Length && steps[index].Op != DeviceBench.Op.Apply)
                index++;
        }

        void Finish(ref DeviceBench.Action act)
        {
            if (finished) { act.Done = true; return; }
            finished = true;
            act.Done = true;
            Add(BenchWindow.DoneLine);
        }

        void Add(string line)
        {
            if (line == null || logCount >= logs.Length) return;
            logs[logCount++] = line;
        }
    }
}
