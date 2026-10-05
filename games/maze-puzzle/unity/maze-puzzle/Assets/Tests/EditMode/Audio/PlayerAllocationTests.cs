using System;
using NUnit.Framework;
using UnityEngine;

namespace OrbitalSalvage.AudioDsp.Tests
{
    /// <summary>
    /// Unity's Mono does not implement GC.GetAllocatedBytesForCurrentThread, so the meter falls back to the managed
    /// heap size (4 KB block granularity) plus the collection count. Both paths are validated against a deliberate
    /// 64 KB allocation by MeterDetectsDeliberateAllocation.
    /// </summary>
    public sealed class AllocationMeter
    {
        readonly bool exact;

        public string Source { get; private set; }

        public AllocationMeter()
        {
            long before = GC.GetAllocatedBytesForCurrentThread();
            byte[] probe = new byte[4096];
            GC.KeepAlive(probe);
            exact = GC.GetAllocatedBytesForCurrentThread() - before >= 4096;
            Source = exact ? "GC.GetAllocatedBytesForCurrentThread (exact)" : "GC.GetTotalMemory + CollectionCount (4 KB granularity)";
        }

        public long Sample()
        {
            return exact ? GC.GetAllocatedBytesForCurrentThread() : GC.GetTotalMemory(false);
        }

        public int Collections
        {
            get { return GC.CollectionCount(0); }
        }
    }
    public class PlayerAllocationTests
    {
        const int BufferFrames = 1024;
        const int OutputRate = 48000;
        const int Channels = 2;

        GameObject host;
        WebAudioPlayer player;
        System.Random scenario;
        float[] buffer;

        [SetUp]
        public void SetUp()
        {
            host = new GameObject("WebAudioPlayerAllocationTest");
            player = host.AddComponent<WebAudioPlayer>();
            player.SetRandomSource(new CountingRandom(0x5eed1234u));
            scenario = new System.Random(20260101);
            buffer = new float[BufferFrames * Channels];
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(host);
        }

        static readonly string[] DebrisTypes = { "titanium", "solar", "volatile", "magnetic" };
        static readonly string[] GateTypes = { "MULTIPLY", "ADD", "BOOST" };

        void PostRandomCue()
        {
            switch (scenario.Next(16))
            {
                case 0:
                    player.PlaySpinRatchet(0.6 + scenario.NextDouble() * 1.2);
                    break;
                case 1:
                case 2:
                    player.PlayDebrisCollision(0.1 + scenario.NextDouble(), DebrisTypes[scenario.Next(DebrisTypes.Length)]);
                    break;
                case 3:
                    player.PlayGatePass(GateTypes[scenario.Next(GateTypes.Length)], 2 + scenario.Next(4));
                    break;
                case 4:
                    player.PlayShredderPass();
                    break;
                case 5:
                    player.PlayCompactorPass();
                    break;
                case 6:
                    player.PlaySuperShredderPass();
                    break;
                case 7:
                    player.PlayBoosterPass();
                    break;
                case 8:
                case 9:
                    player.PlayRcsThrust();
                    break;
                case 10:
                    player.PlayBumperBounce();
                    break;
                case 11:
                case 12:
                    player.PlayDebrisSwallowed();
                    break;
                case 13:
                    player.PlayHazardExplosion();
                    break;
                case 14:
                    if (scenario.Next(4) == 0)
                    {
                        player.PlayVictoryFanfare();
                    }
                    else
                    {
                        player.PlayBackupPodSound();
                    }

                    break;
                default:
                    if (scenario.Next(6) == 0)
                    {
                        player.PlayDefeatSound();
                    }
                    else
                    {
                        player.SetThrusterActive(true);
                    }

                    break;
            }
        }

        double RunMix(double seconds, AllocationMeter meter, out long allocatedBytes, out int worstBuffer)
        {
            int buffers = (int)Math.Ceiling(seconds * OutputRate / BufferFrames);
            allocatedBytes = 0;
            worstBuffer = -1;
            long worst = 0;
            double peak = 0;
            for (int b = 0; b < buffers; b++)
            {
                int cues = scenario.Next(4);
                for (int i = 0; i < cues; i++)
                {
                    PostRandomCue();
                }

                long before = meter != null ? meter.Sample() : 0;
                player.FillBuffer(buffer, Channels, OutputRate);
                if (meter != null)
                {
                    long delta = meter.Sample() - before;
                    allocatedBytes += delta;
                    if (delta > worst)
                    {
                        worst = delta;
                        worstBuffer = b;
                    }
                }

                for (int i = 0; i < buffer.Length; i++)
                {
                    peak = Math.Max(peak, Math.Abs(buffer[i]));
                }
            }

            return peak;
        }

        [Test]
        public void FillBufferDoesNotAllocateAfterWarmup()
        {
            player.PublicInit();
            RunMix(6.0, null, out _, out _);

            AllocationMeter meter = new AllocationMeter();
            {
                long allocated;
                int worstBuffer;
                int collectionsBefore = meter.Collections;
                double peak = RunMix(10.0, meter, out allocated, out worstBuffer);
                int collections = meter.Collections - collectionsBefore;
                TestContext.WriteLine("allocation source: " + meter.Source + ", allocated " + allocated + " bytes over 10 s, peak " + peak);
                Assert.Greater(peak, 0.01, "mix should be audible");
                Assert.AreEqual(0, player.DroppedCommands, "command ring overflowed");
                Assert.AreEqual(0, collections, "a garbage collection ran during FillBuffer");
                Assert.AreEqual(0L, allocated, "FillBuffer allocated " + allocated + " bytes (worst buffer " + worstBuffer + ") via " + meter.Source);
            }
        }

        [Test]
        public void MeterDetectsDeliberateAllocation()
        {
            AllocationMeter meter = new AllocationMeter();
            {
                long before = meter.Sample();
                byte[] garbage = new byte[64 * 1024];
                GC.KeepAlive(garbage);
                Assert.GreaterOrEqual(meter.Sample() - before, 64 * 1024, "allocation meter (" + meter.Source + ") is blind");
            }
        }

        [Test]
        public void FinishedNodesAreRecycledAndGraphDrains()
        {
            player.PublicInit();
            RunMix(5.0, null, out _, out _);
            for (int i = 0; i < 200; i++)
            {
                player.FillBuffer(buffer, Channels, OutputRate);
            }

            AudioEngineDsp engineProbe = null;
            player.Post(e => engineProbe = e);
            player.FillBuffer(buffer, Channels, OutputRate);
            Assert.IsNotNull(engineProbe);
            Assert.AreEqual(0, engineProbe.Context.ActiveNodeCount, "finished nodes were not reclaimed");
        }

        [Test]
        public void CommandRingDropsInsteadOfBlockingWhenFull()
        {
            for (int i = 0; i < WebAudioPlayer.CommandCapacity + 10; i++)
            {
                player.PlayBumperBounce();
            }

            Assert.AreEqual(10, player.DroppedCommands);
            player.FillBuffer(buffer, Channels, OutputRate);
            player.PublicInit();
            player.PlayBumperBounce();
            Assert.AreEqual(10, player.DroppedCommands);
        }
    }
}
