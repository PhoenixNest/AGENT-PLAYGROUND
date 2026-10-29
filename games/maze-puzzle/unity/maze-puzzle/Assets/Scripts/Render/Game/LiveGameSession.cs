using System;
using System.Collections.Generic;
using System.Diagnostics;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Loc;
using OrbitalSalvage.Logic;

namespace OrbitalSalvage.Render
{
    using Loc = OrbitalSalvage.Loc.Loc;
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

    public enum TutorialPhase
    {
        None,
        Swipe,
        Gate,
        Feed,
        Done
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

        /// <summary>
        /// Page pixels from the ring centre. A drag that starts or ends inside this
        /// distance uses sideways movement only, so the hub does not divide by a tiny radius.
        /// </summary>
        public const double MinGrabRadius = 40;

        static readonly string[] NomKeys = { "fx.swallow.nom", "fx.swallow.yum", "fx.swallow.gulp", "fx.swallow.tasty" };

        /// <summary>When true, the physics step and <see cref="Present"/> record Stopwatch ticks. Default false, so play and stills are unchanged.</summary>
        public static bool PhaseTiming;
        public static long PhaseSimTicks;
        public static long PhasePresentTicks;

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
        /// <summary>Bench only. When set, a near win or an empty board restarts the sector instead of opening a modal.</summary>
        public bool HoldOutcome;
        bool pendingHoldRestart;
        public bool UsedBackupPod;
        public TutorialPhase TutorialPhase { get; private set; } = TutorialPhase.None;
        public Action<TutorialPhase> OnTutorialPhaseChanged;
        public bool IsVictoryCelebrationPlaying { get; private set; }
        public double VictoryCelebrationTimer { get; private set; }
        public const double CelebrationTotalDuration = 2.4;
        public Action<string> OnCelebrationCue;

        public bool IsTutorialEligible()
        {
            return ActiveSector != null && ActiveSector.LevelNumber == 1 && (Economy == null || !Economy.Profile.IntroSeen);
        }

        public void SetTutorialPhase(TutorialPhase phase)
        {
            if (TutorialPhase == phase) return;
            TutorialPhase = phase;
            if (OnTutorialPhaseChanged != null) OnTutorialPhaseChanged(phase);
        }

        public int PhysicsSteps;
        public int RenderFrames;
        public bool RenderEnabled = true;

        /// <summary>Forwards to <see cref="GameRenderer.SubmitEnabled"/>. Default true.</summary>
        public bool SubmitEnabled
        {
            get => Renderer.SubmitEnabled;
            set => Renderer.SubmitEnabled = value;
        }

        public bool FixedStep;
        public double FixedDt = 16.0 / 1000.0;

        /// <summary>
        /// Live play only. When set, and <see cref="FixedStep"/> is false, <see cref="Update"/>
        /// accumulates real frame time and integrates at <see cref="LiveStep"/> (1/60 s, a steady
        /// browser frame). Recorded streams stay on <see cref="FixedDt"/> of 16 ms via
        /// <see cref="FixedStep"/>. At most <see cref="LiveMaxSteps"/> integrations run per call,
        /// which is 50 ms, the same cap as <see cref="DtCap"/>. Leftover time past that cap is dropped.
        /// </summary>
        public bool Accumulate;
        public const double LiveStep = 1.0 / 60.0;
        public const int LiveMaxSteps = 3;
        public double SimulatedTime { get; private set; }
        public double PendingAccum { get { return _accum; } }
        double _accum;

        /// <summary>Invoked at the start of each live 1/60 s step with <see cref="SimulatedTime"/>. Null in product play.</summary>
        public Action<double> BeforeAccumStep;

        public bool ForceRcs;
        public Func<double> NowMs = () => 0;

        public Action<GameStats> OnVictory;
        public Action<GameStats> OnDefeat;
        public Action<Debris> OnSwallow;
        public Action<double, double> OnQuotaChanged;
        public Action<double> OnBoostCue;
        public List<Achievement> RecentUnlocks = new List<Achievement>();

        bool _dragging;
        double _lastPointerX;
        double _lastPointerY;
        double _tapPageWidth;
        double _lastTapTime;
        double _lastTapX;
        LocMemo _quotaLabel;

        public double Quota => ActiveSector != null ? ActiveSector.Quota : 30;

        public string QuotaLabel()
        {
            return _quotaLabel.Num("unit.quota", CollectedMass, Quota);
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
            if (Renderer != null)
            {
                Renderer.OnFireworkExploded = (x, y, color) =>
                {
                    var parityCues = Physics.Audio as ParityAudioCues;
                    if (parityCues != null) parityCues.PlayFireworkBurst();
                    if (OnCelebrationCue != null) OnCelebrationCue("burst");
                };
            }
        }

        public static LiveGameSession CreatePlay(double cssW, double cssH, int sectorNumber)
        {
            return CreatePlay(cssW, cssH, sectorNumber, null, true);
        }

        public static LiveGameSession CreatePlay(double cssW, double cssH, int sectorNumber, EconomyManager economy, bool resetRandom)
        {
            if (resetRandom) JsRandom.Global.Reset(JsRandom.DefaultSeed);
            var renderer = new GameRenderer(cssW, cssH, 3);
            var audio = new ParityAudioCues(() => 0);
            var physics = new PhysicsEngine(audio);
            if (economy == null) economy = new EconomyManager(new MemoryKeyValueStore(), new FixedWallClock(0));
            var session = new LiveGameSession(physics, renderer, economy);
            session.ApplyLayout(cssW, cssH);
            var sectors = MazeLevelBuilder.GetSectors();
            int index = sectorNumber - 1;
            if (index < 0) index = 0;
            if (index >= sectors.Count) index = sectors.Count - 1;
            session.LoadSector(sectors[index]);
            return session;
        }

        /// <summary>
        /// Web constructor order: starfield random, <c>initLayout</c>, then sector 0 at that center.
        /// The caller resets <see cref="JsRandom.Global"/> first. A later game-page layout call
        /// repeats the same center and does not respawn debris.
        /// </summary>
        public static LiveGameSession CreateBound(EconomyManager economy, double cssW, double cssH)
        {
            var renderer = new GameRenderer(cssW, cssH, 3);
            LiveGameSession session = null;
            var audio = new ParityAudioCues(() => session.AudioNowMs());
            var physics = new PhysicsEngine(audio);
            session = new LiveGameSession(physics, renderer, economy);
            session.Cues = audio;
            session.ApplyLayout(cssW, cssH);
            var sectors = MazeLevelBuilder.GetSectors();
            session.LoadSector(sectors[0]);
            return session;
        }

        public ParityAudioCues Cues { get; private set; }

        double _audioOrigin;
        double _replayLeadMs;
        bool _replayAudio;
        bool _audioOriginSet;

        /// <summary>
        /// Web <c>currentTime</c> follows rAF timestamps, and each recorded dt is the gap from the
        /// previous rAF. The first burst anchors to <see cref="NowMs"/> (the click instant). Later
        /// bursts continue from the last frame, so an <c>Advance</c> that runs before the replay
        /// cannot shift the 48ms collision cooldown.
        /// </summary>
        public double AudioNowMs()
        {
            if (_replayAudio) return _audioOrigin + _replayLeadMs;
            return _audioOriginSet ? _audioOrigin : NowMs();
        }

        public void AdvanceReplayAudio(double dtMs)
        {
            if (!_replayAudio)
            {
                if (!_audioOriginSet) _audioOrigin = NowMs();
                _replayLeadMs = 0;
                _replayAudio = true;
            }
            _replayLeadMs += dtMs;
        }

        public void FinishReplayAudio()
        {
            if (_replayAudio)
            {
                _audioOrigin += _replayLeadMs;
                _audioOriginSet = true;
            }
            _replayAudio = false;
            _replayLeadMs = 0;
        }

        public void ApplyLayout(double w, double h)
        {
            _tapPageWidth = w;
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
            SimulatedTime = 0;
            ClearAccum();
            Renderer.ResetNibblesScale();
            var bh = Physics.BlackHole;
            Physics.InitSector(sector, Physics.Center, new Vec2(bh.X, bh.Y));
            if (OnQuotaChanged != null) OnQuotaChanged(CollectedMass, Quota);
            IsVictoryCelebrationPlaying = false;
            VictoryCelebrationTimer = 0;
            Renderer.ResetCelebrationVfx();
            bool isTutEligible = sector != null && sector.LevelNumber == 1 && (Economy == null || !Economy.Profile.IntroSeen);
            SetTutorialPhase(isTutEligible ? TutorialPhase.Swipe : TutorialPhase.None);
        }

        public void Restart()
        {
            if (ActiveSector != null) LoadSector(ActiveSector);
        }

        /// <summary>One Chromium rAF while the game page is showing. <paramref name="simulate"/> is false when the pause modal is up; the renderer still advances.</summary>
        public void StepWebFrame(double dtSeconds, bool simulate)
        {
            bool was = IsPaused;
            bool accumulate = Accumulate;
            IsPaused = !simulate;
            Accumulate = false;
            ClearAccum();
            Update(dtSeconds);
            Accumulate = accumulate;
            IsPaused = was;
        }

        public string StateHash()
        {
            return PhysicsSnapshot.Hash(Physics);
        }

        public void ClearAccum()
        {
            _accum = 0;
        }

        public void Update(double dt)
        {
            if (pendingHoldRestart)
            {
                pendingHoldRestart = false;
                if (HoldOutcome) Restart();
            }
            if (Accumulate && !FixedStep)
            {
                UpdateAccumulated(dt);
                return;
            }
            double step = FixedStep ? FixedDt : dt;
            step = JsMath.Min(step, DtCap);
            if (!IsPaused) AdvanceUnpaused(step);
            Present(step);
        }

        void UpdateAccumulated(double dt)
        {
            if (IsPaused)
            {
                ClearAccum();
                Present(JsMath.Min(dt, DtCap));
                return;
            }
            if (dt > 0) _accum += dt;
            int n = 0;
            while (n < LiveMaxSteps && _accum >= LiveStep)
            {
                if (BeforeAccumStep != null) BeforeAccumStep(SimulatedTime);
                AdvanceUnpaused(LiveStep);
                SimulatedTime += LiveStep;
                _accum -= LiveStep;
                n++;
            }
            if (n == LiveMaxSteps) _accum = 0;
            Present(n * LiveStep);
        }

        void AdvanceUnpaused(double step)
        {
            bool measure = PhaseTiming;
            long t0 = measure ? Stopwatch.GetTimestamp() : 0;
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
                if (HoldOutcome)
                {
                    if (CollectedMass >= ActiveSector.Quota * 0.85) pendingHoldRestart = true;
                    else if (!any)
                    {
                        FailureCheckTimer += step;
                        if (FailureCheckTimer >= FailDelay) pendingHoldRestart = true;
                    }
                    else FailureCheckTimer = 0;
                }
                else if (!any && CollectedMass < ActiveSector.Quota)
                {
                    if (ActiveSector.LevelNumber == 1 && (Economy == null || !Economy.Profile.IntroSeen) && !UsedBackupPod)
                    {
                        UsedBackupPod = true;
                        Physics.SpawnBackupScrap(8);
                        FailureCheckTimer = 0;
                        if (TutorialPhase != TutorialPhase.Done) SetTutorialPhase(TutorialPhase.Feed);
                        Renderer.AddFloatingText(Physics.Center.X, Physics.Center.Y - 60, Loc.Tr("intro.safety.backup"), "#38bdf8");
                        Renderer.AddBoostShockwave(Physics.Center.X, Physics.Center.Y, Physics.RingRadius * 0.8, "#38bdf8");
                    }
                    else
                    {
                        FailureCheckTimer += step;
                        if (FailureCheckTimer >= FailDelay) HandleDefeat();
                    }
                }
                else FailureCheckTimer = 0;
            }
            if (IsVictoryCelebrationPlaying)
            {
                double prevT = VictoryCelebrationTimer;
                VictoryCelebrationTimer += step;
                double cx = Physics.Center.X;
                double cy = Physics.Center.Y;
                double bhY = Physics.BlackHole.Y;
                if (prevT < 0.32 && VictoryCelebrationTimer >= 0.32)
                {
                    var parityCues = Physics.Audio as ParityAudioCues;
                    if (parityCues != null) parityCues.PlayFireworkLaunch();
                    if (OnCelebrationCue != null) OnCelebrationCue("launch");
                    Renderer.LaunchFirework(cx - 105.0, bhY + 15.0, cy - 75.0, "#fde047");
                    Renderer.LaunchFirework(cx + 100.0, bhY + 15.0, cy - 40.0, "#4ade80");
                }
                if (prevT < 0.68 && VictoryCelebrationTimer >= 0.68)
                {
                    var parityCues = Physics.Audio as ParityAudioCues;
                    if (parityCues != null) parityCues.PlayFireworkLaunch();
                    if (OnCelebrationCue != null) OnCelebrationCue("launch");
                    Renderer.LaunchFirework(cx, bhY, cy - 115.0, "#c084fc");
                    Renderer.LaunchFirework(cx - 45.0, bhY + 10.0, cy + 55.0, "#fb923c");
                }
                if (VictoryCelebrationTimer >= CelebrationTotalDuration)
                    IsVictoryCelebrationPlaying = false;
            }
            if (measure) PhaseSimTicks += Stopwatch.GetTimestamp() - t0;
        }

        void Present(double step)
        {
            bool measure = PhaseTiming;
            long t0 = measure ? Stopwatch.GetTimestamp() : 0;
            if (ForceRcs) Physics.RcsThrusting = true;
            if (RenderEnabled)
            {
                progress.CollectedMass = CollectedMass;
                progress.Quota = ActiveSector != null ? ActiveSector.Quota : 30;
                Renderer.Render(Physics, Economy, step, progress, TutorialPhase);
            }
            RenderFrames++;
            if (measure) PhasePresentTicks += Stopwatch.GetTimestamp() - t0;
        }

        public void PointerDown(double clientX, double canvasLeft, double canvasWidth)
        {
            if (IsPaused) return;
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

        /// <summary>
        /// Playfield press in page pixels. Stores both axes for <see cref="PointerMove(double, double)"/>.
        /// A double tap still splits the laid-out page width at the left edge, which is what the viewer passed before.
        /// </summary>
        public void PointerDown(double clientX, double clientY)
        {
            if (IsPaused) return;
            if (IsLevelCleared)
            {
                Renderer.LaunchSingleCelebrationConfetti(clientX, clientY);
                Renderer.ExplodeFirework(clientX, clientY, null, 16);
                var parityCues = Physics.Audio as ParityAudioCues;
                if (parityCues != null)
                {
                    parityCues.PlayCelebrationCannon();
                    parityCues.PlayFireworkBurst();
                }
                if (OnCelebrationCue != null)
                {
                    OnCelebrationCue("cannon");
                    OnCelebrationCue("burst");
                }
            }
            PointerDown(clientX, 0, _tapPageWidth);
            _lastPointerY = clientY;
        }

        public void PointerMove(double clientX)
        {
            if (IsPaused || !_dragging) return;
            if (TutorialPhase == TutorialPhase.Swipe) SetTutorialPhase(TutorialPhase.Gate);
            double delta = clientX - _lastPointerX;
            _lastPointerX = clientX;
            Physics.ApplyInputDrag(delta * 1.3);
        }

        /// <summary>
        /// Moves the ring with the finger. The tangential distance, in page pixels, is passed to
        /// <see cref="PhysicsEngine.ApplyInputDrag"/> with the same 1.3 gain as a sideways drag.
        /// Positive is clockwise on screen (Y grows downward), matching <see cref="PhysicsEngine.RingAngle"/>.
        /// </summary>
        public void PointerMove(double clientX, double clientY)
        {
            if (IsPaused) return;
            double previousX = _lastPointerX;
            double previousY = _lastPointerY;
            _lastPointerX = clientX;
            _lastPointerY = clientY;
            if (!_dragging) return;
            if (TutorialPhase == TutorialPhase.Swipe) SetTutorialPhase(TutorialPhase.Gate);
            Physics.ApplyInputDrag(GrabPixels(previousX, previousY, clientX, clientY) * 1.3);
        }

        double GrabPixels(double x0, double y0, double x1, double y1)
        {
            double cx = Physics.Center.X;
            double cy = Physics.Center.Y;
            double v0x = x0 - cx;
            double v0y = y0 - cy;
            double v1x = x1 - cx;
            double v1y = y1 - cy;
            double r0 = Math.Sqrt(v0x * v0x + v0y * v0y);
            double r1 = Math.Sqrt(v1x * v1x + v1y * v1y);
            if (r0 < MinGrabRadius || r1 < MinGrabRadius) return x1 - x0;
            double cross = v0x * v1y - v0y * v1x;
            double dot = v0x * v1x + v0y * v1y;
            double dTheta = Math.Atan2(cross, dot);
            return dTheta * (r0 + r1) * 0.5;
        }

        public void PointerUp()
        {
            _dragging = false;
        }

        public void KeyDown(string key)
        {
            if (IsPaused) return;
            if (key == "a" || key == "A" || key == "ArrowLeft")
            {
                if (TutorialPhase == TutorialPhase.Swipe) SetTutorialPhase(TutorialPhase.Gate);
                Physics.ApplyRcsBurst(-1);
            }
            else if (key == "d" || key == "D" || key == "ArrowRight" || key == " " || key == "Space")
            {
                if (TutorialPhase == TutorialPhase.Swipe) SetTutorialPhase(TutorialPhase.Gate);
                Physics.ApplyRcsBurst(1);
            }
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
                Loc.Tr("fx.swallow.nom"), Loc.Tr("fx.swallow.yum"), Loc.Tr("fx.swallow.gulp"), Loc.Tr("fx.swallow.tasty"),
                Loc.Tr("fx.shred", 5d), Loc.Tr("fx.compact", 7d), Loc.Tr("fx.boost"),
                Loc.Tr("fx.poof"), Loc.Tr("fx.turbo.cw"), Loc.Tr("fx.turbo.ccw")
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
            if (TutorialPhase == TutorialPhase.Feed && ActiveSector != null && CollectedMass >= ActiveSector.Quota * 0.4)
                SetTutorialPhase(TutorialPhase.Done);
            Renderer.TriggerNibblesChew();
            int word = (int)JsMath.Floor(JsRandom.Global.NextDouble() * NomKeys.Length);
            Renderer.AddFloatingText(Physics.BlackHole.X, Physics.BlackHole.Y - 32, Loc.Tr(NomKeys[word]), "#fde047");
            Renderer.AddPopRing(Physics.BlackHole.X, Physics.BlackHole.Y, 46, "#facc15");
            RecentUnlocks = Economy.RecordDebrisSwallowed(1);
            if (OnSwallow != null) OnSwallow(d);
            if (HoldOutcome && ActiveSector != null && CollectedMass >= ActiveSector.Quota * 0.85)
                pendingHoldRestart = true;
            else if (!IsLevelCleared && ActiveSector != null && CollectedMass >= ActiveSector.Quota)
                HandleVictory();
        }

        void OnGate(GateRuntime gate, Debris debris)
        {
            if (TutorialPhase == TutorialPhase.Gate) SetTutorialPhase(TutorialPhase.Feed);
            double rot = gate.IsInner && Physics.ConcentricRings ? Physics.InnerRingAngle - Physics.RingAngle : 0;
            double rMid = (gate.RMin + gate.RMax) * 0.5;
            double ang = gate.Angle + rot + Physics.RingAngle;
            double gx = Physics.Center.X + JsMath.Cos(ang) * rMid;
            double gy = Physics.Center.Y + JsMath.Sin(ang) * rMid;
            string label = gate.Type == "MULTIPLY"
                ? Loc.Tr("fx.shred", gate.Value)
                : gate.Type == "ADD"
                    ? Loc.Tr("fx.compact", gate.Value)
                    : Loc.Tr("fx.boost");
            Renderer.AddFloatingText(gx, gy - 16, label, "#fde047");
            Renderer.AddConfetti(gx, gy, 8);
        }

        void OnExploded(Debris d)
        {
            Renderer.AddConfetti(d.X, d.Y, 22);
            Renderer.AddFloatingText(d.X, d.Y - 20, Loc.Tr("fx.poof"), "#f472b6");
        }

        void OnBoost(double dir)
        {
            double cx = Physics.Center.X;
            double cy = Physics.Center.Y;
            double rOut = Physics.RingRadius;
            Renderer.AddBoostShockwave(cx, cy, rOut, "#38bdf8");
            string label = dir > 0 ? Loc.Tr("fx.turbo.cw") : Loc.Tr("fx.turbo.ccw");
            Renderer.AddFloatingText(cx, cy - rOut * 0.4, label, "#38bdf8");
            if (OnBoostCue != null) OnBoostCue(dir);
        }

        void HandleVictory()
        {
            if (IsLevelCleared) return;
            IsLevelCleared = true;
            LaunchVictoryCelebration();
            if (ActiveSector != null && ActiveSector.LevelNumber == 1)
            {
                if (Economy != null) Economy.CompleteIntro();
                SetTutorialPhase(TutorialPhase.Done);
            }
            double reward = ActiveSector != null && ActiveSector.RewardCredits.HasValue ? ActiveSector.RewardCredits.Value : 0;
            double payout = EconomyManager.CampaignPayout(reward, CollectedMass, Quota);
            Economy.AddCredits(payout);
            if (ActiveSector != null && ActiveSector.IsDaily != true && !string.IsNullOrEmpty(ActiveSector.Id))
                Economy.RecordSectorCompleted(ActiveSector.Id);
            bool daily = ActiveSector != null && ActiveSector.IsDaily == true;
            string tier = ActiveSector != null && !string.IsNullOrEmpty(ActiveSector.DailyTier) ? ActiveSector.DailyTier : "silver";
            RecentUnlocks = Economy.RecordContractComplete(daily, tier);
            if (OnVictory != null) OnVictory(MakeStats(payout));
        }

        
        public void LaunchVictoryCelebration()
        {
            IsVictoryCelebrationPlaying = true;
            VictoryCelebrationTimer = 0;

            double cx = Physics.Center.X;
            double cy = Physics.Center.Y;
            double bhY = Physics.BlackHole.Y;
            double canvasW = _tapPageWidth > 0 ? _tapPageWidth : 430.0;
            double canvasH = 932.0;

            double cannonLeftX = 20.0;
            double cannonRightX = canvasW - 20.0;
            double cannonY = JsMath.Min(canvasH - 50.0, bhY + 50.0);

            var parityCues = Physics.Audio as ParityAudioCues;
            if (parityCues != null) parityCues.PlayCelebrationCannon();
            if (OnCelebrationCue != null) OnCelebrationCue("cannon");
            Renderer.LaunchCelebrationCannons(cannonLeftX, cannonY, cannonRightX, cannonY, 36);

            if (parityCues != null) parityCues.PlayFireworkLaunch();
            if (OnCelebrationCue != null) OnCelebrationCue("launch");
            Renderer.LaunchFirework(cx - 65.0, bhY, cy - 30.0, "#f472b6");
            Renderer.LaunchFirework(cx + 65.0, bhY, cy + 30.0, "#38bdf8");
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
