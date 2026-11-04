using NUnit.Framework;
using UnityEngine;

namespace OrbitalSalvage.Render.Tests
{
    public class PauseReflowTests
    {
        const string SampleName = "LVL 1 • Bubblegum Chute 1";
        const string SampleGoal = "Goal: Feed Nibbles 20 kg space snacks";
        const string LongName = "BRONZE • Bronze Shift • Cadet Sweep";
        const string LongGoal = "Goal: Feed Nibbles 34 kg (34/34 kg eaten)";
        const double ContentLeft = 50 + 3.5;
        const double ContentRight = 50 + 330 - 3.5;

        [Test]
        public void LongPauseText_WrapsInsideCard_OneLineStaysPut()
        {
            var chrome = HomeChrome.Load("modal_pause", "zero");
            HomeChrome.RecordRuns = true;
            try
            {
                chrome.TextOverrides["pause-level-name"] = SampleName;
                chrome.TextOverrides["pause-level-quota"] = SampleGoal;
                chrome.Draw(1000);
                Assert.IsFalse(chrome.BlockReflowActive, "sample text fits one line");
                int built = chrome.CacheBuilds;
                Assert.Greater(built, 0);
                var name = Find(chrome, "pause-level-name");
                Assert.AreEqual(102.141, name.X, 0.6, "one line keeps the captured run x");
                double resumeY = Find(chrome, "btn-pause-resume").Y;
                double titleY = 0;
                bool sawTitle = false;
                foreach (var run in chrome.RecordedRuns)
                    if (run.W > 100 && run.Y > 480 && run.Y < 520) { titleY = run.Y; sawTitle = true; }
                Assert.IsTrue(sawTitle, "header title run");

                chrome.Draw(1000);
                Assert.AreEqual(built, chrome.CacheBuilds, "one-line text does not rebuild the cache");

                chrome.TextOverrides["pause-level-name"] = LongName;
                chrome.TextOverrides["pause-level-quota"] = LongGoal;
                chrome.Draw(1000);
                Assert.IsTrue(chrome.BlockReflowActive);
                int nameLines = 0, goalLines = 0;
                double titleY2 = 0;
                bool sawTitle2 = false;
                foreach (var run in chrome.RecordedRuns)
                {
                    if (run.Id == "pause-level-name") nameLines++;
                    if (run.Id == "pause-level-quota") goalLines++;
                    if (run.Id == "pause-level-name" || run.Id == "pause-level-quota")
                    {
                        Assert.GreaterOrEqual(run.X, ContentLeft - 0.5, run.Id);
                        Assert.LessOrEqual(run.X + run.W, ContentRight + 0.5, run.Id + " x=" + run.X + " w=" + run.W);
                    }
                    if (run.Id == "btn-pause-resume")
                        Assert.AreEqual(resumeY, run.Y, 0.6, "buttons below the text stay");
                    if (run.W > 100 && run.Y < titleY) { titleY2 = run.Y; sawTitle2 = true; }
                }
                Assert.Greater(nameLines, 1, "long name wraps");
                Assert.GreaterOrEqual(goalLines, 1);
                Assert.IsTrue(sawTitle2, "header moved up, was " + titleY);
                Assert.Less(titleY2, titleY - 10, "card grows upward");
                int rebuilt = chrome.CacheBuilds;
                Assert.Greater(rebuilt, built, "moving the card changes the dynamic set");
                chrome.Draw(1000);
                Assert.AreEqual(rebuilt, chrome.CacheBuilds, "same wrap does not rebuild again");
                UnityEngine.Debug.Log("[reflow] nameLines=" + nameLines + " goalLines=" + goalLines + " titleDy=" + (titleY2 - titleY));
            }
            finally
            {
                HomeChrome.RecordRuns = false;
                chrome.Dispose();
            }
        }

        [Test]
        public void Audit_LongestRealisticText_StaysInPauseCard()
        {
            const string campaign = "LVL 50 • The Kessler Singularity";
            const string goal354 = "Goal: Feed Nibbles 354 kg (354/354 kg eaten)";
            var chrome = HomeChrome.Load("modal_pause", "zero");
            HomeChrome.RecordRuns = true;
            try
            {
                chrome.TextOverrides["pause-level-name"] = campaign;
                chrome.TextOverrides["pause-level-quota"] = goal354;
                chrome.Draw(1000);
                int nameLines = 0, goalLines = 0;
                foreach (var run in chrome.RecordedRuns)
                {
                    if (run.Id != "pause-level-name" && run.Id != "pause-level-quota") continue;
                    if (run.Id == "pause-level-name") nameLines++;
                    else goalLines++;
                    Assert.GreaterOrEqual(run.X, ContentLeft - 0.5, run.Id);
                    Assert.LessOrEqual(run.X + run.W, ContentRight + 0.5, run.Id + " w=" + run.W);
                }
                UnityEngine.Debug.Log("[audit] pause campaign nameLines=" + nameLines + " goalLines=" + goalLines + " reflow=" + chrome.BlockReflowActive);
                Assert.GreaterOrEqual(nameLines, 1);
                Assert.GreaterOrEqual(goalLines, 1);
            }
            finally
            {
                HomeChrome.RecordRuns = false;
                chrome.Dispose();
            }

            LogWidth("home_default", "home-mission-name", "LVL 50 • The Kessler Singularity", 392);
            LogWidth("home_default", "home-mission-goal", "Goal: 354 kg", 392);
            LogWidth("home_default", "home-credits-val", "999999", 318);
            LogWidth("shop_rings_default", "gift-banner-title", "Allowance Claimed! ✨", 92 + 196.875);
            LogWidth("shop_rings_default", "gift-banner-desc", "Come back tomorrow for another +100 🪙 care package!", 92 + 196.875, true);
            LogWidth("shop_rings_default", "gift-banner-desc", "Claim your free daily +100 🪙 allowance.", 92 + 196.875, true);
            LogWidth("game_lvl01_t1000ms", "hud-quota-text", "354 / 354 kg", 165.672 + 162.328, true);
            LogWidth("loading_t0500", "loading-tip-text", "Sprinkling candy on orbital rings...", 119.875 + 190.25);
            LogWidth("loading_t0500", "loading-tip-text", "Calibrating cosmic centrifuges...", 119.875 + 190.25);
            LogWidth("modal_debrief_exact", "debrief-mass-val", "354 kg", 21 + 189);
            LogWidth("modal_defeat_12_of_20", "defeat-mass-val", "354 / 354 kg", 21 + 189);
            LogWidth("modal_defeat_12_of_20", "defeat-spilled-val", "354 kg", 220 + 189);
        }

        static void LogWidth(string state, string id, string text, double limitRight, bool enforce = false)
        {
            var chrome = HomeChrome.Load(state, "zero");
            HomeChrome.RecordRuns = true;
            try
            {
                chrome.TextOverrides[id] = text;
                chrome.Draw(1000);
                int n = 0;
                double right = 0;
                foreach (var run in chrome.RecordedRuns)
                {
                    if (run.Id != id) continue;
                    n++;
                    double edge = run.X + run.W;
                    if (edge > right) right = edge;
                }
                UnityEngine.Debug.Log("[audit] " + state + " " + id + " lines=" + n + " right=" + right.ToString("0.0") + " limit=" + limitRight.ToString("0.0") + " text=" + text);
                if (enforce) Assert.LessOrEqual(right, limitRight + 0.6, id + " " + text);
            }
            finally
            {
                HomeChrome.RecordRuns = false;
                chrome.Dispose();
            }
        }

        static HomeChrome.PaintedRun Find(HomeChrome chrome, string id)
        {
            foreach (var run in chrome.RecordedRuns)
                if (run.Id == id) return run;
            Assert.Fail("missing run " + id);
            return default;
        }
    }
}
