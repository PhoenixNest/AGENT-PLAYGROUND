using NUnit.Framework;

namespace OrbitalSalvage.Logic.Tests
{
    public class GameSyncHashTests
    {
        [Test]
        public void HashDoubles_MatchesWebFnv()
        {
            Assert.AreEqual("afda801ca49be31c", PhysicsSnapshot.HashDoubles(new[] { 1.0, -2.5, 0.0 }));
        }
    }
}
