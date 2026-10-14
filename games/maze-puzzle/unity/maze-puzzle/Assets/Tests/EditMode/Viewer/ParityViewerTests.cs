using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using OrbitalSalvage.Render;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Tests
{
    public class ParityViewerTests
    {
        [Test]
        public void ProfileFor_UsesTopOrBottomInset()
        {
            var notch = new Rect(0f, 102f, 1170f, 2328f);
            Assert.AreEqual("iphone", ParityPages.ProfileFor(1170, 2532, notch));
            Assert.AreEqual("zero", ParityPages.ProfileFor(1170, 2532, new Rect(0f, 0f, 1170f, 2532f)));
            Assert.AreEqual("zero", ParityPages.ProfileFor(1920, 1080, new Rect(0f, 0f, 1920f, 1080f)));
            Assert.AreEqual("zero", ParityPages.ProfileFor(430, 932, new Rect(0f, 0f, 430f, 932f)));
            Assert.AreEqual("zero", ParityPages.ProfileFor(1366, 768, new Rect(0f, 0f, 1366f, 768f)));
            Assert.AreEqual("zero", ParityPages.ProfileFor(1170, 2532, new Rect(0f, 0f, 0f, 0f)));
            Assert.AreEqual("zero", ParityPages.ProfileFor(4, 4, notch));
            // Left inset only (landscape editor / rotated phone) stays on the zero spec.
            Assert.AreEqual("zero", ParityPages.ProfileFor(2532, 1170, new Rect(120f, 0f, 2292f, 1170f)));
        }

        [Test]
        public void PageSwitchHint_StartsOffOutsideEditorAndDevelopmentBuilds()
        {
            Assert.IsTrue(ParityViewer.HintStartsVisible(true, false));
            Assert.IsTrue(ParityViewer.HintStartsVisible(false, true));
            Assert.IsFalse(ParityViewer.HintStartsVisible(false, false));
            Assert.IsTrue(ParityViewer.HintVisibleInThisPlayer);
        }

        [Test]
        public void NavTargets_ArePaintedStates()
        {
            Assert.AreEqual(13, ParityPages.StateIds.Length);
            var known = new HashSet<string>(ParityPages.StateIds);
            Assert.Greater(ParityPages.Edges.Count, 0);
            foreach (var edge in ParityPages.Edges)
            {
                Assert.IsTrue(known.Contains(edge.StateId), edge.StateId);
                Assert.IsTrue(known.Contains(edge.TargetId), edge.TargetId);
            }
        }

        [Test]
        public void NavSelectors_ExistInSpec()
        {
            var seen = new Dictionary<string, string>();
            foreach (var edge in ParityPages.Edges)
            {
                if (!seen.TryGetValue(edge.StateId, out var json))
                {
                    json = File.ReadAllText(SpecPath(edge.StateId));
                    seen[edge.StateId] = json;
                }
                string label = edge.Id ?? (edge.ClassToken + "[" + edge.ClassIndex + "]");
                Assert.IsTrue(ParityPages.SelectorExists(json, edge), edge.StateId + " " + label);
            }
        }

        [Test]
        public void Hit_MapsThreeClicks()
        {
            // home play button center (rect 36,764 358x62), including its label child.
            Assert.AreEqual("game_lvl01_t1000ms", Hit("home_default", 215, 795));
            // home MAPS dock center (rect 10,871 102.5x54).
            Assert.AreEqual("maps_world_1_fresh", Hit("home_default", 61.25, 898));
            // shop rings, second tab-btn center (singularities; rect 151.328,234.5 127.344x35).
            Assert.AreEqual("shop_singularities_default", Hit("shop_rings_default", 215, 252));
        }

        [Test]
        public void Lru_EvictsLeastRecentAndRefreshesOnGet()
        {
            var lru = new ChromeLru<string>(3);
            var evicted = new List<string>();
            lru.Add("a", "A", evicted);
            lru.Add("b", "B", evicted);
            lru.Add("c", "C", evicted);
            Assert.AreEqual(0, evicted.Count);
            Assert.IsTrue(lru.TryGet("a", out var got));
            Assert.AreEqual("A", got);
            lru.Add("d", "D", evicted);
            Assert.AreEqual(1, evicted.Count);
            Assert.AreEqual("B", evicted[0]);
            Assert.IsFalse(lru.Contains("b"));
            Assert.IsTrue(lru.Contains("a"));
            Assert.IsTrue(lru.Contains("c"));
            Assert.IsTrue(lru.Contains("d"));
            Assert.AreEqual(3, lru.Count);
        }

        [Test]
        public void PlayDrag_PassesTheVerticalPosition()
        {
            var session = LiveGameSession.CreatePlay(ParityPages.CssWidth, ParityPages.CssHeight, 1);
            try
            {
                double cx = session.Physics.Center.X;
                double cy = session.Physics.Center.Y;
                session.NowMs = () => 8000;
                ParityViewer.DispatchPlayPointer(session, cx, cy + 120, true, false, false);
                ParityViewer.DispatchPlayPointer(session, cx - 30, cy + 120, false, true, false);
                Assert.Greater(session.Physics.AngularVelocity, 0, "a left drag on the bottom is clockwise only when Y is used");
            }
            finally
            {
                session.Renderer.Dispose();
            }
        }

        static string Hit(string stateId, double x, double y)
        {
            return ParityPages.Hit(stateId, File.ReadAllText(SpecPath(stateId)), x, y);
        }

        static string SpecPath(string stateId)
        {
            return Path.Combine(Application.dataPath, "Resources", "Parity", "Spec", stateId + "__zero.json");
        }
    }
}
