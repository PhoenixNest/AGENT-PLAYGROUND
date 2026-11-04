using NUnit.Framework;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Loc;
using OrbitalSalvage.Logic;

namespace OrbitalSalvage.Render.Tests
{
    public class IntroOnboardingTests
    {
        LiveGameSession _session;
        EconomyManager _economy;

        [SetUp]
        public void SetUp()
        {
            JsRandom.Global.Reset(0x12345678);
            var store = new MemoryKeyValueStore();
            var clock = new FixedWallClock(0);
            _economy = new EconomyManager(store, clock);
            var renderer = new GameRenderer(430, 932, 1);
            var physics = new PhysicsEngine(new ParityAudioCues(() => 0));
            _session = new LiveGameSession(physics, renderer, _economy);
            _session.RenderEnabled = false;
            _session.ApplyLayout(430, 932);
        }

        [TearDown]
        public void TearDown()
        {
            if (_session != null) _session.Renderer.Dispose();
            _session = null;
        }

        [Test]
        public void Profile_IntroSeen_DefaultOmitsKey()
        {
            var p = Profile.CreateDefault();
            Assert.IsFalse(p.IntroSeen);
            string json = p.ToText();
            StringAssert.DoesNotContain("\"introSeen\"", json);
        }

        [Test]
        public void Profile_IntroSeen_RoundTripsThroughJson()
        {
            var p = Profile.CreateDefault();
            p.IntroSeen = true;
            string json = p.ToText();
            StringAssert.Contains("\"introSeen\":true", json);

            var parsed = Profile.FromParsed(JsonParser.Parse(json), "2026-10-26");
            Assert.IsTrue(parsed.IntroSeen);
            Assert.AreEqual(0, parsed.ExtraKeys.Count);
        }

        [Test]
        public void LocStrings_ContainsAllIntroKeys()
        {
            string rot, mul, feed, backup;
            Assert.IsTrue(LocStrings.TryGet("intro.step.rotate", out rot));
            Assert.IsTrue(LocStrings.TryGet("intro.step.multiply", out mul));
            Assert.IsTrue(LocStrings.TryGet("intro.step.feed", out feed));
            Assert.IsTrue(LocStrings.TryGet("intro.safety.backup", out backup));
            Assert.IsTrue(LocStrings.TryGet("intro.btn.skip", out var skip));
            Assert.IsTrue(LocStrings.TryGet("intro.pip.spin", out var p1));
            Assert.IsTrue(LocStrings.TryGet("intro.pip.multiply", out var p2));
            Assert.IsTrue(LocStrings.TryGet("intro.pip.feed", out var p3));
            Assert.IsTrue(LocStrings.TryGet("intro.header.radio_comms", out var h1));
            Assert.IsTrue(LocStrings.TryGet("intro.header.energy_amp", out var h2));
            Assert.IsTrue(LocStrings.TryGet("intro.header.hungry_void", out var h3));
            Assert.IsTrue(LocStrings.TryGet("intro.prompt.continue", out var c1));
            Assert.IsTrue(LocStrings.TryGet("intro.prompt.play", out var c2));
            Assert.IsTrue(LocStrings.TryGet("intro.badge.rotate", out var b1));
            Assert.IsTrue(LocStrings.TryGet("intro.badge.gate", out var b2));

            Assert.IsNotEmpty(rot);
            Assert.IsNotEmpty(mul);
            Assert.IsNotEmpty(feed);
            Assert.IsNotEmpty(backup);
            Assert.IsNotEmpty(skip);
            Assert.IsNotEmpty(p1);
            Assert.IsNotEmpty(p2);
            Assert.IsNotEmpty(p3);
            Assert.IsNotEmpty(h1);
            Assert.IsNotEmpty(h2);
            Assert.IsNotEmpty(h3);
            Assert.IsNotEmpty(c1);
            Assert.IsNotEmpty(c2);
            Assert.IsNotEmpty(b1);
            Assert.IsNotEmpty(b2);
        }

        [Test]
        public void Tutorial_InitializesToSwipeForFirstLevelAndUnseenProfile()
        {
            var sectors = MazeLevelBuilder.GetSectors();
            _session.LoadSector(sectors[0]);
            Assert.AreEqual(1, sectors[0].LevelNumber);
            Assert.AreEqual(TutorialPhase.Swipe, _session.TutorialPhase);

            // Once intro seen, level 1 does not start tutorial
            _economy.Profile.IntroSeen = true;
            _session.LoadSector(sectors[0]);
            Assert.AreEqual(TutorialPhase.None, _session.TutorialPhase);

            // Level 2 does not start tutorial even if intro was unseen
            _economy.Profile.IntroSeen = false;
            if (sectors.Count > 1)
            {
                _session.LoadSector(sectors[1]);
                Assert.AreEqual(TutorialPhase.None, _session.TutorialPhase);
            }
        }

        [Test]
        public void Tutorial_AdvancesSequentiallyOnTapAndPausesSimulation()
        {
            var sectors = MazeLevelBuilder.GetSectors();
            _session.LoadSector(sectors[0]);
            Assert.AreEqual(TutorialPhase.Swipe, _session.TutorialPhase);
            Assert.IsTrue(_session.IsPaused, "Simulation should be paused during Step 1 (Swipe)");

            // Tap 1 advances Swipe -> Gate
            _session.PointerDown(200, 200);
            Assert.AreEqual(TutorialPhase.Gate, _session.TutorialPhase);
            Assert.IsTrue(_session.IsPaused, "Simulation should remain paused during Step 2 (Gate)");

            // Tap 2 advances Gate -> Feed
            _session.PointerDown(200, 200);
            Assert.AreEqual(TutorialPhase.Feed, _session.TutorialPhase);
            Assert.IsTrue(_session.IsPaused, "Simulation should remain paused during Step 3 (Feed)");

            // Tap 3 finishes Feed -> Done, resumes simulation, and marks IntroSeen
            _session.PointerDown(200, 200);
            Assert.AreEqual(TutorialPhase.Done, _session.TutorialPhase);
            Assert.IsFalse(_session.IsPaused, "Simulation should resume after tutorial completes");
            Assert.IsTrue(_economy.Profile.IntroSeen, "Profile introSeen should be marked true");
        }

        [Test]
        public void Tutorial_SkipButton_ImmediatelyDismissesAndResumes()
        {
            var sectors = MazeLevelBuilder.GetSectors();
            _session.LoadSector(sectors[0]);
            Assert.AreEqual(TutorialPhase.Swipe, _session.TutorialPhase);
            Assert.IsTrue(_session.IsPaused);

            // Tap on Skip button (top-right: pageW - 40, y = 25)
            _session.PointerDown(430 - 40, 25);
            Assert.AreEqual(TutorialPhase.Done, _session.TutorialPhase);
            Assert.IsFalse(_session.IsPaused, "Simulation should unpause on skip");
            Assert.IsTrue(_economy.Profile.IntroSeen, "Profile introSeen should be marked true on skip");
        }

        [Test]
        public void KeyDown_SpaceOrEnterAdvancesTutorial_EscapeSkips()
        {
            var sectors = MazeLevelBuilder.GetSectors();
            _session.LoadSector(sectors[0]);
            Assert.AreEqual(TutorialPhase.Swipe, _session.TutorialPhase);

            _session.KeyDown("Space");
            Assert.AreEqual(TutorialPhase.Gate, _session.TutorialPhase);

            _session.KeyDown("Enter");
            Assert.AreEqual(TutorialPhase.Feed, _session.TutorialPhase);

            _session.KeyDown("Escape");
            Assert.AreEqual(TutorialPhase.Done, _session.TutorialPhase);
            Assert.IsFalse(_session.IsPaused);
            Assert.IsTrue(_economy.Profile.IntroSeen);
        }

        [Test]
        public void Level1_SafetyNet_TriggersBackupPodWhenScrapExhausted()
        {
            var sectors = MazeLevelBuilder.GetSectors();
            _session.LoadSector(sectors[0]);
            Assert.IsFalse(_session.UsedBackupPod);

            // Remove all debris to simulate running out of scrap
            _session.Physics.DebrisList.Clear();
            _session.CollectedMass = 0;

            // Complete tutorial so gameplay simulation runs
            _session.SetTutorialPhase(TutorialPhase.Done);

            // Update simulation step
            _session.Update(0.016);

            Assert.IsTrue(_session.UsedBackupPod, "Backup pod should have automatically deployed");
            Assert.AreEqual(8, _session.Physics.DebrisList.Count, "Backup pod should have spawned 8 pieces of scrap");
            Assert.IsFalse(_session.IsLevelFailed, "Level should not have failed");
        }

        [Test]
        public void CompleteIntro_SetsIntroSeenAndPersists()
        {
            Assert.IsFalse(_economy.Profile.IntroSeen);
            _economy.CompleteIntro();
            Assert.IsTrue(_economy.Profile.IntroSeen);

            // Reload from store to verify persistence
            var loaded = _economy.LoadProfile();
            Assert.IsTrue(loaded.IntroSeen);
        }

        [Test]
        public void Tutorial_RenderTutorialHud_ExecutesAllPhasesCleanly()
        {
            var sectors = MazeLevelBuilder.GetSectors();
            _session.LoadSector(sectors[0]);
            var ctx = new Canvas2D(430, 932);
            var progress = new RenderProgress();

            Assert.DoesNotThrow(() =>
            {
                _session.Renderer.RenderTutorialHud(ctx, _session.Physics, TutorialPhase.Swipe, progress);
                _session.Renderer.RenderTutorialHud(ctx, _session.Physics, TutorialPhase.Gate, progress);
                _session.Renderer.RenderTutorialHud(ctx, _session.Physics, TutorialPhase.Feed, progress);
            });
            ctx.Dispose();
        }
    }
}
