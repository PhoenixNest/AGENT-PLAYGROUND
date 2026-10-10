using System.IO;
using UnityEditor;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Editor
{
    /// <summary>
    /// Batch stills for the GPU-cost A/B. Writes PNG plus SHA256 under the directory in
    /// <c>-gpuStillDir</c> (default <c>/tmp/maze-gpu</c>). Does not change product prefs.
    /// </summary>
    public static class GpuStillDump
    {
        public static void Run()
        {
            string dir = "/tmp/maze-gpu";
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "-gpuStillDir") dir = args[i + 1];
            Directory.CreateDirectory(dir);
            string tag = "off";
            string opt = "";
            double liveMs = double.NaN;
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-gpuStillTag") tag = args[i + 1];
                else if (args[i] == "-gpuStillOpt") opt = args[i + 1];
                else if (args[i] == "-gpuStillLive") double.TryParse(args[i + 1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out liveMs);
            }
            GpuOpt.Apply(opt);
            OrbitalSalvage.Render.RenderOpts.ShadowScope = GpuOpt.ShadowScope;
            OrbitalSalvage.Render.RenderOpts.ShadowLow = GpuOpt.ShadowLow;
            OrbitalSalvage.Render.RenderOpts.BackdropScope = GpuOpt.BackdropScope;
            OrbitalSalvage.Render.RenderOpts.BackdropLow = GpuOpt.BackdropLow;
            OrbitalSalvage.Render.RenderOpts.Dirty = GpuOpt.Dirty;
            OrbitalSalvage.Render.RenderOpts.GameOne = GpuOpt.GameOne;
            string[] ids = { "home_default", "maps_world_1_fresh", "game_lvl01_t1000ms", "modal_pause" };
            var log = new System.Text.StringBuilder();
            for (int i = 0; i < ids.Length; i++)
            {
                var tex = PagePreviewPainter.Render(ids[i], "zero", true, 0, 2, out byte[] png);
                string path = Path.Combine(dir, tag + "-" + ids[i] + ".png");
                File.WriteAllBytes(path, png);
                string hash;
                using (var sha = System.Security.Cryptography.SHA256.Create())
                {
                    byte[] h = sha.ComputeHash(png);
                    var sb = new System.Text.StringBuilder(h.Length * 2);
                    for (int b = 0; b < h.Length; b++) sb.Append(h[b].ToString("X2"));
                    hash = sb.ToString();
                }
                log.Append(ids[i]).Append(' ').Append(tex.width).Append('x').Append(tex.height).Append(' ').Append(hash).Append('\n');
                Object.DestroyImmediate(tex);
                Debug.Log("[GpuStill] " + path + " " + hash);
            }
            if (!double.IsNaN(liveMs))
            {
                var chrome = OrbitalSalvage.Render.HomeChrome.Load("home_default", "zero");
                try
                {
                    chrome.ResizeRaster(2);
                    chrome.Draw(liveMs);
                    chrome.Draw(liveMs + 40);
                    var source = chrome.Target;
                    var copy = new RenderTexture(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                    copy.Create();
                    Graphics.CopyTexture(source, copy);
                    var prev = RenderTexture.active;
                    RenderTexture.active = copy;
                    var read = new Texture2D(copy.width, copy.height, TextureFormat.RGB24, false, true);
                    read.ReadPixels(new Rect(0, 0, copy.width, copy.height), 0, 0, false);
                    read.Apply(false);
                    RenderTexture.active = prev;
                    byte[] live = read.EncodeToPNG();
                    string livePath = Path.Combine(dir, tag + "-home_live.png");
                    File.WriteAllBytes(livePath, live);
                    Object.DestroyImmediate(read);
                    copy.Release();
                    Object.DestroyImmediate(copy);
                    Debug.Log("[GpuStill] " + livePath);
                }
                finally { chrome.Dispose(); }
            }
            File.WriteAllText(Path.Combine(dir, tag + "-hashes.txt"), log.ToString());
            EditorApplication.Exit(0);
        }
    }
}
