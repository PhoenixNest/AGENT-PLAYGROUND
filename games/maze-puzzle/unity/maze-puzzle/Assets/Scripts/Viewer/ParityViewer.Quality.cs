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
                    qualityCap = StepDown(Math.Min(qualityCap, fit));
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

            HomeChrome.SimplifyEffects = forced <= 0 && target <= 1.01;
            if (current != null && Math.Abs(current.RasterScale - target) >= 0.05)
                current.ResizeRaster(target);
            qualityReady = true;
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
