using System;
using System.Globalization;
using UnityEngine;

namespace OrbitalSalvage.Viewer
{
    /// <summary>
    /// Product-path raster scale. Callers pass time, p95, and device facts; nothing here touches the GPU.
    /// </summary>
    public sealed class QualityGovernor
    {
        public const double PhoneFloor = 2;
        public const float HitchSeconds = 1.5f;
        public const float SustainedSlowSeconds = 1.5f;
        public const float StepDownCooldownSeconds = 0.8f;
        public const float StepUpCooldownSeconds = 10f;
        public const float StablePersistSeconds = 10f;
        public const float SlowSeconds = 0.022f;
        public const float ComfortFactor = 1.1f;
        public const int WarmSamples = 30;
        public const int SampleWindow = 60;
        public const string HudPrefKey = "os.parity.hud";

        public static readonly double[] DesktopSteps = { 1, 1.5, 2, 3 };
        public static readonly double[] PhoneSteps = { 2, 3 };

        readonly float[] samples = new float[SampleWindow];
        readonly float[] sort = new float[SampleWindow];
        int count;
        int head;

        public double Cap = 3;
        public float LastStepAt = float.NegativeInfinity;
        public float IgnoreUntil;
        public float SlowSince = float.NaN;
        public float StableSince = float.NaN;
        public double SteppedDownFrom;
        /// <summary>Scale of a climb that later fell back. Zero means no block. That scale and anything above it are not tried again this session.</summary>
        public double BlockedUpFrom;
        double climbedTo;
        public bool BootNoted;
        public string PageId;
        public bool HasPersisted;
        public double PersistedScale;
        public int SampleCount => count;

        bool haveTick;
        float lastTickNow;

        public interface IScalePrefs
        {
            bool HasKey(string key);
            float GetFloat(string key, float fallback);
            void SetFloat(string key, float value);
        }

        public struct Observation
        {
            public float Now;
            public float P95;
            public int SampleCount;
            public bool SampleAccepted;
            public bool Phone;
            public double Forced;
            public double Fit;
            public int TargetFrameRate;
            public bool ProbeSkipBlur;
            public int Width;
            public int Height;
            public bool HasRaster;
            public double RasterScale;
        }

        public struct Decision
        {
            public double Target;
            public bool SteppedDown;
            public bool SteppedUp;
            public bool Persist;
            public double PersistScale;
            public string PersistKey;
            public bool SimplifyEffects;
        }

        public static double[] Steps(bool phone) => phone ? PhoneSteps : DesktopSteps;

        public static bool IsPhone(bool isMobilePlatform, DeviceType deviceType)
        {
            return isMobilePlatform || deviceType == DeviceType.Handheld;
        }

        public static string ScalePrefKey(int w, int h)
        {
            return "os.parity.scale.v2." + w.ToString(CultureInfo.InvariantCulture) + "x" + h.ToString(CultureInfo.InvariantCulture);
        }

        public static string LegacyScalePrefKey(int w, int h)
        {
            return "os.parity.scale." + w.ToString(CultureInfo.InvariantCulture) + "x" + h.ToString(CultureInfo.InvariantCulture);
        }

        public static double SnapStep(double value, bool phone)
        {
            if (phone)
            {
                if (value >= 2.6) return 3;
                return PhoneFloor;
            }
            if (value >= 2.6) return 3;
            if (value >= 1.75) return 2;
            if (value >= 1.25) return 1.5;
            return 1;
        }

        public static double StepDown(double scale, bool phone)
        {
            double[] steps = Steps(phone);
            for (int i = steps.Length - 1; i >= 0; i--)
                if (steps[i] < scale - 0.05) return steps[i];
            return steps[0];
        }

        public static double StepUp(double scale, bool phone)
        {
            double[] steps = Steps(phone);
            for (int i = 0; i < steps.Length; i++)
                if (steps[i] > scale + 0.05) return steps[i];
            return steps[steps.Length - 1];
        }

        public static double ImpliedScale(double scale, float p95, bool phone)
        {
            double[] steps = Steps(phone);
            double floor = steps[0];
            const double budget = 0.0166;
            if (p95 <= budget) return scale;
            double want = scale * Math.Sqrt(budget / p95);
            double best = floor;
            for (int i = 0; i < steps.Length; i++)
            {
                double step = steps[i];
                if (step < scale - 0.05 && step <= want + 0.02) best = step;
            }
            return best;
        }

        public static double FitScale(int width, int height, double cssWidth, double cssHeight, bool phone)
        {
            if (width < 2 || height < 2 || cssWidth < 1 || cssHeight < 1)
                return phone ? PhoneFloor : 3;
            double raw = Math.Min(width / cssWidth, height / cssHeight);
            return SnapStep(raw, phone);
        }

        public static double ComfortCeiling(int targetFrameRate)
        {
            int fps = targetFrameRate > 1 ? targetFrameRate : 60;
            return (1.0 / fps) * ComfortFactor;
        }

        public static bool ShouldPersist(bool phone, double scale, float stableSeconds, bool warmed, double forced)
        {
            if (forced > 0 || !warmed) return false;
            if (stableSeconds < StablePersistSeconds) return false;
            if (scale <= 0) return false;
            if (phone && scale < PhoneFloor - 0.01) return false;
            return true;
        }

        public static bool ShouldSimplify(bool probeSkipBlur, bool phone, double forced, double target)
        {
            if (probeSkipBlur) return true;
            if (phone) return false;
            return forced <= 0 && target <= 1.01;
        }

        public static int HudFontSize(float dpi)
        {
            double basis = dpi >= 1f ? dpi : 160d;
            int size = (int)Math.Round(14d * basis / 160d, MidpointRounding.AwayFromZero);
            if (size < 14) return 14;
            if (size > 48) return 48;
            return size;
        }

        public static string HudLabel(float fps, float p95Ms, double scale, double cap, double fit, int w, int h, bool hasSaved, double saved)
        {
            string savedText = hasSaved ? saved.ToString("0.0", CultureInfo.InvariantCulture) : "-";
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:0} fps  p95 {1:0.0} ms  scale {2:0.0}  cap {3:0.0}  fit {4:0.0}  {5}x{6}  saved={7}",
                fps, p95Ms, scale, cap, fit, w, h, savedText);
        }

        public static bool FourFingerTap(int touchCount, int beganThisFrame)
        {
            return touchCount >= 4 && beganThisFrame >= 4;
        }

        public static bool HudGestureEnabled(bool editorOrDevelopment, int hudPref)
        {
            return editorOrDevelopment || hudPref == 1;
        }

        public static bool HudToggled(bool showing, bool f3Down, bool gestureEnabled, int touchCount, int beganThisFrame)
        {
            if (f3Down || (gestureEnabled && FourFingerTap(touchCount, beganThisFrame)))
                return !showing;
            return showing;
        }

        public static double LoadScale(IScalePrefs prefs, int w, int h, bool phone)
        {
            if (prefs == null || w < 2 || h < 2) return 0;
            string key = ScalePrefKey(w, h);
            if (!prefs.HasKey(key)) return 0;
            float saved = prefs.GetFloat(key, 0f);
            if (saved <= 0f) return 0;
            if (phone && saved < PhoneFloor - 0.01) return 0;
            return SnapStep(saved, phone);
        }

        public static bool SaveScale(IScalePrefs prefs, bool persist, int w, int h, double scale, bool phone)
        {
            if (!persist || prefs == null || w < 2 || h < 2) return false;
            if (scale <= 0 || (phone && scale < PhoneFloor - 0.01)) return false;
            prefs.SetFloat(ScalePrefKey(w, h), (float)scale);
            return true;
        }

        public void NoteBoot(float now)
        {
            if (BootNoted) return;
            BootNoted = true;
            Arm(now);
            SlowSince = float.NaN;
        }

        public void NoteTransition(float now)
        {
            Arm(now);
            SlowSince = float.NaN;
        }

        public void NoteResize(float now)
        {
            Arm(now);
            SlowSince = float.NaN;
            StableSince = float.NaN;
        }

        public bool WatchPage(string pageId, float now)
        {
            if (string.IsNullOrEmpty(pageId)) return false;
            if (PageId == null)
            {
                PageId = pageId;
                return false;
            }
            if (PageId == pageId) return false;
            PageId = pageId;
            NoteTransition(now);
            return true;
        }

        public bool AcceptSample(float now, float dt)
        {
            if (!BootNoted) NoteBoot(now);
            if (now < IgnoreUntil) return false;
            if (dt < 0f) dt = 0f;
            samples[head] = dt;
            head = (head + 1) % SampleWindow;
            if (count < SampleWindow) count++;
            return true;
        }

        public float P95()
        {
            int n = count;
            if (n <= 0) return 0f;
            for (int i = 0; i < n; i++) sort[i] = samples[i];
            Array.Sort(sort, 0, n);
            int rank = (int)Math.Ceiling(n * 0.95) - 1;
            if (rank < 0) rank = 0;
            if (rank >= n) rank = n - 1;
            return sort[rank];
        }

        public Decision Tick(Observation o)
        {
            var decision = new Decision();
            bool phone = o.Phone;
            double forced = o.Forced;
            if (phone && forced <= 0 && Cap < PhoneFloor - 0.01)
                Cap = PhoneFloor;

            double target;
            bool steppedDown = false;
            bool steppedUp = false;
            if (forced > 0)
            {
                target = SnapStep(forced, false);
            }
            else
            {
                bool warmed = o.SampleCount >= WarmSamples;
                bool live = o.SampleAccepted && warmed;
                if (!live)
                    SlowSince = float.NaN;
                else if (o.P95 > SlowSeconds)
                {
                    if (float.IsNaN(SlowSince)) SlowSince = o.Now;
                    bool sustained = o.Now - SlowSince >= SustainedSlowSeconds;
                    bool cooled = o.Now - LastStepAt >= StepDownCooldownSeconds;
                    if (sustained && cooled)
                    {
                        double cur = Math.Min(Cap, o.Fit);
                        double implied = ImpliedScale(cur, o.P95, phone);
                        double one = StepDown(cur, phone);
                        double next = implied < cur - 0.05 ? implied : one;
                        if (next < cur - 0.05)
                        {
                            if (climbedTo > 0 && cur >= climbedTo - 0.05
                                && (BlockedUpFrom <= 0 || climbedTo < BlockedUpFrom - 0.01))
                                BlockedUpFrom = climbedTo;
                            climbedTo = 0;
                            SteppedDownFrom = cur;
                            Cap = next;
                            LastStepAt = o.Now;
                            steppedDown = true;
                            SlowSince = float.NaN;
                        }
                    }
                }
                else
                {
                    SlowSince = float.NaN;
                    if (o.P95 <= ComfortCeiling(o.TargetFrameRate) && o.Now - LastStepAt >= StepUpCooldownSeconds)
                    {
                        double up = StepUp(Cap, phone);
                        bool blocked = BlockedUpFrom > 0 && up >= BlockedUpFrom - 0.05;
                        if (!blocked && up <= o.Fit + 0.01 && up > Cap + 0.05)
                        {
                            climbedTo = up;
                            Cap = up;
                            LastStepAt = o.Now;
                            steppedUp = true;
                        }
                    }
                }

                target = Math.Min(Cap, o.Fit);
                if (phone && target < PhoneFloor - 0.01) target = PhoneFloor;
            }

            bool resized = o.HasRaster && Math.Abs(o.RasterScale - target) >= 0.05;
            if (steppedDown || steppedUp || resized)
                StableSince = float.NaN;
            else if (o.SampleAccepted && o.SampleCount >= WarmSamples && forced <= 0)
            {
                if (float.IsNaN(StableSince)) StableSince = o.Now;
            }
            else if (!o.SampleAccepted && !float.IsNaN(StableSince) && haveTick)
                StableSince += o.Now - lastTickNow;

            haveTick = true;
            lastTickNow = o.Now;

            float stableFor = float.IsNaN(StableSince) ? 0f : o.Now - StableSince;
            bool warmedNow = o.SampleCount >= WarmSamples && o.SampleAccepted;
            bool aboveBlock = BlockedUpFrom > 0 && target >= BlockedUpFrom - 0.05;
            if (o.Width >= 2 && o.Height >= 2
                && !aboveBlock
                && ShouldPersist(phone, target, stableFor, warmedNow, forced)
                && (!HasPersisted || Math.Abs(PersistedScale - target) >= 0.05))
            {
                decision.Persist = true;
                decision.PersistScale = target;
                decision.PersistKey = ScalePrefKey(o.Width, o.Height);
                HasPersisted = true;
                PersistedScale = target;
            }

            decision.Target = target;
            decision.SteppedDown = steppedDown;
            decision.SteppedUp = steppedUp;
            decision.SimplifyEffects = ShouldSimplify(o.ProbeSkipBlur, phone, forced, target);
            return decision;
        }

        /// <summary>Drop samples and step memory so a bench cell starts from the boot cap. Not used on the product path.</summary>
        public void ResetSession()
        {
            count = 0;
            head = 0;
            Cap = 3;
            LastStepAt = float.NegativeInfinity;
            IgnoreUntil = 0f;
            SlowSince = float.NaN;
            StableSince = float.NaN;
            SteppedDownFrom = 0;
            BlockedUpFrom = 0;
            climbedTo = 0;
            BootNoted = false;
            PageId = null;
            HasPersisted = false;
            PersistedScale = 0;
            haveTick = false;
            lastTickNow = 0f;
        }

        void Arm(float now)
        {
            float until = now + HitchSeconds;
            if (until > IgnoreUntil) IgnoreUntil = until;
        }
    }
}
