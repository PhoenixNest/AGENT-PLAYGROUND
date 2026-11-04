using NUnit.Framework;

namespace OrbitalSalvage.App.Tests
{
    public class QuotaBarTests
    {
        [Test]
        public void CollectedMass_DrivesQuotaBarWidth_AndRestartClearsIt()
        {
            var app = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 0));
            app.LoadSector(app.Sectors[0], false);
            app.ShowPage("game");
            double quota = app.ActiveSector.Quota;
            Assert.Greater(quota, 0);

            app.SetLiveMass(0, 0);
            Assert.AreEqual(0, Width(app), 1e-9);

            app.SetLiveMass(quota * 0.5, 0);
            Assert.AreEqual(AppController.QuotaTrackWidth * 0.5, Width(app), 1e-6);

            app.SetLiveMass(quota, 0);
            Assert.AreEqual(AppController.QuotaTrackWidth, Width(app), 1e-6);

            app.RestartSector();
            Assert.AreEqual(0, app.CollectedMass, 1e-9);
            Assert.AreEqual(0, Width(app), 1e-9);
        }

        static double Width(AppController app)
        {
            Assert.IsTrue(app.Model.Elements.ContainsKey("hud-quota-bar"));
            var width = app.Model.Elements["hud-quota-bar"].WidthPx;
            Assert.IsTrue(width.HasValue);
            return width.Value;
        }
    }
}
