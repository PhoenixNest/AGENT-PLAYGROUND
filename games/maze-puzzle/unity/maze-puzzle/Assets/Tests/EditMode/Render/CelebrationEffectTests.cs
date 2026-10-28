using System.Collections.Generic;
using NUnit.Framework;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Logic;

namespace OrbitalSalvage.Render.Tests
{
    public class CelebrationEffectTests
    {
        LiveGameSession _session;
        EconomyManager _economy;
        GameRenderer _renderer;
        PhysicsEngine _physics;
        ParityAudioCues _audio;

        [SetUp]
        public void SetUp()
        {
            JsRandom.Global.Reset(0x12345678);
            var store = new MemoryKeyValueStore();
            var clock = new FixedWallClock(0);
            _economy = new EconomyManager(store, clock);
            _renderer = new GameRenderer(430, 932, 1);
            _audio = new ParityAudioCues(() => 0);
            _audio.PublicInit();
            _physics = new PhysicsEngine(_audio);
            _session = new LiveGameSession(_physics, _renderer, _economy);
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
        public void CelebrationCannons_PoolAllocation_And_Kinematics()
        {
            _renderer.ResetCelebrationVfx();
            Assert.IsFalse(_renderer.HasActiveCelebration);

            _renderer.LaunchCelebrationCannons(20, 850, 410, 850, 38);
            Assert.IsTrue(_renderer.HasActiveCelebration);

            int activeCount = 0;
            int strips = 0;
            int stars = 0;
            int discs = 0;

            for (int i = 0; i < _renderer.CelebrationConfetti.Length; i++)
            {
                var p = _renderer.CelebrationConfetti[i];
                if (!p.Active) continue;
                activeCount++;
                if (p.Shape == CelebrationConfettiShape.Strip) strips++;
                else if (p.Shape == CelebrationConfettiShape.Star) stars++;
                else if (p.Shape == CelebrationConfettiShape.Disc) discs++;

                // Initial velocities must project inward and upward
                Assert.Less(p.Vy, 0, "Confetti should initially launch upward");
                Assert.Greater(p.Life, 0);
                Assert.IsNotEmpty(p.Color);
            }

            Assert.AreEqual(76, activeCount, "Twin cannons with 38 pieces each should total 76 active particles");
            Assert.Greater(strips, 0, "Should generate foil strips");
            Assert.Greater(stars, 0, "Should generate spangle stars");
            Assert.Greater(discs, 0, "Should generate sequin discs");
        }

        [Test]
        public void Fireworks_RocketAndBurstLifecycle()
        {
            _renderer.ResetCelebrationVfx();
            bool burstExploded = false;
            string explodedColor = null;
            _renderer.OnFireworkExploded = (x, y, color) =>
            {
                burstExploded = true;
                explodedColor = color;
            };

            _renderer.LaunchFirework(200, 800, 300, "#f472b6");

            int activeRockets = 0;
            for (int i = 0; i < _renderer.CelebrationRockets.Length; i++)
                if (_renderer.CelebrationRockets[i].Active) activeRockets++;

            Assert.AreEqual(1, activeRockets, "One rocket should be launched");
            Assert.IsFalse(burstExploded);

            // Step simulation until rocket reaches apex / target height
            var ctx = new Canvas2D(430, 932);
            for (int step = 0; step < 120 && !burstExploded; step++)
            {
                _renderer.RenderCelebrationVfx(ctx, 0.016);
            }
            ctx.Dispose();

            Assert.IsTrue(burstExploded, "Rocket should have detonated at target altitude");
            Assert.AreEqual("#f472b6", explodedColor);

            int activeSparks = 0;
            for (int i = 0; i < _renderer.CelebrationSparks.Length; i++)
                if (_renderer.CelebrationSparks[i].Active) activeSparks++;

            Assert.Greater(activeSparks, 0, "Burst detonation should populate spark particles");
        }

        [Test]
        public void Celebration_ZeroLag_RenderCelebrationVfx_CleanExecution()
        {
            _renderer.ResetCelebrationVfx();
            _renderer.LaunchCelebrationCannons(20, 850, 410, 850, 38);
            _renderer.LaunchFirework(150, 800, 350, "#fde047");
            _renderer.ExplodeFirework(215, 300, "#38bdf8", 24);

            var ctx = new Canvas2D(430, 932);
            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 60; i++)
                {
                    _renderer.RenderCelebrationVfx(ctx, 0.016);
                }
            });
            ctx.Dispose();
        }

        [Test]
        public void LiveGameSession_VictorySequencer_CelebrationTimeline()
        {
            var sectors = MazeLevelBuilder.GetSectors();
            _session.LoadSector(sectors[0]);

            var cues = new List<string>();
            _session.OnCelebrationCue = cue => cues.Add(cue);

            Assert.IsFalse(_session.IsLevelCleared);
            Assert.IsFalse(_session.IsVictoryCelebrationPlaying);

            // Trigger victory by satisfying quota
            var debris = _session.Physics.DebrisList[0];
            debris.Weight = _session.Quota;
            _session.Physics.OnDebrisSwallowed(debris);

            Assert.IsTrue(_session.IsLevelCleared);
            Assert.IsTrue(_session.IsVictoryCelebrationPlaying);
            Assert.AreEqual(0.0, _session.VictoryCelebrationTimer);
            CollectionAssert.Contains(cues, "cannon", "Celebration cannons should fire immediately on victory");
            CollectionAssert.Contains(cues, "launch", "Wave 1 firework rockets should launch immediately on victory");

            // Advance to 0.35s -> Wave 2 triggers (stepped within DtCap=0.05)
            cues.Clear();
            for (int i = 0; i < 7; i++) _session.Update(0.05);
            CollectionAssert.Contains(cues, "launch", "Wave 2 firework rockets should launch at 320ms");

            // Advance to 0.70s -> Wave 3 triggers
            cues.Clear();
            for (int i = 0; i < 7; i++) _session.Update(0.05);
            CollectionAssert.Contains(cues, "launch", "Wave 3 firework rockets should launch at 680ms");

            // Advance past total duration (2.4s) -> Celebration completes
            for (int i = 0; i < 36; i++) _session.Update(0.05);
            Assert.GreaterOrEqual(_session.VictoryCelebrationTimer, LiveGameSession.CelebrationTotalDuration);
            Assert.IsFalse(_session.IsVictoryCelebrationPlaying, "Celebration sequence should finish after 2.4s");
        }

        [Test]
        public void InteractiveTap_DuringVictory_SpawnsCelebrationConfetti()
        {
            var sectors = MazeLevelBuilder.GetSectors();
            _session.LoadSector(sectors[0]);

            // Satisfy quota to enter victory state
            var debris = _session.Physics.DebrisList[0];
            debris.Weight = _session.Quota;
            _session.Physics.OnDebrisSwallowed(debris);
            Assert.IsTrue(_session.IsLevelCleared);

            _renderer.ResetCelebrationVfx();
            Assert.IsFalse(_renderer.HasActiveCelebration);

            var cues = new List<string>();
            _session.OnCelebrationCue = cue => cues.Add(cue);

            // Player taps screen during victory
            _session.PointerDown(215, 450);

            Assert.IsTrue(_renderer.HasActiveCelebration, "Tapping during victory should spawn celebration confetti & firework");
            CollectionAssert.Contains(cues, "cannon");
            CollectionAssert.Contains(cues, "burst");
        }
    }
}

