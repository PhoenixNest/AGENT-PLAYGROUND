using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using OrbitalSalvage.Render;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace OrbitalSalvage.Viewer
{
    public sealed partial class ParityViewer
    {
        const int ProbeFrames = 300;

        /// <summary>
        /// Editor Play measurement. Does not change vSync, frame rate, or the paint path.
        /// The normal <see cref="LateUpdate"/> keeps drawing; this only records and then exits the editor.
        /// </summary>
        IEnumerator EditorPlayProbe(string dir)
        {
            Application.runInBackground = true;
            for (int i = 0; i < 30; i++)
            {
                ApplyProbeGameView();
                yield return null;
                if (Screen.width >= 1200 && Screen.height >= 2500) break;
            }
            PaintProfile.Enabled = true;
            PaintProfile.Reset();
            string error = null;
            try { Directory.CreateDirectory(dir); }
            catch (Exception ex) { error = ex.ToString(); }

            float deadline = Time.realtimeSinceStartup + 60f;
            while (error == null && (app == null || app.CurrentPage != "home" || current == null || current.Target == null))
            {
                if (Time.realtimeSinceStartup > deadline)
                {
                    error = "home did not appear page=" + (app != null ? app.CurrentPage : "no-app");
                    break;
                }
                yield return null;
            }

            // Let the page-enter animation settle so the sample is the looping home, not the intro.
            if (error == null)
            {
                float settle = Time.realtimeSinceStartup + 1.2f;
                while (Time.realtimeSinceStartup < settle) yield return null;
            }

            bool diag = Array.IndexOf(Environment.GetCommandLineArgs(), "-probe-diag") >= 0;
            if (error == null && diag && current != null)
            {
                try { File.WriteAllText(Path.Combine(dir, "diag.txt"), RunDiag()); }
                catch (Exception ex) { File.WriteAllText(Path.Combine(dir, "diag.txt"), ex.ToString()); }
                yield return null;
                ExitEditor(0);
                yield break;
            }

            string frozenNote = "";
            string isolatedNote = "";
            if (error == null && current != null)
            {
                try
                {
                    current.Draw();
                    frozenNote = SampleNote(current.Target);
                    current.Draw(clock.Milliseconds);
                    isolatedNote = IsolatedGradient();
                }
                catch (Exception ex) { frozenNote = ex.Message; }
            }

            var frameMs = new double[ProbeFrames];
            var drawMs = new double[ProbeFrames];
            var submitMs = new double[ProbeFrames];
            var backdropMs = new double[ProbeFrames];
            var readbackMs = new double[ProbeFrames];
            var gcBytes = new long[ProbeFrames];
            int recorded = 0;
            ProfilerRecorder gcRec = default;
            bool gcOk = false;
            if (error == null)
            {
                gcRec = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
                gcOk = gcRec.Valid;
                PaintProfile.Reset();
                yield return null;
                double freq = Stopwatch.Frequency / 1000.0;
                for (int i = 0; i < ProbeFrames; i++)
                {
                    long wall0 = PaintProfile.WallTicks;
                    long sub0 = PaintProfile.StageTicks(PaintProfile.Submit);
                    long back0 = PaintProfile.StageTicks(PaintProfile.Backdrop);
                    long read0 = PaintProfile.StageTicks(PaintProfile.Readback);
                    yield return null;
                    frameMs[i] = Time.unscaledDeltaTime * 1000.0;
                    drawMs[i] = (PaintProfile.WallTicks - wall0) / freq;
                    submitMs[i] = (PaintProfile.StageTicks(PaintProfile.Submit) - sub0) / freq;
                    backdropMs[i] = (PaintProfile.StageTicks(PaintProfile.Backdrop) - back0) / freq;
                    readbackMs[i] = (PaintProfile.StageTicks(PaintProfile.Readback) - read0) / freq;
                    gcBytes[i] = gcOk ? gcRec.LastValue : 0;
                    recorded++;
                }
            }

            if (error == null)
            {
                try { File.WriteAllText(Path.Combine(dir, "paint.txt"), PaintProfile.Report(recorded)); }
                catch (Exception ex) { error = ex.ToString(); }
            }

            try
            {
                var sb = new StringBuilder();
                var inv = CultureInfo.InvariantCulture;
                sb.Append("{\"ok\":").Append(error == null ? "true" : "false");
                sb.Append(",\"error\":");
                Quote(sb, error);
                sb.Append(",\"frames\":").Append(recorded);
                sb.Append(",\"gpu\":");
                Quote(sb, SystemInfo.graphicsDeviceName + " | " + SystemInfo.graphicsDeviceType);
                sb.Append(",\"colorSpace\":");
                Quote(sb, QualitySettings.activeColorSpace.ToString());
                sb.Append(",\"screen\":\"").Append(Screen.width).Append('x').Append(Screen.height).Append('"');
                sb.Append(",\"dpi\":").Append(Screen.dpi.ToString("R", inv));
                var safe = Screen.safeArea;
                sb.Append(",\"safe\":\"").Append(safe.x.ToString("0.#", inv)).Append(',')
                    .Append(safe.y.ToString("0.#", inv)).Append(',')
                    .Append(safe.width.ToString("0.#", inv)).Append(',')
                    .Append(safe.height.ToString("0.#", inv)).Append('"');
                sb.Append(",\"vSync\":").Append(QualitySettings.vSyncCount);
                sb.Append(",\"targetFps\":").Append(Application.targetFrameRate);
                sb.Append(",\"runInBackground\":").Append(Application.runInBackground ? "true" : "false");
                sb.Append(",\"page\":");
                Quote(sb, app != null ? app.CurrentPage : "");
                sb.Append(",\"credits\":");
                Quote(sb, app != null ? app.CreditText : "");
                sb.Append(",\"template\":");
                Quote(sb, currentId);
                sb.Append(",\"loops\":").Append(current != null && current.Loops ? "true" : "false");
                sb.Append(",\"layerCache\":").Append(current != null && current.UsesLayerCache ? "true" : "false");
                if (current != null)
                    sb.Append(",\"raster\":\"").Append(current.PixelWidth).Append('x').Append(current.PixelHeight).Append('"');
                sb.Append(",\"focused\":").Append(EditorFocused() ? "true" : "false");
                sb.Append(",\"gcRecorder\":").Append(gcOk ? "true" : "false");
                AppendStats(sb, "frameMs", frameMs, recorded);
                AppendStats(sb, "drawMs", drawMs, recorded);
                AppendStats(sb, "submitMs", submitMs, recorded);
                AppendStats(sb, "backdropMs", backdropMs, recorded);
                AppendStats(sb, "readbackMs", readbackMs, recorded);
                AppendLongStats(sb, "gcBytes", gcBytes, recorded);
                AppendEmoji(sb);
                AppendSamples(sb);
                sb.Append(",\"frozen\":");
                Quote(sb, frozenNote);
                sb.Append(",\"isolated\":");
                Quote(sb, isolatedNote);
                sb.Append('}');
                File.WriteAllText(Path.Combine(dir, "probe.json"), sb.ToString());
                UnityEngine.Debug.Log("[EditorPlayProbe] " + sb);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[EditorPlayProbe] write " + ex);
                if (error == null) error = ex.Message;
            }

            if (gcOk) gcRec.Dispose();
            if (error == null && current != null && current.Target != null)
            {
                try { WriteProbeCaptures(dir); }
                catch (Exception ex) { UnityEngine.Debug.LogError("[EditorPlayProbe] capture " + ex); }
            }
            yield return null;
            ExitEditor(error == null ? 0 : 1);
        }

        void WriteProbeCaptures(string dir)
        {
            var rt = current.Target;
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false, true);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
            tex.Apply(false);
            RenderTexture.active = prev;
            File.WriteAllBytes(Path.Combine(dir, "canvas.png"), tex.EncodeToPNG());
            Destroy(tex);
        }

        void AppendSamples(StringBuilder sb)
        {
            if (current == null || current.Target == null) return;
            var rt = current.Target;
            // Canvas y grows downward in CSS and the RT stores that row at (height - 1 - y).
            int scale = Math.Max(1, rt.width / ParityPages.CssWidth);
            sb.Append(",\"samples\":[");
            int[] xs = { 20, 215, 215, 215 };
            int[] ys = { 200, 200, 430, 500 };
            string[] names = { "bg", "bgMid", "emoji", "belowHero" };
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var one = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
            for (int i = 0; i < xs.Length; i++)
            {
                int px = Mathf.Clamp(xs[i] * scale, 0, rt.width - 1);
                int py = Mathf.Clamp(rt.height - 1 - ys[i] * scale, 0, rt.height - 1);
                one.ReadPixels(new Rect(px, py, 1, 1), 0, 0, false);
                one.Apply(false);
                var c = one.GetPixel(0, 0);
                if (i > 0) sb.Append(',');
                sb.Append("{\"n\":\"").Append(names[i]).Append("\",\"rgb\":[")
                    .Append(Mathf.RoundToInt(c.r * 255)).Append(',')
                    .Append(Mathf.RoundToInt(c.g * 255)).Append(',')
                    .Append(Mathf.RoundToInt(c.b * 255)).Append(',')
                    .Append(Mathf.RoundToInt(c.a * 255)).Append("]}");
            }
            sb.Append(']');
            RenderTexture.active = prev;
            Destroy(one);
        }

        string RunDiag()
        {
            var sb = new StringBuilder();
            bool savedCache = HomeChrome.UseLayerCache;
            HomeChrome.UseLayerCache = false;
            current.InvalidateLayerCache();
            int[] cuts = { 0, 12, 13, 25, 47, 54, 58, 60, 63, 75, 90, 200, 336 };
            foreach (int cut in cuts)
            {
                HomeChrome.DebugStopAfter = cut;
                HomeChrome.DebugFreezeMotion = false;
                HomeChrome.DebugExactBackdrop = false;
                current.Draw(clock.Milliseconds);
                sb.Append("cut ").Append(cut).Append(' ').Append(SampleNote(current.Target)).Append('\n');
            }
            HomeChrome.DebugStopAfter = -1;
            HomeChrome.DebugFreezeMotion = true;
            current.InvalidateLayerCache();
            current.Draw(clock.Milliseconds);
            sb.Append("freezeMotion ").Append(SampleNote(current.Target)).Append('\n');
            HomeChrome.DebugFreezeMotion = false;
            HomeChrome.DebugExactBackdrop = true;
            current.Draw(clock.Milliseconds);
            sb.Append("exactBackdrop ").Append(SampleNote(current.Target)).Append('\n');
            HomeChrome.DebugExactBackdrop = false;
            current.Draw();
            sb.Append("frozen ").Append(SampleNote(current.Target)).Append('\n');
            sb.Append("motionImage ").Append(MotionImageProbe()).Append('\n');
            HomeChrome.UseLayerCache = savedCache;
            HomeChrome.DebugStopAfter = -1;
            HomeChrome.DebugFreezeMotion = false;
            HomeChrome.DebugExactBackdrop = false;
            return sb.ToString();
        }

        static string MotionImageProbe()
        {
            var canvas = new Canvas2D(128, 128);
            try
            {
                var img = CanvasEmoji.Get("😋");
                if (img == null) return "no-emoji";
                canvas.BeginFrame();
                canvas.SetTransform(3, 0, 0, 3, 0, 0);
                canvas.Translate(20, 20);
                canvas.Transform(1.02, 0, 0, 1.02, 0, -1);
                canvas.Translate(-20, -20);
                canvas.DrawImage(img, 8, 8, 24, 24);
                canvas.EndFrame();
                var prev = RenderTexture.active;
                RenderTexture.active = canvas.Target;
                var one = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
                // Face center in user space is (20, 20), times scale 3, plus the motion around (20,20) which keeps it.
                one.ReadPixels(new Rect(60, 128 - 1 - 60, 1, 1), 0, 0, false);
                one.Apply(false);
                var c = one.GetPixel(0, 0);
                RenderTexture.active = prev;
                UnityEngine.Object.Destroy(one);
                return Mathf.RoundToInt(c.r * 255) + "," + Mathf.RoundToInt(c.g * 255) + "," + Mathf.RoundToInt(c.b * 255) + "," + Mathf.RoundToInt(c.a * 255);
            }
            finally { canvas.Dispose(); }
        }

        static void ApplyProbeGameView()
        {
            int w = 1284, h = 2778;
            var args = Environment.GetCommandLineArgs();
            int iw = Array.IndexOf(args, "-probe-w");
            int ih = Array.IndexOf(args, "-probe-h");
            if (iw >= 0 && iw + 1 < args.Length) int.TryParse(args[iw + 1], out w);
            if (ih >= 0 && ih + 1 < args.Length) int.TryParse(args[ih + 1], out h);
            try
            {
                var windowType = Type.GetType("UnityEditor.PlayModeWindow, UnityEditor");
                if (windowType == null) return;
                var viewTypes = windowType.GetNestedType("PlayModeViewTypes");
                var gameView = Enum.Parse(viewTypes, "GameView");
                windowType.GetMethod("SetViewType").Invoke(null, new[] { gameView });
                windowType.GetMethod("SetCustomRenderingResolution").Invoke(null, new object[] { (uint)w, (uint)h, "Probe " + w + "x" + h });
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning("[EditorPlayProbe] game view " + ex.Message);
            }
        }

        static string SampleNote(RenderTexture rt)
        {
            if (rt == null) return "no-rt";
            int scale = Math.Max(1, rt.width / ParityPages.CssWidth);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var one = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
            var sb = new StringBuilder();
            int[] xs = { 20, 215 };
            int[] ys = { 200, 430 };
            for (int i = 0; i < xs.Length; i++)
            {
                int px = Mathf.Clamp(xs[i] * scale, 0, rt.width - 1);
                int py = Mathf.Clamp(rt.height - 1 - ys[i] * scale, 0, rt.height - 1);
                one.ReadPixels(new Rect(px, py, 1, 1), 0, 0, false);
                one.Apply(false);
                var c = one.GetPixel(0, 0);
                if (i > 0) sb.Append(' ');
                sb.Append(Mathf.RoundToInt(c.r * 255)).Append(',')
                    .Append(Mathf.RoundToInt(c.g * 255)).Append(',')
                    .Append(Mathf.RoundToInt(c.b * 255));
            }
            RenderTexture.active = prev;
            UnityEngine.Object.Destroy(one);
            return sb.ToString();
        }

        static string IsolatedGradient()
        {
            var canvas = new Canvas2D(64, 64);
            try
            {
                var g = canvas.CreateRadialGradient(32, 19, 0, 32, 19, 40);
                g.AddColorStop(0, "rgb(49, 46, 129)");
                g.AddColorStop(0.6, "rgb(30, 27, 75)");
                g.AddColorStop(1, "rgb(15, 23, 42)");
                canvas.BeginFrame();
                canvas.FillStyle = g;
                canvas.FillRect(0, 0, 64, 64);
                canvas.EndFrame();
                var prev = RenderTexture.active;
                RenderTexture.active = canvas.Target;
                var one = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
                one.ReadPixels(new Rect(8, 40, 1, 1), 0, 0, false);
                one.Apply(false);
                var c = one.GetPixel(0, 0);
                RenderTexture.active = prev;
                UnityEngine.Object.Destroy(one);
                var img = CanvasEmoji.Get("😋");
                canvas.BeginFrame();
                canvas.DrawImage(img, 12, 12, 40, 40);
                canvas.EndFrame();
                RenderTexture.active = canvas.Target;
                var two = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
                two.ReadPixels(new Rect(32, 32, 1, 1), 0, 0, false);
                two.Apply(false);
                var e = two.GetPixel(0, 0);
                RenderTexture.active = prev;
                UnityEngine.Object.Destroy(two);
                return "grad " + Mathf.RoundToInt(c.r * 255) + "," + Mathf.RoundToInt(c.g * 255) + "," + Mathf.RoundToInt(c.b * 255)
                    + " face " + Mathf.RoundToInt(e.r * 255) + "," + Mathf.RoundToInt(e.g * 255) + "," + Mathf.RoundToInt(e.b * 255) + "," + Mathf.RoundToInt(e.a * 255);
            }
            finally { canvas.Dispose(); }
        }

        static void AppendEmoji(StringBuilder sb)
        {
            sb.Append(",\"emoji\":");
            var img = CanvasEmoji.Get("😋");
            if (img == null || img.texture == null)
            {
                sb.Append("null");
                return;
            }
            var tex = img.texture;
            sb.Append("{\"w\":").Append(tex.width).Append(",\"h\":").Append(tex.height)
                .Append(",\"format\":\"").Append(tex.format).Append("\",\"srgb\":").Append(tex.isDataSRGB ? "true" : "false").Append('}');
        }

        static void AppendStats(StringBuilder sb, string name, double[] values, int n)
        {
            double avg = 0, mx = 0;
            var copy = new double[n];
            for (int i = 0; i < n; i++)
            {
                copy[i] = values[i];
                avg += values[i];
                if (values[i] > mx) mx = values[i];
            }
            if (n > 0) avg /= n;
            Array.Sort(copy);
            var inv = CultureInfo.InvariantCulture;
            sb.Append(",\"").Append(name).Append("\":{\"avg\":").Append(avg.ToString("R", inv))
                .Append(",\"p95\":").Append(Percentile(copy, 0.95).ToString("R", inv))
                .Append(",\"p99\":").Append(Percentile(copy, 0.99).ToString("R", inv))
                .Append(",\"max\":").Append(mx.ToString("R", inv)).Append('}');
        }

        static void AppendLongStats(StringBuilder sb, string name, long[] values, int n)
        {
            double avg = 0;
            long mx = 0;
            var copy = new double[n];
            for (int i = 0; i < n; i++)
            {
                copy[i] = values[i];
                avg += values[i];
                if (values[i] > mx) mx = values[i];
            }
            if (n > 0) avg /= n;
            Array.Sort(copy);
            var inv = CultureInfo.InvariantCulture;
            sb.Append(",\"").Append(name).Append("\":{\"avg\":").Append(avg.ToString("R", inv))
                .Append(",\"p95\":").Append(Percentile(copy, 0.95).ToString("R", inv))
                .Append(",\"max\":").Append(mx).Append('}');
        }

        static double Percentile(double[] sorted, double p)
        {
            if (sorted.Length == 0) return 0;
            double rank = (sorted.Length - 1) * p;
            int lo = (int)Math.Floor(rank);
            int hi = (int)Math.Ceiling(rank);
            if (lo == hi) return sorted[lo];
            return sorted[lo] + (sorted[hi] - sorted[lo]) * (rank - lo);
        }

        static bool EditorFocused()
        {
            var t = Type.GetType("UnityEditorInternal.InternalEditorUtility, UnityEditor");
            if (t == null) return false;
            var prop = t.GetProperty("isApplicationActive", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (prop == null) return false;
            return prop.GetValue(null, null) is bool b && b;
        }

        static void ExitEditor(int code)
        {
            var t = Type.GetType("UnityEditor.EditorApplication, UnityEditor");
            var method = t != null ? t.GetMethod("Exit", new[] { typeof(int) }) : null;
            if (method != null) method.Invoke(null, new object[] { code });
            else Application.Quit(code);
        }
    }
}
