using System.Globalization;
using NUnit.Framework;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Logic;
using UnityEngine;

namespace OrbitalSalvage.Render.Tests
{
    public class LiveGameSessionTests
    {
        LiveGameSession _session;

        [SetUp]
        public void SetUp()
        {
            JsRandom.Global.Reset(0x12345678);
            var renderer = new GameRenderer(32, 32, 1);
            var economy = new EconomyManager(new MemoryKeyValueStore(), new FixedWallClock(0));
            _session = new LiveGameSession(new PhysicsEngine(new ParityAudioCues(() => 0)), renderer, economy);
            _session.RenderEnabled = false;
            _session.Physics.Center = new Vec2(215, 354);
            _session.Physics.BlackHole.X = 215;
            _session.Physics.BlackHole.Y = 700;
            _session.LoadSector(MazeLevelBuilder.GetSectors()[0]);
        }

        [TearDown]
        public void TearDown()
        {
            if (_session != null) _session.Renderer.Dispose();
            _session = null;
        }

        [Test]
        public void PausedWebFrameRendersWithoutSteppingPhysics()
        {
            int steps = _session.PhysicsSteps;
            int frames = _session.RenderFrames;
            _session.StepWebFrame(0.016, false);
            Assert.AreEqual(steps, _session.PhysicsSteps);
            Assert.AreEqual(frames + 1, _session.RenderFrames);
        }

        [Test]
        public void BoundBootMatchesWebSectorZeroSpawn()
        {
            JsRandom.Global.Reset(JsRandom.DefaultSeed);
            var economy = new EconomyManager(new MemoryKeyValueStore(), new FixedWallClock(0));
            var session = LiveGameSession.CreateBound(economy, 430, 932);
            try
            {
                Assert.AreEqual(215, session.Physics.Center.X, 0);
                Assert.AreEqual(354.16, session.Physics.Center.Y, 1e-9);
                Assert.AreEqual(215, session.Physics.BlackHole.X, 0);
                Assert.AreEqual(708.32, session.Physics.BlackHole.Y, 1e-9);
                Assert.AreEqual(11, session.Physics.DebrisList.Count);
                var d = session.Physics.DebrisList[0];
                Assert.AreEqual(237.67067744939985, d.X, 0);
                Assert.AreEqual(250.60372610393105, d.Y, 0);
                Assert.AreEqual(1.966082425788045, d.Vx, 0);
                Assert.AreEqual(0.035724048502743244, d.Vy, 0);
            }
            finally
            {
                session.Renderer.Dispose();
            }
        }

        [Test]
        public void VictoryFiresOnceWhenCollectedMassReachesQuota()
        {
            int wins = 0;
            _session.OnVictory += stats =>
            {
                wins++;
                Assert.GreaterOrEqual(stats.CollectedMass, stats.Quota);
                Assert.Greater(stats.Payout, 0);
            };
            var debris = _session.Physics.DebrisList[0];
            debris.Weight = _session.Quota;
            _session.Physics.OnDebrisSwallowed(debris);
            _session.Physics.OnDebrisSwallowed(debris);
            Assert.AreEqual(1, wins);
            Assert.IsTrue(_session.IsLevelCleared);
            Assert.GreaterOrEqual(_session.CollectedMass, _session.Quota);
        }

        [Test]
        public void DefeatFiresAfterEmptyFieldForOnePointTwoSeconds()
        {
            for (int i = 0; i < _session.Physics.DebrisList.Count; i++)
                _session.Physics.DebrisList[i].State = DebrisState.Removed;
            _session.FixedStep = true;
            _session.FixedDt = 0.05;
            int defeats = 0;
            _session.OnDefeat += stats => defeats++;
            for (int i = 0; i < 23; i++) _session.Update(1);
            Assert.AreEqual(0, defeats, "timer " + _session.FailureCheckTimer.ToString(CultureInfo.InvariantCulture));
            _session.Update(1);
            Assert.AreEqual(1, defeats);
            Assert.IsTrue(_session.IsLevelFailed);
            _session.Update(1);
            Assert.AreEqual(1, defeats);
        }

        [Test]
        public void PauseFreezesPhysicsAndTheFailTimer()
        {
            for (int i = 0; i < _session.Physics.DebrisList.Count; i++)
                _session.Physics.DebrisList[i].State = DebrisState.Removed;
            _session.Physics.AngularVelocity = 3;
            double angle = _session.Physics.RingAngle;
            _session.IsPaused = true;
            int steps = _session.PhysicsSteps;
            _session.FixedStep = true;
            _session.FixedDt = 0.05;
            _session.Update(0.05);
            _session.Update(0.05);
            Assert.AreEqual(steps, _session.PhysicsSteps);
            Assert.AreEqual(angle, _session.Physics.RingAngle);
            Assert.AreEqual(0, _session.FailureCheckTimer);
            Assert.IsFalse(_session.IsLevelFailed);
        }

        [Test]
        public void RestartResetsMassFlagsAndRing()
        {
            _session.Physics.OnDebrisSwallowed(_session.Physics.DebrisList[0]);
            _session.Physics.RingAngle = 1.25;
            _session.IsPaused = true;
            _session.Restart();
            Assert.AreEqual(0, _session.CollectedMass);
            Assert.AreEqual(0, _session.SpilledMass);
            Assert.AreEqual(0, _session.Physics.RingAngle);
            Assert.IsFalse(_session.IsPaused);
            Assert.IsFalse(_session.IsLevelCleared);
            Assert.IsFalse(_session.IsLevelFailed);
            Assert.Greater(_session.Physics.DebrisList.Count, 0);
        }

        [Test]
        public void DragClampsAngularVelocityToTwelve()
        {
            _session.PointerDown(0, 0, 430);
            _session.PointerMove(5000);
            Assert.AreEqual(12, _session.Physics.AngularVelocity, 1e-9);
            _session.PointerMove(-8000);
            Assert.AreEqual(-12, _session.Physics.AngularVelocity, 1e-9);
        }

        [Test]
        public void DoubleTapRespectsTimeAndDistanceWindow()
        {
            double t = 10000;
            _session.NowMs = () => t;
            _session.PointerDown(10, 0, 430);
            Assert.AreEqual(0, _session.Physics.AngularVelocity, 1e-12);
            t = 10100;
            _session.PointerDown(30, 0, 430);
            Assert.AreEqual(-3.8, _session.Physics.AngularVelocity, 1e-9);
            t = 10600;
            _session.PointerDown(30, 0, 430);
            Assert.AreEqual(-3.8, _session.Physics.AngularVelocity, 1e-9);
            t = 20000;
            _session.PointerDown(300, 0, 430);
            t = 20100;
            _session.PointerDown(320, 0, 430);
            Assert.AreEqual(0, _session.Physics.AngularVelocity, 1e-9);
        }

        [Test]
        public void ReplayAudioClockMatchesWebCollisionCooldown()
        {
            JsRandom.Global.Reset(JsRandom.DefaultSeed);
            var economy = new EconomyManager(new MemoryKeyValueStore(), new FixedWallClock(0));
            var session = LiveGameSession.CreateBound(economy, 430, 932);
            try
            {
                double now = 5000;
                session.NowMs = () => now;
                session.Cues.PublicInit();
                long baseline = JsRandom.Global.CallCount;
                session.AdvanceReplayAudio(16);
                session.Cues.PlayDebrisCollision(1, "titanium");
                Assert.AreEqual(baseline, JsRandom.Global.CallCount, "16ms is inside the 48ms cooldown");
                session.AdvanceReplayAudio(32);
                session.Cues.PlayDebrisCollision(1, "titanium");
                Assert.AreEqual(baseline + 1, JsRandom.Global.CallCount, "48ms cooldown elapsed draws one frequency");
                session.AdvanceReplayAudio(16);
                session.Cues.PlayDebrisCollision(1, "titanium");
                Assert.AreEqual(baseline + 1, JsRandom.Global.CallCount, "next 16ms is still inside the cooldown");
                session.FinishReplayAudio();
                now = 9000;
                session.AdvanceReplayAudio(16);
                session.Cues.PlayDebrisCollision(1, "titanium");
                Assert.AreEqual(baseline + 1, JsRandom.Global.CallCount, "a pre-replay app-clock jump does not open the cooldown");
            }
            finally
            {
                session.Renderer.Dispose();
            }
        }
    }
}
