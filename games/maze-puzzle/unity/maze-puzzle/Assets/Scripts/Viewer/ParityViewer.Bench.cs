using System.Diagnostics;
using System.Globalization;
using OrbitalSalvage.Render;
using UnityEngine;

namespace OrbitalSalvage.Viewer
{
    /// <summary>
    /// Product-boot experiment flags. Defaults stay off, so a launch with no prefs does not
    /// branch into a different paint, save, or frame-rate path.
    /// </summary>
    public static class DeviceFlags
    {
        public static DeviceExp Current;
        public static DeviceExp Manual;
        public static DeviceExp Boot;
        public static string BenchMode;
        public static string[] BenchExps;
        public static float BenchSeconds = DeviceBench.DefaultWindowSeconds;
        public static double CommandForced;
        public static bool RasterRebuild;

        static bool read;
        static bool cacheOff;
        static bool rateOn;

        public static void Read(double commandForced)
        {
            if (read) return;
            read = true;
            CommandForced = commandForced;
            Manual = DeviceExp.Parse(PlayerPrefs.GetString("os.parity.exp", ""));
            Boot = DeviceExp.Parse(PlayerPrefs.GetString(DeviceBench.BootExpPrefKey, ""));
            string modePref = PlayerPrefs.GetString(DeviceBench.PrefKey, "");
            if (!DeviceBench.TryMode(modePref, out string mode)) return;
            BenchMode = mode;
            BenchExps = DeviceBench.ParseExps(PlayerPrefs.GetString(DeviceBench.ExpsPrefKey, ""));
            float secs = PlayerPrefs.GetFloat(DeviceBench.SecsPrefKey, DeviceBench.DefaultWindowSeconds);
            if (secs < 1f) secs = DeviceBench.DefaultWindowSeconds;
            BenchSeconds = secs;
        }

        public static void Apply(DeviceExp exp, bool benchCell)
        {
            RasterRebuild = false;
            Current = exp;
            Canvas2D.SkipBlurredShadows = exp.NoShadow;
            Canvas2D.SkipGameFx = exp.NoGameFx;
            bool wantHalf = exp.Rt16 || Boot.Rt16;
            if (benchCell)
            {
                if (wantHalf != Canvas2D.HalfAccum)
                {
                    Canvas2D.HalfAccum = wantHalf;
                    RasterRebuild = true;
                }
            }
            else Canvas2D.HalfAccum = wantHalf;
            if (exp.NoBackdrop) HomeChrome.SimplifyEffects = true;
            if (exp.NoCache)
            {
                HomeChrome.UseLayerCache = false;
                cacheOff = true;
            }
            else if (cacheOff)
            {
                HomeChrome.UseLayerCache = true;
                cacheOff = false;
            }
            if (benchCell)
                PlayQuality.Forced = exp.Scale > 0d ? exp.Scale : CommandForced;
            else if (exp.Scale > 0d)
                PlayQuality.Forced = exp.Scale;
            else if (CommandForced > 0d)
                PlayQuality.Forced = CommandForced;

            if (exp.FrameRate > 0)
            {
                Application.targetFrameRate = exp.FrameRate;
                rateOn = true;
            }
            else if (rateOn)
            {
                Application.targetFrameRate = 60;
                rateOn = false;
            }
        }
    }

    public sealed partial class ParityViewer
    {
        bool benchSession;
        BenchPlayer benchPlayer;
        long benchRequestTicks;
        RenderStats.Snap benchPaintSnap;
        bool benchPaintReady;

        void DriveBench()
        {
            if (DeviceFlags.BenchMode == null || (benchPlayer != null && benchPlayer.Done)) return;
            if (benchPlayer == null)
            {
                if (app == null || app.CurrentPage != "home") return;
                var steps = DeviceBench.Build(
                    DeviceFlags.BenchMode,
                    DeviceFlags.BenchSeconds,
                    DeviceBench.WarmupSeconds,
                    DeviceBench.TabDwellSeconds,
                    DeviceFlags.BenchExps);
                benchPlayer = new BenchPlayer(steps, DeviceBench.NavTimeoutSeconds);
                benchPaintSnap = RenderStats.Read();
                benchPaintReady = true;
                UnityEngine.Debug.Log("[Bench] start mode=" + DeviceFlags.BenchMode
                    + " secs=" + DeviceFlags.BenchSeconds.ToString("0.###", CultureInfo.InvariantCulture));
            }

            var paintNow = RenderStats.Read();
            var delta = benchPaintReady ? RenderStats.Delta(paintNow, benchPaintSnap) : paintNow;
            benchPaintSnap = paintNow;
            benchPaintReady = true;
            string page = currentId;
            if (string.IsNullOrEmpty(page)) page = BenchPage();
            var inn = new DeviceBench.TickIn
            {
                Now = Time.unscaledTime,
                Page = page,
                Dt = Time.unscaledDeltaTime < 0f ? 0f : Time.unscaledDeltaTime,
                PresentMs = benchRequestTicks != 0 ? BenchElapsedMs() : -1f,
                GpuMs = benchGpu,
                Submits = delta.Submits,
                BackdropExact = delta.BackdropExact,
                BackdropGpu = delta.BackdropGpu,
                Shadows = delta.Shadows,
                CacheBuilds = delta.CacheBuilds
            };
            var act = benchPlayer.Tick(inn);
            if (act.ResetGovernor) governor.ResetSession();
            if (act.Apply)
            {
                DeviceFlags.Apply(DeviceExp.Parse(act.Exp), true);
                if (session != null) session.HoldOutcome = DeviceBench.HoldsPlay(act.Scenario);
                if (DeviceFlags.RasterRebuild) RebuildBenchRasters();
                int ambient = DeviceFlags.Current.AmbientFps;
                if (ambient > 0) ambientInterval = 1.0 / ambient;
            }
            for (int i = 0; i < benchPlayer.LogCount; i++)
                UnityEngine.Debug.Log(benchPlayer.Log(i));
            if (act.ResetState) BenchReset();
            if (act.Click) BenchClick(act.ClickId);
            if (act.Done)
            {
                benchRequestTicks = 0;
                if (session != null) session.HoldOutcome = false;
            }
        }

        void BenchReset()
        {
            if (app == null) return;
            app.PrepareBenchCell();
            appliedEpoch = -1;
            SyncApp(true);
        }

        void RebuildBenchRasters()
        {
            cache.ForEach((key, chrome) =>
            {
                if (chrome != null) chrome.RebuildRasterInPlace();
            });
            if (session != null && session.Renderer != null) session.Renderer.RebuildCanvas();
        }

        string BenchPage()
        {
            if (app == null) return "";
            if (app.ModalOpen("modal-pause")) return "pause";
            return app.CurrentPage ?? "";
        }

        float BenchElapsedMs()
        {
            long now = Stopwatch.GetTimestamp();
            long span = now - benchRequestTicks;
            if (span < 0) span = 0;
            return (float)(span * 1000.0 / Stopwatch.Frequency);
        }

        void BenchClick(string id)
        {
            benchRequestTicks = Stopwatch.GetTimestamp();
            if (currentId == null || app == null)
            {
                UnityEngine.Debug.Log("[Bench] miss click=" + id + " page=-");
                return;
            }
            string spec = Spec(currentId);
            if (!ParityPages.TryCenter(spec, id, out double x, out double y))
            {
                UnityEngine.Debug.Log("[Bench] miss click=" + id + " page=" + currentId);
                return;
            }
            ClickCss(x, y);
        }
    }
}
