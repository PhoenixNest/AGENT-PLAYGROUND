using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Tests
{
    public class QualityGovernorTests
    {
        [Test]
        public void VersionedKey_IgnoresLegacyKey()
        {
            Assert.AreEqual("os.parity.scale.v2.1170x2532", QualityGovernor.ScalePrefKey(1170, 2532));
            Assert.AreEqual("os.parity.scale.1170x2532", QualityGovernor.LegacyScalePrefKey(1170, 2532));
            Assert.AreNotEqual(QualityGovernor.ScalePrefKey(1170, 2532), QualityGovernor.LegacyScalePrefKey(1170, 2532));

            var prefs = new MemoryPrefs();
            prefs.SetFloat(QualityGovernor.LegacyScalePrefKey(1170, 2532), 1f);
            Assert.AreEqual(0, QualityGovernor.LoadScale(prefs, 1170, 2532, false), 0.001);
            Assert.AreEqual(0, QualityGovernor.LoadScale(prefs, 1170, 2532, true), 0.001);

            prefs.SetFloat(QualityGovernor.ScalePrefKey(1170, 2532), 2f);
            Assert.AreEqual(2, QualityGovernor.LoadScale(prefs, 1170, 2532, false), 0.001);
            Assert.AreEqual(2, QualityGovernor.LoadScale(prefs, 1170, 2532, true), 0.001);
            Assert.AreEqual(1f, prefs.GetFloat(QualityGovernor.LegacyScalePrefKey(1170, 2532), 0f));
        }

        [Test]
        public void Persist_WaitsUntilTheScaleIsStable()
        {
            var gov = new QualityGovernor { Cap = 2 };
            var early = gov.Tick(Frame(0f, 0.0168f, false, 2));
            Assert.IsFalse(early.Persist);
            Assert.IsFalse(QualityGovernor.ShouldPersist(false, 2, 9.9f, true, 0));

            var before = gov.Tick(Frame(9.9f, 0.0168f, false, 2));
            Assert.IsFalse(before.Persist, "stable for 9.9s");

            var ready = gov.Tick(Frame(10f, 0.0168f, false, 2));
            Assert.IsTrue(ready.Persist);
            Assert.AreEqual(2, ready.PersistScale, 0.001);
            Assert.AreEqual(QualityGovernor.ScalePrefKey(1170, 2532), ready.PersistKey);

            var prefs = new MemoryPrefs();
            Assert.IsFalse(QualityGovernor.SaveScale(prefs, false, 1170, 2532, 2, false));
            Assert.IsFalse(prefs.HasKey(QualityGovernor.ScalePrefKey(1170, 2532)));
            Assert.IsTrue(QualityGovernor.SaveScale(prefs, true, 1170, 2532, ready.PersistScale, false));
            Assert.AreEqual(2f, prefs.GetFloat(ready.PersistKey, 0f));
        }

        [Test]
        public void Persist_RefusesScaleBelowThePhoneFloor()
        {
            Assert.IsFalse(QualityGovernor.ShouldPersist(true, 1.5, 30f, true, 0));
            Assert.IsFalse(QualityGovernor.ShouldPersist(true, 1, 30f, true, 0));
            Assert.IsTrue(QualityGovernor.ShouldPersist(true, 2, 10f, true, 0));
            Assert.IsTrue(QualityGovernor.ShouldPersist(false, 1.5, 10f, true, 0));
            Assert.IsFalse(QualityGovernor.ShouldPersist(false, 2, 10f, true, 1));
            Assert.IsFalse(QualityGovernor.ShouldPersist(false, 2, 10f, false, 0));

            var prefs = new MemoryPrefs();
            prefs.SetFloat(QualityGovernor.ScalePrefKey(390, 844), 2f);
            Assert.IsFalse(QualityGovernor.SaveScale(prefs, true, 390, 844, 1.5, true));
            Assert.AreEqual(2f, prefs.GetFloat(QualityGovernor.ScalePrefKey(390, 844), 0f));
            Assert.AreEqual(2, QualityGovernor.LoadScale(prefs, 390, 844, true), 0.001);

            prefs.SetFloat(QualityGovernor.ScalePrefKey(390, 844), 1f);
            Assert.AreEqual(0, QualityGovernor.LoadScale(prefs, 390, 844, true), 0.001, "a stored 1x on a phone is not applied");

            var gov = new QualityGovernor { Cap = 1 };
            gov.Tick(Frame(0f, 0.0168f, true, 2));
            var saved = gov.Tick(Frame(10f, 0.0168f, true, 2));
            Assert.GreaterOrEqual(saved.Target, QualityGovernor.PhoneFloor - 0.001);
            Assert.IsTrue(saved.Persist);
            Assert.GreaterOrEqual(saved.PersistScale, QualityGovernor.PhoneFloor - 0.001);
        }

        [Test]
        public void HitchWindow_IgnoresBootTransitionAndResizeSamples()
        {
            var gov = new QualityGovernor();
            gov.NoteBoot(0f);
            Assert.IsFalse(gov.AcceptSample(0.2f, 0.25f));
            Assert.AreEqual(0, gov.SampleCount);
            Assert.IsTrue(gov.AcceptSample(1.5f, 0.016f));
            Assert.AreEqual(0.016f, gov.P95(), 0.0001f);

            Assert.IsFalse(gov.WatchPage("home_default", 2f));
            Assert.IsTrue(gov.AcceptSample(2.2f, 0.017f));
            Assert.IsTrue(gov.WatchPage("game_lvl01_t1000ms", 3f));
            Assert.IsFalse(gov.AcceptSample(3.4f, 0.40f));
            Assert.IsTrue(gov.AcceptSample(4.5f, 0.018f));
            Assert.Less(gov.P95(), 0.05f);

            gov.NoteResize(5f);
            Assert.IsFalse(gov.AcceptSample(5.4f, 0.55f));
            Assert.IsTrue(gov.AcceptSample(6.5f, 0.016f));
            Assert.Less(gov.P95(), 0.05f);

            gov.NoteTransition(8f);
            Assert.IsFalse(gov.AcceptSample(8.2f, 0.70f));
            Assert.Less(gov.P95(), 0.05f);
        }

        [Test]
        public void StepDown_RequiresSustainedSlowness()
        {
            var gov = new QualityGovernor { Cap = 3 };
            var first = gov.Tick(Frame(10f, 0.030f, false, 3));
            Assert.AreEqual(3, first.Target, 0.001, "one slow frame only arms the timer");

            var early = gov.Tick(Frame(11.4f, 0.030f, false, 3));
            Assert.AreEqual(3, early.Target, 0.001, "1.4s is short of the sustained window");

            var stepped = gov.Tick(Frame(11.6f, 0.030f, false, 3));
            Assert.AreEqual(2, stepped.Target, 0.001);
            Assert.IsTrue(stepped.SteppedDown);

            gov.NoteResize(11.6f);
            var duringHitch = gov.Tick(Frame(12.4f, 0.200f, false, 3, accepted: false));
            Assert.AreEqual(2, duringHitch.Target, 0.001, "resize hitch does not cascade");

            var recovered = new QualityGovernor { Cap = 3 };
            recovered.Tick(Frame(10f, 0.030f, false, 3));
            recovered.Tick(Frame(11f, 0.010f, false, 3));
            var again = recovered.Tick(Frame(12.4f, 0.030f, false, 3));
            Assert.AreEqual(3, again.Target, 0.001, "a fast frame resets the sustained-slowness timer");
        }

        [Test]
        public void PhoneFloor_UsesStepsTwoAndThree()
        {
            CollectionAssert.AreEqual(new[] { 2d, 3d }, QualityGovernor.Steps(true));
            Assert.AreEqual(2, QualityGovernor.StepDown(3, true), 0.001);
            Assert.AreEqual(2, QualityGovernor.StepDown(2, true), 0.001);
            Assert.AreEqual(3, QualityGovernor.StepUp(2, true), 0.001);
            Assert.AreEqual(2, QualityGovernor.SnapStep(1.1, true), 0.001);
            Assert.AreEqual(2, QualityGovernor.SnapStep(1.8, true), 0.001);
            Assert.AreEqual(3, QualityGovernor.SnapStep(2.7, true), 0.001);
            Assert.AreEqual(2, QualityGovernor.ImpliedScale(3, 0.2f, true), 0.001);
            Assert.AreEqual(2, QualityGovernor.FitScale(430, 932, 430, 932, true), 0.001);
            Assert.AreEqual(3, QualityGovernor.FitScale(1170, 2532, 430, 932, true), 0.001);
            Assert.IsTrue(QualityGovernor.IsPhone(true, DeviceType.Desktop));
            Assert.IsTrue(QualityGovernor.IsPhone(false, DeviceType.Handheld));
            Assert.IsFalse(QualityGovernor.IsPhone(false, DeviceType.Desktop));
            Assert.IsFalse(QualityGovernor.ShouldSimplify(false, true, 0, 1));
            Assert.IsTrue(QualityGovernor.ShouldSimplify(true, true, 0, 3));

            var gov = new QualityGovernor { Cap = 3 };
            gov.Tick(Frame(0f, 0.200f, true, 3));
            var down = gov.Tick(Frame(1.6f, 0.200f, true, 3));
            Assert.AreEqual(2, down.Target, 0.001);
            gov.Tick(Frame(1.7f, 0.200f, true, 2));
            var held = gov.Tick(Frame(3.4f, 0.200f, true, 2));
            Assert.AreEqual(2, held.Target, 0.001);
        }

        [Test]
        public void DesktopSteps_StayOnTheFullLadder()
        {
            CollectionAssert.AreEqual(new[] { 1d, 1.5d, 2d, 3d }, QualityGovernor.Steps(false));
            Assert.AreEqual(1, QualityGovernor.SnapStep(1.1, false), 0.001);
            Assert.AreEqual(1.5, QualityGovernor.SnapStep(1.3, false), 0.001);
            Assert.AreEqual(2, QualityGovernor.SnapStep(1.8, false), 0.001);
            Assert.AreEqual(3, QualityGovernor.SnapStep(2.7, false), 0.001);
            Assert.AreEqual(2, QualityGovernor.StepDown(3, false), 0.001);
            Assert.AreEqual(1.5, QualityGovernor.StepDown(2, false), 0.001);
            Assert.AreEqual(1, QualityGovernor.StepDown(1.5, false), 0.001);
            Assert.AreEqual(1, QualityGovernor.StepDown(1, false), 0.001);
            Assert.AreEqual(1.5, QualityGovernor.StepUp(1, false), 0.001);
            Assert.AreEqual(1, QualityGovernor.FitScale(430, 932, 430, 932, false), 0.001);
            Assert.IsTrue(QualityGovernor.ShouldSimplify(false, false, 0, 1));
            Assert.IsFalse(QualityGovernor.ShouldSimplify(false, false, 1, 1));

            var gov = new QualityGovernor { Cap = 3 };
            gov.Tick(Frame(0f, 0.030f, false, 3));
            Assert.AreEqual(2, gov.Tick(Frame(1.6f, 0.030f, false, 3)).Target, 0.001);
            gov.Tick(Frame(1.7f, 0.030f, false, 2));
            Assert.AreEqual(1.5, gov.Tick(Frame(3.3f, 0.030f, false, 2)).Target, 0.001);
            gov.Tick(Frame(3.4f, 0.030f, false, 1.5));
            Assert.AreEqual(1, gov.Tick(Frame(5.0f, 0.030f, false, 1.5)).Target, 0.001);
        }

        [Test]
        public void StepUp_IsReachableAtTheFrameCapAndNotAt25ms()
        {
            double comfort = QualityGovernor.ComfortCeiling(60);
            Assert.LessOrEqual(0.0168, comfort);
            Assert.Greater(0.025, comfort);
            Assert.Greater(comfort, 0.014);

            var up = new QualityGovernor { Cap = 2 };
            var raised = up.Tick(Frame(30f, 0.0168f, false, 3));
            Assert.AreEqual(3, raised.Target, 0.001);
            Assert.IsTrue(raised.SteppedUp);

            var slow = new QualityGovernor { Cap = 2 };
            var held = slow.Tick(Frame(30f, 0.025f, false, 3));
            Assert.AreEqual(2, held.Target, 0.001);
            Assert.IsFalse(held.SteppedUp);
        }

        [Test]
        public void Hysteresis_HoldsTheLowerScaleAfterAFallback()
        {
            var gov = new QualityGovernor { Cap = 3 };
            gov.Tick(Frame(10f, 0.023f, false, 3));
            var down = gov.Tick(Frame(11.6f, 0.023f, false, 3));
            Assert.AreEqual(2, down.Target, 0.001);

            var back = gov.Tick(Frame(21.6f, 0.0168f, false, 3));
            Assert.AreEqual(3, back.Target, 0.001, "first recovery is allowed after the step-up gap");

            gov.Tick(Frame(22f, 0.023f, false, 3));
            var fell = gov.Tick(Frame(23.6f, 0.023f, false, 3));
            Assert.AreEqual(2, fell.Target, 0.001);

            var blocked = gov.Tick(Frame(40f, 0.0168f, false, 3, raster: 2));
            Assert.AreEqual(2, blocked.Target, 0.001, "fallback holds the lower scale");
            Assert.IsFalse(blocked.SteppedUp);

            var later = gov.Tick(Frame(83.6f, 0.0168f, false, 3, raster: 2));
            Assert.AreEqual(2, later.Target, 0.001, "the 60s hold must not retry the climbed scale");
            Assert.IsFalse(later.SteppedUp);

            var minutes = gov.Tick(Frame(23.6f + 600f, 0.0168f, false, 3, raster: 2));
            Assert.AreEqual(2, minutes.Target, 0.001);
            Assert.IsFalse(minutes.SteppedUp);
        }

        [Test]
        public void Fallback_DoesNotReclimbForTheRestOfTheSession()
        {
            var gov = new QualityGovernor { Cap = 2 };
            var climbed = gov.Tick(Frame(0f, 0.0168f, true, 3, raster: 2));
            Assert.AreEqual(3, climbed.Target, 0.001);
            Assert.IsTrue(climbed.SteppedUp);
            Assert.IsFalse(climbed.Persist, "a climb is not saved on the step frame");

            gov.Tick(Frame(0.1f, 0.0168f, true, 3, raster: 3));
            var beforeWindow = gov.Tick(Frame(9f, 0.0168f, true, 3, raster: 3));
            Assert.AreEqual(3, beforeWindow.Target, 0.001);
            Assert.IsFalse(beforeWindow.Persist);

            gov.Tick(Frame(20f, 0.023f, true, 3, raster: 3));
            var fell = gov.Tick(Frame(21.6f, 0.023f, true, 3, raster: 3));
            Assert.AreEqual(2, fell.Target, 0.001);
            Assert.IsTrue(fell.SteppedDown);
            Assert.IsFalse(fell.Persist);

            gov.Tick(Frame(21.7f, 0.0168f, true, 3, raster: 2));
            var saved = gov.Tick(Frame(31.7f, 0.0168f, true, 3, raster: 2));
            Assert.AreEqual(2, saved.Target, 0.001);
            Assert.IsFalse(saved.SteppedUp);
            Assert.IsTrue(saved.Persist);
            Assert.AreEqual(2, saved.PersistScale, 0.001, "the saved scale stays at the fallen-back ceiling");

            var minutes = gov.Tick(Frame(21.6f + 600f, 0.0168f, true, 3, raster: 2));
            Assert.AreEqual(2, minutes.Target, 0.001);
            Assert.IsFalse(minutes.SteppedUp);
        }

        [Test]
        public void FreshGovernor_MayClimbAgain()
        {
            var gov = new QualityGovernor { Cap = 2 };
            var climbed = gov.Tick(Frame(5f, 0.0168f, true, 3, raster: 2));
            Assert.AreEqual(3, climbed.Target, 0.001);
            Assert.IsTrue(climbed.SteppedUp);
        }

        [Test]
        public void ForcedQuality_BypassesThePhoneFloor()
        {
            var gov = new QualityGovernor { Cap = 3 };
            var forced = gov.Tick(Frame(5f, 0.050f, true, 3, forced: 1, raster: 1));
            Assert.AreEqual(1, forced.Target, 0.001);
            Assert.IsFalse(forced.SimplifyEffects);
            Assert.IsFalse(forced.Persist);
            Assert.AreEqual(3, gov.Cap, 0.001);

            var mid = gov.Tick(Frame(6f, 0.050f, true, 3, forced: 1.5, raster: 1.5));
            Assert.AreEqual(1.5, mid.Target, 0.001);
            Assert.IsFalse(mid.Persist);
        }

        [Test]
        public void FourFingerTap_TogglesFromTouchCounts()
        {
            Assert.IsTrue(QualityGovernor.FourFingerTap(4, 4));
            Assert.IsTrue(QualityGovernor.FourFingerTap(5, 5));
            Assert.IsFalse(QualityGovernor.FourFingerTap(4, 3));
            Assert.IsFalse(QualityGovernor.FourFingerTap(3, 3));
            Assert.IsFalse(QualityGovernor.FourFingerTap(0, 0));

            Assert.IsTrue(QualityGovernor.HudGestureEnabled(true, 0));
            Assert.IsFalse(QualityGovernor.HudGestureEnabled(false, 0));
            Assert.IsTrue(QualityGovernor.HudGestureEnabled(false, 1));
            Assert.AreEqual("os.parity.hud", QualityGovernor.HudPrefKey);

            Assert.IsTrue(QualityGovernor.HudToggled(false, true, false, 0, 0));
            Assert.IsFalse(QualityGovernor.HudToggled(true, true, false, 0, 0));
            Assert.IsTrue(QualityGovernor.HudToggled(false, false, true, 4, 4));
            Assert.IsFalse(QualityGovernor.HudToggled(true, false, true, 4, 4));
            Assert.IsFalse(QualityGovernor.HudToggled(false, false, false, 4, 4));
            Assert.IsFalse(QualityGovernor.HudToggled(false, false, true, 2, 2));
        }

        [Test]
        public void HudLabel_ScalesWithDpiAndNamesTheSavedScale()
        {
            Assert.AreEqual(14, QualityGovernor.HudFontSize(0f));
            Assert.AreEqual(14, QualityGovernor.HudFontSize(96f));
            Assert.AreEqual(14, QualityGovernor.HudFontSize(160f));
            Assert.AreEqual(28, QualityGovernor.HudFontSize(320f));
            Assert.AreEqual(48, QualityGovernor.HudFontSize(800f));

            string missing = QualityGovernor.HudLabel(60f, 16.8f, 2, 2, 3, 1170, 2532, false, 0);
            StringAssert.Contains("60 fps", missing);
            StringAssert.Contains("p95 16.8 ms", missing);
            StringAssert.Contains("scale 2.0", missing);
            StringAssert.Contains("cap 2.0", missing);
            StringAssert.Contains("fit 3.0", missing);
            StringAssert.Contains("1170x2532", missing);
            StringAssert.Contains("saved=-", missing);

            string saved = QualityGovernor.HudLabel(58f, 18.2f, 3, 3, 3, 1290, 2796, true, 3);
            StringAssert.Contains("saved=3.0", saved);
            StringAssert.Contains("1290x2796", saved);
        }

        static QualityGovernor.Observation Frame(
            float now, float p95, bool phone, double fit,
            int count = 40, bool accepted = true, double forced = 0,
            int fps = 60, bool hasRaster = true, double raster = -1)
        {
            return new QualityGovernor.Observation
            {
                Now = now,
                P95 = p95,
                SampleCount = count,
                SampleAccepted = accepted,
                Phone = phone,
                Forced = forced,
                Fit = fit,
                TargetFrameRate = fps,
                Width = 1170,
                Height = 2532,
                HasRaster = hasRaster,
                RasterScale = raster < 0 ? fit : raster,
                ProbeSkipBlur = false
            };
        }

        sealed class MemoryPrefs : QualityGovernor.IScalePrefs
        {
            readonly Dictionary<string, float> values = new Dictionary<string, float>();

            public bool HasKey(string key) => values.ContainsKey(key);

            public float GetFloat(string key, float fallback)
            {
                return values.TryGetValue(key, out float value) ? value : fallback;
            }

            public void SetFloat(string key, float value) => values[key] = value;
        }
    }
}
