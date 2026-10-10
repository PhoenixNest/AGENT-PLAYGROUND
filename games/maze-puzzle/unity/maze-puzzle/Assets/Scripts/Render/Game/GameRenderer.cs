using System;
using System.Collections.Generic;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Logic;

namespace OrbitalSalvage.Render
{
    /// <summary>
    /// Port of <c>web/engine/renderer.js</c> <c>GameRenderer</c>. Draws into a <see cref="Canvas2D"/>
    /// using <see cref="JsMath"/> and <see cref="JsRandom.Global"/> (mulberry32, the harness seed).
    /// CSS pixel size is <see cref="Width"/> x <see cref="Height"/>. The backing store is
    /// <c>round(css * min(dpr, 3) * RenderScale)</c>.
    /// </summary>
    public sealed partial class GameRenderer : IDisposable
    {
        public static double DefaultRenderScale = 1.0;

        static readonly string[] ConfettiColors =
        {
            "#f472b6", "#fde047", "#38bdf8", "#4ade80", "#c084fc", "#fb923c"
        };
        static readonly string[] SingularityColors =
        {
            "#fde047", "#38bdf8", "#f472b6", "#c084fc", "#ffffff", "#4ade80", "#fbbf24"
        };
        static readonly string[] SparkColors = { "#fde047", "#38bdf8", "#ffffff", "#f472b6" };
        static readonly double[] Dash12_8 = { 12, 8 };
        static readonly double[] Dash14_16 = { 14, 16 };
        static readonly double[] Dash10_14 = { 10, 14 };
        static readonly double[] Dash6_8 = { 6, 8 };

        public readonly List<StarSprite> Stars = new List<StarSprite>();
        public readonly List<PlanetSprite> Planets = new List<PlanetSprite>();
        public readonly List<SingularityParticle> SingularityParticles = new List<SingularityParticle>();
        public readonly List<ConfettiParticle> Particles = new List<ConfettiParticle>();
        public readonly List<FloatingText> FloatingTexts = new List<FloatingText>();
        public readonly List<PopRing> PopRings = new List<PopRing>();
        public readonly List<ThrusterPuff> ThrusterPuffs = new List<ThrusterPuff>();

        public double Width;
        public double Height;
        public double Dpr = 1;
        public double RenderScale = 1;
        public double CosmicTime;
        public double NibblesBlinkTimer;
        public bool IsBlinking;
        public double MouthOpenAmount;
        public double ChewTimer;
        public double CurrentBhScale = 1;
        public double TargetBhScale = 1;
        public double BhWobble;
        public int SparkEmitTimer;

        /// <summary>Last frame's section times in ms: sky, nibbles, ring, flappers, bumpers, gates, debris, vfx, face.</summary>
        public static readonly double[] SectionMs = new double[9];

        Canvas2D _canvas;
        CanvasGradient skyGrad;
        string skyGradKey;
        double skyGradW, skyGradH;
        public bool RenderToCanvas = true;

        bool _submitEnabled = true;

        /// <summary>
        /// When false, draws and <see cref="GameDraw.Rand"/> still run, and <see cref="Canvas2D.EndFrame"/>
        /// does not execute the command buffer. Default true (Play mode and stills).
        /// </summary>
        public bool SubmitEnabled
        {
            get => _submitEnabled;
            set
            {
                _submitEnabled = value;
                if (_canvas != null) _canvas.SkipSubmit = !value;
            }
        }

        public Canvas2D Canvas => _canvas;
        public UnityEngine.RenderTexture Target => _canvas != null ? _canvas.Target : null;

        public GameRenderer(double cssWidth, double cssHeight, double devicePixelRatio = 3)
        {
            Dpr = JsMath.Min(devicePixelRatio > 0 ? devicePixelRatio : 1, 3);
            RenderScale = DefaultRenderScale > 0 ? DefaultRenderScale : 1;
            GenerateCartoonStars(65);
            GenerateCartoonPlanets();
            GenerateSingularityParticles(32);
            Resize(cssWidth, cssHeight);
        }

        public void Dispose()
        {
            if (_canvas != null)
            {
                _canvas.Dispose();
                _canvas = null;
            }
        }

        public void ResetNibblesScale()
        {
            CurrentBhScale = 1;
            TargetBhScale = 1;
            BhWobble = 0;
        }

        public void Resize(double w, double h)
        {
            Width = w;
            Height = h;
            int pw = (int)JsMath.Round(w * Dpr * RenderScale);
            int ph = (int)JsMath.Round(h * Dpr * RenderScale);
            if (pw < 1) pw = 1;
            if (ph < 1) ph = 1;
            if (_canvas != null && _canvas.Width == pw && _canvas.Height == ph) return;
            if (_canvas != null) _canvas.Dispose();
            _canvas = new Canvas2D(pw, ph);
            _canvas.SkipSubmit = !_submitEnabled;
        }

        public void SetRenderScale(double scale)
        {
            RenderScale = scale > 0 ? scale : 1;
            Resize(Width, Height);
        }

        void GenerateCartoonStars(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var s = new StarSprite();
                s.X = GameDraw.Rand();
                s.Y = GameDraw.Rand();
                s.Size = GameDraw.Rand() * 6 + 4;
                s.Type = GameDraw.Rand() > 0.4 ? "star" : "circle";
                double c0 = GameDraw.Rand();
                if (c0 > 0.5) s.Color = "#fde047";
                else if (GameDraw.Rand() > 0.3) s.Color = "#ffffff";
                else s.Color = "#f472b6";
                s.Speed = GameDraw.Rand() * 2 + 1;
                Stars.Add(s);
            }
        }

        void GenerateCartoonPlanets()
        {
            Planets.Add(new PlanetSprite { X = 0.15, Y = 0.18, Radius = 24, Color = "#f472b6", Ring = true, RingColor = "#fde047" });
            Planets.Add(new PlanetSprite { X = 0.82, Y = 0.25, Radius = 32, Color = "#38bdf8", Ring = false });
            Planets.Add(new PlanetSprite { X = 0.88, Y = 0.82, Radius = 20, Color = "#facc15", Ring = false });
        }

        void GenerateSingularityParticles(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var p = new SingularityParticle();
                p.RRatio = 1.05 + GameDraw.Rand() * 2.0;
                p.Angle = GameDraw.Rand() * JsMath.PI * 2;
                p.Speed = GameDraw.Rand() * 1.6 + 1.2;
                p.InwardRate = GameDraw.Rand() * 0.35 + 0.22;
                p.Size = GameDraw.Rand() * 2.6 + 1.8;
                p.Color = SingularityColors[(int)JsMath.Floor(GameDraw.Rand() * SingularityColors.Length)];
                p.Twinkle = GameDraw.Rand() * JsMath.PI * 2;
                p.TwinkleSpeed = GameDraw.Rand() * 4 + 2;
                SingularityParticles.Add(p);
            }
        }

        public void AddFloatingText(double x, double y, string text, string color = "#fde047")
        {
            FloatingTexts.Add(new FloatingText
            {
                X = x, Y = y, Text = text, Color = color, Alpha = 1, Vy = -30, Scale = 1.3, Life = 0.85
            });
        }

        public void AddPopRing(double x, double y, double radiusMax = 40, string color = "#fde047")
        {
            PopRings.Add(new PopRing { X = x, Y = y, Radius = 6, RadiusMax = radiusMax, Color = color, Alpha = 1 });
        }

        public void AddConfetti(double x, double y, int count = 12)
        {
            for (int i = 0; i < count; i++)
            {
                double angle = GameDraw.Rand() * JsMath.PI * 2;
                double speed = GameDraw.Rand() * 110 + 40;
                Particles.Add(new ConfettiParticle
                {
                    X = x,
                    Y = y,
                    Vx = JsMath.Cos(angle) * speed,
                    Vy = JsMath.Sin(angle) * speed - 20,
                    Gravity = 120,
                    Rot = GameDraw.Rand() * JsMath.PI,
                    VRot = (GameDraw.Rand() - 0.5) * 8,
                    Life = 0.65,
                    MaxLife = 0.65,
                    W = GameDraw.Rand() * 6 + 4,
                    H = GameDraw.Rand() * 4 + 3,
                    Color = ConfettiColors[(int)JsMath.Floor(GameDraw.Rand() * ConfettiColors.Length)]
                });
            }
        }

        public void UpdateSingularityParticles(PhysicsEngine physics, double dt)
        {
            var bh = physics.BlackHole;
            double closest = 9999;
            var debris = physics.DebrisList;
            for (int i = 0; i < debris.Count; i++)
            {
                var d = debris[i];
                if (d.State == DebrisState.Falling || d.State == DebrisState.Ingesting)
                {
                    double dist = JsMath.Hypot(bh.X - d.X, bh.Y - d.Y);
                    if (dist < closest) closest = dist;
                }
            }
            double suction = closest < 140 || ChewTimer > 0 ? 1.85 : 1.0;
            for (int i = 0; i < SingularityParticles.Count; i++)
            {
                var p = SingularityParticles[i];
                p.Twinkle += dt * p.TwinkleSpeed;
                double swirl = (p.Speed / (p.RRatio * 0.65 + 0.35)) * suction;
                p.Angle += swirl * dt;
                p.RRatio -= p.InwardRate * suction * dt;
                if (p.RRatio <= 0.98)
                {
                    p.RRatio = 2.4 + GameDraw.Rand() * 0.7;
                    p.Angle = GameDraw.Rand() * JsMath.PI * 2;
                }
            }
        }

        public void AddBoostShockwave(double x, double y, double radiusStart = 180, string color = "#38bdf8")
        {
            PopRings.Add(new PopRing
            {
                X = x, Y = y, Radius = radiusStart * 0.88, RadiusMax = radiusStart + 90,
                Color = color, Alpha = 1, LineWidth = 7, Speed = 260, FadeRate = 2.4
            });
            PopRings.Add(new PopRing
            {
                X = x, Y = y, Radius = radiusStart * 0.72, RadiusMax = radiusStart + 60,
                Color = "#fde047", Alpha = 0.9, LineWidth = 4, Speed = 190, FadeRate = 2.8
            });
        }

        public void AddThrusterPuff(double x, double y, double vx, double vy, double radius = 7, string color = "#ffffff")
        {
            if (ThrusterPuffs.Count > 25) return;
            ThrusterPuffs.Add(new ThrusterPuff
            {
                X = x, Y = y, Vx = vx, Vy = vy, Radius = radius, Color = color, Life = 0.28, MaxLife = 0.28
            });
        }

        public void AddRimSpark(double x, double y, double vx, double vy, string color = "#fde047")
        {
            if (Particles.Count > 100) return;
            Particles.Add(new ConfettiParticle
            {
                X = x,
                Y = y,
                Vx = vx + (GameDraw.Rand() - 0.5) * 40,
                Vy = vy + (GameDraw.Rand() - 0.5) * 40,
                Gravity = 40,
                Rot = GameDraw.Rand() * JsMath.PI,
                VRot = (GameDraw.Rand() - 0.5) * 12,
                Life = 0.32,
                MaxLife = 0.32,
                W = GameDraw.Rand() * 4 + 3,
                H = GameDraw.Rand() * 4 + 3,
                Color = color
            });
        }

        public void UpdateThrusterPuffs(double dt)
        {
            for (int i = ThrusterPuffs.Count - 1; i >= 0; i--)
            {
                var puff = ThrusterPuffs[i];
                puff.Life -= dt;
                if (puff.Life <= 0)
                {
                    ThrusterPuffs.RemoveAt(i);
                    continue;
                }
                puff.X += puff.Vx * dt;
                puff.Y += puff.Vy * dt;
                puff.Radius += dt * 32;
            }
        }

        public void TriggerNibblesChew()
        {
            ChewTimer = 0.35;
            MouthOpenAmount = 1;
            BhWobble = 0.09;
        }

        public void Render(PhysicsEngine physics, EconomyManager economy, double dt, RenderProgress progress)
        {
            CosmicTime += dt;
            UpdateNibblesFace(physics, dt, progress);
            UpdateSingularityParticles(physics, dt);
            UpdateThrusterPuffs(dt);
            if (!RenderToCanvas || _canvas == null) return;

            var ctx = _canvas;
            if (Canvas2D.SkipGameFx) ctx.SkipBlurredFx = true;
            else if (ctx.SkipBlurredFx) ctx.SkipBlurredFx = false;
            ctx.BeginFrame();
            double s = Dpr * RenderScale;
            ctx.SetTransform(s, 0, 0, s, 0, 0);
            ctx.ClearRect(0, 0, Width, Height);

            string ringId = economy != null && economy.Profile != null ? economy.Profile.Equipped.Get("ring") : null;
            string singId = economy != null && economy.Profile != null ? economy.Profile.Equipped.Get("singularity") : null;
            string skyId = economy != null && economy.Profile != null ? economy.Profile.Equipped.Get("skybox") : null;
            var ringSkin = GameDraw.FindSkin(EconomyCatalog.Rings, ringId);
            var singularitySkin = GameDraw.FindSkin(EconomyCatalog.Singularities, singId);
            var skyboxSkin = GameDraw.FindSkin(EconomyCatalog.Skyboxes, skyId);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            RenderCartoonSky(ctx, skyboxSkin);
            SectionMs[0] = sw.Elapsed.TotalMilliseconds;
            RenderNibblesBlackHole(ctx, physics, singularitySkin);
            SectionMs[1] = sw.Elapsed.TotalMilliseconds;
            RenderCartoonRing(ctx, physics, ringSkin);
            SectionMs[2] = sw.Elapsed.TotalMilliseconds;
            RenderOneWayFlappers(ctx, physics);
            SectionMs[3] = sw.Elapsed.TotalMilliseconds;
            RenderCartoonBumpers(ctx, physics);
            SectionMs[4] = sw.Elapsed.TotalMilliseconds;
            RenderCartoonGates(ctx, physics);
            SectionMs[5] = sw.Elapsed.TotalMilliseconds;
            RenderCartoonDebris(ctx, physics.DebrisList);
            SectionMs[6] = sw.Elapsed.TotalMilliseconds;
            RenderVfx(ctx, dt);
            SectionMs[7] = sw.Elapsed.TotalMilliseconds;
            ctx.EndFrame();
            SectionMs[8] = sw.Elapsed.TotalMilliseconds;
        }

        void UpdateNibblesFace(PhysicsEngine physics, double dt, RenderProgress progress)
        {
            if (progress != null && progress.Quota > 0)
            {
                double ratio = JsMath.Min(1, JsMath.Max(0, (progress.CollectedMass) / progress.Quota));
                TargetBhScale = 1 + ratio * 0.65;
            }
            else TargetBhScale = 1;

            if (BhWobble > 0.001) BhWobble *= JsMath.Pow(0.84, dt * 60);
            else BhWobble = 0;

            double targetWithWobble = TargetBhScale + BhWobble;
            CurrentBhScale += (targetWithWobble - CurrentBhScale) * JsMath.Min(1, dt * 6);

            NibblesBlinkTimer += dt;
            if (NibblesBlinkTimer > 3.2)
            {
                IsBlinking = true;
                if (NibblesBlinkTimer > 3.4)
                {
                    IsBlinking = false;
                    NibblesBlinkTimer = 0;
                }
            }

            var bh = physics.BlackHole;
            double closest = 9999;
            var debris = physics.DebrisList;
            for (int i = 0; i < debris.Count; i++)
            {
                var d = debris[i];
                if (d.State == DebrisState.Falling || d.State == DebrisState.Ingesting)
                {
                    double dist = JsMath.Hypot(bh.X - d.X, bh.Y - d.Y);
                    if (dist < closest) closest = dist;
                }
            }

            if (ChewTimer > 0)
            {
                ChewTimer -= dt;
                MouthOpenAmount = 0.5 + JsMath.Sin(CosmicTime * 25) * 0.4;
            }
            else if (closest < 120)
            {
                MouthOpenAmount = JsMath.Min(1, MouthOpenAmount + dt * 6);
            }
            else
            {
                MouthOpenAmount = JsMath.Max(0, MouthOpenAmount - dt * 3);
            }
        }

        void RenderCartoonSky(Canvas2D ctx, CosmeticItem skyboxSkin)
        {
            string key = skyboxSkin != null ? skyboxSkin.Id : "";
            if (skyGrad == null || skyGradKey != key || skyGradW != Width || skyGradH != Height)
            {
                skyGrad = ctx.CreateLinearGradient(0, 0, 0, Height);
                skyGrad.AddColorStop(0, GameDraw.SkyStop(skyboxSkin, 0, "#1e1b4b"));
                skyGrad.AddColorStop(0.5, GameDraw.SkyStop(skyboxSkin, 1, "#312e81"));
                skyGrad.AddColorStop(1, GameDraw.SkyStop(skyboxSkin, 2, "#4338ca"));
                skyGradKey = key;
                skyGradW = Width;
                skyGradH = Height;
            }
            ctx.FillStyle = skyGrad;
            ctx.FillRect(0, 0, Width, Height);

            for (int i = 0; i < Planets.Count; i++)
            {
                var p = Planets[i];
                double px = p.X * Width;
                double py = p.Y * Height;
                ctx.Save();
                ctx.FillStyle = p.Color;
                ctx.BeginPath();
                ctx.Arc(px, py, p.Radius, 0, JsMath.PI * 2);
                ctx.Fill();
                ctx.FillStyle = "rgba(255, 255, 255, 0.4)";
                ctx.BeginPath();
                ctx.Arc(px - p.Radius * 0.35, py - p.Radius * 0.35, p.Radius * 0.35, 0, JsMath.PI * 2);
                ctx.Fill();
                if (p.Ring)
                {
                    ctx.StrokeStyle = p.RingColor;
                    ctx.LineWidth = 4;
                    ctx.BeginPath();
                    ctx.Ellipse(px, py, p.Radius * 1.7, p.Radius * 0.45, -0.3, 0, JsMath.PI * 2);
                    ctx.Stroke();
                }
                ctx.Restore();
            }

            for (int i = 0; i < Stars.Count; i++)
            {
                var s = Stars[i];
                double sx = s.X * Width;
                double sy = s.Y * Height;
                double pulse = 1 + JsMath.Sin(CosmicTime * s.Speed + s.X * 10) * 0.25;
                double size = s.Size * pulse;
                ctx.Save();
                ctx.FillStyle = s.Color;
                ctx.Translate(sx, sy);
                if (s.Type == "star")
                {
                    ctx.BeginPath();
                    ctx.MoveTo(0, -size);
                    ctx.QuadraticCurveTo(0, 0, size, 0);
                    ctx.QuadraticCurveTo(0, 0, 0, size);
                    ctx.QuadraticCurveTo(0, 0, -size, 0);
                    ctx.QuadraticCurveTo(0, 0, 0, -size);
                    ctx.Fill();
                }
                else
                {
                    ctx.BeginPath();
                    ctx.Arc(0, 0, size * 0.5, 0, JsMath.PI * 2);
                    ctx.Fill();
                }
                ctx.Restore();
            }
        }
    }
}
