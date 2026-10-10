using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;

namespace OrbitalSalvage.Viewer.Tests
{
    public class DeviceBenchTests
    {
        [Test]
        public void Parse_DefaultsToNoneAndIgnoresUnknownTokens()
        {
            Assert.AreEqual("none", new DeviceExp().Text());
            Assert.AreEqual("none", DeviceExp.Parse(null).Text());
            Assert.AreEqual("none", DeviceExp.Parse("").Text());
            Assert.AreEqual("none", DeviceExp.Parse("none").Text());
            Assert.AreEqual("none", DeviceExp.Parse("foo, ,bar").Text());

            var exp = DeviceExp.Parse(" scale2, noshadow , nobackdrop,scale1 , f15, f30 , rt16 ");
            Assert.IsTrue(exp.NoShadow);
            Assert.IsTrue(exp.NoBackdrop);
            Assert.IsFalse(exp.NoLoops);
            Assert.AreEqual(1d, exp.Scale);
            Assert.AreEqual(30, exp.FrameRate);
            Assert.IsTrue(exp.Rt16);
            Assert.AreEqual("noshadow,nobackdrop,scale1,f30,rt16", exp.Text());

            var game = DeviceExp.Parse("nogamehud,nogamefx,noimgui,nocache,noloops,scale1.5");
            Assert.IsTrue(game.NoGameHud && game.NoGameFx && game.NoImgui && game.NoCache && game.NoLoops);
            Assert.AreEqual(1.5d, game.Scale);
            Assert.AreEqual("noloops,nocache,nogamehud,nogamefx,noimgui,scale1.5", game.Text());
        }

        [Test]
        public void Mode_AcceptsAutoAndScenarioNamesOnly()
        {
            Assert.IsFalse(DeviceBench.TryMode(null, out _));
            Assert.IsFalse(DeviceBench.TryMode("", out _));
            Assert.IsFalse(DeviceBench.TryMode("nope", out _));
            Assert.IsTrue(DeviceBench.TryMode(" auto ", out string auto));
            Assert.AreEqual("auto", auto);
            Assert.IsTrue(DeviceBench.TryMode("Daily", out string daily));
            Assert.AreEqual("daily", daily);
            Assert.IsFalse(DeviceBench.IsScenario("tabsx"));
            Assert.IsTrue(DeviceBench.IsScenario("achievements"));
        }

        [Test]
        public void Matrix_MatchesTheMeasuredSet()
        {
            Assert.AreEqual(8, DeviceBench.Configs("home").Length);
            Assert.AreEqual(8, DeviceBench.Configs("tabs").Length);
            Assert.AreEqual(11, DeviceBench.Configs("game").Length);
            Assert.AreEqual(3, DeviceBench.Configs("maps").Length);
            Assert.AreEqual(3, DeviceBench.Configs("pause").Length);
            Assert.AreEqual(3, DeviceBench.Configs("daily").Length);
            Assert.AreEqual("nogamefx", DeviceBench.Configs("game")[10]);
            Assert.AreEqual("scale1", DeviceBench.Configs("maps")[2]);

            var auto = DeviceBench.Build("auto", 0.2f, 0f, 0.2f);
            Assert.AreEqual(33, Count(auto, DeviceBench.Op.Apply));
            Assert.AreEqual("home", First(auto, DeviceBench.Op.Apply).B);
            Assert.AreEqual("tabs", LastApplyScenario(auto));
            Assert.AreEqual("btn-home-play", ClickGoingTo(auto, "game"));
            Assert.AreEqual("btn-game-pause", ClickGoingTo(auto, "pause"));
            Assert.IsFalse(HasClick(DeviceBench.Build("home", 0.2f, 0f, 0.2f), "btn-home-play"));

            var tabs = DeviceBench.Build("tabs", 0.2f, 0f, 0.2f);
            var ids = MeasuredClicks(tabs);
            CollectionAssert.AreEqual(new[]
            {
                "btn-open-maps-page", "btn-back-maps",
                "btn-open-daily-shift", "btn-back-daily",
                "btn-open-shop-page", "btn-back-shop",
                "btn-open-trophies-page", "btn-back-achievements"
            }, FirstCycle(ids));
        }

        [Test]
        public void Window_PercentilesThresholdsAndGpu()
        {
            var window = new BenchWindow();
            window.Push(0.010f, -1f, 1, 2, 0, 3, 0);
            window.Push(0.020f, -1f, 1, 0, 1, 0, 0);
            window.Push(0.050f, 4f, 0, 0, 0, 0, 1);
            window.Push(0.080f, 8f, 0, 0, 0, 1, 0);
            Assert.AreEqual(4, window.Frames);
            string line = window.Format("home", "none");
            Assert.IsTrue(line.StartsWith("[Bench] scenario=home exp=none ", System.StringComparison.Ordinal));
            Assert.IsTrue(line.Contains(" over50=1 "));
            Assert.IsTrue(line.Contains(" gpu_ms=6.0/8.0 "));
            Assert.IsTrue(line.Contains(" submits=2 "));
            Assert.IsTrue(line.Contains(" backdrop=2/1 "));
            Assert.IsTrue(line.Contains(" shadows=4 "));
            Assert.IsTrue(line.Contains(" cachebuilds=1"));
            Assert.IsTrue(line.Contains(" max=80.0 "));
            Assert.AreEqual("[Bench] done", BenchWindow.DoneLine);

            var empty = new BenchWindow();
            Assert.IsTrue(empty.Format("maps", "nobackdrop").Contains(" gpu_ms=na "));
            string sw = BenchWindow.FormatSwitch("tabs", "noshadow", "home", "maps", 580f, 5, 734f);
            Assert.AreEqual("[Bench] switch scenario=tabs exp=noshadow from=home to=maps ms=580.0 over100=5 worst=734.0", sw);
        }

        [Test]
        public void Player_HomeCellsLogAWindowThenDone()
        {
            var logs = Run("home", 0.05f, 0.05f, 0.05f, true, 0.05f);
            int windows = 0;
            for (int i = 0; i < logs.Count; i++)
                if (logs[i].StartsWith("[Bench] scenario=home ", System.StringComparison.Ordinal)) windows++;
            Assert.AreEqual(8, windows);
            Assert.AreEqual("[Bench] done", logs[logs.Count - 1]);
            Assert.IsFalse(logs.Exists(s => s.StartsWith("[Bench] switch", System.StringComparison.Ordinal)));
        }

        [Test]
        public void Player_TabsCountFramesOver100DuringTheDwell()
        {
            var logs = Run("tabs", 0.2f, 0f, 0.25f, true, 0.15f);
            string sw = null;
            for (int i = 0; i < logs.Count; i++)
            {
                if (logs[i].Contains(" from=home to=maps ")) { sw = logs[i]; break; }
            }
            Assert.IsNotNull(sw, string.Join("\n", logs));
            Assert.IsTrue(sw.StartsWith("[Bench] switch scenario=tabs exp=none ", System.StringComparison.Ordinal));
            int over = Over100(sw);
            Assert.Greater(over, 0, sw);
            Assert.IsTrue(logs[logs.Count - 1] == "[Bench] done" || logs.Exists(s => s.StartsWith("[Bench] scenario=tabs ", System.StringComparison.Ordinal)));
        }

        [Test]
        public void Player_NavTimeoutSkipsTheCell()
        {
            var logs = Run("maps", 0.2f, 0f, 0.2f, false, 1f);
            Assert.IsTrue(logs.Exists(s => s.StartsWith("[Bench] nav timeout scenario=maps exp=none click=btn-open-maps-page page=home", System.StringComparison.Ordinal)), string.Join("\n", logs));
            Assert.AreEqual("[Bench] done", logs[logs.Count - 1]);
        }

        [Test]
        public void GovernorReset_DropsSamplesAndRestoresTheBootCap()
        {
            var governor = new QualityGovernor();
            governor.BootNoted = true;
            Assert.IsTrue(governor.AcceptSample(1f, 0.03f));
            governor.Cap = 1.5;
            governor.ResetSession();
            Assert.AreEqual(0, governor.SampleCount);
            Assert.AreEqual(3d, governor.Cap);
            Assert.IsFalse(governor.BootNoted);
        }

        static List<string> Run(string mode, float window, float warmup, float dwell, bool followClicks, float dt)
        {
            var player = new BenchPlayer(DeviceBench.Build(mode, window, warmup, dwell), 8f);
            var logs = new List<string>();
            string page = "home";
            string issued = null;
            float now = 0f;
            for (int n = 0; n < 20000 && !player.Done; n++)
            {
                if (issued != null)
                {
                    if (followClicks) page = DeviceBench.PageAfterClick(issued);
                    issued = null;
                }
                var act = player.Tick(new DeviceBench.TickIn
                {
                    Now = now,
                    Page = page,
                    Dt = n == 0 ? 0f : dt,
                    PresentMs = -1f,
                    GpuMs = -1f
                });
                for (int i = 0; i < player.LogCount; i++) logs.Add(player.Log(i));
                if (act.Click) issued = act.ClickId;
                now += dt;
            }
            Assert.IsTrue(player.Done, "stopped early\n" + string.Join("\n", logs));
            return logs;
        }

        static int Over100(string line)
        {
            const string key = "over100=";
            int at = line.IndexOf(key, System.StringComparison.Ordinal);
            Assert.GreaterOrEqual(at, 0);
            int end = line.IndexOf(' ', at + key.Length);
            string num = end < 0 ? line.Substring(at + key.Length) : line.Substring(at + key.Length, end - (at + key.Length));
            return int.Parse(num, CultureInfo.InvariantCulture);
        }

        static int Count(DeviceBench.Step[] steps, DeviceBench.Op op)
        {
            int n = 0;
            for (int i = 0; i < steps.Length; i++)
                if (steps[i].Op == op) n++;
            return n;
        }

        static DeviceBench.Step First(DeviceBench.Step[] steps, DeviceBench.Op op)
        {
            for (int i = 0; i < steps.Length; i++)
                if (steps[i].Op == op) return steps[i];
            Assert.Fail("missing " + op);
            return default;
        }

        static string LastApplyScenario(DeviceBench.Step[] steps)
        {
            string scenario = null;
            for (int i = 0; i < steps.Length; i++)
                if (steps[i].Op == DeviceBench.Op.Apply) scenario = steps[i].B;
            return scenario;
        }

        static string ClickGoingTo(DeviceBench.Step[] steps, string page)
        {
            for (int i = 0; i < steps.Length; i++)
            {
                if (steps[i].Op != DeviceBench.Op.Click || steps[i].Switch) continue;
                if (steps[i].B == page) return steps[i].A;
            }
            return null;
        }

        static bool HasClick(DeviceBench.Step[] steps, string id)
        {
            for (int i = 0; i < steps.Length; i++)
                if (steps[i].Op == DeviceBench.Op.Click && steps[i].A == id) return true;
            return false;
        }

        static List<string> MeasuredClicks(DeviceBench.Step[] steps)
        {
            var ids = new List<string>();
            for (int i = 0; i < steps.Length; i++)
                if (steps[i].Op == DeviceBench.Op.Click && steps[i].Switch) ids.Add(steps[i].A);
            return ids;
        }

        static List<string> FirstCycle(List<string> ids)
        {
            Assert.GreaterOrEqual(ids.Count, 8);
            return ids.GetRange(0, 8);
        }
    }
}
