using NUnit.Framework;

namespace OrbitalSalvage.Logic.Tests
{
    public class InfiniteModeTests
    {
        [Test]
        public void NextLevel_StaysInPoolForBadSources()
        {
            int[] samples = { 0, 20, 21, 1000, -5 };
            for (int i = 0; i < samples.Length; i++)
            {
                int sample = samples[i];
                int level = InfiniteMode.NextLevel(_ => sample);
                Assert.IsTrue(level >= InfiniteMode.PoolFirst && level <= InfiniteMode.PoolLast, sample + " -> " + level);
            }
        }

        [Test]
        public void NextLevel_FixedSourceMapsEnds()
        {
            Assert.AreEqual(30, InfiniteMode.NextLevel(_ => 0));
            Assert.AreEqual(50, InfiniteMode.NextLevel(_ => 20));
        }

        [Test]
        public void NextLevel_DefaultRandomStaysInPool()
        {
            for (int i = 0; i < 2000; i++)
            {
                int level = InfiniteMode.NextLevel();
                Assert.IsTrue(level >= 30 && level <= 50, "call " + i + " -> " + level);
            }
        }
    }
}
