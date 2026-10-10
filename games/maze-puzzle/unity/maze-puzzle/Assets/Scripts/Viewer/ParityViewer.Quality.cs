using System;
using OrbitalSalvage.Render;
using UnityEngine;

namespace OrbitalSalvage.Viewer
{
    public sealed partial class ParityViewer
    {
        readonly QualityGovernor governor = new QualityGovernor();

        double latchedFit = 3;
        float fitChangedAt = -1f;
        float nextAmbientDraw;
        double ambientInterval;
        bool qualityHud;
        bool qualityReady;
        bool qualityBooted;
        float cacheDeferUntil = -1f;
        GUIStyle qualityHudStyle;

        void ConfigurePresentation()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            Application.runInBackground = true;
            double forced = PlayQuality.FromCommandLine();
            if (forced > 0) PlayQuality.Forced = forced;
        }

        /// <summary>
        /// Ignore frame samples for 1.5 s. GovernQuality also calls this when <c>currentId</c> changes,
        /// so a page change does not need a call from <c>Show</c>. Call it for a transition that keeps the same id.
        /// </summary>
        public void NoteTransition()
        {
            governor.NoteTransition(Time.unscaledTime);
        }

        void GovernQuality()
        {
            if (selfTesting) return;
            float now = Time.unscaledTime;
            float dt = Time.unscaledDeltaTime;
            if (dt < 0) dt = 0;
            bool phone = QualityGovernor.IsPhone(Application.isMobilePlatform, SystemInfo.deviceType);
            if (!governor.BootNoted) governor.NoteBoot(now);
            governor.WatchPage(currentId, now);
            bool accepted = governor.AcceptSample(now, dt);

            double forced = PlayQuality.Forced;
            if (!qualityBooted)
            {
                qualityBooted = true;
                if (forced <= 0)
                {
                    double saved = QualityGovernor.LoadScale(ScalePrefsStore.Instance, Screen.width, Screen.height, phone);
                    if (saved > 0) governor.Cap = saved;
                }
            }

            double fit = LatchedFit(now, phone);
            int fps = Application.targetFrameRate > 1 ? Application.targetFrameRate : 60;
            double capBefore = governor.Cap;
            var decision = governor.Tick(new QualityGovernor.Observation
            {
                Now = now,
                P95 = governor.P95(),
                SampleCount = governor.SampleCount,
                SampleAccepted = accepted,
                Phone = phone,
                Forced = forced,
                Fit = fit,
                TargetFrameRate = fps,
                ProbeSkipBlur = ProbeSkipBlur(),
                Width = Screen.width,
                Height = Screen.height,
                HasRaster = current != null,
                RasterScale = current != null ? current.RasterScale : fit
            });

            double target = decision.Target;
            if (forced > 0) ambientInterval = 0;
            else
            {
                float p95 = governor.P95();
                bool warmed = accepted && governor.SampleCount >= QualityGovernor.WarmSamples;
                if (target <= 1.01 && warmed && p95 > QualityGovernor.SlowSeconds) ambientInterval = 1.0 / 30.0;
                else if (p95 < 0.016f) ambientInterval = 0;
            }

            HomeChrome.SimplifyEffects = decision.SimplifyEffects;
            bool resized = current != null && Math.Abs(current.RasterScale - target) >= 0.05;
            if (resized)
            {
                current.ResizeRaster(target);
                cacheDeferUntil = forced <= 0 ? now + 0.9f : -1f;
                if (forced <= 0) governor.NoteResize(now);
            }
            HomeChrome.DeferLayerCache = forced <= 0 && cacheDeferUntil > 0f && now < cacheDeferUntil;
            if (resized) PaintCurrent(true);
            if (decision.Persist)
                QualityGovernor.SaveScale(ScalePrefsStore.Instance, true, Screen.width, Screen.height, decision.PersistScale, phone);
            qualityReady = true;
            if (decision.SteppedDown || decision.SteppedUp)
            {
                double fromScale = decision.SteppedDown ? governor.SteppedDownFrom : capBefore;
                PerfLogGovernor(decision.SteppedDown, decision.SteppedUp, fromScale, decision.Target, governor.P95());
            }
        }

        double LatchedFit(float now, bool phone)
        {
            int w = Screen.width, h = Screen.height;
            if (w < 2 || h < 2) return latchedFit;
            double fit = QualityGovernor.SnapStep(Math.Min(w / (double)ParityPages.CssWidth, h / (double)ParityPages.CssHeight), phone);
            if (Math.Abs(fit - latchedFit) < 0.2)
            {
                fitChangedAt = -1f;
                return latchedFit;
            }
            if (fitChangedAt < 0) fitChangedAt = now;
            if (now - fitChangedAt >= 0.4f) latchedFit = fit;
            return latchedFit;
        }

        static int probeSkipBlur = -1;

        static bool ProbeSkipBlur()
        {
            if (probeSkipBlur < 0)
                probeSkipBlur = Array.IndexOf(Environment.GetCommandLineArgs(), "-probe-no-blur") >= 0 ? 1 : 0;
            return probeSkipBlur == 1;
        }

        bool HoldAmbientDraw(bool ambient)
        {
            if (selfTesting || !ambient || ambientInterval <= 0) return false;
            if (Time.unscaledTime < nextAmbientDraw) return true;
            nextAmbientDraw = Time.unscaledTime + (float)ambientInterval;
            return false;
        }

        bool QualityHudTouchDown()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            bool editorOrDevelopment = true;
#else
            bool editorOrDevelopment = false;
#endif
            int pref = PlayerPrefs.GetInt(QualityGovernor.HudPrefKey, 0);
            if (!QualityGovernor.HudGestureEnabled(editorOrDevelopment, pref)) return false;
            int began = 0;
            int n = Input.touchCount;
            for (int i = 0; i < n; i++)
                if (Input.GetTouch(i).phase == TouchPhase.Began) began++;
            return QualityGovernor.FourFingerTap(n, began);
        }

        void DrawQualityHud()
        {
            if (!qualityHud || selfTesting) return;
            int font = QualityGovernor.HudFontSize(Screen.dpi);
            if (qualityHudStyle == null)
            {
                qualityHudStyle = new GUIStyle(GUI.skin.label);
                qualityHudStyle.normal.textColor = Color.white;
                qualityHudStyle.wordWrap = true;
            }
            qualityHudStyle.fontSize = font;
            float p95 = governor.P95() * 1000f;
            float fps = Time.unscaledDeltaTime > 1e-4f ? 1f / Time.unscaledDeltaTime : 0f;
            double scale = current != null ? current.RasterScale : 0;
            int w = Screen.width;
            int h = Screen.height;
            string key = QualityGovernor.ScalePrefKey(w, h);
            var prefs = ScalePrefsStore.Instance;
            bool hasSaved = prefs.HasKey(key);
            double saved = hasSaved ? prefs.GetFloat(key, 0f) : 0;
            string label = QualityGovernor.HudLabel(fps, p95, scale, governor.Cap, latchedFit, w, h, hasSaved, saved);
            GUI.Label(new Rect(8, 8, Mathf.Max(32f, w - 16f), font * 2.6f), label, qualityHudStyle);
        }

        sealed class ScalePrefsStore : QualityGovernor.IScalePrefs
        {
            public static readonly ScalePrefsStore Instance = new ScalePrefsStore();

            public bool HasKey(string key)
            {
#if UNITY_EDITOR
                return UnityEditor.EditorPrefs.HasKey(key);
#else
                return PlayerPrefs.HasKey(key);
#endif
            }

            public float GetFloat(string key, float fallback)
            {
#if UNITY_EDITOR
                return UnityEditor.EditorPrefs.GetFloat(key, fallback);
#else
                return PlayerPrefs.GetFloat(key, fallback);
#endif
            }

            public void SetFloat(string key, float value)
            {
#if UNITY_EDITOR
                UnityEditor.EditorPrefs.SetFloat(key, value);
#else
                PlayerPrefs.SetFloat(key, value);
#endif
            }
        }
    }
}
