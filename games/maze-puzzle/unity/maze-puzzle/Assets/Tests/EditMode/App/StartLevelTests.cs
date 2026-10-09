using System;
using System.IO;
using NUnit.Framework;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Logic;
using OrbitalSalvage.Render;

namespace OrbitalSalvage.App.Tests
{
    public class StartLevelTests
    {
        [Test]
        public void EmptyProfile_BootThenPlay_LoadsLevelOneQuota20()
        {
            JsRandom.Global.Reset(JsRandom.DefaultSeed);
            string dir = TempDir();
            try
            {
                var app = new AppController(new FileProfileStore(dir), new ManualAppClock(0, 0));
                var session = LiveGameSession.CreateBound(app.Economy, 430, 932);
                try
                {
                    app.AdoptSector(session.ActiveSector);
                    int epoch = app.SectorEpoch;
                    app.ResumeCurrentSector();
                    if (app.SectorEpoch != epoch) session.LoadSector(app.ActiveSector);
                    app.BeginBoot();
                    int guard = 0;
                    while (app.CurrentPage != "home" && guard++ < 80) app.Advance(AppController.BootIntervalMs);
                    Assert.AreEqual("home", app.CurrentPage);
                    app.Click("btn-home-play");

                    Assert.AreEqual("level_1", app.ActiveSector.Id);
                    Assert.AreEqual(1, app.ActiveSector.LevelNumber);
                    Assert.AreEqual(20, app.ActiveSector.Quota);
                    Assert.AreEqual(0, app.SectorIndex);
                    Assert.AreEqual(1, app.WorldIndex);
                    Assert.AreEqual("game", app.CurrentPage);
                    Assert.AreEqual("level_1", session.ActiveSector.Id);
                    Assert.AreEqual(20, session.Quota);
                    Assert.AreEqual("LVL 1 • Bubblegum Chute 1", app.MissionName);
                    Assert.AreEqual("maps_world_1_fresh", TemplateAfterMaps(app));
                }
                finally
                {
                    session.Renderer.Dispose();
                }
            }
            finally
            {
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }

        [Test]
        public void AdoptSector_SetsSectorIndex_SoNextAdvancesFromThatLevel()
        {
            var app = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 0));
            app.AdoptSector(app.Sectors[10]);
            Assert.AreEqual("level_11", app.ActiveSector.Id);
            Assert.AreEqual(65, app.ActiveSector.Quota);
            Assert.AreEqual(10, app.SectorIndex, "adopting level 11 must not leave SectorIndex at 0");
            Assert.AreEqual(2, app.WorldIndex);
            app.NextSector();
            Assert.AreEqual("level_12", app.ActiveSector.Id);
            Assert.AreEqual(11, app.SectorIndex);
        }

        [Test]
        public void CompletedSectors_PlayLoadsTheFirstUncompleted()
        {
            string dir = TempDir();
            try
            {
                var store = new FileProfileStore(dir);
                var seeded = new AppController(store, new ManualAppClock(0, 0));
                for (int n = 1; n <= 10; n++) seeded.Economy.Profile.CompletedSectors.Add("level_" + n);
                seeded.Economy.SaveProfile();

                var app = new AppController(store, new ManualAppClock(0, 0));
                app.AdoptSector(app.Sectors[0]);
                app.ResumeCurrentSector();
                app.Click("btn-home-play");
                Assert.AreEqual("level_11", app.ActiveSector.Id);
                Assert.AreEqual(65, app.ActiveSector.Quota);
                Assert.AreEqual(10, app.SectorIndex);
                Assert.AreEqual(2, app.WorldIndex);

                app.Click("btn-open-maps-page");
                Assert.AreEqual("maps_world_1_all_complete_other_active", app.TemplateId);
                Assert.IsTrue(TemplateSelector.MapsCaptureMatches(app.TemplateId, app.WorldIndex, 11));

                app.ClickClass("world-tab-btn", 0);
                Assert.AreEqual(1, app.WorldIndex);
                Assert.AreEqual("level_11", app.ActiveSector.Id);
                Assert.AreEqual("maps_world_1_all_complete", app.TemplateId);
                Assert.IsFalse(TemplateSelector.MapsCaptureMatches(app.TemplateId, app.WorldIndex, 11));

                app.ClickClass("world-tab-btn", 3);
                Assert.AreEqual(4, app.WorldIndex);
                Assert.AreEqual("maps_world_4", app.TemplateId);
                Assert.IsFalse(TemplateSelector.MapsCaptureMatches(app.TemplateId, 4, 31));
                Assert.IsTrue(TemplateSelector.MapsCaptureMatches(app.TemplateId, 4, 34));
            }
            finally
            {
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }

        [Test]
        public void ResetSavedProgress_ReturnsARunningAppToLevelOne()
        {
            string dir = TempDir();
            try
            {
                var app = new AppController(new FileProfileStore(dir), new ManualAppClock(0, 0));
                app.AdoptSector(app.Sectors[30]);
                app.ShowPage("game");
                Assert.AreEqual("level_31", app.ActiveSector.Id);
                app.Economy.Profile.CompletedSectors.Add("level_1");
                app.Economy.SaveProfile();

                app.ResetSavedProgress();
                Assert.AreEqual("home", app.CurrentPage);
                Assert.AreEqual("level_1", app.ActiveSector.Id);
                Assert.AreEqual(20, app.ActiveSector.Quota);
                Assert.AreEqual(0, app.SectorIndex);
                Assert.AreEqual(250, app.Economy.Profile.Credits);
                Assert.AreEqual(0, app.Economy.Profile.CompletedSectors.Count);
                Assert.That(app.ToastTexts(), Does.Contain("Progress Reset to Day 1! 🧹"));
            }
            finally
            {
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }

        [Test]
        public void MapsStamp_FollowsLiveLevelOnlyWhenTheCaptureDiffers()
        {
            var fresh = HomeChrome.Load("maps_world_1_fresh", "zero");
            var world4 = HomeChrome.Load("maps_world_4", "zero");
            try
            {
                Assert.IsTrue(fresh.MapsLevelMarkedCurrent(1));
                Assert.AreEqual(1, fresh.MapsActiveWorldTab());
                fresh.StampMapsCurrent(1, 1, true);
                Assert.IsTrue(fresh.MapsLevelMarkedCurrent(1));
                Assert.AreEqual(1, fresh.MapsActiveWorldTab());

                Assert.IsTrue(world4.MapsLevelMarkedCurrent(34));
                Assert.IsFalse(world4.MapsLevelMarkedCurrent(31));
                Assert.AreEqual(4, world4.MapsActiveWorldTab());
                world4.StampMapsCurrent(4, 31, false);
                Assert.IsTrue(world4.MapsLevelMarkedCurrent(31));
                Assert.IsFalse(world4.MapsLevelMarkedCurrent(34));
                Assert.AreEqual(4, world4.MapsActiveWorldTab());
                world4.StampMapsCurrent(1, 1, false);
                Assert.AreEqual(1, world4.MapsActiveWorldTab());
                Assert.IsFalse(world4.MapsLevelMarkedCurrent(34));
                world4.StampMapsCurrent(4, 34, true);
                Assert.IsTrue(world4.MapsLevelMarkedCurrent(34));
                Assert.AreEqual(4, world4.MapsActiveWorldTab());
            }
            finally
            {
                fresh.Dispose();
                world4.Dispose();
            }
        }

        static string TemplateAfterMaps(AppController app)
        {
            app.Click("btn-game-to-home");
            app.Click("btn-open-maps-page");
            string id = app.TemplateId;
            app.Click("btn-home-play");
            return id;
        }

        static string TempDir()
        {
            string dir = Path.Combine(Path.GetTempPath(), "orbital-start-level-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }
    }
}
