using NUnit.Framework;
using OrbitalSalvage.JsCompat;

namespace OrbitalSalvage.Logic.Tests
{
    public class FunnelIdleTests
    {
        const double Dt = 1.0 / 60.0;
        const int Steps = 1800;

        [TestCase(5)]
        [TestCase(31)]
        public void IdleThirtySeconds_SpillsNothing(int level)
        {
            JsRandom.Global.Reset(JsRandom.DefaultSeed);
            var engine = new PhysicsEngine(new ParityAudioCues(() => 0));
            const double w = 430;
            const double h = 932;
            double centerX = w * 0.5;
            double centerY = JsMath.Max(175, h * 0.38);
            double blackHoleY = JsMath.Min(h - 110, JsMath.Max(centerY + 240, h * 0.76));
            engine.InitSector(MazeLevelBuilder.BuildLevel(level), new Vec2(centerX, centerY), new Vec2(centerX, blackHoleY));

            int spilled = 0;
            int swallowed = 0;
            engine.OnDebrisSpilled = _ => spilled++;
            engine.OnDebrisSwallowed = _ => swallowed++;
            for (int i = 0; i < Steps; i++) engine.Update(Dt);

            Assert.Greater(swallowed, 0, "level " + level + " released no debris into the hole");
            Assert.AreEqual(0, spilled, "level " + level + " idle spill count");
        }
    }
}
