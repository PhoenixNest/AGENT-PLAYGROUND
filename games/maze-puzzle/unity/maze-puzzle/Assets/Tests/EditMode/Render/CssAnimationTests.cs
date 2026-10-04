using NUnit.Framework;

namespace OrbitalSalvage.Render.Tests
{
    public class CssAnimationTests
    {
        // CSS cubic-bezier at t = 0.25 / 0.5 / 0.75. y is the bezier output after solving x(u) = t.
        // linear; ease = (.25,.1,.25,1); ease-out = (0,0,.58,1); ease-in-out = (.42,0,.58,1);
        // cb1 = (.34,1.56,.64,1); cb2 = (.25,1,.5,1).
        static readonly (string name, double a, double b, double c)[] Curves =
        {
            ("linear", 0.25, 0.5, 0.75),
            ("ease", 0.4085105914, 0.8024033876, 0.9604589783),
            ("ease-out", 0.3781381308, 0.6846431874, 0.9065353493),
            ("ease-in-out", 0.1291619310, 0.5000000000, 0.8708380690),
            ("cubic-bezier(0.34, 1.56, 0.64, 1)", 0.8162891987, 1.0874006702, 1.0596468600),
            ("cubic-bezier(0.25, 1, 0.5, 1)", 0.6885897918, 0.9340958072, 0.9939447286),
        };

        [Test]
        public void Easing_MatchesCssAtQuarterPoints()
        {
            foreach (var curve in Curves)
            {
                Assert.AreEqual(curve.a, CssEasing.Ease(curve.name, 0.25), 1e-6, curve.name + " 0.25");
                Assert.AreEqual(curve.b, CssEasing.Ease(curve.name, 0.5), 1e-6, curve.name + " 0.5");
                Assert.AreEqual(curve.c, CssEasing.Ease(curve.name, 0.75), 1e-6, curve.name + " 0.75");
            }
            Assert.AreEqual(0, CssEasing.Ease("cubic-bezier(0.34, 1.56, 0.64, 1)", 0), 1e-9);
            Assert.AreEqual(1, CssEasing.Ease("cubic-bezier(0.34, 1.56, 0.64, 1)", 1), 1e-9);
        }

        [Test]
        public void Iteration_DirectionAndFill()
        {
            Assert.IsTrue(CssAnimation.TryDirected(250, 0, 1000, 1, "normal", "none", out var p));
            Assert.AreEqual(0.25, p, 1e-9);
            Assert.IsTrue(CssAnimation.TryDirected(500, 0, 1000, 1, "normal", "none", out p));
            Assert.AreEqual(0.5, p, 1e-9);

            // Second iteration of alternate plays backwards: 25% through it is directed 0.75.
            Assert.IsTrue(CssAnimation.TryDirected(1250, 0, 1000, 0, "alternate", "none", out p));
            Assert.AreEqual(0.75, p, 1e-9);

            // Before the delay, fill none has no effect; backwards holds the first keyframe.
            Assert.IsFalse(CssAnimation.TryDirected(100, 500, 1000, 1, "normal", "none", out _));
            Assert.IsTrue(CssAnimation.TryDirected(100, 500, 1000, 1, "normal", "backwards", out p));
            Assert.AreEqual(0, p, 1e-9);

            // Forwards holds the end. Alternate with 2 iterations ends on the reversed iteration, at 0.
            Assert.IsFalse(CssAnimation.TryDirected(2500, 0, 1000, 2, "alternate", "none", out _));
            Assert.IsTrue(CssAnimation.TryDirected(2500, 0, 1000, 2, "alternate", "forwards", out p));
            Assert.AreEqual(0, p, 1e-9);
            Assert.IsTrue(CssAnimation.TryDirected(2000, 0, 1000, 1, "normal", "forwards", out p));
            Assert.AreEqual(1, p, 1e-9);

            // reverse plays the first iteration from the end.
            Assert.IsTrue(CssAnimation.TryDirected(250, 0, 1000, 1, "reverse", "none", out p));
            Assert.AreEqual(0.75, p, 1e-9);
        }

        [Test]
        public void Keyframes_InterpolateMatchingTransformsAndStepMismatchedLists()
        {
            var matched = new KeyframeSet(new[]
            {
                new KeyframeSet.Stop { T = 0, Transform = "translateX(0px) rotate(0deg)" },
                new KeyframeSet.Stop { T = 1, Transform = "translateX(10px) rotate(90deg)" },
            });
            Assert.IsTrue(matched.TryTransform(0.5, "linear", out var a, out var b, out var u));
            Assert.AreEqual(0.5, u, 1e-9);
            var mid = CssTransform.Interpolate(a, b, u, 100, 40, 0);
            Assert.AreEqual(5, mid.e, 1e-6);
            Assert.AreEqual(0.7071067812, mid.a, 1e-6);

            var mismatch = CssTransform.Interpolate("translateX(0px)", "translateX(10px) rotate(90deg)", 0, 80, 20, 0);
            var start = CssTransform.ToMatrix("translateX(0px)", 80, 20, 0);
            Assert.AreEqual(start.a, mismatch.a, 1e-9);
            Assert.AreEqual(start.e, mismatch.e, 1e-9);
            var end = CssTransform.Interpolate("translateX(0px)", "translateX(10px) rotate(90deg)", 1, 80, 20, 0);
            var expect = CssTransform.ToMatrix("translateX(10px) rotate(90deg)", 80, 20, 0);
            Assert.AreEqual(expect.a, end.a, 1e-6);
            Assert.AreEqual(expect.e, end.e, 1e-6);
            var half = CssTransform.Interpolate("translateX(0px)", "rotate(90deg)", 0.5, 40, 40, 0);
            Assert.AreNotEqual(start.a, half.a);
        }

        [Test]
        public void OvershootBezier_ExtrapolatesPastTheKeyframe()
        {
            // cartoonToast 0% → 15% at 200ms: cubic-bezier(0.34, 1.56, 0.64, 1) yields u ≈ 1.0909.
            var m = CssTransform.Interpolate("translateY(-16px) scale(0.85)", "translateY(0px) scale(1)", 1.0909, 360, 42, 0);
            Assert.AreEqual(1.0136, m.a, 1e-3);
            Assert.AreEqual(1.0136, m.d, 1e-3);
            Assert.AreEqual(1.454, m.f, 2e-2);
        }

        [Test]
        public void FloatUpFade_ResolvesCalcVar()
        {
            var m = CssTransform.ToMatrix("translate(calc(-50% + var(--dx, 0px)), calc(-50% - 40px)) scale(0.8)", 20, 20, 12);
            Assert.AreEqual(2, m.e, 1e-6);
            Assert.AreEqual(-50, m.f, 1e-6);
            Assert.AreEqual(0.8, m.a, 1e-9);
            Assert.AreEqual(0.8, m.d, 1e-9);

            var from = "translate(-50%, -50%) scale(0.5)";
            var to = "translate(calc(-50% + var(--dx, 0px)), calc(-50% - 40px)) scale(1.3)";
            var blended = CssTransform.Interpolate(from, to, 0.5, 20, 20, 12);
            Assert.AreEqual(-4, blended.e, 1e-4);
            Assert.AreEqual(-30, blended.f, 1e-4);
            Assert.AreEqual(0.9, blended.a, 1e-4);
        }

        [Test]
        public void OpacityKeyframes_UseSegmentEasing()
        {
            var set = new KeyframeSet(new[]
            {
                new KeyframeSet.Stop { T = 0, Opacity = "0" },
                new KeyframeSet.Stop { T = 1, Opacity = "1" },
            });
            var sample = CssAnimation.Evaluate(500, 0, 1000, 0, "normal", "none", "linear", set, 10, 10, 0);
            Assert.IsTrue(sample.Opacity);
            Assert.AreEqual(0.5, sample.OpacityValue, 1e-9);
            sample = CssAnimation.Evaluate(100, 1000, 2500, 0, "normal", "none", "linear", set, 10, 10, 0);
            Assert.IsFalse(sample.Opacity);
        }

        [Test]
        public void UiClock_ScalesInjectedTime()
        {
            double now = 10;
            var clock = new UiClock { NowSeconds = () => now, Scale = 2 };
            clock.Restart();
            now = 10.5;
            Assert.AreEqual(1000, clock.Milliseconds, 1e-6);
        }
    }
}
