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

            Assert.IsNotEmpty(rot);
            Assert.IsNotEmpty(mul);
            Assert.IsNotEmpty(feed);
            Assert.IsNotEmpty(backup);
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
        public void Tutorial_AdvancesPhasesOnEvents()
        {
            var sectors = MazeLevelBuilder.GetSectors();
            _session.LoadSector(sectors[0]);
            Assert.AreEqual(TutorialPhase.Swipe, _session.TutorialPhase);

            // Drag ring advances Swipe -> Gate
            _session.PointerDown(100, 100);
            _session.PointerMove(150, 100);
            _session.PointerUp();
            Assert.AreEqual(TutorialPhase.Gate, _session.TutorialPhase);

            // Gate passed advances Gate -> Feed
            var gate = new GateRuntime { Id = 1, Type = "MULTIPLY", Value = 2, RMin = 100, RMax = 150 };
            var debris = new Debris { Id = 1, Weight = 1, Mass = 1 };
            _session.Physics.OnGatePassed(gate, debris);
            Assert.AreEqual(TutorialPhase.Feed, _session.TutorialPhase);

            // Swallow debris past 40% quota advances Feed -> Done
            _session.CollectedMass = _session.Quota * 0.5;
            _session.Physics.OnDebrisSwallowed(debris);
            Assert.AreEqual(TutorialPhase.Done, _session.TutorialPhase);
        }

        [Test]
        public void KeyDown_AdvancesSwipeToGate()
        {
            var sectors = MazeLevelBuilder.GetSectors();
            _session.LoadSector(sectors[0]);
            Assert.AreEqual(TutorialPhase.Swipe, _session.TutorialPhase);

            _session.KeyDown("ArrowRight");
            Assert.AreEqual(TutorialPhase.Gate, _session.TutorialPhase);
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

            // Update simulation step
            _session.Update(0.016);

            Assert.IsTrue(_session.UsedBackupPod, "Backup pod should have automatically deployed");
            Assert.AreEqual(8, _session.Physics.DebrisList.Count, "Backup pod should have spawned 8 pieces of scrap");
            Assert.AreEqual(TutorialPhase.Feed, _session.TutorialPhase, "Phase should be Feed after backup deployment");
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
