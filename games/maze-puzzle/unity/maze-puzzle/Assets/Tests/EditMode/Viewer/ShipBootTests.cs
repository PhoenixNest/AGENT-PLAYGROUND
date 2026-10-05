using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Tests
{
    public class ShipBootTests
    {
        [Test]
        public void ProductBoot_SelectsNewStackWithoutFlags()
        {
            Assert.IsTrue(ParityViewer.ProductBoot(new string[0]));
            Assert.IsTrue(ParityViewer.ProductBoot(new[] { "player.exe" }));
        }

        [Test]
        public void ProductBoot_ParityFlagsStayOffTheProductPath()
        {
            Assert.IsFalse(ParityViewer.ProductBoot(new[] { "--parity" }));
            Assert.IsFalse(ParityViewer.ShouldStart(new[] { "--parity" }));
            Assert.IsFalse(ParityViewer.ProductBoot(new[] { "-parity-app-selftest", "out" }));
            Assert.IsFalse(ParityViewer.ProductBoot(new[] { "-parity-app-flows", "out" }));
            Assert.IsTrue(ParityViewer.ShouldStart(new[] { "-parity-app-selftest", "out" }));
            Assert.IsTrue(ParityViewer.ProductBoot(new[] { "-ship-selftest", "out" }));
            Assert.AreEqual("out", ParityViewer.ShipSelfTestDirectory(new[] { "-ship-selftest", "out" }));
            Assert.IsFalse(ParityViewer.ProductBoot(new[] { "-no-parity-viewer" }));
        }

        [Test]
        public void Pointer_RoundTripsThroughLetterbox()
        {
            var box = ParityViewer.Letterbox(1920, 1080);
            Assert.Less(box.width / box.height, 0.47f);
            Assert.Greater(box.x, 0f);
            var screen = ParityViewer.CssToScreen(box, 215, 795);
            Assert.IsTrue(ParityViewer.TryCss(box, screen, out double cssX, out double cssY));
            Assert.AreEqual(215, cssX, 0.05);
            Assert.AreEqual(795, cssY, 0.05);

            string spec = File.ReadAllText(Path.Combine(Application.dataPath, "Resources", "Parity", "Spec", "home_default__zero.json"));
            Assert.IsTrue(ParityPages.TryHitControl(spec, cssX, cssY, out var hit));
            Assert.AreEqual("btn-home-play", hit.Id);
        }

        [Test]
        public void FitPage_StaysInsideSafeArea()
        {
            var safe = new Rect(20, 40, 400, 800);
            var page = ParityViewer.FitPage(safe);
            Assert.GreaterOrEqual(page.x, safe.x - 0.01f);
            Assert.GreaterOrEqual(page.y, safe.y - 0.01f);
            Assert.LessOrEqual(page.xMax, safe.xMax + 0.01f);
            Assert.LessOrEqual(page.yMax, safe.yMax + 0.01f);
            var landscape = ParityViewer.FitPage(new Rect(0, 0, 2000, 800));
            Assert.AreEqual(800f, landscape.height, 0.01f);
            Assert.Less(landscape.width, 2000f);
        }

        [Test]
        public void Slider_MapsCssXToPercent()
        {
            Assert.AreEqual(0, ParityPages.SliderValue(10, 10, 100));
            Assert.AreEqual(50, ParityPages.SliderValue(60, 10, 100));
            Assert.AreEqual(100, ParityPages.SliderValue(200, 10, 100));
        }
    }
}
