using NUnit.Framework;
using OrbitalSalvage.JsCompat;

namespace OrbitalSalvage.Logic.Tests
{
    /// <summary>
    /// Every infinite-mode level (30 to 50) builds and runs in the physics engine for 30 seconds with no input:
    /// no exception, debris exists, and every debris position stays a real number.
    /// This is a smoke check only. It does NOT prove a child can win a level: no player input is simulated.
    /// </summary>
    public class InfinitePoolLevelsTests
    {
        const double Dt = 1.0 / 60.0;
        const int Steps = 1800;

        [Test]
        public void Level_BuildsAndRunsWithoutErrors([NUnit.Framework.Range(InfiniteMode.PoolFirst, InfiniteMode.PoolLast)] int level)
        {
            JsRandom.Global.Reset(JsRandom.DefaultSeed);
            var engine = new PhysicsEngine(new ParityAudioCues(() => 0));
            const double w = 430;
            const double h = 932;
            double centerX = w * 0.5;
            double centerY = JsMath.Max(175, h * 0.38);
            double blackHoleY = JsMath.Min(h - 110, JsMath.Max(centerY + 240, h * 0.76));
            SectorConfig sector = MazeLevelBuilder.BuildLevel(level);
            Assert.AreEqual(level, sector.LevelNumber.Value);
            Assert.Greater(sector.Quota, 0);
            engine.InitSector(sector, new Vec2(centerX, centerY), new Vec2(centerX, blackHoleY));
            Assert.Greater(engine.DebrisList.Count, 0, "level " + level + " has no debris");

            for (int i = 0; i < Steps; i++) engine.Update(Dt);

            for (int i = 0; i < engine.DebrisList.Count; i++)
            {
                Debris d = engine.DebrisList[i];
                Assert.IsFalse(double.IsNaN(d.X) || double.IsNaN(d.Y) || double.IsInfinity(d.X) || double.IsInfinity(d.Y),
                    "level " + level + " debris " + i + " has a broken position");
            }
        }
    }
}
