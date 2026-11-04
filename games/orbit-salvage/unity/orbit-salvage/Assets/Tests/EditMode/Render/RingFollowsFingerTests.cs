using System;
using System.Reflection;
using NUnit.Framework;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Logic;

namespace OrbitalSalvage.Render.Tests
{
    public class RingFollowsFingerTests
    {
        LiveGameSession _session;
        double _now;

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
            _now = 10000;
            _session.NowMs = () => _now;
        }

        [TearDown]
        public void TearDown()
        {
            if (_session != null) _session.Renderer.Dispose();
            _session = null;
        }

        [Test]
        public void Grab_MovesTheRingPointWithTheFingerAtEightCompassPoints()
        {
            double cx = _session.Physics.Center.X;
            double cy = _session.Physics.Center.Y;
            const double radius = 100;
            const double step = 24;
            string[] names = { "E", "SE", "S", "SW", "W", "NW", "N", "NE" };
            for (int i = 0; i < 8; i++)
            {
                double theta = i * Math.PI / 4.0;
                double ox = Math.Cos(theta) * radius;
                double oy = Math.Sin(theta) * radius;
                double tx = -Math.Sin(theta);
                double ty = Math.Cos(theta);
                for (int dir = -1; dir <= 1; dir += 2)
                {
                    double moveX = tx * step * dir;
                    double moveY = ty * step * dir;
                    Grab(cx + ox, cy + oy, cx + ox + moveX, cy + oy + moveY);
                    double spin = _session.Physics.AngularVelocity;
                    Assert.AreNotEqual(0, Math.Sign(spin), names[i] + " dir " + dir);
                    double dot = DotRingPoint(ox, oy, moveX, moveY, spin);
                    Assert.Greater(dot, 0, names[i] + " dir " + dir + " dot " + dot.ToString("R"));
                }
            }
        }

        [Test]
        public void BottomLeft_IsClockwise_AndTheOtherThreeSidesMatchTheFinger()
        {
            double cx = _session.Physics.Center.X;
            double cy = _session.Physics.Center.Y;
            const double radius = 120;

            Grab(cx, cy + radius, cx - 30, cy + radius);
            Assert.AreEqual(0.19402378676870707, _session.Physics.AngularVelocity, 1e-9, "bottom, left");

            Grab(cx, cy - radius, cx - 30, cy - radius);
            Assert.Less(_session.Physics.AngularVelocity, 0, "top, left");

            Grab(cx + radius, cy, cx + radius, cy + 30);
            Assert.Greater(_session.Physics.AngularVelocity, 0, "right, down");

            Grab(cx - radius, cy, cx - radius, cy - 30);
            Assert.Greater(_session.Physics.AngularVelocity, 0, "left, up");
        }

        [Test]
        public void RadialMove_DoesNotSpin()
        {
            double cx = _session.Physics.Center.X;
            double cy = _session.Physics.Center.Y;
            Grab(cx + 150, cy, cx + 80, cy);
            Assert.AreEqual(0, _session.Physics.AngularVelocity, 1e-9);

            double ox = 80;
            double oy = 60;
            Grab(cx + ox, cy + oy, cx + ox * 0.55, cy + oy * 0.55);
            Assert.AreEqual(0, _session.Physics.AngularVelocity, 1e-9);
        }

        [Test]
        public void InsideMinGrabRadius_UsesHorizontalRule()
        {
            double cx = _session.Physics.Center.X;
            double cy = _session.Physics.Center.Y;
            double x = cx + 18;
            double gain = 1.3 * _session.Physics.DragSensitivity;

            Grab(x, cy, x, cy + 24);
            Assert.AreEqual(0, _session.Physics.AngularVelocity, 1e-12, "a vertical drag inside the hub");

            Grab(x, cy, x + 16, cy);
            Assert.AreEqual(16 * gain, _session.Physics.AngularVelocity, 1e-9);

            Grab(x, cy, x + 50, cy);
            Assert.AreEqual(50 * gain, _session.Physics.AngularVelocity, 1e-9, "a start inside the hub stays sideways");
        }

        [Test]
        public void MoveWhileUp_StoresThePositionAndDoesNotSpin()
        {
            double cx = _session.Physics.Center.X;
            double cy = _session.Physics.Center.Y;
            _session.PointerUp();
            _session.PointerMove(cx + 120, cy);
            Assert.AreEqual(0, _session.Physics.AngularVelocity, 1e-12);

            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(LiveGameSession).GetField("_dragging", flags).SetValue(_session, true);
            _session.PointerMove(cx + 120, cy + 30);
            Assert.Greater(_session.Physics.AngularVelocity, 0);
            Assert.Less(_session.Physics.AngularVelocity, 1);
        }

        [Test]
        public void HubExit_UsesTheUpdatedPosition()
        {
            double cx = _session.Physics.Center.X;
            double cy = _session.Physics.Center.Y;
            _now += 1000;
            _session.Physics.AngularVelocity = 0;
            _session.PointerDown(cx + 100, cy);
            _session.PointerMove(cx + 20, cy);
            double afterHub = _session.Physics.AngularVelocity;
            Assert.AreEqual(-80 * 1.3 * _session.Physics.DragSensitivity, afterHub, 1e-9);
            _session.PointerMove(cx + 20, cy + 90);
            Assert.AreEqual(afterHub, _session.Physics.AngularVelocity, 1e-9);
        }

        [Test]
        public void SingleArgumentMove_StaysHorizontal()
        {
            _now += 1000;
            _session.Physics.AngularVelocity = 0;
            _session.PointerDown(10, 0, 430);
            _session.PointerMove(50);
            Assert.AreEqual(40 * 1.3 * _session.Physics.DragSensitivity, _session.Physics.AngularVelocity, 1e-9);
        }

        [Test]
        public void SingleArgumentMove_IgnoresAStoredVerticalPosition()
        {
            double cx = _session.Physics.Center.X;
            double cy = _session.Physics.Center.Y;
            _now += 1000;
            _session.Physics.AngularVelocity = 0;
            _session.PointerDown(cx, cy + 120);
            _session.PointerMove(cx - 40);
            Assert.AreEqual(-40 * 1.3 * _session.Physics.DragSensitivity, _session.Physics.AngularVelocity, 1e-9);
        }

        void Grab(double x0, double y0, double x1, double y1)
        {
            _now += 1000;
            _session.Physics.AngularVelocity = 0;
            _session.PointerUp();
            _session.PointerDown(x0, y0);
            _session.PointerMove(x1, y1);
        }

        /// <summary>
        /// Displacement of the ring point under the finger after a short turn with the sign of
        /// <paramref name="angularVelocity"/>. Positive angle uses the canvas matrix [cos, sin, -sin, cos].
        /// </summary>
        static double DotRingPoint(double ox, double oy, double moveX, double moveY, double angularVelocity)
        {
            double dAngle = (angularVelocity > 0 ? 1 : -1) * 0.05;
            double cos = Math.Cos(dAngle);
            double sin = Math.Sin(dAngle);
            double nx = cos * ox - sin * oy;
            double ny = sin * ox + cos * oy;
            return moveX * (nx - ox) + moveY * (ny - oy);
        }
    }
}
