using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using NUnit.Framework;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Logic;
using UnityEngine;

namespace OrbitalSalvage.Render.Tests
{
    /// <summary>
    /// Drives the live <see cref="LiveGameSession.Update"/> path (FixedStep false) under several
    /// frame-time profiles. Inputs are the physics-golden programs, placed on a 16 ms timeline
    /// and applied on the first frame whose simulated time has reached that timestamp.
    /// Marked Explicit so a full EditMode run does not repeat the sweep.
    /// </summary>
    [Explicit]
    public class FrameRateHarness
    {
        const double Duration = 30.0;
        const double InputFrame = 16.0 / 1000.0;
        const uint RandomSeed = 20261007u;

        static readonly int[] CampaignSectors = { 1, 2, 3, 4, 5, 6, 11, 21, 31, 41 };
        static readonly string[] Streams = { "idle", "drag", "bursts" };

        struct Ev
        {
            public double T;
            public bool Drag;
            public double V;
        }

        sealed class RunRow
        {
            public string Sector;
            public string Stream;
            public string Profile;
            public double Collected;
            public double Spilled;
            public int Swallowed;
            public int SpilledCount;
            public int Exploded;
            public int InPlay;
            public int StillIngesting;
            public int StillFalling;
            public int StillInRing;
            public double VictoryTime;
            public double DefeatTime;
            public double SimTime;
            public double RequestedTime;
            public int Frames;
            public int Steps;
            public string Hash;
            public string Debris;
        }

        [Test]
        [Timeout(1800000)]
        public void Measure()
        {
            double savedScale = GameRenderer.DefaultRenderScale;
            GameRenderer.DefaultRenderScale = 0.001;
            try
            {
                string root = MazeRoot();
                string reportDir = Path.Combine(root, "tools", "parity", "reports");
                Directory.CreateDirectory(reportDir);
                string frameDir = Path.Combine(reportDir, "physics-framerate-2026-10-07-frames");
                var profiles = BuildProfiles(frameDir);
                var sectors = BuildSectors();

                var boot = new StringBuilder();
                boot.Append("boot");
                foreach (int n in new[] { 1, 11, 41 })
                {
                    var viaDriver = RecordedFrameDriver.Create(n);
                    var viaHarness = CreateFor(SectorByNumber(sectors, n));
                    try
                    {
                        string a = viaDriver.StateHash();
                        string b = viaHarness.StateHash();
                        boot.Append('\n').Append(n).Append(' ').Append(a).Append(' ').Append(b).Append(' ').Append(a == b ? "match" : "DIFFER");
                        Assert.AreEqual(a, b, "boot hash sector " + n);
                    }
                    finally
                    {
                        viaDriver.Renderer.Dispose();
                        viaHarness.Renderer.Dispose();
                    }
                }

                var layout = MeasureLayout();
                var rows = new List<RunRow>();
                string runsPath = Path.Combine(reportDir, "physics-framerate-2026-10-07-runs.csv");
                string debrisPath = Path.Combine(reportDir, "physics-framerate-2026-10-07-debris.csv");
                string progressPath = Path.Combine(reportDir, "physics-framerate-2026-10-07.progress.txt");
                File.WriteAllText(runsPath, "sector,stream,profile,collected,spilled,swallowed,spilledCount,exploded,inPlay,stillIngesting,stillFalling,stillInRing,victoryTime,defeatTime,simTime,requestedTime,frames,steps,hash\n");
                File.WriteAllText(debrisPath, "sector,stream,profile,id,outcome,weight\n");
                File.WriteAllText(progressPath, "start " + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture) + "\n" + boot + "\n");

                int done = 0;
                int total = sectors.Count * Streams.Length * profiles.Count;
                foreach (var sector in sectors)
                {
                    foreach (string stream in Streams)
                    {
                        var events = Program(stream);
                        foreach (var profile in profiles)
                        {
                            profile.Reset();
                            RunRow row = RunOne(sector, stream, profile, events);
                            rows.Add(row);
                            AppendRun(runsPath, row);
                            File.AppendAllText(debrisPath, row.Debris);
                            done++;
                            File.AppendAllText(progressPath, done + "/" + total + " " + row.Sector + " " + row.Stream + " " + row.Profile
                                + " spilled " + row.Spilled.ToString("R", CultureInfo.InvariantCulture)
                                + " swallowed " + row.Swallowed + "\n");
                        }
                    }
                }

                RunRow again = RunOne(sectors[0], "bursts", profiles[0], Program("bursts"));
                RunRow first = rows.Find(r => r.Sector == again.Sector && r.Stream == "bursts" && r.Profile == profiles[0].Name);
                Assert.AreEqual(first.Hash, again.Hash, "steady-16ms sector 1 bursts repeated");
                Assert.AreEqual(first.Spilled, again.Spilled);
                Assert.AreEqual(first.Swallowed, again.Swallowed);

                string md = RenderReport(layout, rows, profiles, boot.ToString());
                File.WriteAllText(Path.Combine(reportDir, "physics-framerate-2026-10-07.md"), md);
                Assert.Greater(rows.Count, 0);
            }
            finally
            {
                GameRenderer.DefaultRenderScale = savedScale;
            }
        }

        sealed class SectorCase
        {
            public string Id;
            public SectorConfig Config;
        }

        static List<SectorCase> BuildSectors()
        {
            var list = new List<SectorCase>();
            var all = MazeLevelBuilder.GetSectors();
            foreach (int n in CampaignSectors)
            {
                SectorConfig cfg = all[n - 1];
                list.Add(new SectorCase { Id = "L" + n.ToString("00", CultureInfo.InvariantCulture), Config = cfg });
            }
            var clock = new FixedWallClock(new DateTimeOffset(2026, 10, 1, 16, 0, 1, TimeSpan.Zero).ToUnixTimeMilliseconds());
            var economy = new EconomyManager(new MemoryKeyValueStore(), clock);
            DailyShiftInfo bronze = null;
            foreach (DailyShiftInfo shift in economy.GetDailyShifts())
            {
                if (shift.Tier == "bronze") bronze = shift;
            }
            if (bronze == null) throw new InvalidOperationException("bronze daily shift missing for " + economy.Today());
            SectorConfig daily = MazeLevelBuilder.BuildDailyShift(bronze);
            list.Add(new SectorCase
            {
                Id = "daily-bronze-" + economy.Today() + "-seed" + bronze.Seed.GetValueOrDefault().ToString("R", CultureInfo.InvariantCulture),
                Config = daily
            });
            return list;
        }

        static SectorConfig SectorByNumber(List<SectorCase> sectors, int number)
        {
            string id = "L" + number.ToString("00", CultureInfo.InvariantCulture);
            for (int i = 0; i < sectors.Count; i++)
                if (sectors[i].Id == id) return sectors[i].Config;
            throw new InvalidOperationException(id);
        }

        static LiveGameSession CreateFor(SectorConfig sector)
        {
            JsRandom.Global.Reset(JsRandom.DefaultSeed);
            var renderer = new GameRenderer(430, 932, 3);
            var audio = new ParityAudioCues(() => 0);
            var physics = new PhysicsEngine(audio);
            var economy = new EconomyManager(new MemoryKeyValueStore(), new FixedWallClock(0));
            var session = new LiveGameSession(physics, renderer, economy);
            session.FixedStep = false;
            session.RenderEnabled = false;
            var sectors = MazeLevelBuilder.GetSectors();
            session.ApplyLayout(430, 932);
            session.LoadSector(sectors[0]);
            RecordedFrameDriver.ConsumeBootRandom();
            session.LoadSector(sector);
            return session;
        }

        sealed class LayoutSnap
        {
            public string When;
            public double CenterX, CenterY, HoleX, HoleY, Horizon, GravityRadius, GravityStrength;
        }

        static List<LayoutSnap> MeasureLayout()
        {
            var snaps = new List<LayoutSnap>();
            JsRandom.Global.Reset(JsRandom.DefaultSeed);
            var renderer = new GameRenderer(430, 932, 3);
            var session = new LiveGameSession(new PhysicsEngine(new ParityAudioCues(() => 0)), renderer,
                new EconomyManager(new MemoryKeyValueStore(), new FixedWallClock(0)));
            try
            {
                session.ApplyLayout(430, 932);
                snaps.Add(Snap("after ApplyLayout(430, 932), before LoadSector", session));
                session.LoadSector(MazeLevelBuilder.GetSectors()[0]);
                snaps.Add(Snap("after LoadSector campaign 1", session));
            }
            finally
            {
                renderer.Dispose();
            }
            return snaps;
        }

        static LayoutSnap Snap(string when, LiveGameSession session)
        {
            var h = session.Physics.BlackHole;
            return new LayoutSnap
            {
                When = when,
                CenterX = session.Physics.Center.X,
                CenterY = session.Physics.Center.Y,
                HoleX = h.X,
                HoleY = h.Y,
                Horizon = h.EventHorizonRadius,
                GravityRadius = h.GravityRadius,
                GravityStrength = h.GravityStrength
            };
        }

        abstract class Profile
        {
            public string Name;
            public abstract void Reset();
            public abstract double NextDt();
        }

        sealed class SteadyProfile : Profile
        {
            public double Dt;
            public override void Reset() { }
            public override double NextDt() { return Dt; }
        }

        sealed class RandomProfile : Profile
        {
            uint _state;
            public override void Reset() { _state = RandomSeed; }
            public override double NextDt()
            {
                _state = _state * 1664525u + 1013904223u;
                double u = (_state & 0xFFFFFFu) / 16777216.0;
                return (14.0 + u * 16.0) / 1000.0;
            }
        }

        sealed class SpikeProfile : Profile
        {
            int _frame;
            public override void Reset() { _frame = 0; }
            public override double NextDt()
            {
                _frame++;
                return (_frame % 25 == 0) ? 0.100 : 0.016;
            }
        }

        sealed class TraceProfile : Profile
        {
            public double[] Samples;
            int _i;
            public override void Reset() { _i = 0; }
            public override double NextDt()
            {
                double ms = Samples[_i % Samples.Length];
                _i++;
                return ms / 1000.0;
            }
        }

        static List<Profile> BuildProfiles(string frameDir)
        {
            var list = new List<Profile>
            {
                new SteadyProfile { Name = "steady-16ms", Dt = InputFrame },
                new SteadyProfile { Name = "steady-1-60", Dt = 1.0 / 60.0 },
                new SteadyProfile { Name = "steady-33.3ms", Dt = 33.3 / 1000.0 },
                new RandomProfile { Name = "random-14-30" },
                new SpikeProfile { Name = "spike-100-every-25" },
                LoadTrace(frameDir, "editor-s1-baseline-1", "home-s1-baseline-1"),
                LoadTrace(frameDir, "editor-s3-baseline-1", "home-s3-baseline-1"),
                LoadTrace(frameDir, "editor-auto-timeline-1", "home-auto-timeline-1")
            };
            return list;
        }

        static TraceProfile LoadTrace(string frameDir, string name, string file)
        {
            string path = Path.Combine(frameDir, file + ".txt");
            var samples = new List<double>();
            foreach (string raw in File.ReadAllLines(path))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line[0] == '#') continue;
                samples.Add(double.Parse(line, CultureInfo.InvariantCulture));
            }
            if (samples.Count == 0) throw new InvalidOperationException("empty trace " + path);
            return new TraceProfile { Name = name, Samples = samples.ToArray() };
        }

        static List<Ev> Program(string name)
        {
            var list = new List<Ev>();
            int maxF = (int)Math.Floor(Duration / InputFrame) + 1;
            for (int f = 1; f <= maxF; f++)
            {
                double t = (f - 1) * InputFrame;
                if (t > Duration) break;
                if (name == "idle") continue;
                if (name == "drag")
                {
                    list.Add(new Ev { T = t, Drag = true, V = 40 });
                    continue;
                }
                if (name != "bursts") throw new ArgumentException(name);
                if (f >= 10 && f <= 40) list.Add(new Ev { T = t, Drag = true, V = 30 });
                if (f >= 100 && f <= 130) list.Add(new Ev { T = t, Drag = true, V = -30 });
                if (f >= 250 && f <= 300) list.Add(new Ev { T = t, Drag = true, V = 20 });
                if (f == 60 || f == 330 || f == 481) list.Add(new Ev { T = t, Drag = false, V = 1 });
                if (f == 200 || f == 480) list.Add(new Ev { T = t, Drag = false, V = -1 });
            }
            return list;
        }

        [Test]
        [Timeout(1800000)]
        public void MeasureAccumulated()
        {
            double savedScale = GameRenderer.DefaultRenderScale;
            GameRenderer.DefaultRenderScale = 0.001;
            try
            {
                string root = MazeRoot();
                string reportDir = Path.Combine(root, "tools", "parity", "reports");
                var profiles = BuildProfiles(Path.Combine(reportDir, "physics-framerate-2026-10-07-frames"));
                var sectors = BuildSectors();
                var rows = new List<RunRow>();
                string runsPath = Path.Combine(reportDir, "physics-framerate-2026-10-07-accum.csv");
                File.WriteAllText(runsPath, "sector,stream,profile,collected,spilled,swallowed,steps,simTime,hash\n");
                foreach (var sector in sectors)
                {
                    foreach (string stream in Streams)
                    {
                        var events = Program(stream);
                        foreach (var profile in profiles)
                        {
                            profile.Reset();
                            RunRow row = RunOne(sector, stream, profile, events, true);
                            rows.Add(row);
                            var inv = CultureInfo.InvariantCulture;
                            File.AppendAllText(runsPath, string.Join(",", row.Sector, row.Stream, row.Profile,
                                row.Collected.ToString("R", inv), row.Spilled.ToString("R", inv),
                                row.Swallowed.ToString(inv), row.Steps.ToString(inv),
                                row.SimTime.ToString("R", inv), row.Hash) + "\n");
                        }
                    }
                }
                string reference = "steady-1-60";
                int mismatch = 0;
                var detail = new StringBuilder();
                for (int i = 0; i < rows.Count; i++)
                {
                    if (rows[i].Profile == reference) continue;
                    RunRow src = Find(rows, rows[i].Sector, rows[i].Stream, reference);
                    if (rows[i].Hash != src.Hash || rows[i].Spilled != src.Spilled || rows[i].Swallowed != src.Swallowed
                        || rows[i].Collected != src.Collected || rows[i].Steps != src.Steps)
                    {
                        mismatch++;
                        if (detail.Length < 4000)
                        {
                            detail.Append(rows[i].Sector).Append(' ').Append(rows[i].Stream).Append(' ').Append(rows[i].Profile)
                                .Append(" hash ").Append(rows[i].Hash).Append(" vs ").Append(src.Hash)
                                .Append(" spill ").Append(rows[i].Spilled.ToString("R", CultureInfo.InvariantCulture))
                                .Append(" vs ").Append(src.Spilled.ToString("R", CultureInfo.InvariantCulture))
                                .Append(" sw ").Append(rows[i].Swallowed).Append(" vs ").Append(src.Swallowed)
                                .Append(" steps ").Append(rows[i].Steps).Append(" vs ").Append(src.Steps)
                                .Append('\n');
                        }
                    }
                }
                File.WriteAllText(Path.Combine(reportDir, "physics-framerate-2026-10-07-accum.txt"),
                    "rows " + rows.Count + "\nmismatches " + mismatch + "\n" + detail);
                Assert.AreEqual(0, mismatch, detail.ToString());
            }
            finally
            {
                GameRenderer.DefaultRenderScale = savedScale;
            }
        }

        static RunRow RunOne(SectorCase sector, string stream, Profile profile, List<Ev> events, bool accumulate = false)
        {
            var session = CreateFor(sector.Config);
            if (accumulate) session.Accumulate = true;
            double nowMs = 0;
            session.NowMs = () => nowMs;
            var outcome = new Dictionary<int, string>();
            var weight = new Dictionary<int, double>();
            int swallowed = 0;
            int spilledCount = 0;
            int exploded = 0;
            Action<Debris> prevSpill = session.Physics.OnDebrisSpilled;
            Action<Debris> prevExplode = session.Physics.OnDebrisExploded;
            session.OnSwallow += d =>
            {
                swallowed++;
                outcome[d.Id] = "swallowed";
                weight[d.Id] = d.Weight;
            };
            session.Physics.OnDebrisSpilled = d =>
            {
                spilledCount++;
                if (!outcome.ContainsKey(d.Id)) outcome[d.Id] = "spilled";
                weight[d.Id] = d.Weight;
                if (prevSpill != null) prevSpill(d);
            };
            session.Physics.OnDebrisExploded = d =>
            {
                exploded++;
                if (!outcome.ContainsKey(d.Id)) outcome[d.Id] = "exploded";
                weight[d.Id] = d.Weight;
                if (prevExplode != null) prevExplode(d);
            };

            double victory = -1;
            double defeat = -1;
            double sim = 0;
            double requestedSum = 0;
            int frames = 0;
            int cursor = 0;
            double pointerX = 0;
            bool down = false;
            void ApplyDue(double time)
            {
                while (cursor < events.Count && events[cursor].T <= time + 1e-9)
                {
                    Ev ev = events[cursor++];
                    if (ev.Drag)
                    {
                        if (!down)
                        {
                            session.PointerDown(pointerX, 0, 430);
                            down = true;
                        }
                        pointerX += ev.V;
                        session.PointerMove(pointerX);
                    }
                    else
                    {
                        session.KeyDown(ev.V > 0 ? "d" : "a");
                    }
                }
            }
            const int AccumTarget = 1800;
            try
            {
                session.BeforeAccumStep = time =>
                {
                    nowMs = time * 1000.0;
                    ApplyDue(time);
                };
                int stall = 0;
                while (accumulate ? session.PhysicsSteps < AccumTarget : sim < Duration - 1e-12)
                {
                    if (!accumulate)
                    {
                        nowMs = sim * 1000.0;
                        ApplyDue(sim);
                    }
                    double requested = profile.NextDt();
                    if (requested <= 0) throw new InvalidOperationException("non-positive dt from " + profile.Name);
                    double passed = requested;
                    if (!accumulate)
                    {
                        if (passed > LiveGameSession.DtCap) passed = LiveGameSession.DtCap;
                        double remain = Duration - sim;
                        if (passed > remain) passed = remain;
                    }
                    else
                    {
                        int room = AccumTarget - session.PhysicsSteps;
                        int cap = room < LiveGameSession.LiveMaxSteps ? room : LiveGameSession.LiveMaxSteps;
                        double budget = cap * LiveGameSession.LiveStep - session.PendingAccum;
                        if (budget < 0) budget = 0;
                        if (passed > budget) passed = budget;
                    }
                    int stepsBefore = session.PhysicsSteps;
                    requestedSum += requested;
                    session.Update(passed);
                    frames++;
                    if (accumulate)
                    {
                        sim = session.SimulatedTime;
                        if (session.PhysicsSteps == stepsBefore)
                        {
                            if (++stall > 10000) throw new InvalidOperationException("accumulator stalled on " + profile.Name);
                        }
                        else stall = 0;
                    }
                    else
                    {
                        sim += passed;
                    }
                    if (victory < 0 && session.IsLevelCleared) victory = sim;
                    if (defeat < 0 && session.IsLevelFailed) defeat = sim;
                }

                int inPlay = 0, ingesting = 0, falling = 0, inRing = 0;
                var list = session.Physics.DebrisList;
                for (int i = 0; i < list.Count; i++)
                {
                    Debris d = list[i];
                    inPlay++;
                    if (d.State == DebrisState.Ingesting) ingesting++;
                    else if (d.State == DebrisState.Falling) falling++;
                    else if (d.State == DebrisState.InRing) inRing++;
                    if (!outcome.ContainsKey(d.Id))
                    {
                        outcome[d.Id] = d.State == DebrisState.Ingesting ? "ingesting"
                            : d.State == DebrisState.Falling ? "falling"
                            : d.State == DebrisState.InRing ? "in_ring"
                            : d.State.ToString();
                        weight[d.Id] = d.Weight;
                    }
                }

                var debris = new StringBuilder();
                var ids = new List<int>(outcome.Keys);
                ids.Sort();
                var inv = CultureInfo.InvariantCulture;
                for (int i = 0; i < ids.Count; i++)
                {
                    int id = ids[i];
                    debris.Append(sector.Id).Append(',').Append(stream).Append(',').Append(profile.Name).Append(',')
                        .Append(id.ToString(inv)).Append(',').Append(outcome[id]).Append(',')
                        .Append(weight[id].ToString("R", inv)).Append('\n');
                }

                return new RunRow
                {
                    Sector = sector.Id,
                    Stream = stream,
                    Profile = profile.Name,
                    Collected = session.CollectedMass,
                    Spilled = session.SpilledMass,
                    Swallowed = swallowed,
                    SpilledCount = spilledCount,
                    Exploded = exploded,
                    InPlay = inPlay,
                    StillIngesting = ingesting,
                    StillFalling = falling,
                    StillInRing = inRing,
                    VictoryTime = victory,
                    DefeatTime = defeat,
                    SimTime = sim,
                    RequestedTime = requestedSum,
                    Frames = frames,
                    Steps = session.PhysicsSteps,
                    Hash = session.StateHash(),
                    Debris = debris.ToString()
                };
            }
            finally
            {
                session.Renderer.Dispose();
            }
        }

        static void AppendRun(string path, RunRow row)
        {
            var inv = CultureInfo.InvariantCulture;
            string line = string.Join(",",
                row.Sector, row.Stream, row.Profile,
                row.Collected.ToString("R", inv),
                row.Spilled.ToString("R", inv),
                row.Swallowed.ToString(inv),
                row.SpilledCount.ToString(inv),
                row.Exploded.ToString(inv),
                row.InPlay.ToString(inv),
                row.StillIngesting.ToString(inv),
                row.StillFalling.ToString(inv),
                row.StillInRing.ToString(inv),
                row.VictoryTime.ToString("R", inv),
                row.DefeatTime.ToString("R", inv),
                row.SimTime.ToString("R", inv),
                row.RequestedTime.ToString("R", inv),
                row.Frames.ToString(inv),
                row.Steps.ToString(inv),
                row.Hash);
            File.AppendAllText(path, line + "\n");
        }

        static string RenderReport(List<LayoutSnap> layout, List<RunRow> rows, List<Profile> profiles, string boot)
        {
            var inv = CultureInfo.InvariantCulture;
            var sb = new StringBuilder();
            sb.Append("# Physics frame-time sweep 2026-10-07\n\n");
            sb.Append("Live path: `LiveGameSession.Update` with `FixedStep` false, `RenderEnabled` false, simulated time ")
                .Append(Duration.ToString("R", inv)).Append(" s. Session caps each call at `DtCap` ")
                .Append(LiveGameSession.DtCap.ToString("R", inv)).Append(" s. Boot matches `RecordedFrameDriver.Create` (renderer RNG, layout 430x932, discarded sector 1, `ConsumeBootRandom`, then the sector) except `FixedStep` is cleared so the passed dt is the step.\n\n");
            sb.Append("Inputs: physics golden programs `idle`, `drag`, `bursts` (`PhysicsRunner.Program`). A program frame f is an event at t = (f-1) * 0.016 s, applied on the first harness frame whose simulated time is at least that t, before `Update`. Drag values are pointer deltas in CSS pixels (`PointerMove` multiplies by 1.3, matching `ApplyInputDrag(delta * 1.3)`). RCS events use `KeyDown`.\n\n");
            sb.Append("Random profile seed ").Append(RandomSeed.ToString(inv))
                .Append(" (LCG 1664525/1013904223, 24-bit unit interval, dt uniform on [14, 30) ms). Spike profile: 16 ms, and 100 ms when the 1-based frame index is a multiple of 25; the session cap shortens a 100 ms request to 50 ms. Editor traces cycle `frameMs` from tag `archive/parity-homeperf`.\n\n");
            sb.Append("## Boot hashes\n\n```\n").Append(boot).Append("\n```\n\n");
            sb.Append("## Layout after Unity ApplyLayout\n\n");
            sb.Append("| when | center.x | center.y | blackHole.x | blackHole.y | eventHorizonRadius | gravityRadius | gravityStrength |\n");
            sb.Append("| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |\n");
            for (int i = 0; i < layout.Count; i++)
            {
                LayoutSnap s = layout[i];
                sb.Append("| ").Append(s.When)
                    .Append(" | ").Append(s.CenterX.ToString("R", inv))
                    .Append(" | ").Append(s.CenterY.ToString("R", inv))
                    .Append(" | ").Append(s.HoleX.ToString("R", inv))
                    .Append(" | ").Append(s.HoleY.ToString("R", inv))
                    .Append(" | ").Append(s.Horizon.ToString("R", inv))
                    .Append(" | ").Append(s.GravityRadius.ToString("R", inv))
                    .Append(" | ").Append(s.GravityStrength.ToString("R", inv))
                    .Append(" |\n");
            }
            sb.Append("\n## Profile vs steady-16ms\n\n");
            sb.Append("Difference is profile minus `steady-16ms`, across every sector and stream. Swallowed is the `OnSwallow` count (event-horizon entry). Spilled is `SpilledMass`.\n\n");
            sb.Append("| profile | runs | mean spilled | mean swallowed | spilled delta mean | spilled delta min | spilled delta max | swallowed delta mean | swallowed delta min | swallowed delta max | runs spilled higher | runs swallowed lower |\n");
            sb.Append("| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |\n");
            string reference = profiles[0].Name;
            for (int p = 0; p < profiles.Count; p++)
            {
                AppendDelta(sb, rows, reference, profiles[p].Name, null);
            }
            sb.Append("\n## Same differences by stream\n\n");
            sb.Append("| stream | profile | runs | spilled delta mean | spilled delta min | spilled delta max | swallowed delta mean | swallowed delta min | swallowed delta max | runs spilled higher | runs swallowed lower |\n");
            sb.Append("| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |\n");
            for (int s = 0; s < Streams.Length; s++)
            {
                for (int p = 0; p < profiles.Count; p++)
                    AppendDelta(sb, rows, reference, profiles[p].Name, Streams[s]);
            }
            sb.Append("\n## Means by profile\n\n");
            sb.Append("| profile | mean collected | mean spilled | mean swallowed | mean in play | mean still ingesting | mean still falling | defeats | victories |\n");
            sb.Append("| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |\n");
            for (int p = 0; p < profiles.Count; p++)
                AppendMeans(sb, rows, profiles[p].Name);
            sb.Append("\nPer-run rows: `physics-framerate-2026-10-07-runs.csv`. Per-debris rows: `physics-framerate-2026-10-07-debris.csv`.\n");
            return sb.ToString();
        }

        static void AppendDelta(StringBuilder sb, List<RunRow> rows, string reference, string profile, string stream)
        {
            var inv = CultureInfo.InvariantCulture;
            double spillSum = 0, swSum = 0, spillAbs = 0, swAbs = 0;
            double spillMin = double.PositiveInfinity, spillMax = double.NegativeInfinity;
            double swMin = double.PositiveInfinity, swMax = double.NegativeInfinity;
            int n = 0, spillHigher = 0, swLower = 0;
            for (int i = 0; i < rows.Count; i++)
            {
                RunRow row = rows[i];
                if (row.Profile != profile) continue;
                if (stream != null && row.Stream != stream) continue;
                RunRow src = Find(rows, row.Sector, row.Stream, reference);
                double ds = row.Spilled - src.Spilled;
                double dw = row.Swallowed - src.Swallowed;
                spillSum += ds;
                swSum += dw;
                spillAbs += row.Spilled;
                swAbs += row.Swallowed;
                if (ds < spillMin) spillMin = ds;
                if (ds > spillMax) spillMax = ds;
                if (dw < swMin) swMin = dw;
                if (dw > swMax) swMax = dw;
                if (ds > 0) spillHigher++;
                if (dw < 0) swLower++;
                n++;
            }
            if (n == 0) return;
            if (stream != null) sb.Append("| ").Append(stream).Append(" | ").Append(profile);
            else sb.Append("| ").Append(profile);
            sb.Append(" | ").Append(n.ToString(inv));
            if (stream == null)
            {
                sb.Append(" | ").Append((spillAbs / n).ToString("R", inv));
                sb.Append(" | ").Append((swAbs / n).ToString("R", inv));
            }
            sb.Append(" | ").Append((spillSum / n).ToString("R", inv));
            sb.Append(" | ").Append(spillMin.ToString("R", inv));
            sb.Append(" | ").Append(spillMax.ToString("R", inv));
            sb.Append(" | ").Append((swSum / n).ToString("R", inv));
            sb.Append(" | ").Append(swMin.ToString("R", inv));
            sb.Append(" | ").Append(swMax.ToString("R", inv));
            sb.Append(" | ").Append(spillHigher.ToString(inv));
            sb.Append(" | ").Append(swLower.ToString(inv));
            sb.Append(" |\n");
        }

        static void AppendMeans(StringBuilder sb, List<RunRow> rows, string profile)
        {
            var inv = CultureInfo.InvariantCulture;
            double c = 0, s = 0, sw = 0, play = 0, ing = 0, fall = 0;
            int n = 0, defeats = 0, victories = 0;
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].Profile != profile) continue;
                c += rows[i].Collected;
                s += rows[i].Spilled;
                sw += rows[i].Swallowed;
                play += rows[i].InPlay;
                ing += rows[i].StillIngesting;
                fall += rows[i].StillFalling;
                if (rows[i].DefeatTime >= 0) defeats++;
                if (rows[i].VictoryTime >= 0) victories++;
                n++;
            }
            sb.Append("| ").Append(profile)
                .Append(" | ").Append((c / n).ToString("R", inv))
                .Append(" | ").Append((s / n).ToString("R", inv))
                .Append(" | ").Append((sw / n).ToString("R", inv))
                .Append(" | ").Append((play / n).ToString("R", inv))
                .Append(" | ").Append((ing / n).ToString("R", inv))
                .Append(" | ").Append((fall / n).ToString("R", inv))
                .Append(" | ").Append(defeats.ToString(inv))
                .Append(" | ").Append(victories.ToString(inv))
                .Append(" |\n");
        }

        static RunRow Find(List<RunRow> rows, string sector, string stream, string profile)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].Sector == sector && rows[i].Stream == stream && rows[i].Profile == profile)
                    return rows[i];
            }
            throw new InvalidOperationException("missing " + sector + " " + stream + " " + profile);
        }

        static string MazeRoot()
        {
            var dir = new DirectoryInfo(Application.dataPath);
            for (int i = 0; i < 8 && dir != null; i++)
            {
                string parity = Path.Combine(dir.FullName, "tools", "parity");
                string web = Path.Combine(dir.FullName, "web", "index.html");
                if (Directory.Exists(parity) && File.Exists(web)) return dir.FullName;
                dir = dir.Parent;
            }
            throw new InvalidOperationException("games/maze-puzzle marker (tools/parity + web) not found from " + Application.dataPath);
        }
    }
}
