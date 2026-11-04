using System;
using System.IO;
using NUnit.Framework;
using OrbitalSalvage.Logic;

namespace OrbitalSalvage.App.Tests
{
    public class ProfileStoreTests
    {
        [Test]
        public void FileProfile_RoundTripsStorageKey()
        {
            string dir = Path.Combine(Path.GetTempPath(), "orbital-ship-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                var store = new FileProfileStore(dir);
                Assert.IsNull(store.GetItem(EconomyManager.StorageKey));
                const string payload = "{\"version\":2,\"credits\":250,\"settings\":{\"soundEnabled\":true,\"sfxVolume\":80}}";
                store.SetItem(EconomyManager.StorageKey, payload);
                var again = new FileProfileStore(dir);
                Assert.AreEqual(payload, again.GetItem(EconomyManager.StorageKey));
                string path = Path.Combine(dir, FileProfileStore.FileName);
                Assert.IsTrue(File.Exists(path));
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        [Test]
        public void Clock_InjectionAdvancesController()
        {
            var clock = new ManualAppClock(0, 1_700_000_000_000);
            var app = new AppController(new MemoryProfileStore(), clock, new SilentAudio(), () => 0);
            Assert.AreEqual(0, app.NowMs, 0.001);
            Assert.AreEqual(1_700_000_000_000, app.EpochMs, 0.001);
            app.Advance(240);
            Assert.AreEqual(240, app.NowMs, 0.001);
            Assert.AreEqual(1_700_000_000_240d, app.EpochMs, 0.001);
            app.CatchUp(500, 1_800_000_000_000d);
            Assert.AreEqual(500, app.NowMs, 0.001);
            Assert.AreEqual(1_800_000_000_260d, app.EpochMs, 0.001);
            Assert.AreEqual(500, clock.NowMs, 0.001);
            Assert.AreEqual(app.EpochMs, clock.EpochMs, 0.001);
        }
    }
}
