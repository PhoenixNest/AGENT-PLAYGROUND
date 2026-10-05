using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace OrbitalSalvage.Render.Editor
{
    /// <summary>
    /// Batchmode replayer. Usage (no -nographics, a real D3D11 device is required):
    /// Unity.exe -batchmode -projectPath P -executeMethod OrbitalSalvage.Render.Editor.CanvasConformance.Run
    ///   -streams DIR_OR_FILE -out DIR [-filter SUBSTRING] [-perf N] -logFile LOG
    /// </summary>
    public static class CanvasConformance
    {
        static string Arg(string[] args, string name)
        {
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == name) return args[i + 1];
            return null;
        }

        public static void Run()
        {
            int code = 0;
            try { code = RunImpl(); }
            catch (Exception e) { Debug.LogError("[CanvasConformance] " + e); code = 1; }
            EditorApplication.Exit(code);
        }

        /// <summary>Batchmode: draws the home screen from the web spec (HomeChrome) for both profiles into <c>-out DIR/home_default__&lt;profile&gt;.png</c>.</summary>
        public static void HomeChromeRender()
        {
            int code = 0;
            try
            {
                string outDir = Arg(Environment.GetCommandLineArgs(), "-out");
                Directory.CreateDirectory(outDir);
                foreach (var profile in new[] { "zero", "iphone" })
                {
                    using (var chrome = HomeChrome.Load(profile))
                    {
                        chrome.Draw();
                        Sync(chrome.Target);
                        WritePng(chrome.Target, Path.Combine(outDir, "home_default__" + profile + ".png"));
                        var sb = new StringBuilder();
                        foreach (var kv in chrome.Deferred) sb.Append(kv.Key + " x" + kv.Value + "; ");
                        Debug.Log("[HomeChrome] " + profile + " boxes=" + chrome.DrawnBoxes + " runs=" + chrome.DrawnRuns + " emoji=" + chrome.DrawnEmoji + " deferred: " + sb);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[HomeChromeRender] " + ex);
                code = 1;
            }
            EditorApplication.Exit(code);
        }

        /// <summary>
        /// Batchmode: writes <c>-out DIR/unity-metrics.json</c> with Canvas2D.MeasureText widths for the fonts and strings of
        /// <c>probe-metrics.mjs</c> so they can be compared with Chromium's <c>measureText</c> (out/metrics.json).
        /// </summary>
        public static void TextMetrics()
        {
            int code = 0;
            try
            {
                string outDir = Arg(Environment.GetCommandLineArgs(), "-out");
                Directory.CreateDirectory(outDir);
                string[] fonts =
                {
                    "bold 13px Fredoka, sans-serif", "900 10.5px Fredoka, sans-serif", "bold 10px Fredoka, sans-serif", "bold 9.5px Fredoka, sans-serif",
                    "900 16px Fredoka, sans-serif", "400 14px Fredoka, sans-serif", "11px Nunito, sans-serif", "800 14px Nunito, sans-serif", "bold 40px Fredoka, sans-serif",
                };
                string[] strings = { "Hxg", "SORT RIG 0123456789 \u00D7\u2022", "To Wa", "AVATAR", "fi fl", "ORBITAL x2 +3 \u2713 \u25B6 \u2190",
                    "Hamburgefonstiv AVATAR To Wa", "SPARKLE PLANETARY COSMIC RIG fi fl", "AV AV", "A V" };
                string wordsFile = Arg(Environment.GetCommandLineArgs(), "-words");
                if (wordsFile != null)
                {
                    var list = (List<object>)MiniJson.Parse(File.ReadAllText(wordsFile));
                    strings = new string[list.Count];
                    for (int i = 0; i < list.Count; i++) strings[i] = (string)list[i];
                    fonts = new[] { "bold 13px Fredoka, sans-serif", "400 14px Fredoka, sans-serif", "11px Nunito, sans-serif" };
                }
                var sb = new StringBuilder("{\n");
                using (var canvas = new Canvas2D(16, 16))
                {
                    for (int i = 0; i < fonts.Length; i++)
                    {
                        canvas.Font = fonts[i];
                        sb.Append("  \"" + fonts[i] + "\": {");
                        for (int j = 0; j < strings.Length; j++)
                            sb.Append((j == 0 ? "" : ", ") + "\"" + strings[j] + "\": " + canvas.MeasureText(strings[j]).ToString("R", CultureInfo.InvariantCulture));
                        sb.Append("}" + (i + 1 < fonts.Length ? "," : "") + "\n");
                    }
                }
                sb.Append("}\n");
                File.WriteAllText(Path.Combine(outDir, "unity-metrics.json"), sb.ToString(), new UTF8Encoding(false));
            }
            catch (Exception e) { Debug.LogError("[CanvasConformance] " + e); code = 1; }
            EditorApplication.Exit(code);
        }

        static int RunImpl()
        {
            var args = Environment.GetCommandLineArgs();
            string streams = Arg(args, "-streams");
            string outDir = Arg(args, "-out");
            string filter = Arg(args, "-filter");
            string perfArg = Arg(args, "-perf");
            int perfN = perfArg != null ? int.Parse(perfArg) : 0;
            if (streams == null || outDir == null) throw new ArgumentException("-streams and -out are required");
            for (int a = 0; a < args.Length - 1; a++)
            {
                if (args[a] != "-set") continue;
                int eq = args[a + 1].IndexOf('=');
                string fieldName = args[a + 1].Substring(0, eq), value = args[a + 1].Substring(eq + 1);
                var field = typeof(Canvas2D).GetField(fieldName);
                if (field == null) throw new ArgumentException("unknown Canvas2D static: " + fieldName);
                object parsed = field.FieldType == typeof(bool) ? (object)(value == "1" || value == "true") : Convert.ChangeType(value, field.FieldType, CultureInfo.InvariantCulture);
                field.SetValue(null, parsed);
                Debug.Log("[CanvasConformance] set " + fieldName + " = " + parsed);
            }
            var combos = new List<KeyValuePair<string, List<KeyValuePair<System.Reflection.FieldInfo, object>>>>();
            combos.Add(new KeyValuePair<string, List<KeyValuePair<System.Reflection.FieldInfo, object>>>("", new List<KeyValuePair<System.Reflection.FieldInfo, object>>()));
            for (int a = 0; a < args.Length - 1; a++)
            {
                if (args[a] != "-sweep") continue;
                int eq = args[a + 1].IndexOf('=');
                string fieldName = args[a + 1].Substring(0, eq);
                var field = typeof(Canvas2D).GetField(fieldName);
                if (field == null) throw new ArgumentException("unknown Canvas2D static: " + fieldName);
                var next = new List<KeyValuePair<string, List<KeyValuePair<System.Reflection.FieldInfo, object>>>>();
                foreach (var combo in combos)
                    foreach (var v in args[a + 1].Substring(eq + 1).Split(','))
                    {
                        object parsed = field.FieldType == typeof(bool) ? (object)(v == "1" || v == "true") : Convert.ChangeType(v, field.FieldType, CultureInfo.InvariantCulture);
                        var sets = new List<KeyValuePair<System.Reflection.FieldInfo, object>>(combo.Value) { new KeyValuePair<System.Reflection.FieldInfo, object>(field, parsed) };
                        next.Add(new KeyValuePair<string, List<KeyValuePair<System.Reflection.FieldInfo, object>>>(combo.Key + "~" + fieldName + "=" + v, sets));
                    }
                combos = next;
            }
            Directory.CreateDirectory(outDir);

            var gdt = SystemInfo.graphicsDeviceType;
            Debug.Log("[CanvasConformance] graphics device: " + gdt + " / " + SystemInfo.graphicsDeviceName + " / " + SystemInfo.graphicsDeviceVersion);
            if (gdt == UnityEngine.Rendering.GraphicsDeviceType.Null) throw new InvalidOperationException("no graphics device (running with -nographics?)");
            if (gdt != UnityEngine.Rendering.GraphicsDeviceType.Direct3D11) Debug.LogWarning("[CanvasConformance] expected Direct3D11, got " + gdt);

            var files = new List<string>();
            if (File.Exists(streams)) files.Add(streams);
            else
            {
                foreach (var f in Directory.GetFiles(streams, "*.json"))
                {
                    if (Path.GetFileName(f) == "index.json") continue;
                    files.Add(f);
                }
                files.Sort(StringComparer.Ordinal);
            }

            var perf = new StringBuilder("[\n");
            Canvas2D canvas = null;
            int failures = 0;
            foreach (var file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);
                if (!string.IsNullOrEmpty(filter) && name.IndexOf(filter, StringComparison.Ordinal) < 0) continue;
                try
                {
                    var player = StreamPlayer.Load(File.ReadAllText(file));
                    if (canvas == null || canvas.Width != player.Width || canvas.Height != player.Height)
                    {
                        canvas?.Dispose();
                        canvas = new Canvas2D(player.Width, player.Height);
                    }
                    double recordMs = 0, submitMs = 0;
                    foreach (var combo in combos)
                    {
                        foreach (var set in combo.Value) set.Key.SetValue(null, set.Value);
                        var sw = Stopwatch.StartNew();
                        canvas.BeginFrame();
                        player.Play(canvas);
                        recordMs = sw.Elapsed.TotalMilliseconds;
                        canvas.EndFrame();
                        submitMs = sw.Elapsed.TotalMilliseconds - recordMs;
                        WritePng(canvas.Target, Path.Combine(outDir, name + combo.Key + ".png"));
                    }

                    double avgRecord = 0, avgSubmit = 0, avgTotal = 0, avgReplay = 0;
                    if (perfN > 0)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            canvas.BeginFrame();
                            player.Play(canvas);
                            canvas.EndFrame();
                        }
                        Sync(canvas.Target);
                        var recs = new List<double>(); var subs = new List<double>(); var tots = new List<double>();
                        for (int i = 0; i < perfN; i++)
                        {
                            var t0 = Stopwatch.StartNew();
                            canvas.BeginFrame();
                            player.Play(canvas);
                            double r = t0.Elapsed.TotalMilliseconds;
                            canvas.EndFrame();
                            double s2 = t0.Elapsed.TotalMilliseconds - r;
                            Sync(canvas.Target);
                            recs.Add(r); subs.Add(s2); tots.Add(t0.Elapsed.TotalMilliseconds);
                        }
                        avgTotal = Median(tots);
                        avgRecord = Median(recs);
                        avgSubmit = Median(subs);
                        var rp = Stopwatch.StartNew();
                        for (int i = 0; i < perfN; i++) canvas.Replay();
                        Sync(canvas.Target);
                        avgReplay = rp.Elapsed.TotalMilliseconds / perfN;
                    }
                    perf.Append("  {\"name\":\"" + name + "\",\"width\":" + player.Width + ",\"height\":" + player.Height + ",\"ops\":" + player.Ops.Count +
                        ",\"edges\":" + canvas.EdgeCount + ",\"draws\":" + canvas.DrawCount + ",\"avgRecordMs\":" + F(avgRecord) + ",\"avgSubmitMs\":" + F(avgSubmit) +
                        ",\"avgReplayMs\":" + F(avgReplay) + ",\"avgFrameMs\":" + F(avgTotal) + "},\n");
                    Debug.Log("[CanvasConformance] rendered " + name + " ops=" + player.Ops.Count + " edges=" + canvas.EdgeCount + " draws=" + canvas.DrawCount);
                    player.Dispose();
                }
                catch (Exception e)
                {
                    failures++;
                    Debug.LogError("[CanvasConformance] " + name + " failed: " + e);
                }
            }
            canvas?.Dispose();
            if (perf.Length > 2) perf.Length -= 2;
            perf.Append("\n]\n");
            File.WriteAllText(Path.Combine(outDir, "perf.json"), perf.ToString());
            return failures == 0 ? 0 : 2;
        }

        static double Median(List<double> v)
        {
            var a = v.ToArray();
            Array.Sort(a);
            return a.Length == 0 ? 0 : a[a.Length / 2];
        }

        static string F(double v) => v.ToString("F3", CultureInfo.InvariantCulture);

        static void Sync(RenderTexture rt)
        {
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var t = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
            t.ReadPixels(new Rect(0, 0, 1, 1), 0, 0, false);
            UnityEngine.Object.DestroyImmediate(t);
            RenderTexture.active = prev;
        }

        public static void WritePng(RenderTexture rt, string path)
        {
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false, true);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
            tex.Apply(false, false);
            RenderTexture.active = prev;
            var px = tex.GetPixels32();
            for (int i = 0; i < px.Length; i++)
            {
                var c = px[i];
                if (c.a == 0) { px[i] = new Color32(0, 0, 0, 0); continue; }
                if (c.a == 255) continue;
                int a = c.a;
                px[i] = new Color32((byte)Math.Min(255, (c.r * 255 + a / 2) / a), (byte)Math.Min(255, (c.g * 255 + a / 2) / a),
                    (byte)Math.Min(255, (c.b * 255 + a / 2) / a), c.a);
            }
            tex.SetPixels32(px);
            tex.Apply(false, false);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }
    }
}
