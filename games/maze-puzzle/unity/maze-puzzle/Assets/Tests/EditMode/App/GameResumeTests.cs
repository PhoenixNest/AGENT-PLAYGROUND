using NUnit.Framework;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Render;

namespace OrbitalSalvage.App.Tests
{
    public class GameResumeTests
    {
        [Test]
        public void GestureHint_ShowsOnceThenStaysHidden()
        {
            var first = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 0));
            first.ShowPage("game");
            Assert.IsFalse(first.HintHidden);
            Assert.IsFalse(HintConcealed(first), "fresh entry shows the hint");

            first.Advance(AppController.HintMs);
            Assert.IsTrue(first.HintHidden);
            Assert.IsFalse(HintConcealed(first), "fade has not finished");
            first.Advance(AppController.HintFadeMs);
            Assert.IsTrue(HintConcealed(first));

            first.ShowPage("home");
            first.ShowPage("game");
            Assert.IsTrue(first.HintHidden);
            Assert.IsTrue(HintConcealed(first), "re-entry does not show the hint again");

            var fresh = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 0));
            fresh.ShowPage("game");
            Assert.IsFalse(fresh.HintHidden);
            Assert.IsFalse(HintConcealed(fresh), "a new app still shows the hint");
        }

        [Test]
        public void PauseHomeStart_KeepsBoardAndResumesUnpaused()
        {
            JsRandom.Global.Reset(1);
            var app = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 0));
            var session = LiveGameSession.CreateBound(app.Economy, 430, 932);
            try
            {
                session.RenderEnabled = false;
                app.Playfield = new ClockPlayfield(session);
                app.AdoptSector(session.ActiveSector);
                app.ShowPage("game");
                session.CollectedMass = 4;
                session.SpilledMass = 1.5;
                app.SetLiveMass(4, 1.5);
                session.FailureCheckTimer = 3.5;
                int debris = session.Physics.DebrisList.Count;
                Assert.Greater(debris, 0);

                app.Click("btn-game-pause");
                Assert.IsTrue(app.IsPaused);
                app.Click("btn-pause-home");
                Assert.AreEqual("home", app.CurrentPage);
                Assert.IsFalse(app.IsPaused);
                Assert.AreEqual(0, session.FailureCheckTimer, 1e-9);
                Assert.AreEqual(4, session.CollectedMass, 1e-9);
                Assert.AreEqual(1.5, session.SpilledMass, 1e-9);
                Assert.AreEqual(debris, session.Physics.DebrisList.Count);

                app.Click("btn-home-play");
                Assert.AreEqual("game", app.CurrentPage);
                Assert.IsFalse(app.IsPaused);
                Assert.AreEqual(0, session.FailureCheckTimer, 1e-9);
                Assert.AreEqual(4, session.CollectedMass, 1e-9);
                Assert.AreEqual(1.5, session.SpilledMass, 1e-9);
                Assert.AreEqual(debris, session.Physics.DebrisList.Count);
            }
            finally
            {
                session.Renderer.Dispose();
            }
        }

        static bool HintConcealed(AppController app)
        {
            ElementOverride over;
            if (!app.Model.Elements.TryGetValue("gesture-hint", out over) || over.Visible == null) return false;
            return over.Visible == false;
        }

        sealed class ClockPlayfield : IFixedPlayfield, IFailureClock
        {
            readonly LiveGameSession _session;
            public ClockPlayfield(LiveGameSession session) { _session = session; }
            public void ResetFailureClock() { _session.FailureCheckTimer = 0; }
            public void Step(double dtSeconds, bool simulate) { }
            public void FinishReplay() { }
            public void PointerDown(double clientX, double canvasLeft, double canvasWidth) { }
            public void PointerMove(double clientX) { }
            public void PointerUp() { }
            public void KeyDown(string key) { }
            public string StateHash() { return ""; }
        }
    }
}
