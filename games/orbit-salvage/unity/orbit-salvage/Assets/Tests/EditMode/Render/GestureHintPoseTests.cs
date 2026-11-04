using NUnit.Framework;

namespace OrbitalSalvage.Render.Tests
{
    /// <summary>
    /// Boxes measured in Chromium (frozen clock, play click, then 400ms and 3600ms).
    /// The spec stores the dismissed card; the pose rebuilds the live one.
    /// </summary>
    public class GestureHintPoseTests
    {
        static readonly GestureHintPose.Box Card = new GestureHintPose.Box(131.279, 412.65, 167.442, 87.3);
        static readonly GestureHintPose.Box Hand = new GestureHintPose.Box(200.593, 426.15, 28.8, 38.7);
        static readonly GestureHintPose.Box Text = new GestureHintPose.Box(151.979, 472.05, 126.042, 14.4);
        static readonly GestureHintPose.Box Emoji = new GestureHintPose.Box(195.872, 429.189, 29.923, 29.923);

        static GestureHintPose.Result At(double age)
        {
            return GestureHintPose.At(age, 430, 932, Card, 0.9, -93.0234, -58.2, Hand, 0.999208, -0.0397984, -4.56176, Text, Emoji);
        }

        static void Near(GestureHintPose.Box box, double x, double y, double w, double h)
        {
            Assert.AreEqual(x, box.X, 0.05, "x");
            Assert.AreEqual(y, box.Y, 0.05, "y");
            Assert.AreEqual(w, box.W, 0.05, "w");
            Assert.AreEqual(h, box.H, 0.05, "h");
        }

        [Test]
        public void VisibleAt400ms_MatchesChromium()
        {
            var pose = At(400);
            Assert.IsTrue(pose.Live);
            Assert.AreEqual(1, pose.Opacity, 1e-9);
            Assert.AreEqual(1, pose.Scale, 1e-9);
            Near(pose.Card, 121.9765625, 417.5, 186.046875, 97);
            Near(pose.Text, 144.9765625, 483.5, 140.046875, 16);
            Near(pose.Hand, 193.58743286132812, 431.8802490234375, 33.68597412109375, 44.239501953125);
            Near(pose.Emoji, 193.74664306640625, 435.8770751953125, 33.24818420410156, 33.248199462890625);
            Assert.AreEqual(-4.56176, pose.HandTx, 0.02);
        }

        [Test]
        public void FadeAt3600ms_MatchesChromium()
        {
            var pose = At(3600);
            Assert.IsTrue(pose.Live);
            Assert.AreEqual(0.591489, pose.Opacity, 0.002);
            Assert.AreEqual(0.959149, pose.Scale, 0.002);
            Near(pose.Card, 125.77666473388672, 415.51873779296875, 178.4466552734375, 93.03744506835938);
        }

        [Test]
        public void AfterFade_LeavesTheBakedPose()
        {
            Assert.IsFalse(At(3900).Live);
            Assert.IsFalse(At(4000).Live);
        }
    }
}
