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
    }
}
