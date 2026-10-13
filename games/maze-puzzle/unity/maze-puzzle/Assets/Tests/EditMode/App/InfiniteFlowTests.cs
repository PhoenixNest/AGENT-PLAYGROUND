using System.Collections.Generic;
using NUnit.Framework;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Logic;

namespace OrbitalSalvage.App.Tests
{
    public class InfiniteFlowTests
    {
        const string Rocket = "\U0001F680";

        static AppController NewApp(IProfileStore store = null)
        {
            var app = new AppController(store ?? new MemoryProfileStore(), new ManualAppClock(0, 0));
            app.InfiniteRandom = max => 0;
            return app;
        }

        static void CompleteLevels(AppController app, int count)
        {
            for (int n = 1; n <= count; n++) app.Economy.RecordSectorCompleted("level_" + n);
        }

        static bool HasToast(AppController app, string text)
        {
            IReadOnlyList<string> toasts = app.ToastTexts();
            for (int i = 0; i < toasts.Count; i++) if (toasts[i] == text) return true;
            return false;
        }

        [Test]
        public void NotAllDone_PlayStartsCampaign()
        {
            AppController app = NewApp();
            CompleteLevels(app, 49);
            app.Click("btn-home-play");
            Assert.IsFalse(app.InfiniteActive);
            Assert.AreEqual("level_50", app.ActiveSector.Id);
            Assert.AreEqual(0, app.Economy.Profile.InfiniteRound);
        }

        [Test]
        public void AllDone_PlayStartsInfiniteRoundOne()
        {
            AppController app = NewApp();
            CompleteLevels(app, 50);
            app.Click("btn-home-play");
            Assert.IsTrue(app.InfiniteActive);
            Assert.AreEqual("level_30", app.ActiveSector.Id);
            Assert.AreEqual("game", app.CurrentPage);
            Assert.AreEqual(1, app.Economy.Profile.InfiniteRound);
            Assert.AreEqual(1, app.Economy.Profile.InfiniteBest);
            Assert.IsTrue(HasToast(app, "Round 1, best 1! " + Rocket), string.Join(" | ", app.ToastTexts()));
        }

        [Test]
        public void Win_ThenNext_PicksNewLevelFromPoolAndCountsRound()
        {
            AppController app = NewApp();
            CompleteLevels(app, 50);
            app.Click("btn-home-play");
            app.InfiniteRandom = max => 3;
            app.ForceVictory(app.ActiveSector.Quota);
            Assert.AreEqual(2, app.Economy.Profile.InfiniteRound);
            Assert.AreEqual(2, app.Economy.Profile.InfiniteBest);

            app.Click("btn-debrief-next");
            Assert.IsTrue(app.InfiniteActive);
            Assert.AreEqual("level_33", app.ActiveSector.Id);
            Assert.IsTrue(HasToast(app, "Round 2, best 2! " + Rocket), string.Join(" | ", app.ToastTexts()));
        }

        [Test]
        public void Win_PresentSessionVictory_CountsRound()
        {
            AppController app = NewApp();
            CompleteLevels(app, 50);
            app.Click("btn-home-play");
            app.PresentSessionVictory(10, 5, null);
            Assert.AreEqual(2, app.Economy.Profile.InfiniteRound);
        }

        [Test]
        public void Next_NeverLeavesPool_EvenWithBadRandom()
        {
            AppController app = NewApp();
            CompleteLevels(app, 50);
            app.Click("btn-home-play");
            int[] bad = { -7, 0, 20, 21, 5000 };
            for (int i = 0; i < bad.Length; i++)
            {
                int pick = bad[i];
                app.InfiniteRandom = max => pick;
                app.Click("btn-debrief-next");
                double n = app.ActiveSector.LevelNumber.Value;
                Assert.IsTrue(n >= 30 && n <= 50, "level " + n + " for random " + pick);
            }
        }

        [Test]
        public void WinningLevel50_AsLastLevel_ContinuesIntoInfinite()
        {
            AppController app = NewApp();
            CompleteLevels(app, 49);
            app.LoadSector(app.Sectors[49], false);
            app.ForceVictory(app.ActiveSector.Quota);
            Assert.AreEqual(-1, app.FirstUncompletedIndex());
            app.Click("btn-debrief-next");
            Assert.IsTrue(app.InfiniteActive);
            Assert.AreEqual(1, app.Economy.Profile.InfiniteRound);
        }

        [Test]
        public void WinningLevel50_WithEarlierLevelsOpen_StillWrapsToLevel1()
        {
            AppController app = NewApp();
            app.LoadSector(app.Sectors[49], false);
            app.ForceVictory(app.ActiveSector.Quota);
            app.Click("btn-debrief-next");
            Assert.IsFalse(app.InfiniteActive);
            Assert.AreEqual("level_1", app.ActiveSector.Id);
            Assert.AreEqual(0, app.Economy.Profile.InfiniteRound);
        }

        [Test]
        public void Restart_KeepsRoundAndMode()
        {
            AppController app = NewApp();
            CompleteLevels(app, 50);
            app.Click("btn-home-play");
            string id = app.ActiveSector.Id;
            app.RestartSector();
            Assert.IsTrue(app.InfiniteActive);
            Assert.AreEqual(id, app.ActiveSector.Id);
            Assert.AreEqual(1, app.Economy.Profile.InfiniteRound);
        }

        [Test]
        public void Quit_ThenPlay_ResumesAtSavedRound()
        {
            var store = new MemoryProfileStore();
            AppController first = NewApp(store);
            CompleteLevels(first, 50);
            first.Click("btn-home-play");
            first.ForceVictory(first.ActiveSector.Quota);
            first.Click("btn-debrief-next");
            Assert.AreEqual(2, first.Economy.Profile.InfiniteRound);

            AppController second = NewApp(store);
            Assert.AreEqual(2, second.Economy.Profile.InfiniteRound);
            second.Click("btn-home-play");
            Assert.IsTrue(second.InfiniteActive);
            Assert.AreEqual(2, second.Economy.Profile.InfiniteRound);
            Assert.IsTrue(HasToast(second, "Round 2, best 2! " + Rocket), string.Join(" | ", second.ToastTexts()));
        }

        [Test]
        public void LevelSelect_LeavesInfinite()
        {
            AppController app = NewApp();
            CompleteLevels(app, 50);
            app.Click("btn-home-play");
            Assert.IsTrue(app.InfiniteActive);
            app.LoadSector(app.Sectors[4], true);
            Assert.IsFalse(app.InfiniteActive);
        }

        [Test]
        public void ResetProgress_ClearsInfinite()
        {
            AppController app = NewApp();
            CompleteLevels(app, 50);
            app.Click("btn-home-play");
            app.ResetSavedProgress();
            Assert.IsFalse(app.InfiniteActive);
            Assert.AreEqual(0, app.Economy.Profile.InfiniteRound);
            Assert.AreEqual("level_1", app.ActiveSector.Id);
        }

        [Test]
        public void Home_ShowsInfiniteCard_WhenAllLevelsDone()
        {
            AppController app = NewApp();
            CompleteLevels(app, 50);
            app.Click("btn-home-play");
            app.ShowPage("home");
            Assert.AreEqual("Round 1", app.MissionName);
            Assert.AreEqual("Best: 1", app.MissionGoal);
            Assert.AreEqual("Infinite Mode", app.MissionWorld);

            app.ForceVictory(app.ActiveSector.Quota);
            app.ShowPage("home");
            Assert.AreEqual("Round 2", app.MissionName);
            Assert.AreEqual("Best: 2", app.MissionGoal);
        }

        [Test]
        public void Home_ShowsInfiniteCard_EvenAfterReplayingACampaignLevel()
        {
            AppController app = NewApp();
            CompleteLevels(app, 50);
            app.LoadSector(app.Sectors[4], false);
            app.ShowPage("home");
            Assert.AreEqual("Round 1", app.MissionName);
            Assert.AreEqual("Infinite Mode", app.MissionWorld);
        }

        [Test]
        public void Home_ShowsLevelCard_WhenNotAllDone()
        {
            AppController app = NewApp();
            CompleteLevels(app, 49);
            app.ShowPage("home");
            StringAssert.StartsWith("LVL ", app.MissionName);
            StringAssert.StartsWith("Goal: ", app.MissionGoal);
            Assert.AreNotEqual("Infinite Mode", app.MissionWorld);
        }

        [Test]
        public void Pick_DoesNotUseGlobalRandom()
        {
            JsRandom.Global.Reset(JsRandom.DefaultSeed);
            AppController a = NewApp();
            CompleteLevels(a, 50);
            a.LoadInfiniteRound(false);
            double afterInfinite = JsRandom.Global.NextDouble();

            JsRandom.Global.Reset(JsRandom.DefaultSeed);
            AppController b = NewApp();
            CompleteLevels(b, 50);
            b.LoadSector(b.Sectors[29], false);
            double afterPlain = JsRandom.Global.NextDouble();

            Assert.AreEqual(afterPlain, afterInfinite);
        }
    }
}
