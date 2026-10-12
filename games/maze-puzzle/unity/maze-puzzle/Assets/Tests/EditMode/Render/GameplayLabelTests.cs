using NUnit.Framework;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Loc;
using OrbitalSalvage.Logic;

namespace OrbitalSalvage.Render.Tests
{
    using Loc = OrbitalSalvage.Loc.Loc;

    public class GameplayLabelTests
    {
        [SetUp]
        public void ResetBefore()
        {
            Loc.Reset(null);
        }

        [TearDown]
        public void ResetAfter()
        {
            Loc.Reset(null);
        }

        [Test]
        public void English_GateHubAndQuotaMatchTheOldLiteralsAndStayCached()
        {
            var labels = new GameplayLabels();
            string zap = labels.Zap(2d);
            Assert.AreEqual("\u26A1 \u00D7" + JsNumber.ToString(2d), zap);
            Assert.AreSame(zap, labels.Zap(2d));
            Assert.AreEqual("\U0001F4A5 \u00D7" + JsNumber.ToString(3d) + "!", labels.Shred(3d));
            Assert.AreEqual("\U0001F9F2 +" + JsNumber.ToString(5d), labels.Magnet(5d));
            string boost = labels.Boost();
            Assert.AreEqual("\u23E9 BOOST", boost);
            Assert.AreSame(boost, labels.Boost());

            string emoji;
            string sub;
            labels.Hub("ring_donut", false, out emoji, out sub);
            Assert.AreEqual("\U0001F369 DONUT \U0001F369", emoji);
            Assert.AreEqual("SWEET RIG", sub);
            string again;
            string subAgain;
            labels.Hub("ring_donut", false, out again, out subAgain);
            Assert.AreSame(emoji, again);
            Assert.AreSame(sub, subAgain);
            labels.Hub("ring_duckie", true, out emoji, out sub);
            Assert.AreEqual("\u2699\uFE0F GEAR RIG \u2699\uFE0F", emoji);
            Assert.AreEqual("PLANETARY", sub);

            var session = new LiveGameSession(
                new PhysicsEngine(new ParityAudioCues(() => 0)),
                null,
                new EconomyManager(new MemoryKeyValueStore(), new FixedWallClock(0)));
            session.ActiveSector = new SectorConfig { Quota = 20 };
            session.CollectedMass = 12;
            string quota = session.QuotaLabel();
            Assert.AreEqual(JsNumber.ToString(12d) + " / " + JsNumber.ToString(20d) + " kg", quota);
            Assert.AreSame(quota, session.QuotaLabel());
        }

        [Test]
        public void PseudoLocale_ChangesPlayfieldWordsAndKeepsNumbers()
        {
            var tables = new InlineTables();
            tables.Json["en"] = LocJson.WriteTable(
                "en",
                "English",
                new string[0],
                delegate (string key) { return key; },
                LocStrings.SortedPairs(),
                new string[0]);
            Loc.SetLoader(tables);
            Assert.IsTrue(Loc.SetDiagnostic(PseudoLocale.Code));

            var labels = new GameplayLabels();
            string zap = labels.Zap(2d);
            Assert.AreNotEqual("\u26A1 \u00D7" + JsNumber.ToString(2d), zap);
            Assert.IsTrue(zap.Contains("2"), zap);
            string boost = labels.Boost();
            Assert.AreNotEqual("\u23E9 BOOST", boost);
            string emoji;
            string sub;
            labels.Hub("ring_donut", false, out emoji, out sub);
            Assert.AreNotEqual("\U0001F369 DONUT \U0001F369", emoji);
            Assert.AreNotEqual("SWEET RIG", sub);

            var session = new LiveGameSession(
                new PhysicsEngine(new ParityAudioCues(() => 0)),
                null,
                new EconomyManager(new MemoryKeyValueStore(), new FixedWallClock(0)));
            session.ActiveSector = new SectorConfig { Quota = 20 };
            session.CollectedMass = 12;
            string quota = session.QuotaLabel();
            Assert.AreNotEqual("12 / 20 kg", quota, quota);
            Assert.IsTrue(quota.Contains("12"), quota);
            Assert.IsTrue(quota.Contains("20"), quota);
            Assert.AreNotEqual("NOM! \U0001F60B", Loc.Tr("fx.swallow.nom"));
            Assert.AreNotEqual("\u00D75! SHRED \u26A1", Loc.Tr("fx.shred", 5d));
            Assert.IsTrue(Loc.Tr("fx.shred", 5d).Contains("5"));
            Assert.AreNotEqual("POOF! \U0001F4A8", Loc.Tr("fx.poof"));
            Assert.AreNotEqual("TURBO! \u23E9", Loc.Tr("fx.turbo.cw"));
            Assert.AreNotEqual("\u23EA TURBO!", Loc.Tr("fx.turbo.ccw"));
        }

        sealed class InlineTables : ILocTableLoader
        {
            public readonly System.Collections.Generic.Dictionary<string, string> Json =
                new System.Collections.Generic.Dictionary<string, string>(System.StringComparer.Ordinal);

            public bool TryLoad(string code, out string json)
            {
                return Json.TryGetValue(code, out json);
            }
        }
    }
}
