using System;
using System.Collections.Generic;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Logic;

namespace OrbitalSalvage.Render
{
    public sealed class GameStats
    {
        public double CollectedMass;
        public double SpilledMass;
        public double Quota;
        public double Payout;
        public string SectorId;
        public string SectorCode;
        public bool Cleared;
        public bool Failed;
    }

    /// <summary>
    /// The gameplay half of <c>web/main.js</c>: sector load, the capped timestep, pointer and key
    /// input, swallow/spill mass, and the win and fail rules. Page navigation stays in the viewer.
    /// </summary>
    public sealed class LiveGameSession
    {
        public const double DtCap = 0.05;
        public const double FailDelay = 1.2;
        public const double DoubleTapMs = 280;
        public const double DoubleTapPx = 45;

        static readonly string[] NomWords = { "NOM! 😋", "YUM! ✨", "GULP! 🍩", "TASTY! ⭐" };

        public readonly PhysicsEngine Physics;
        public readonly GameRenderer Renderer;
        public readonly EconomyManager Economy;
        readonly RenderProgress progress = new RenderProgress();

        public SectorConfig ActiveSector;
        public double CollectedMass;
        public double SpilledMass;
        public bool IsPaused;
        public bool IsLevelCleared;
        public bool IsLevelFailed;
        public double FailureCheckTimer;
        public bool UsedBackupPod;
        public int PhysicsSteps;
        public int RenderFrames;
        public bool RenderEnabled = true;
        public bool FixedStep;
        public double FixedDt = 16.0 / 1000.0;
        public bool ForceRcs;
        public Func<double> NowMs = () => 0;

        public Action<GameStats> OnVictory;
        public Action<GameStats> OnDefeat;
        public Action<Debris> OnSwallow;
        public Action<double, double> OnQuotaChanged;

        bool _dragging;
        double _lastPointerX;
        double _lastTapTime;
        double _lastTapX;

        public double Quota => ActiveSector != null ? ActiveSector.Quota : 30;

        public string QuotaLabel()
        {
            return JsNumber.ToString(CollectedMass) + " / " + JsNumber.ToString(Quota) + " kg";
        }

        public double QuotaBarWidth(double trackWidth)
        {
            if (!(Quota > 0)) return 0;
            double ratio = JsMath.Min(1, JsMath.Max(0, CollectedMass / Quota));
            return trackWidth * ratio;
        }

        public LiveGameSession(PhysicsEngine physics, GameRenderer renderer, EconomyManager economy)
        {
            Physics = physics;
            Renderer = renderer;
            Economy = economy;
            Physics.OnDebrisSpilled = OnSpilled;
            Physics.OnDebrisSwallowed = OnSwallowed;
            Physics.OnGatePassed = OnGate;
            Physics.OnDebrisExploded = OnExploded;
            Physics.OnBoostTriggered = OnBoost;
        }

        public static LiveGameSession CreatePlay(double cssW, double cssH, int sectorNumber)
        {
            JsRandom.Global.Reset(JsRandom.DefaultSeed);
            var renderer = new GameRenderer(cssW, cssH, 3);
            var audio = new ParityAudioCues(() => 0);
            var physics = new PhysicsEngine(audio);
            var economy = new EconomyManager(new MemoryKeyValueStore(), new FixedWallClock(0));
            var session = new LiveGameSession(physics, renderer, economy);
            session.ApplyLayout(cssW, cssH);
            var sectors = MazeLevelBuilder.GetSectors();
            int index = sectorNumber - 1;
            if (index < 0) index = 0;
            if (index >= sectors.Count) index = sectors.Count - 1;
            session.LoadSector(sectors[index]);
            return session;
        }

        public void ApplyLayout(double w, double h)
        {
            Renderer.Resize(w, h);
            double centerX = w * 0.5;
            double centerY = JsMath.Max(175, h * 0.38);
            double blackHoleY = JsMath.Min(h - 110, JsMath.Max(centerY + 240, h * 0.76));
            Physics.Center = new Vec2(centerX, centerY);
            Physics.BlackHole.X = centerX;
            Physics.BlackHole.Y = blackHoleY;
        }

        public void LoadSector(SectorConfig sector)
        {
            ActiveSector = sector;
            CollectedMass = 0;
            SpilledMass = 0;
            IsPaused = false;
            IsLevelCleared = false;
            IsLevelFailed = false;
            FailureCheckTimer = 0;
            UsedBackupPod = false;
            Renderer.ResetNibblesScale();
            var bh = Physics.BlackHole;
            Physics.InitSector(sector, Physics.Center, new Vec2(bh.X, bh.Y));
            if (OnQuotaChanged != null) OnQuotaChanged(CollectedMass, Quota);
        }

        public void Restart()
        {
            if (ActiveSector != null) LoadSector(ActiveSector);
        }

        public void Update(double dt)
        {
            double step = FixedStep ? FixedDt : dt;
            step = JsMath.Min(step, DtCap);
            if (!IsPaused)
            {
                Physics.Update(step);
                PhysicsSteps++;
                if (!IsLevelCleared && !IsLevelFailed && ActiveSector != null)
                {
                    bool any = false;
                    var list = Physics.DebrisList;
                    for (int i = 0; i < list.Count; i++)
                    {
                        var state = list[i].State;
                        if (state == DebrisState.InRing || state == DebrisState.Falling || state == DebrisState.Ingesting)
                        {
                            any = true;
                            break;
                        }
                    }
                    if (!any && CollectedMass < ActiveSector.Quota)
                    {
                        FailureCheckTimer += step;
                        if (FailureCheckTimer >= FailDelay) HandleDefeat();
                    }
                    else FailureCheckTimer = 0;
                }
            }
            if (ForceRcs) Physics.RcsThrusting = true;
            if (RenderEnabled)
            {
                progress.CollectedMass = CollectedMass;
                progress.Quota = ActiveSector != null ? ActiveSector.Quota : 30;
                Renderer.Render(Physics, Economy, step, progress);
            }
            RenderFrames++;
        }

        public void PointerDown(double clientX, double canvasLeft, double canvasWidth)
        {
            double now = NowMs();
            _dragging = true;
            _lastPointerX = clientX;
            double since = now - _lastTapTime;
            double dist = JsMath.Abs(clientX - _lastTapX);
            if (since < DoubleTapMs && dist < DoubleTapPx)
            {
                double norm = canvasWidth != 0 ? (clientX - canvasLeft) / canvasWidth : 0;
                Physics.ApplyRcsBurst(norm > 0.5 ? 1 : -1);
            }
            _lastTapTime = now;
            _lastTapX = clientX;
        }

        public void PointerMove(double clientX)
        {
            if (!_dragging) return;
            double delta = clientX - _lastPointerX;
            _lastPointerX = clientX;
            Physics.ApplyInputDrag(delta * 1.3);
        }

        public void PointerUp()
        {
            _dragging = false;
        }

        public void KeyDown(string key)
        {
            if (key == "a" || key == "A" || key == "ArrowLeft") Physics.ApplyRcsBurst(-1);
            else if (key == "d" || key == "D" || key == "ArrowRight" || key == " " || key == "Space") Physics.ApplyRcsBurst(1);
            else if (key == "r" || key == "R") Restart();
        }

        public void InjectRecordedVfx()
        {
            double cx = Physics.Center.X;
            double cy = Physics.Center.Y;
            Renderer.AddConfetti(cx, cy, 24);
            Renderer.AddConfetti(Physics.BlackHole.X, Physics.BlackHole.Y - 30, 12);
            Renderer.AddBoostShockwave(cx, cy, 120, "#38bdf8");
            Renderer.AddPopRing(cx + 60, cy + 80, 50, "#fde047");
            Renderer.AddThrusterPuff(cx - 40, cy + 30, 10, 40, 7, "#ffffff");
            Renderer.AddThrusterPuff(cx + 40, cy + 30, -10, 40, 9, "#fde047");
            Renderer.AddRimSpark(cx, cy - 100, 20, -30, "#fde047");
            Renderer.TriggerNibblesChew();
            string[] texts =
            {
                "NOM! 😋", "YUM! ✨", "GULP! 🍩", "TASTY! ⭐",
                "×5! SHRED ⚡", "+7! COMPACT ✨", "BOOST! ⏩",
                "POOF! 💨", "TURBO! ⏩", "⏪ TURBO!"
            };
            string[] colors =
            {
                "#fde047", "#fde047", "#fde047", "#fde047",
                "#fde047", "#4ade80", "#38bdf8",
                "#f472b6", "#38bdf8", "#38bdf8"
            };
            for (int i = 0; i < texts.Length; i++)
            {
                Renderer.AddFloatingText(60 + (i % 3) * 150, 120 + (i / 3) * 70, texts[i], colors[i]);
            }
        }

        void OnSpilled(Debris d)
        {
            SpilledMass += d.Weight * 2;
        }

        void OnSwallowed(Debris d)
        {
            CollectedMass += d.Weight * 2;
            if (OnQuotaChanged != null) OnQuotaChanged(CollectedMass, Quota);
            Renderer.TriggerNibblesChew();
            int word = (int)JsMath.Floor(JsRandom.Global.NextDouble() * NomWords.Length);
            Renderer.AddFloatingText(Physics.BlackHole.X, Physics.BlackHole.Y - 32, NomWords[word], "#fde047");
            Renderer.AddPopRing(Physics.BlackHole.X, Physics.BlackHole.Y, 46, "#facc15");
            Economy.RecordDebrisSwallowed(1);
            if (OnSwallow != null) OnSwallow(d);
            if (!IsLevelCleared && ActiveSector != null && CollectedMass >= ActiveSector.Quota)
                HandleVictory();
        }

        void OnGate(GateRuntime gate, Debris debris)
        {
            double rot = gate.IsInner && Physics.ConcentricRings ? Physics.InnerRingAngle - Physics.RingAngle : 0;
            double rMid = (gate.RMin + gate.RMax) * 0.5;
            double ang = gate.Angle + rot + Physics.RingAngle;
            double gx = Physics.Center.X + JsMath.Cos(ang) * rMid;
            double gy = Physics.Center.Y + JsMath.Sin(ang) * rMid;
            string label = gate.Type == "MULTIPLY"
                ? "×" + JsNumber.ToString(gate.Value) + "! SHRED ⚡"
                : gate.Type == "ADD"
                    ? "+" + JsNumber.ToString(gate.Value) + "! COMPACT ✨"
                    : "BOOST! ⏩";
            Renderer.AddFloatingText(gx, gy - 16, label, "#fde047");
            Renderer.AddConfetti(gx, gy, 8);
        }

        void OnExploded(Debris d)
        {
            Renderer.AddConfetti(d.X, d.Y, 22);
            Renderer.AddFloatingText(d.X, d.Y - 20, "POOF! 💨", "#f472b6");
        }

        void OnBoost(double dir)
        {
            double cx = Physics.Center.X;
            double cy = Physics.Center.Y;
            double rOut = Physics.RingRadius;
            Renderer.AddBoostShockwave(cx, cy, rOut, "#38bdf8");
            string label = dir > 0 ? "TURBO! ⏩" : "⏪ TURBO!";
            Renderer.AddFloatingText(cx, cy - rOut * 0.4, label, "#38bdf8");
        }

        void HandleVictory()
        {
            if (IsLevelCleared) return;
            IsLevelCleared = true;
            double reward = ActiveSector != null && ActiveSector.RewardCredits.HasValue ? ActiveSector.RewardCredits.Value : 0;
            double payout = EconomyManager.CampaignPayout(reward, CollectedMass, Quota);
            Economy.AddCredits(payout);
            if (ActiveSector != null && ActiveSector.IsDaily != true && !string.IsNullOrEmpty(ActiveSector.Id))
                Economy.RecordSectorCompleted(ActiveSector.Id);
            bool daily = ActiveSector != null && ActiveSector.IsDaily == true;
            string tier = ActiveSector != null && !string.IsNullOrEmpty(ActiveSector.DailyTier) ? ActiveSector.DailyTier : "silver";
            Economy.RecordContractComplete(daily, tier);
            if (OnVictory != null) OnVictory(MakeStats(payout));
        }

        void HandleDefeat()
        {
            if (IsLevelCleared || IsLevelFailed) return;
            IsLevelFailed = true;
            if (OnDefeat != null) OnDefeat(MakeStats(0));
        }

        GameStats MakeStats(double payout)
        {
            return new GameStats
            {
                CollectedMass = CollectedMass,
                SpilledMass = SpilledMass,
                Quota = Quota,
                Payout = payout,
                SectorId = ActiveSector != null ? ActiveSector.Id : null,
                SectorCode = ActiveSector != null ? ActiveSector.Code : null,
                Cleared = IsLevelCleared,
                Failed = IsLevelFailed
            };
        }
    }

    /// <summary>
    /// Replays the canvas-conformance <c>record.mjs</c> timeline: boot RNG, a discarded sector-1
    /// init, then the recorded sector, at a fixed 16 ms step.
    /// </summary>
    public static class RecordedFrameDriver
    {
        public const double FrameDt = 16.0 / 1000.0;

        public struct Step
        {
            public string Label;
            public int Advance;
            public string Before;
        }

        public static readonly Step[] Plan =
        {
            new Step { Label = "f001", Advance = 1 },
            new Step { Label = "f060", Advance = 59 },
            new Step { Label = "f180", Advance = 120 },
            new Step { Label = "vfx1", Advance = 1, Before = "vfx" },
            new Step { Label = "vfx2", Advance = 9 },
            new Step { Label = "pause", Advance = 12, Before = "pause" }
        };

        public static void ConsumeBootRandom()
        {
            double progress = 12;
            while (progress < 100)
            {
                progress += JsMath.Floor(JsRandom.Global.NextDouble() * 22) + 14;
                if (progress > 100) progress = 100;
            }
        }

        public static LiveGameSession Create(int sectorNumber)
        {
            JsRandom.Global.Reset(JsRandom.DefaultSeed);
            var renderer = new GameRenderer(430, 932, 3);
            var audio = new ParityAudioCues(() => 0);
            var physics = new PhysicsEngine(audio);
            var economy = new EconomyManager(new MemoryKeyValueStore(), new FixedWallClock(0));
            var session = new LiveGameSession(physics, renderer, economy);
            session.FixedStep = true;
            session.FixedDt = FrameDt;
            var sectors = MazeLevelBuilder.GetSectors();
            session.ApplyLayout(430, 932);
            session.LoadSector(sectors[0]);
            ConsumeBootRandom();
            session.LoadSector(sectors[sectorNumber - 1]);
            return session;
        }

        public static void Run(LiveGameSession session, Action<string> onCapture)
        {
            for (int s = 0; s < Plan.Length; s++)
            {
                var step = Plan[s];
                if (step.Before == "vfx")
                {
                    session.InjectRecordedVfx();
                    session.ForceRcs = true;
                }
                else if (step.Before == "pause")
                {
                    session.ForceRcs = false;
                    session.IsPaused = true;
                }
                int pre = step.Advance - 1;
                for (int i = 0; i < pre; i++) session.Update(FrameDt);
                session.Update(FrameDt);
                if (onCapture != null) onCapture(step.Label);
            }
        }
    }
}
