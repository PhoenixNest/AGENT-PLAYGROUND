using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using OrbitalSalvage.App;
using OrbitalSalvage.Render;
using OrbitalSalvage.Viewer.Editor;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Tests
{
    public class QuotaAndModalTests
    {
        const string GameHash = "38730953997DE30D033C10E6CFF0B9AE2E47FBAB1671D07178039B484609A12A";
        const string PauseHash = "CE4A6AE83A5563E98F7F722334E17A7FC06FA1375C26390A9714FC838FEE7078";

        [Test]
        public void GameAndPauseStills_MatchKnownHashes()
        {
            OutlineFonts.Clear();
            string game = Sha256(Paint("game_lvl01_t1000ms", true, 0));
            OutlineFonts.Clear();
            string pause = Sha256(Paint("modal_pause", true, 0));
            Assert.AreEqual(GameHash, game);
            Assert.AreEqual(PauseHash, pause);
        }

        [Test]
        public void CollectedMass_DrivesTheViewerBarWidth_RestartDoesNotLeaveIt()
        {
            var session = LiveGameSession.CreatePlay(ParityPages.CssWidth, ParityPages.CssHeight, 1);
            var chrome = HomeChrome.Load("game_lvl01_t1000ms", "zero");
            var go = new GameObject("quota-probe");
            try
            {
                var viewer = go.AddComponent<ParityViewer>();
                session.CollectedMass = session.Quota * 0.5;
                viewer.ProbeApplyQuota(session, chrome, "game_lvl01_t1000ms");
                double filled = chrome.WidthOverrides["hud-quota-bar"];
                Assert.AreEqual(session.QuotaBarWidth(AppController.QuotaTrackWidth), filled, 1e-6);
                Assert.Greater(filled, 1);

                session.Restart();
                viewer.ProbeApplyQuota(session, chrome, "game_lvl01_t1000ms");
                Assert.AreEqual(0, chrome.WidthOverrides["hud-quota-bar"], 1e-6);

                chrome.WidthOverrides["hud-quota-bar"] = filled;
                viewer.ProbeApplyQuota(session, chrome, "game_lvl01_t1000ms");
                Assert.AreEqual(0, chrome.WidthOverrides["hud-quota-bar"], 1e-6, "same quota label must not keep a stale width");
            }
            finally
            {
                chrome.Dispose();
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GamePageModals_AreLiveTemplates()
        {
            var ids = GameModalIds();
            Assert.AreEqual(10, ids.Count);
            foreach (string id in ids)
            {
                Assert.IsTrue(id.StartsWith("modal_", StringComparison.Ordinal), id);
                Assert.IsTrue(ParityViewer.IsGamePageTemplate(id), id);
            }
            Assert.IsTrue(ParityViewer.IsGamePageTemplate("game_lvl01_t1000ms"));
            Assert.IsTrue(ParityViewer.IsGamePageTemplate("game_daily_bronze_t1000"));
            Assert.IsFalse(ParityViewer.IsGamePageTemplate("settings_modal_privacy"));
            Assert.IsFalse(ParityViewer.IsGamePageTemplate("settings_modal_reset_confirm"));
            Assert.IsFalse(ParityViewer.IsGamePageTemplate("settings_modal_language"));
            Assert.IsFalse(ParityViewer.IsGamePageTemplate("home_default"));
        }

        [Test]
        public void GamePageModals_FirstFrameBindsTheLivePlayfield()
        {
            var session = LiveGameSession.CreatePlay(ParityPages.CssWidth, ParityPages.CssHeight, 1);
            var go = new GameObject("modal-probe");
            try
            {
                var viewer = go.AddComponent<ParityViewer>();
                Assert.AreEqual(0, session.RenderFrames);
                foreach (string id in GameModalIds())
                {
                    var bound = viewer.ProbeFirstModalFrame(id, session);
                    HomeChrome bare = null;
                    try
                    {
                        Assert.IsNotNull(bound.ExternalPlayfield, id);
                        Assert.AreSame(session.Renderer.Target, bound.ExternalPlayfield, id);
                        Assert.Greater(session.RenderFrames, 0, id);

                        bare = HomeChrome.Load(id, "zero");
                        bare.SkipPlayfield = true;
                        bare.Draw(0, -1, null, true, true);
                        AssertPlayfieldDiffers(bound, bare, id);
                    }
                    finally
                    {
                        bound.Dispose();
                        if (bare != null) bare.Dispose();
                    }
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        static void AssertPlayfieldDiffers(HomeChrome bound, HomeChrome bare, string id)
        {
            int[] xs = { 24, 40, 80 };
            int[] ys = { 280, 400, 520 };
            int best = 0;
            Color32 boundPx = default, barePx = default;
            for (int i = 0; i < xs.Length; i++)
            {
                var a = Sample(bound, xs[i], ys[i]);
                var b = Sample(bare, xs[i], ys[i]);
                int d = Math.Max(Math.Abs(a.r - b.r), Math.Max(Math.Abs(a.g - b.g), Math.Abs(a.b - b.b)));
                if (d > best)
                {
                    best = d;
                    boundPx = a;
                    barePx = b;
                }
            }
            Assert.Greater(best, 20, id + " bound " + boundPx + " bare " + barePx);
        }

        static List<string> GameModalIds()
        {
            var ids = new List<string>();
            var pause = Base();
            pause.Page = "game";
            pause.Modal = "pause";
            ids.Add(TemplateSelector.Select(pause, out _));

            var debrief = Base();
            debrief.Page = "game";
            debrief.Modal = "debrief";
            ids.Add(TemplateSelector.Select(debrief, out _));
            debrief.DebriefSurplus = true;
            ids.Add(TemplateSelector.Select(debrief, out _));
            debrief.DebriefSurplus = false;
            debrief.WorldIndex = 3;
            ids.Add(TemplateSelector.Select(debrief, out _));
            debrief.WorldIndex = 5;
            ids.Add(TemplateSelector.Select(debrief, out _));
            debrief.WorldIndex = 1;
            debrief.DebriefDaily = true;
            ids.Add(TemplateSelector.Select(debrief, out _));

            var defeat = Base();
            defeat.Page = "game";
            defeat.Modal = "defeat";
            defeat.DefeatRatio = 0.60;
            ids.Add(TemplateSelector.Select(defeat, out _));
            defeat.DefeatUsed = true;
            ids.Add(TemplateSelector.Select(defeat, out _));
            defeat.DefeatUsed = false;
            defeat.DefeatRatio = 0.80;
            ids.Add(TemplateSelector.Select(defeat, out _));
            defeat.DefeatRatio = 0.20;
            ids.Add(TemplateSelector.Select(defeat, out _));
            return ids;
        }

        static TemplateQuery Base()
        {
            return new TemplateQuery
            {
                Page = "game",
                Modal = "",
                ShopTab = "rings",
                ShopSignature = TemplateSelector.RingsDefault,
                SlidersDefault = true,
                SoundOn = true,
                HapticsOn = true,
                WorldIndex = 1,
                ActiveWorld = 1,
                WorldTotal = 10
            };
        }

        static byte[] Paint(string id, bool capturePose, double timeMs)
        {
            Texture2D tex = null;
            try
            {
                tex = PagePreviewPainter.Render(id, "zero", capturePose, timeMs, 0, out byte[] png);
                Assert.IsNotNull(png, id);
                return png;
            }
            finally
            {
                if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
            }
        }

        static string Sha256(byte[] data)
        {
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(data);
                var sb = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("X2"));
                return sb.ToString();
            }
        }

        static Color32 Sample(HomeChrome chrome, int cssX, int cssY)
        {
            var rt = chrome.Target;
            int scale = System.Math.Max(1, rt.width / 430);
            int px = Mathf.Clamp(cssX * scale, 0, rt.width - 1);
            int py = Mathf.Clamp(rt.height - 1 - cssY * scale, 0, rt.height - 1);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var one = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
            one.ReadPixels(new Rect(px, py, 1, 1), 0, 0, false);
            one.Apply(false);
            var c = one.GetPixel(0, 0);
            RenderTexture.active = prev;
            UnityEngine.Object.DestroyImmediate(one);
            return new Color32((byte)Mathf.RoundToInt(c.r * 255), (byte)Mathf.RoundToInt(c.g * 255), (byte)Mathf.RoundToInt(c.b * 255), (byte)Mathf.RoundToInt(c.a * 255));
        }
    }
}
