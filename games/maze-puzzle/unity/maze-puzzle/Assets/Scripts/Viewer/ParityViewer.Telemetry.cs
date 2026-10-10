using UnityEngine;
using UnityEngine.Profiling;
using OrbitalSalvage.Render;

namespace OrbitalSalvage.Viewer
{
    public sealed partial class ParityViewer
    {
        DevicePerf perf;
        bool perfOn;
        float perfPrefAt = -1f;
        long perfUpdateStamp;
        bool perfPageArmed;
        string perfPage;
        bool perfSwitchOpen;
        string perfSwitchFrom;
        string perfSwitchTo;
        long perfSwitchStamp;
        long perfBakeAt;
        long perfBakeExtra;
        int perfGc0;
        long perfGcMem;
        bool perfGcReady;

        void PerfSet(bool on)
        {
            if (on == perfOn) return;
            if (on)
            {
                if (perf == null) perf = new DevicePerf();
                perf.Enabled = true;
                LiveGameSession.PhaseTiming = true;
                perfOn = true;
                return;
            }
            perfOn = false;
            if (perf != null) perf.Enabled = false;
            LiveGameSession.PhaseTiming = false;
            LiveGameSession.PhaseSimTicks = 0;
            LiveGameSession.PhasePresentTicks = 0;
            HomeChrome.ProbeBake = false;
            perfSwitchOpen = false;
            perfUpdateStamp = 0;
            perfBakeExtra = 0;
        }

        void PerfOpen()
        {
            if (selfTesting)
            {
                if (perfOn) PerfSet(false);
                return;
            }
            float now = Time.unscaledTime;
            if (perfPrefAt < 0f || now - perfPrefAt >= DevicePerf.ReportIntervalSeconds)
            {
                perfPrefAt = now;
                PerfSet(DevicePerf.IsEnabled(PlayerPrefs.GetInt(DevicePerf.PrefKey, 0)));
            }
            if (!perfOn || perf == null) return;
            float dt = Time.unscaledDeltaTime;
            var hitch = perf.BeginFrame(dt, now);
            if (hitch.Emit) Debug.Log(perf.FormatHitch(hitch));
            if (!perf.TrySnapshot(now, out var stats)) return;
            var view = new DevicePerf.View
            {
                Page = DevicePerf.PageId(app != null ? app.CurrentPage : null, currentId),
                Scale = current != null ? current.RasterScale : 0,
                Cap = governor.Cap,
                Fit = latchedFit
            };
            Debug.Log(perf.FormatSummary(stats, view, PerfCounters()));
        }

        DevicePerf.Counters PerfCounters()
        {
            int gc0 = System.GC.CollectionCount(0);
            long gcm = System.GC.GetTotalMemory(false);
            int d0 = perfGcReady ? gc0 - perfGc0 : 0;
            long dm = perfGcReady ? gcm - perfGcMem : 0;
            perfGcReady = true;
            perfGc0 = gc0;
            perfGcMem = gcm;
            var counters = new DevicePerf.Counters
            {
                Gc0 = d0,
                GcMemory = dm,
                Allocated = Profiler.GetTotalAllocatedMemoryLong(),
                Mono = Profiler.GetMonoUsedSizeLong(),
                Reserved = Profiler.GetTotalReservedMemoryLong(),
                Resident = ResidentCacheBytes,
                Budget = cache.ByteBudget,
                MemoryMb = SystemInfo.systemMemorySize,
                Thermal = PerfThermal()
            };
            return counters;
        }

        static string PerfThermal()
        {
#if UNITY_IOS && !UNITY_EDITOR
            switch (UnityEngine.iOS.Device.thermalState)
            {
                case UnityEngine.iOS.ThermalState.Nominal: return "nominal";
                case UnityEngine.iOS.ThermalState.Fair: return "fair";
                case UnityEngine.iOS.ThermalState.Serious: return "serious";
                case UnityEngine.iOS.ThermalState.Critical: return "critical";
                default: return null;
            }
#else
            return null;
#endif
        }

        void PerfLogGovernor(bool steppedDown, bool steppedUp, double fromScale, double toScale, float p95Seconds)
        {
            if (!perfOn || perf == null || !DevicePerf.ShouldLogGovernor(steppedDown, steppedUp)) return;
            float p95Ms = p95Seconds > 0f ? p95Seconds * 1000f : 0f;
            string page = DevicePerf.PageId(app != null ? app.CurrentPage : null, currentId);
            Debug.Log(perf.FormatGovernor(steppedDown, fromScale, toScale, p95Ms, perf.AvgMs(), page));
        }

        long PerfStamp()
        {
            return perfOn ? System.Diagnostics.Stopwatch.GetTimestamp() : 0;
        }

        void PerfAdd(DevicePerf.Phase phase, long stamp)
        {
            if (!perfOn || stamp == 0 || perf == null) return;
            long ticks = System.Diagnostics.Stopwatch.GetTimestamp() - stamp;
            if (ticks > 0) perf.AddTicks(phase, ticks);
        }

        void PerfResumeUpdate()
        {
            if (!perfOn || perfUpdateStamp != 0) return;
            perfUpdateStamp = System.Diagnostics.Stopwatch.GetTimestamp();
        }

        void PerfSuspendUpdate()
        {
            if (perfUpdateStamp == 0) return;
            long stamp = perfUpdateStamp;
            perfUpdateStamp = 0;
            PerfAdd(DevicePerf.Phase.Update, stamp);
        }

        void PerfMaybeBeginSwitch(string id)
        {
            if (!perfOn || string.IsNullOrEmpty(id)) return;
            if (!perfPageArmed)
            {
                perfPage = id;
                perfPageArmed = true;
                return;
            }
            if (perfSwitchOpen || id == perfPage) return;
            perfSwitchOpen = true;
            perfSwitchFrom = perfPage;
            perfSwitchTo = id;
            perfPage = id;
            perfSwitchStamp = System.Diagnostics.Stopwatch.GetTimestamp();
            perfBakeAt = HomeChrome.ProbeBakeTicks;
            perfBakeExtra = 0;
            HomeChrome.ProbeBake = true;
        }

        void PerfBeginShow(string stateId)
        {
            if (!perfOn) return;
            PerfMaybeBeginSwitch(DevicePerf.PageId(app != null ? app.CurrentPage : null, stateId));
        }

        long PerfLoadStamp()
        {
            if (!perfSwitchOpen) return 0;
            return System.Diagnostics.Stopwatch.GetTimestamp();
        }

        void PerfLoadAdd(long stamp)
        {
            if (stamp == 0) return;
            long ticks = System.Diagnostics.Stopwatch.GetTimestamp() - stamp;
            if (ticks > 0) perfBakeExtra += ticks;
        }

        void PerfCancelSwitch()
        {
            if (!perfSwitchOpen) return;
            perfSwitchOpen = false;
            HomeChrome.ProbeBake = false;
            perfPage = perfSwitchFrom;
            perfBakeExtra = 0;
        }

        void PerfDrawn()
        {
            if (!perfSwitchOpen || perf == null) return;
            string id = DevicePerf.PageId(app != null ? app.CurrentPage : null, currentId);
            if (id != perfSwitchTo) return;
            perfSwitchOpen = false;
            HomeChrome.ProbeBake = false;
            long nowTicks = System.Diagnostics.Stopwatch.GetTimestamp();
            long bakeTicks = (HomeChrome.ProbeBakeTicks - perfBakeAt) + perfBakeExtra;
            perfBakeExtra = 0;
            DevicePerf.SwitchDurations(perfSwitchStamp, nowTicks, 0, bakeTicks, 0, out float ms, out float bakeMs);
            Debug.Log(perf.FormatPage(perfSwitchFrom, perfSwitchTo, ms, bakeMs));
        }

        void PerfAfterFrame()
        {
            if (!perfOn || perf == null) return;
            perf.SetPage(DevicePerf.PageId(app != null ? app.CurrentPage : null, currentId));
        }

        void PerfShutdown()
        {
            PerfSet(false);
        }
    }
}
