using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace OrbitalSalvage.Render
{
    /// <summary>
    /// Drives <see cref="RecordedFrameDriver"/> and writes playfield PNGs next to StreamPlayer replays
    /// of the recorded Chromium command streams.
    /// </summary>
    public static class LiveConformance
    {
        static readonly int[] Sectors = { 1, 11, 21, 31, 41 };

        public static void Export(string outDir, string streamDir)
        {
            Directory.CreateDirectory(outDir);
            string liveDir = Path.Combine(outDir, "live");
            string refDir = Path.Combine(outDir, "stream");
            Directory.CreateDirectory(liveDir);
            Directory.CreateDirectory(refDir);
            var log = new System.Text.StringBuilder();
            for (int s = 0; s < Sectors.Length; s++)
            {
                int sector = Sectors[s];
                var session = RecordedFrameDriver.Create(sector);
                string pad = sector.ToString("00", CultureInfo.InvariantCulture);
                RecordedFrameDriver.Run(session, label =>
                {
                    string name = "s" + pad + "_" + label;
                    File.WriteAllBytes(Path.Combine(liveDir, name + ".png"), Encode(session.Renderer.Target));
                    string streamPath = Path.Combine(streamDir, name + ".json");
                    if (!File.Exists(streamPath))
                    {
                        log.Append(name).Append(" missing stream\n");
                        return;
                    }
                    var player = StreamPlayer.Load(File.ReadAllText(streamPath));
                    var canvas = new Canvas2D(player.Width, player.Height);
                    canvas.BeginFrame();
                    player.Play(canvas);
                    canvas.EndFrame();
                    File.WriteAllBytes(Path.Combine(refDir, name + ".png"), Encode(canvas.Target));
                    canvas.Dispose();
                    player.Dispose();
                    log.Append(name).Append(" steps ").Append(session.PhysicsSteps).Append('\n');
                });
                session.Renderer.Dispose();
            }
            bool same = SameHash();
            log.Append("deterministic ").Append(same ? "true" : "false").Append('\n');
            File.WriteAllText(Path.Combine(outDir, "export.log"), log.ToString());
            if (!same) throw new InvalidOperationException("live renderer PNG differed across two identical runs");
        }

        static bool SameHash()
        {
            byte[] a = OneRun();
            byte[] b = OneRun();
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
            return true;
        }

        static byte[] OneRun()
        {
            var session = RecordedFrameDriver.Create(1);
            session.Update(RecordedFrameDriver.FrameDt);
            if (session.PhysicsSteps != 1)
            {
                session.Renderer.Dispose();
                return new byte[] { 1 };
            }
            var png = Encode(session.Renderer.Target);
            session.Renderer.Dispose();
            return png;
        }

        public static byte[] Encode(RenderTexture source)
        {
            var rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(source, rt);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(source.width, source.height, TextureFormat.RGB24, false, true);
            tex.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);
            tex.Apply(false);
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            byte[] bytes = tex.EncodeToPNG();
            if (Application.isPlaying) UnityEngine.Object.Destroy(tex);
            else UnityEngine.Object.DestroyImmediate(tex);
            return bytes;
        }
    }
}
