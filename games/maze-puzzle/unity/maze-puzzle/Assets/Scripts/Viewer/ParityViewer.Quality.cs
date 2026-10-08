using System;
using OrbitalSalvage.Render;
using UnityEngine;

namespace OrbitalSalvage.Viewer
{
    public sealed partial class ParityViewer
    {
        static readonly double[] QualitySteps = { 1, 1.5, 2, 3 };
        const int QualityWindow = 60;

        readonly float[] qualityDts = new float[QualityWindow];
        readonly float[] qualitySort = new float[QualityWindow];
        int qualityCount;
        int qualityHead;
        double qualityCap = 3;
        double latchedFit = 3;
        float fitChangedAt = -1f;
        float lastQualityStep;
        float nextAmbientDraw;
        double ambientInterval;
        bool qualityHud;
        bool qualityReady;
        bool qualityBooted;
        float cacheDeferUntil = -1f;

        void ConfigurePresentation()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            Application.runInBackground = true;
            double forced = PlayQuality.FromCommandLine();
            if (forced > 0) PlayQuality.Forced = forced;
        }

        void GovernQuality()
        {
            if (selfTesting) return;
            float now = Time.unscaledTime;
            float dt = Time.unscaledDeltaTime;
            if (dt < 0) dt = 0;
            qualityDts[qualityHead] = dt;
            qualityHead = (qualityHead + 1) % QualityWindow;
            if (qualityCount < QualityWindow) qualityCount++;

            double forced = PlayQuality.Forced;
            if (!qualityBooted)
            {
                qualityBooted = true;
                if (forced <= 0)
                {
                    double saved = LoadSettledScale(Screen.width, Screen.height);
                    if (saved > 0) qualityCap = SnapStep(saved);
                }
            }
            double target;
            if (forced > 0)
            {
                target = SnapStep(forced);
                ambientInterval = 0;
            }
            else
            {
                double fit = LatchedFit(now);
                float p95 = QualityP95();
                bool warmed = qualityCount >= 30;
                if (warmed && p95 > 0.022f && now - lastQualityStep > 0.8f)
                {
                    double cur = Math.Min(qualityCap, fit);
                    double implied = ImpliedScale(cur, p95);
                    qualityCap = implied < cur - 0.05 ? implied : StepDown(cur);
                    lastQualityStep = now;
                }
                else if (warmed && p95 < 0.014f && now - lastQualityStep > 3f)
                {
                    double up = StepUp(qualityCap);
                    if (up <= fit + 0.01)
                    {
                        qualityCap = up;
                        lastQualityStep = now;
                    }
                }
                target = Math.Min(qualityCap, fit);
                if (target <= 1.01 && warmed && p95 > 0.022f) ambientInterval = 1.0 / 30.0;
                else if (p95 < 0.016f) ambientInterval = 0;
            }

            HomeChrome.SimplifyEffects = ProbeSkipBlur() || (forced <= 0 && target <= 1.01);
            bool resized = current != null && Math.Abs(current.RasterScale - target) >= 0.05;
            if (resized)
            {
                current.ResizeRaster(target);
                cacheDeferUntil = forced <= 0 ? now + 0.9f : -1f;
            }
            HomeChrome.DeferLayerCache = forced <= 0 && cacheDeferUntil > 0f && now < cacheDeferUntil;
            if (resized) PaintCurrent(true);
            if (!HomeChrome.DeferLayerCache && forced <= 0 && current != null && current.UsesLayerCache
                && Math.Abs(current.RasterScale - target) < 0.05)
                SaveSettledScale(Screen.width, Screen.height, current.RasterScale);
            qualityReady = true;
        }

        static double ImpliedScale(double scale, float p95)
        {
            const double budget = 0.0166;
            if (p95 <= budget) return scale;
            double want = scale * Math.Sqrt(budget / p95);
            double best = 1;
            for (int i = 0; i < QualitySteps.Length; i++)
            {
                double step = QualitySteps[i];
                if (step < scale - 0.05 && step <= want + 0.02) best = step;
            }
            return best;
        }

        static string ScalePrefKey(int w, int h) => "os.parity.scale." + w.ToString(System.Globalization.CultureInfo.InvariantCulture) + "x" + h.ToString(System.Globalization.CultureInfo.InvariantCulture);

        static double LoadSettledScale(int w, int h)
        {
            if (w < 2 || h < 2) return 0;
            string key = ScalePrefKey(w, h);
#if UNITY_EDITOR
            if (!UnityEditor.EditorPrefs.HasKey(key)) return 0;
            return UnityEditor.EditorPrefs.GetFloat(key, 0f);
#else
            if (!PlayerPrefs.HasKey(key)) return 0;
            return PlayerPrefs.GetFloat(key, 0f);
#endif
        }

        static void SaveSettledScale(int w, int h, double scale)
        {
            if (w < 2 || h < 2) return;
            string key = ScalePrefKey(w, h);
#if UNITY_EDITOR
            UnityEditor.EditorPrefs.SetFloat(key, (float)scale);
#else
            PlayerPrefs.SetFloat(key, (float)scale);
#endif
        }

        double LatchedFit(float now)
        {
            int w = Screen.width, h = Screen.height;
            if (w < 2 || h < 2) return latchedFit;
            double fit = SnapStep(Math.Min(w / (double)ParityPages.CssWidth, h / (double)ParityPages.CssHeight));
            if (Math.Abs(fit - latchedFit) < 0.2)
            {
                fitChangedAt = -1f;
                return latchedFit;
            }
            if (fitChangedAt < 0) fitChangedAt = now;
            if (now - fitChangedAt >= 0.4f) latchedFit = fit;
            return latchedFit;
        }

        static double SnapStep(double value)
        {
            if (value >= 2.6) return 3;
            if (value >= 1.75) return 2;
            if (value >= 1.25) return 1.5;
            return 1;
        }

        static double StepDown(double scale)
        {
            for (int i = QualitySteps.Length - 1; i >= 0; i--)
                if (QualitySteps[i] < scale - 0.05) return QualitySteps[i];
            return 1;
        }

        static double StepUp(double scale)
        {
            for (int i = 0; i < QualitySteps.Length; i++)
                if (QualitySteps[i] > scale + 0.05) return QualitySteps[i];
            return 3;
        }

        float QualityP95()
        {
            int n = qualityCount;
            if (n <= 0) return 0;
            for (int i = 0; i < n; i++) qualitySort[i] = qualityDts[i];
            Array.Sort(qualitySort, 0, n);
            int rank = (int)Math.Ceiling(n * 0.95) - 1;
            if (rank < 0) rank = 0;
            if (rank >= n) rank = n - 1;
            return qualitySort[rank];
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

        void DrawQualityHud()
        {
            if (!qualityHud || selfTesting) return;
            if (hintStyle == null)
            {
                hintStyle = new GUIStyle(GUI.skin.label) { fontSize = 14 };
                hintStyle.normal.textColor = Color.white;
            }
            float p95 = QualityP95() * 1000f;
            float fps = Time.unscaledDeltaTime > 1e-4f ? 1f / Time.unscaledDeltaTime : 0f;
            double scale = current != null ? current.RasterScale : 0;
            string label = fps.ToString("0") + " fps  p95 " + p95.ToString("0.0") + " ms  scale " + scale.ToString("0.0");
            GUI.Label(new Rect(8, 8, 420, 22), label, hintStyle);
        }
    }
}
