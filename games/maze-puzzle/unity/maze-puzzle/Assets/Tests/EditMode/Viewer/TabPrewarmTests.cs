using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using OrbitalSalvage.App;
using OrbitalSalvage.Render;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Tests
{
    public class TabPrewarmTests
    {
        [Test]
        public void PrewarmTemplates_FreshProfile_ResolvesTheFiveMainTabs()
        {
            var app = FreshLoadingApp();
            Assert.AreEqual("loading", app.CurrentPage);
            var zero = ParityPages.PrewarmTemplates(app, "zero");
            Assert.AreEqual(ParityPages.MainTabPages.Length, zero.Length);
            Assert.AreEqual(5, zero.Length);
            string[] expect =
            {
                "home_default",
                "maps_world_1_fresh",
                "daily_default",
                "shop_rings_default",
                "achievements_none"
            };
            var seen = new HashSet<string>();
            for (int i = 0; i < zero.Length; i++)
            {
                Assert.AreEqual(ParityPages.MainTabPages[i], zero[i].Page);
                Assert.AreEqual(expect[i], zero[i].TemplateId, zero[i].Page);
                Assert.IsTrue(string.IsNullOrEmpty(zero[i].CardStamp), zero[i].Page);
                Assert.AreEqual("zero", zero[i].Profile);
                Assert.AreEqual(ParityPages.CacheKey(expect[i], "zero", null), zero[i].CacheKey);
                Assert.IsTrue(seen.Add(zero[i].TemplateId));
            }
            Assert.AreEqual("loading", app.CurrentPage, "resolving templates must not leave the loading page");

            var phone = ParityPages.PrewarmTemplates(app, ParityPages.IphoneProfile);
            Assert.AreEqual(zero.Length, phone.Length);
            for (int i = 0; i < phone.Length; i++)
            {
                Assert.AreEqual(zero[i].TemplateId, phone[i].TemplateId);
                Assert.AreEqual(ParityPages.IphoneProfile, phone[i].Profile);
                Assert.AreEqual(ParityPages.CacheKey(phone[i].TemplateId, ParityPages.IphoneProfile, phone[i].CardStamp), phone[i].CacheKey);
            }
        }

        [Test]
        public void CacheKey_IncludesProfileAndCardStamp()
        {
            Assert.AreEqual("home_default@zero", ParityPages.CacheKey("home_default", "zero", null));
            Assert.AreEqual("home_default@zero", ParityPages.CacheKey("home_default", "zero", ""));
            Assert.AreEqual("shop_rings_default@iphone#ring_donut:owned", ParityPages.CacheKey("shop_rings_default", "iphone", "ring_donut:owned"));
        }

        [Test]
        public void CacheCapacity_HoldsEveryMainTabPlusAModalMargin()
        {
            Assert.GreaterOrEqual(ParityPages.CacheCapacity, ParityPages.MainTabPages.Length + ParityPages.ModalCacheMargin);
            Assert.GreaterOrEqual(ParityPages.ModalCacheMargin, 1);
        }

        [Test]
        public void ShouldPrewarmTabs_IsOffWhileSelfTesting()
        {
            Assert.IsFalse(ParityPages.ShouldPrewarmTabs(true));
            Assert.IsTrue(ParityPages.ShouldPrewarmTabs(false));
        }

        [Test]
        public void RasterScale_IsStaleWhenTheLiveStepDiffers()
        {
            Assert.IsTrue(ParityPages.RasterScaleIsStale(3, 2));
            Assert.IsTrue(ParityPages.RasterScaleIsStale(2, 3));
            Assert.IsFalse(ParityPages.RasterScaleIsStale(3, 3));
            Assert.IsFalse(ParityPages.RasterScaleIsStale(2, 2.04));
            Assert.IsTrue(ParityPages.RasterScaleIsStale(2, 1.5));
        }

        [Test]
        public void PinnedTabs_SurviveRepeatedCycling_ModalsStayBounded()
        {
            int tabs = ParityPages.MainTabPages.Length;
            int capacity = tabs + ParityPages.ModalCacheMargin;
            var lru = new ChromeLru<string>(capacity);
            var evicted = new List<string>();
            var values = new string[tabs];
            for (int i = 0; i < tabs; i++)
            {
                string key = "tab" + i;
                values[i] = "T" + i;
                lru.Pin(key);
                lru.Add(key, values[i], evicted);
                Assert.IsTrue(lru.IsPinned(key));
            }
            Assert.AreEqual(0, evicted.Count);
            for (int cycle = 0; cycle < 6; cycle++)
            {
                for (int i = 0; i < tabs; i++)
                {
                    Assert.IsTrue(lru.TryGet("tab" + i, out var value));
                    Assert.AreEqual(values[i], value);
                    lru.Add("tab" + i, values[i], evicted);
                }
            }
            Assert.AreEqual(0, evicted.Count, "cycling the main tabs evicted a resident page");

            int opened = ParityPages.ModalCacheMargin + 5;
            for (int i = 0; i < opened; i++)
                lru.Add("modal" + i, "M" + i, evicted);
            Assert.LessOrEqual(lru.Count, capacity, "modal overflow grew the cache without bound");
            Assert.AreEqual(capacity, lru.Count);
            for (int i = 0; i < tabs; i++)
                Assert.IsTrue(lru.Contains("tab" + i), "modal evicted tab" + i);
            Assert.Greater(evicted.Count, 0);
            Assert.IsFalse(lru.Contains("modal0"), "the oldest modal should leave once the margin is full");
            Assert.IsTrue(lru.Contains("modal" + (opened - 1)));
        }

        [Test]
        public void TryTakeTabPrewarm_BakesOneTemplatePerCall()
        {
            var pending = new Queue<string>();
            pending.Enqueue("home_default");
            pending.Enqueue("maps_world_1_fresh");
            pending.Enqueue("daily_default");
            Assert.IsFalse(ParityPages.TryTakeTabPrewarm(pending, true, false, out _));
            Assert.AreEqual(3, pending.Count, "a deferred frame must not start a bake");
            Assert.IsFalse(ParityPages.TryTakeTabPrewarm(pending, false, true, out _));
            Assert.AreEqual(3, pending.Count, "the loading bar's override set is not ready yet");
            Assert.IsTrue(ParityPages.TryTakeTabPrewarm(pending, false, false, out string first));
            Assert.AreEqual("home_default", first);
            Assert.AreEqual(2, pending.Count);
            Assert.IsTrue(ParityPages.TryTakeTabPrewarm(pending, false, false, out string second));
            Assert.AreEqual("maps_world_1_fresh", second);
        }

        [Test]
        public void Resize_DropsBakedLayersUntilTheNewScaleIsBaked()
        {
            bool savedDefer = HomeChrome.DeferLayerCache;
            HomeChrome.DeferLayerCache = false;
            var chrome = HomeChrome.Load("home_default", "zero");
            try
            {
                chrome.BakeStaticLayers();
                Assert.IsTrue(chrome.LayerCacheReady);
                Assert.Greater(chrome.BakedLayerCount, 0);
                long bakedAt3 = chrome.BakedLayerBytes;
                Assert.Greater(bakedAt3, 0);
                Assert.IsFalse(chrome.InvalidateIfScaleDiffers(chrome.RasterScale));
                Assert.IsTrue(chrome.LayerCacheReady, "the same scale must keep the bake");
                Assert.IsTrue(chrome.InvalidateIfScaleDiffers(2));
                Assert.IsFalse(chrome.LayerCacheReady);
                Assert.AreEqual(0, chrome.BakedLayerBytes);
                Assert.IsTrue(ParityPages.RasterScaleIsStale(3, chrome.RasterScale));
                chrome.BakeStaticLayers();
                Assert.IsTrue(chrome.LayerCacheReady);
                Assert.AreEqual(2, chrome.RasterScale, 0.01);
                Assert.AreEqual(Mathf.RoundToInt(430 * 2f), chrome.PixelWidth);
                Assert.Greater(chrome.BakedLayerBytes, 0);
                Assert.Less(chrome.BakedLayerBytes, bakedAt3);
            }
            finally
            {
                HomeChrome.DeferLayerCache = savedDefer;
                chrome.Dispose();
            }
        }

        [Test]
        public void Dispose_ReleasesBakedRenderTextures()
        {
            bool savedDefer = HomeChrome.DeferLayerCache;
            HomeChrome.DeferLayerCache = false;
            var chrome = HomeChrome.Load("daily_default", "zero");
            try
            {
                chrome.BakeStaticLayers();
                long layers = chrome.BakedLayerBytes;
                Assert.Greater(layers, 0);
                var target = chrome.Target;
                Assert.IsTrue(target.IsCreated());
                chrome.Dispose();
                chrome = null;
                Assert.IsFalse(target.IsCreated());
            }
            finally
            {
                HomeChrome.DeferLayerCache = savedDefer;
                if (chrome != null) chrome.Dispose();
            }
        }

        sealed class Blob
        {
            public Blob(long bytes) { Bytes = bytes; }
            public long Bytes;
        }

        [Test]
        public void ResidentBudget_MatchesDeviceMemory()
        {
            Assert.AreEqual(ParityPages.Phone4GbBudgetBytes, ParityPages.ResidentBudgetBytes(4096, true), "4 GB phone");
            Assert.AreEqual(320L * 1024 * 1024, ParityPages.ResidentBudgetBytes(4096, true));
            Assert.AreEqual(ParityPages.Phone6GbBudgetBytes, ParityPages.ResidentBudgetBytes(6144, true), "6 GB phone");
            Assert.AreEqual(ParityPages.Phone6GbBudgetBytes, ParityPages.ResidentBudgetBytes(8192, true), "8 GB phone stays on the 6 GB cap");
            Assert.AreEqual(ParityPages.DesktopBudgetBytes, ParityPages.ResidentBudgetBytes(16384, false), "desktop");
            Assert.AreEqual(ParityPages.DesktopBudgetBytes, ParityPages.ResidentBudgetBytes(4096, false), "editor on a small machine still uses the desktop cap");
            long small = ParityPages.ResidentBudgetBytes(512, true);
            Assert.AreEqual(512L * 1024 * 1024 * ParityPages.PhoneBudgetPercent / 100, small);
            Assert.Less(small, ParityPages.Phone4GbBudgetBytes, "the 35% share binds below the 4 GB cap");
            Assert.AreEqual(ParityPages.Phone4GbBudgetBytes, ParityPages.ResidentBudgetBytes(6000, true), "just under 6 GB stays on the 4 GB cap");
        }

        [Test]
        public void PrewarmPrefix_StopsWhenTheNextPageWouldExceed()
        {
            long phone4 = ParityPages.ResidentBudgetBytes(4096, true);
            long phone6 = ParityPages.ResidentBudgetBytes(6144, true);
            long desk = ParityPages.ResidentBudgetBytes(16384, false);
            long[] at2 = ParityPages.MainTabBytes(2);
            long[] at3 = ParityPages.MainTabBytes(3);
            Assert.AreEqual(ParityPages.HomeBytesAt2x, at2[0]);
            Assert.AreEqual(346256640L, at3[0], "3× is 2.25× the 2× measurement");
            Assert.AreEqual(2, ParityPages.CountFitting(at2, phone4), "4 GB phone at 2× keeps home and maps");
            Assert.AreEqual(0, ParityPages.CountFitting(at3, phone4), "4 GB phone at 3× cannot pre-bake home");
            Assert.AreEqual(3, ParityPages.CountFitting(at2, phone6), "6 GB phone at 2× keeps home, maps, and daily");
            Assert.AreEqual(1, ParityPages.CountFitting(at3, phone6), "6 GB phone at 3× keeps home only");
            Assert.AreEqual(5, ParityPages.CountFitting(at2, desk));
            Assert.AreEqual(4, ParityPages.CountFitting(at3, desk), "desktop at 3× stops before the fifth tab");
            long homeAndMaps = at2[0] + at2[1];
            Assert.LessOrEqual(homeAndMaps, phone4);
            Assert.Greater(homeAndMaps + at2[2], phone4);
            Assert.AreEqual(0, ParityPages.CountFitting(at2, phone4, phone4), "nothing else fits once the budget is already full");
        }

        [Test]
        public void ByteBudget_EvictsUnpinnedThenOldestPinned_KeepsTheCurrentPage()
        {
            var lru = new ChromeLru<Blob>(8);
            lru.Cost = blob => blob.Bytes;
            lru.ByteBudget = 100;
            lru.ProtectKey = "home";
            var evicted = new List<Blob>();
            var home = new Blob(60);
            var modal = new Blob(60);
            lru.Pin("home");
            lru.Add("home", home, evicted);
            lru.Add("modal", modal, evicted);
            Assert.AreEqual(1, evicted.Count, "the unpinned modal leaves before the pinned tab");
            Assert.AreSame(modal, evicted[0]);
            Assert.IsTrue(lru.Contains("home"));
            Assert.IsFalse(lru.Contains("modal"));

            evicted.Clear();
            lru.ByteBudget = 150;
            var maps = new Blob(100);
            var daily = new Blob(100);
            lru.Pin("maps");
            lru.Pin("daily");
            lru.ProtectKey = "daily";
            lru.Add("maps", maps, evicted);
            lru.Add("daily", daily, evicted);
            Assert.IsFalse(lru.Contains("home"), "the oldest pinned tab leaves once unpinned entries are gone");
            Assert.IsFalse(lru.Contains("maps"), "maps is older than the current page");
            Assert.IsTrue(lru.Contains("daily"));
            Assert.Greater(evicted.Count, 0);

            evicted.Clear();
            lru.ByteBudget = 50;
            lru.Trim(evicted);
            Assert.AreEqual(0, evicted.Count, "the current page stays even when it alone is over the budget");
            Assert.IsTrue(lru.Contains("daily"));
            Assert.AreEqual(1, lru.Count);
            Assert.Greater(lru.TotalBytes, lru.ByteBudget);
        }

        [Test]
        public void ScaleRaise_DoesNotRebakeBackgroundPagesPastTheBudget()
        {
            long budget = ParityPages.ResidentBudgetBytes(4096, true);
            long home2 = ParityPages.EstimateResidentBytes("home_default", 2);
            long maps2 = ParityPages.EstimateResidentBytes("maps_world_1_fresh", 2);
            long home3 = ParityPages.EstimateResidentBytes("home_default", 3);
            long maps3 = ParityPages.EstimateResidentBytes("maps_world_1_fresh", 3);
            long daily2 = ParityPages.EstimateResidentBytes("daily_default", 2);
            Assert.IsTrue(ParityPages.MayRebakeResident(home2, 0, maps2, budget, false), "home + maps at 2× still fits");
            Assert.IsFalse(ParityPages.MayRebakeResident(home2 + maps2, 0, daily2, budget, false), "daily would pass the 4 GB budget");
            Assert.IsFalse(
                ParityPages.MayRebakeResident(home2, maps2, maps3, budget, false),
                "raising maps to 3× beside home at 2× passes the budget");
            Assert.IsTrue(
                ParityPages.MayRebakeResident(home3, maps3, maps2, budget, false),
                "dropping maps from 3× to 2× frees memory");
            Assert.IsTrue(
                ParityPages.MayRebakeResident(maps3, home2, home3, budget, true),
                "the current page is shown even when the rebake passes the budget");
        }

        [Test]
        public void MainTabs_ReportBakedBytesAt3xAnd2x()
        {
            bool savedDefer = HomeChrome.DeferLayerCache;
            HomeChrome.DeferLayerCache = false;
            var app = FreshLoadingApp();
            var entries = ParityPages.PrewarmTemplates(app, "zero");
            var report = new StringBuilder();
            long budget = ParityPages.ResidentBudgetBytes(4096, true);
            try
            {
                for (int s = 0; s < 2; s++)
                {
                    double scale = s == 0 ? 3 : 2;
                    var totals = new long[entries.Length];
                    for (int i = 0; i < entries.Length; i++)
                    {
                        var chrome = HomeChrome.Load(entries[i].TemplateId, "zero");
                        try
                        {
                            if (scale < 2.5) chrome.InvalidateIfScaleDiffers(scale);
                            chrome.BakeStaticLayers();
                            Assert.IsTrue(chrome.LayerCacheReady, entries[i].TemplateId);
                            Assert.Greater(chrome.BakedLayerCount, 0, entries[i].TemplateId);
                            Assert.Greater(chrome.BakedLayerBytes, 0, entries[i].TemplateId);
                            Assert.AreEqual(scale, chrome.RasterScale, 0.01, entries[i].TemplateId);
                            long expect = ParityPages.EstimateResidentBytes(entries[i].TemplateId, scale);
                            Assert.AreEqual(expect, chrome.ResidentBytes, entries[i].TemplateId + " @" + scale);
                            totals[i] = chrome.ResidentBytes;
                            int skipped = chrome.SkippedLayerCount;
                            int clear = 0;
                            int identical = 0;
                            if (scale < 2.5)
                            {
                                var audit = chrome.AuditLayerIdentity();
                                skipped = audit.Skipped;
                                clear = audit.FullyClear;
                                identical = audit.IdenticalToAnother;
                                Assert.AreEqual(0, audit.Unreadable, entries[i].TemplateId + " layer readback");
                            }
                            string line = entries[i].TemplateId
                                + " scale=" + scale.ToString("0.#")
                                + " layers=" + chrome.BakedLayerCount
                                + " slots=" + chrome.BakedLayerSlots
                                + " skipped=" + skipped
                                + " clear=" + clear
                                + " identical=" + identical
                                + " px=" + chrome.PixelWidth + "x" + chrome.PixelHeight
                                + " layerBytes=" + chrome.BakedLayerBytes
                                + " canvasBytes=" + chrome.CanvasResidentBytes
                                + " totalBytes=" + chrome.ResidentBytes;
                            report.Append(line).Append('\n');
                            Debug.Log("[TabBake] " + line);
                        }
                        finally
                        {
                            chrome.Dispose();
                        }
                    }
                    int fit = ParityPages.CountFitting(totals, budget);
                    long resident = 0;
                    for (int i = 0; i < fit; i++) resident += totals[i];
                    string summary = "4gb-phone scale=" + scale.ToString("0.#")
                        + " budget=" + budget
                        + " fit=" + fit
                        + " resident=" + resident;
                    report.Append(summary).Append('\n');
                    Debug.Log("[TabBake] " + summary);
                    if (scale > 2.5) Assert.AreEqual(0, fit, "3× home alone is over the 4 GB phone budget");
                    else Assert.AreEqual(2, fit, "2× keeps home and maps under the 4 GB phone budget");
                    if (fit < totals.Length)
                        Assert.Greater(resident + totals[fit], budget);
                    if (fit > 0) Assert.LessOrEqual(resident, budget);
                }
                File.WriteAllText("/tmp/prebake-tabs/memory.txt", report.ToString());
            }
            finally
            {
                HomeChrome.DeferLayerCache = savedDefer;
            }
        }

        static AppController FreshLoadingApp()
        {
            var app = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 0));
            app.BeginBoot();
            return app;
        }
    }
}
