using System.Diagnostics;
using NUnit.Framework;

namespace OrbitalSalvage.Viewer.Tests
{
    public class DevicePerfTests
    {
        [Test]
        public void Pref_IsOnOnlyWhenTheIntIsOne()
        {
            Assert.IsFalse(DevicePerf.IsEnabled(0));
            Assert.IsFalse(DevicePerf.IsEnabled(-1));
            Assert.IsFalse(DevicePerf.IsEnabled(2));
            Assert.IsTrue(DevicePerf.IsEnabled(1));
            Assert.AreEqual(QualityGovernor.SlowSeconds, DevicePerf.SlowSeconds);
            var off = new DevicePerf();
            Assert.IsFalse(off.Enabled);
        }

        [Test]
        public void Percentile_UsesCeilingRank()
        {
            var five = new[] { 10f, 20f, 30f, 40f, 50f };
            Assert.AreEqual(30f, DevicePerf.Percentile(five, 5, 0.50), 0.001f);
            Assert.AreEqual(50f, DevicePerf.Percentile(five, 5, 0.95), 0.001f);
            Assert.AreEqual(50f, DevicePerf.Percentile(five, 5, 0.99), 0.001f);
            Assert.AreEqual(10f, DevicePerf.Percentile(five, 1, 0.99), 0.001f);

            var four = new[] { 10f, 20f, 30f, 40f };
            Assert.AreEqual(20f, DevicePerf.Percentile(four, 4, 0.50), 0.001f);
            Assert.AreEqual(40f, DevicePerf.Percentile(four, 4, 0.95), 0.001f);
            Assert.AreEqual(40f, DevicePerf.Percentile(four, 4, 0.99), 0.001f);
            Assert.AreEqual(0f, DevicePerf.Percentile(four, 0, 0.95), 0.001f);
            Assert.AreEqual(0f, DevicePerf.Percentile(null, 4, 0.95), 0.001f);
        }

        [Test]
        public void Window_CountsThresholdsAndThenResets()
        {
            var perf = new DevicePerf { Enabled = true };
            Assert.IsFalse(perf.BeginFrame(0.5f, 0f).Emit, "the first gap has no finished phases");
            Assert.IsFalse(perf.TrySnapshot(0.5f, out _));

            perf.BeginFrame(0.010f, 0.25f);
            perf.BeginFrame(0.020f, 0.50f);
            perf.BeginFrame(0.022f, 0.70f);
            perf.BeginFrame(0.023f, 0.80f);
            perf.BeginFrame(0.050f, 0.90f);
            perf.BeginFrame(0.051f, 0.95f);

            Assert.IsTrue(perf.TrySnapshot(1f, out var stats));
            Assert.AreEqual(6, stats.Frames);
            Assert.AreEqual(3, stats.Over22, "22 ms and 50 ms are not over; 23, 50, and 51 are");
            Assert.AreEqual(1, stats.Over50);
            Assert.AreEqual(51f, stats.MaxMs, 0.05f);
            Assert.AreEqual(6f / stats.ElapsedSeconds, stats.Fps, 0.02f);
            Assert.Greater(stats.AvgMs, 20f);
            Assert.Less(stats.AvgMs, 40f);
            Assert.AreEqual(22f, stats.P50, 2f);
            Assert.AreEqual(51f, stats.P95, 0.2f);
            Assert.AreEqual(51f, stats.P99, 0.2f);

            Assert.IsFalse(perf.TrySnapshot(1.4f, out _), "the next window has not lasted a second");
            var hitch = perf.BeginFrame(0.060f, 1.5f);
            Assert.IsFalse(hitch.Emit);
            Assert.IsTrue(perf.TrySnapshot(2f, out var next));
            Assert.AreEqual(1, next.Frames);
            Assert.AreEqual(1, next.Over22);
            Assert.AreEqual(1, next.Over50);
            Assert.AreEqual(60f, next.MaxMs, 0.2f);
            Assert.Less(next.MaxMs, 70f);
        }

        [Test]
        public void Hitch_AttributesTheFrameThatJustFinished()
        {
            var perf = new DevicePerf { Enabled = true };
            Assert.IsFalse(perf.BeginFrame(0.016f, 0f).Emit);
            perf.SetPage("home");
            perf.AddTicks(DevicePerf.Phase.Update, MsTicks(12));
            perf.AddTicks(DevicePerf.Phase.Prewarm, MsTicks(4));
            perf.AddTicks(DevicePerf.Phase.Draw, MsTicks(40));
            perf.AddTicks(DevicePerf.Phase.Sim, MsTicks(8));
            perf.AddTicks(DevicePerf.Phase.Submit, MsTicks(21));
            perf.AddTicks(DevicePerf.Phase.Gui, MsTicks(1));

            Assert.IsFalse(perf.BeginFrame(0.080f, 0.1f).Emit, "80 ms is not over 80");
            perf.SetPage("game");
            perf.AddTicks(DevicePerf.Phase.Draw, MsTicks(1));

            var hitch = perf.BeginFrame(0.091f, 0.2f);
            Assert.IsTrue(hitch.Emit);
            Assert.AreEqual("game", hitch.Page);
            Assert.AreEqual(0.091f * 1000f, hitch.Ms, 0.05f);
            Assert.AreEqual(0f, hitch.UpdateMs, 0.05f, "phases added after the 80 ms frame belong to the next one");
            Assert.AreEqual(DevicePerf.TicksToMs(MsTicks(1)), hitch.DrawMs, 0.05f);

            var home = new DevicePerf { Enabled = true };
            home.BeginFrame(0.016f, 0f);
            home.SetPage("home");
            home.AddTicks(DevicePerf.Phase.Update, MsTicks(12));
            home.AddTicks(DevicePerf.Phase.Prewarm, MsTicks(4));
            home.AddTicks(DevicePerf.Phase.Draw, MsTicks(40));
            home.AddTicks(DevicePerf.Phase.Sim, MsTicks(8));
            home.AddTicks(DevicePerf.Phase.Submit, MsTicks(21));
            home.AddTicks(DevicePerf.Phase.Gui, MsTicks(1));
            var attributed = home.BeginFrame(0.090f, 0.1f);
            Assert.IsTrue(attributed.Emit);
            Assert.AreEqual("home", attributed.Page);
            Assert.AreEqual(DevicePerf.TicksToMs(MsTicks(12)), attributed.UpdateMs, 0.05f);
            Assert.AreEqual(DevicePerf.TicksToMs(MsTicks(4)), attributed.PrewarmMs, 0.05f);
            Assert.AreEqual(DevicePerf.TicksToMs(MsTicks(40)), attributed.DrawMs, 0.05f);
            Assert.AreEqual(DevicePerf.TicksToMs(MsTicks(8)), attributed.SimMs, 0.05f);
            Assert.AreEqual(DevicePerf.TicksToMs(MsTicks(21)), attributed.SubmitMs, 0.05f);
            Assert.AreEqual(DevicePerf.TicksToMs(MsTicks(1)), attributed.GuiMs, 0.05f);

            home.SetPage("maps");
            home.AddTicks(DevicePerf.Phase.Draw, MsTicks(99));
            Assert.AreEqual("home", attributed.Page);
            Assert.AreEqual(DevicePerf.TicksToMs(MsTicks(40)), attributed.DrawMs, 0.05f);
        }

        [Test]
        public void Disabled_IgnoresSamplesAndHitches()
        {
            var perf = new DevicePerf();
            perf.AddTicks(DevicePerf.Phase.Draw, MsTicks(80));
            Assert.IsFalse(perf.BeginFrame(0.2f, 0f).Emit);
            Assert.IsFalse(perf.BeginFrame(0.2f, 1f).Emit);
            Assert.IsFalse(perf.TrySnapshot(2f, out _));
            Assert.AreEqual(0f, perf.AvgMs(), 0.001f);

            perf.Enabled = true;
            perf.BeginFrame(0.016f, 0f);
            perf.BeginFrame(0.030f, 0.5f);
            perf.Enabled = false;
            Assert.IsFalse(perf.TrySnapshot(2f, out _));
            Assert.IsFalse(perf.BeginFrame(0.2f, 3f).Emit);
        }

        [Test]
        public void Lines_UseKeyValueAndOmitThermalWhenMissing()
        {
            var perf = new DevicePerf { Enabled = true };
            var stats = new DevicePerf.Stats
            {
                Frames = 58,
                Fps = 57.5f,
                AvgMs = 17f,
                P50 = 16f,
                P95 = 22.5f,
                P99 = 40f,
                MaxMs = 41f,
                Over22 = 4,
                Over50 = 0
            };
            var view = new DevicePerf.View { Page = "game", Scale = 2, Cap = 2, Fit = 1.5 };
            var counters = new DevicePerf.Counters
            {
                Gc0 = 1,
                GcMemory = -128,
                Allocated = 10,
                Mono = 20,
                Reserved = 30,
                Resident = 301371520,
                Budget = 335544320,
                MemoryMb = 2794,
                Thermal = null
            };
            var gpu = new DevicePerf.Gpu();
            var paint = new DevicePerf.Paint();
            const string tail = " gpu_ms=na submits=0 cmds=0 draws=0 backdrop=0/0 shadows=0 blits=0 cachebuilds=0 edges=0 opt ss=0 sl=0 scache=0 missclip=0 missfmt=0 misssmall=0 bd=0/0 rr=0 dirty=0/0 cold=0 fullpage=0 area=0 shp=0/0/0/0/0/0 se=0/0/0/0/0/0 gshp=0/0/0/0/0/0 gse=0/0/0/0/0/0 fast=0/0/0/0/0/0 rrwhy=0/0/0/0 bake=0 wide=-";
            string summary = perf.FormatSummary(stats, view, counters, gpu, paint);
            Assert.AreEqual(
                "[Perf] page=game scale=2.0 cap=2.0 fit=1.5 frames=58 fps=57.5 avg=17.0 p50=16.0 p95=22.5 p99=40.0 max=41.0 over22=4 over50=0 gc0=1 gcm=-128 alloc=10 mono=20 reserved=30 resident=301371520 budget=335544320 mem=2794" + tail,
                summary);
            Assert.IsFalse(summary.Contains("thermal"));

            counters.Thermal = "nominal";
            string warm = perf.FormatSummary(stats, view, counters, gpu, paint);
            Assert.IsTrue(warm.Contains(" thermal=nominal "));
            Assert.IsTrue(warm.EndsWith(tail.TrimStart()));
            gpu.Available = true;
            gpu.Avg = 12f;
            gpu.Max = 40.04f;
            paint.Submits = 3;
            paint.Commands = 9;
            paint.Draws = 10;
            paint.BackdropExact = 2;
            paint.BackdropGpu = 1;
            paint.Shadows = 4;
            paint.Blits = 5;
            paint.CacheBuilds = 1;
            string timed = perf.FormatSummary(stats, view, counters, gpu, paint);
            Assert.IsTrue(timed.Contains(" gpu_ms=12.0/40.0 submits=3 cmds=9 draws=10 backdrop=2/1 shadows=4 blits=5 cachebuilds=1"));
            Assert.IsFalse(warm.Contains("page=game page=game"));

            var hitch = new DevicePerf.Hitch
            {
                Emit = true,
                Ms = 91f,
                Page = "home",
                UpdateMs = 12f,
                PrewarmMs = 4f,
                DrawMs = 40f,
                SimMs = 8f,
                SubmitMs = 21f,
                GuiMs = 1f
            };
            Assert.AreEqual(
                "[Perf] hitch ms=91.0 page=home update=12.0 prewarm=4.0 draw=40.0 sim=8.0 submit=21.0 gui=1.0",
                perf.FormatHitch(hitch));
            Assert.AreEqual(
                "[Perf] page home -> maps ms=120.5 bake_ms=80.0",
                perf.FormatPage("home", "maps", 120.5f, 80f));
            Assert.AreEqual(
                "[Perf] governor step=down from=3.0 to=2.0 scale=2.0 reason=sustained page=game p95=28.0 avg=22.0",
                perf.FormatGovernor(true, 3, 2, 28f, 22f, "game"));
            Assert.AreEqual(
                "[Perf] governor step=up from=2.0 to=3.0 scale=3.0 reason=comfort page=maps p95=12.0 avg=10.0",
                perf.FormatGovernor(false, 2, 3, 12f, 10f, "maps"));
        }

        [Test]
        public void PageSwitch_AddsLoadTicksToBake()
        {
            long freq = Stopwatch.Frequency;
            DevicePerf.SwitchDurations(0, freq, 0, freq / 10, freq / 20, out float ms, out float bake);
            Assert.AreEqual(1000f, ms, 0.05f);
            Assert.AreEqual(150f, bake, 0.05f);

            DevicePerf.SwitchDurations(50, 40, 10, 0, 0, out float clampedMs, out float clampedBake);
            Assert.AreEqual(0f, clampedMs, 0.001f);
            Assert.AreEqual(0f, clampedBake, 0.001f);
        }

        [Test]
        public void PageId_UsesTheProductPageAndKeepsModals()
        {
            Assert.AreEqual("home", DevicePerf.PageId("home", "home_default"));
            Assert.AreEqual("maps", DevicePerf.PageId("maps", "maps_world_1_fresh"));
            Assert.AreEqual("game", DevicePerf.PageId("game", "game_lvl01_t1000ms"));
            Assert.AreEqual("modal_pause", DevicePerf.PageId("game", "modal_pause"));
            Assert.AreEqual("shop", DevicePerf.PageId("shop", null));
            Assert.AreEqual("daily", DevicePerf.PageId(null, "daily_default"));
            Assert.AreEqual("-", DevicePerf.PageId(null, null));
            Assert.AreEqual("-", DevicePerf.PageId("", ""));
        }

        [Test]
        public void Governor_LogsOnlyStepChanges()
        {
            Assert.IsFalse(DevicePerf.ShouldLogGovernor(false, false));
            Assert.IsTrue(DevicePerf.ShouldLogGovernor(true, false));
            Assert.IsTrue(DevicePerf.ShouldLogGovernor(false, true));
            Assert.AreEqual("sustained", DevicePerf.GovernorReason(true));
            Assert.AreEqual("comfort", DevicePerf.GovernorReason(false));
            Assert.IsFalse(DevicePerf.IsHitch(0.080f));
            Assert.IsFalse(DevicePerf.IsHitch(0f));
            Assert.IsFalse(DevicePerf.IsHitch(-1f));
            Assert.IsTrue(DevicePerf.IsHitch(0.081f));
        }

        static long MsTicks(double ms)
        {
            return (long)(ms * Stopwatch.Frequency / 1000.0);
        }
    }
}
