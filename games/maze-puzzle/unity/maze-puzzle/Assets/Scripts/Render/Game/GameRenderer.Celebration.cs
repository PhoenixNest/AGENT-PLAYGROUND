using System;
using OrbitalSalvage.JsCompat;

namespace OrbitalSalvage.Render
{
    public enum CelebrationConfettiShape
    {
        Strip,
        Star,
        Disc
    }

    public struct CelebrationConfettiPiece
    {
        public bool Active;
        public double X, Y;
        public double Vx, Vy;
        public CelebrationConfettiShape Shape;
        public double W, H;
        public string Color;
        public double Rot, VRot;
        public double Pitch, VPitch;
        public double Drag;
        public double Gravity;
        public double FlutterFreq;
        public double FlutterAmp;
        public double FlutterPhase;
        public double Life, MaxLife;
    }

    public struct RocketTrailSpark
    {
        public double X, Y;
        public double Alpha;
        public double Size;
    }

    public struct CelebrationRocket
    {
        public bool Active;
        public double X, Y;
        public double Vx, Vy;
        public double TargetY;
        public string Color;
        public double Life;
        public RocketTrailSpark[] Trail;
        public int TrailCount;
    }

    public struct CelebrationSpark
    {
        public bool Active;
        public double X, Y;
        public double PrevX, PrevY;
        public double Vx, Vy;
        public double Gravity;
        public double Drag;
        public string Color;
        public double Size;
        public double Life, MaxLife;
        public double Rot, VRot;
        public double TwinkleRate;
        public bool IsStar;
    }

    public sealed partial class GameRenderer
    {
        public static readonly string[] CelebrationConfettiColors =
        {
            "#facc15", // Metallic Gold
            "#fef08a", // Champagne Shimmer
            "#f472b6", // Rose Gold / Romantic Pink
            "#fda4af", // Pastel Blush
            "#ffffff", // Holographic Pearl / Silver
            "#38bdf8", // Tiffany Sky Cyan
            "#c084fc", // Electric Violet
            "#4ade80", // Spring Emerald
            "#fb923c"  // Warm Coral
        };

        public static readonly string[] CelebrationFireworkColors =
        {
            "#f472b6", // Cotton Candy Pink
            "#fde047", // Star Yellow
            "#38bdf8", // Cyan Spark
            "#4ade80", // Alien Emerald
            "#c084fc", // Galaxy Violet
            "#fb923c", // Tangerine Spark
            "#ffffff"  // Starlight White
        };

        public readonly CelebrationConfettiPiece[] CelebrationConfetti = new CelebrationConfettiPiece[160];
        public readonly CelebrationRocket[] CelebrationRockets = new CelebrationRocket[8];
        public readonly CelebrationSpark[] CelebrationSparks = new CelebrationSpark[256];

        public Action<double, double, string> OnFireworkExploded;

        public bool HasActiveCelebration
        {
            get
            {
                for (int i = 0; i < CelebrationRockets.Length; i++)
                    if (CelebrationRockets[i].Active) return true;
                for (int i = 0; i < CelebrationSparks.Length; i++)
                    if (CelebrationSparks[i].Active) return true;
                for (int i = 0; i < CelebrationConfetti.Length; i++)
                    if (CelebrationConfetti[i].Active) return true;
                return false;
            }
        }

        public void ResetCelebrationVfx()
        {
            for (int i = 0; i < CelebrationRockets.Length; i++)
                CelebrationRockets[i].Active = false;
            for (int i = 0; i < CelebrationSparks.Length; i++)
                CelebrationSparks[i].Active = false;
            for (int i = 0; i < CelebrationConfetti.Length; i++)
                CelebrationConfetti[i].Active = false;
        }

        int AllocConfettiIndex()
        {
            for (int i = 0; i < CelebrationConfetti.Length; i++)
                if (!CelebrationConfetti[i].Active) return i;
            return -1;
        }

        int AllocRocketIndex()
        {
            for (int i = 0; i < CelebrationRockets.Length; i++)
                if (!CelebrationRockets[i].Active) return i;
            return -1;
        }

        int AllocSparkIndex()
        {
            for (int i = 0; i < CelebrationSparks.Length; i++)
                if (!CelebrationSparks[i].Active) return i;
            return -1;
        }

        /// <summary>
        /// Launch twin Celebration Confetti Cannons from lower-left and lower-right
        /// projecting dense fountains of tumbling metallic foil ribbons, stars, and sequins
        /// toward the center arena.
        /// </summary>
        public void LaunchCelebrationCannons(double leftX, double leftY, double rightX, double rightY, int countPerSide = 38)
        {
            // 1. Muzzle Blast Rings & Flash at Cannon Mouths
            AddPopRing(leftX, leftY, 52, "#ffffff");
            AddPopRing(leftX, leftY, 32, "#facc15");
            AddPopRing(rightX, rightY, 52, "#ffffff");
            AddPopRing(rightX, rightY, 32, "#38bdf8");

            // 2. Left Cannon (Conical ejection upward-inward at -63° ± 16°)
            for (int i = 0; i < countPerSide; i++)
            {
                int slot = AllocConfettiIndex();
                if (slot < 0) break;

                double spread = (JsRandom.Global.NextDouble() - 0.5) * 0.35;
                double angle = -JsMath.PI * 0.35 + spread;
                double speed = 820.0 + JsRandom.Global.NextDouble() * 380.0;
                string color = CelebrationConfettiColors[i % CelebrationConfettiColors.Length];

                double randType = JsRandom.Global.NextDouble();
                CelebrationConfettiShape shape = randType < 0.6 ? CelebrationConfettiShape.Strip : randType < 0.8 ? CelebrationConfettiShape.Star : CelebrationConfettiShape.Disc;
                double w = shape == CelebrationConfettiShape.Strip ? JsRandom.Global.NextDouble() * 2.5 + 4.5 : JsRandom.Global.NextDouble() * 3.0 + 5.0;
                double h = shape == CelebrationConfettiShape.Strip ? JsRandom.Global.NextDouble() * 9.0 + 14.0 : w;

                CelebrationConfetti[slot] = new CelebrationConfettiPiece
                {
                    Active = true,
                    X = leftX,
                    Y = leftY,
                    Vx = JsMath.Cos(angle) * speed,
                    Vy = JsMath.Sin(angle) * speed,
                    Shape = shape,
                    W = w,
                    H = h,
                    Color = color,
                    Rot = JsRandom.Global.NextDouble() * JsMath.PI * 2.0,
                    VRot = (JsRandom.Global.NextDouble() - 0.5) * 14.0,
                    Pitch = JsRandom.Global.NextDouble() * JsMath.PI * 2.0,
                    VPitch = (JsRandom.Global.NextDouble() - 0.5) * 16.0,
                    Drag = 0.965 + JsRandom.Global.NextDouble() * 0.012,
                    Gravity = 105.0 + JsRandom.Global.NextDouble() * 45.0,
                    FlutterFreq = 7.0 + JsRandom.Global.NextDouble() * 5.0,
                    FlutterAmp = 30.0 + JsRandom.Global.NextDouble() * 25.0,
                    FlutterPhase = JsRandom.Global.NextDouble() * JsMath.PI * 2.0,
                    Life = 2.4 + JsRandom.Global.NextDouble() * 0.5,
                    MaxLife = 2.9
                };
            }

            // 3. Right Cannon (Conical ejection upward-inward at -117° ± 16°)
            for (int i = 0; i < countPerSide; i++)
            {
                int slot = AllocConfettiIndex();
                if (slot < 0) break;

                double spread = (JsRandom.Global.NextDouble() - 0.5) * 0.35;
                double angle = -JsMath.PI * 0.65 + spread;
                double speed = 820.0 + JsRandom.Global.NextDouble() * 380.0;
                string color = CelebrationConfettiColors[(i + 4) % CelebrationConfettiColors.Length];

                double randType = JsRandom.Global.NextDouble();
                CelebrationConfettiShape shape = randType < 0.6 ? CelebrationConfettiShape.Strip : randType < 0.8 ? CelebrationConfettiShape.Star : CelebrationConfettiShape.Disc;
                double w = shape == CelebrationConfettiShape.Strip ? JsRandom.Global.NextDouble() * 2.5 + 4.5 : JsRandom.Global.NextDouble() * 3.0 + 5.0;
                double h = shape == CelebrationConfettiShape.Strip ? JsRandom.Global.NextDouble() * 9.0 + 14.0 : w;

                CelebrationConfetti[slot] = new CelebrationConfettiPiece
                {
                    Active = true,
                    X = rightX,
                    Y = rightY,
                    Vx = JsMath.Cos(angle) * speed,
                    Vy = JsMath.Sin(angle) * speed,
                    Shape = shape,
                    W = w,
                    H = h,
                    Color = color,
                    Rot = JsRandom.Global.NextDouble() * JsMath.PI * 2.0,
                    VRot = (JsRandom.Global.NextDouble() - 0.5) * 14.0,
                    Pitch = JsRandom.Global.NextDouble() * JsMath.PI * 2.0,
                    VPitch = (JsRandom.Global.NextDouble() - 0.5) * 16.0,
                    Drag = 0.965 + JsRandom.Global.NextDouble() * 0.012,
                    Gravity = 105.0 + JsRandom.Global.NextDouble() * 45.0,
                    FlutterFreq = 7.0 + JsRandom.Global.NextDouble() * 5.0,
                    FlutterAmp = 30.0 + JsRandom.Global.NextDouble() * 25.0,
                    FlutterPhase = JsRandom.Global.NextDouble() * JsMath.PI * 2.0,
                    Life = 2.4 + JsRandom.Global.NextDouble() * 0.5,
                    MaxLife = 2.9
                };
            }
        }

        /// <summary>
        /// Launch a mini celebration confetti burst towards a target point (e.g. on user tap).
        /// </summary>
        public void LaunchSingleCelebrationConfetti(double targetX, double targetY)
        {
            bool fromLeft = JsRandom.Global.NextDouble() < 0.5;
            double startX = fromLeft ? 20.0 : Width - 20.0;
            double startY = Height - 40.0;
            double dx = targetX - startX;
            double dy = targetY - startY;
            double baseAngle = JsMath.Atan2(dy, dx);
            double dist = JsMath.Hypot(dx, dy);

            AddPopRing(startX, startY, 34, "#ffffff");

            for (int i = 0; i < 10; i++)
            {
                int slot = AllocConfettiIndex();
                if (slot < 0) break;

                double angle = baseAngle + (JsRandom.Global.NextDouble() - 0.5) * 0.35;
                double speed = JsMath.Max(420.0, JsMath.Min(800.0, dist * 1.6)) + (JsRandom.Global.NextDouble() - 0.5) * 120.0;
                string color = CelebrationConfettiColors[(int)JsMath.Floor(JsRandom.Global.NextDouble() * CelebrationConfettiColors.Length)];
                double randType = JsRandom.Global.NextDouble();
                CelebrationConfettiShape shape = randType < 0.6 ? CelebrationConfettiShape.Strip : randType < 0.8 ? CelebrationConfettiShape.Star : CelebrationConfettiShape.Disc;
                double w = shape == CelebrationConfettiShape.Strip ? JsRandom.Global.NextDouble() * 2.2 + 4.0 : JsRandom.Global.NextDouble() * 2.5 + 4.5;
                double h = shape == CelebrationConfettiShape.Strip ? JsRandom.Global.NextDouble() * 6.0 + 10.0 : w;

                CelebrationConfetti[slot] = new CelebrationConfettiPiece
                {
                    Active = true,
                    X = startX,
                    Y = startY,
                    Vx = JsMath.Cos(angle) * speed,
                    Vy = JsMath.Sin(angle) * speed,
                    Shape = shape,
                    W = w,
                    H = h,
                    Color = color,
                    Rot = JsRandom.Global.NextDouble() * JsMath.PI * 2.0,
                    VRot = (JsRandom.Global.NextDouble() - 0.5) * 14.0,
                    Pitch = JsRandom.Global.NextDouble() * JsMath.PI * 2.0,
                    VPitch = (JsRandom.Global.NextDouble() - 0.5) * 16.0,
                    Drag = 0.954,
                    Gravity = 85.0,
                    FlutterFreq = 8.0 + JsRandom.Global.NextDouble() * 5.0,
                    FlutterAmp = 20.0 + JsRandom.Global.NextDouble() * 15.0,
                    FlutterPhase = JsRandom.Global.NextDouble() * JsMath.PI * 2.0,
                    Life = 1.9 + JsRandom.Global.NextDouble() * 0.4,
                    MaxLife = 2.3
                };
            }
        }

        /// <summary>
        /// Launch an ascending firework rocket with fizzing particle spark trail.
        /// </summary>
        public void LaunchFirework(double startX, double startY, double targetY, string color = null)
        {
            int slot = AllocRocketIndex();
            if (slot < 0) return;

            string burstColor = !string.IsNullOrEmpty(color)
                ? color
                : CelebrationFireworkColors[(int)JsMath.Floor(JsRandom.Global.NextDouble() * CelebrationFireworkColors.Length)];
            double distY = JsMath.Max(80.0, JsMath.Abs(startY - targetY));
            double speed = JsMath.Sqrt(2.0 * 460.0 * distY);

            if (CelebrationRockets[slot].Trail == null || CelebrationRockets[slot].Trail.Length < 16)
                CelebrationRockets[slot].Trail = new RocketTrailSpark[16];

            CelebrationRockets[slot].Active = true;
            CelebrationRockets[slot].X = startX;
            CelebrationRockets[slot].Y = startY;
            CelebrationRockets[slot].Vx = (JsRandom.Global.NextDouble() - 0.5) * 60.0;
            CelebrationRockets[slot].Vy = -JsMath.Max(260.0, JsMath.Min(520.0, speed));
            CelebrationRockets[slot].TargetY = targetY;
            CelebrationRockets[slot].Color = burstColor;
            CelebrationRockets[slot].Life = 1.2;
            CelebrationRockets[slot].TrailCount = 0;
        }

        /// <summary>
        /// Explode a cosmic starburst firework (radial sparks + sparkling 4-point stars + shockwave ring).
        /// </summary>
        public void ExplodeFirework(double x, double y, string color = null, int count = 24)
        {
            string burstColor = !string.IsNullOrEmpty(color)
                ? color
                : CelebrationFireworkColors[(int)JsMath.Floor(JsRandom.Global.NextDouble() * CelebrationFireworkColors.Length)];

            // 1. Dual Holographic Expanding Shockwaves
            AddPopRing(x, y, 75, burstColor);
            AddPopRing(x, y, 38, "#ffffff");

            // 2. Radial Glowing Sparks & Sparkle Stars
            for (int i = 0; i < count; i++)
            {
                int slot = AllocSparkIndex();
                if (slot < 0) break;

                double angle = ((double)i / count) * JsMath.PI * 2.0 + (JsRandom.Global.NextDouble() - 0.5) * 0.28;
                double speed = JsRandom.Global.NextDouble() * 160.0 + 80.0;
                bool isStar = i % 4 == 0;

                CelebrationSparks[slot] = new CelebrationSpark
                {
                    Active = true,
                    X = x,
                    Y = y,
                    PrevX = x,
                    PrevY = y,
                    Vx = JsMath.Cos(angle) * speed,
                    Vy = JsMath.Sin(angle) * speed,
                    Gravity = isStar ? 50.0 : 85.0,
                    Drag = 0.965,
                    Color = JsRandom.Global.NextDouble() < 0.2 ? "#ffffff" : burstColor,
                    Size = isStar ? JsRandom.Global.NextDouble() * 6.0 + 7.0 : JsRandom.Global.NextDouble() * 2.8 + 2.5,
                    Life = JsRandom.Global.NextDouble() * 0.4 + (isStar ? 0.95 : 0.75),
                    MaxLife = isStar ? 1.35 : 1.15,
                    Rot = JsRandom.Global.NextDouble() * JsMath.PI * 2.0,
                    VRot = (JsRandom.Global.NextDouble() - 0.5) * 8.0,
                    TwinkleRate = JsRandom.Global.NextDouble() * 14.0 + 10.0,
                    IsStar = isStar
                };
            }

            if (OnFireworkExploded != null)
                OnFireworkExploded(x, y, burstColor);
        }

        public void DrawSparkleStar(Canvas2D ctx, double x, double y, double size)
        {
            double rOuter = size;
            double rInner = size * 0.28;
            ctx.BeginPath();
            for (int i = 0; i < 8; i++)
            {
                double r = i % 2 == 0 ? rOuter : rInner;
                double ang = (i * JsMath.PI) / 4.0;
                double px = x + JsMath.Cos(ang) * r;
                double py = y + JsMath.Sin(ang) * r;
                if (i == 0) ctx.MoveTo(px, py);
                else ctx.LineTo(px, py);
            }
            ctx.ClosePath();
            ctx.Fill();
            ctx.StrokeStyle = "#1e1b4b";
            ctx.LineWidth = 1.6;
            ctx.Stroke();
        }

        public void RenderCelebrationVfx(Canvas2D ctx, double dt)
        {
            // 0a. Active Firework Rockets (Ascending with sparkling fizz trail)
            for (int i = 0; i < CelebrationRockets.Length; i++)
            {
                if (!CelebrationRockets[i].Active) continue;

                ref var r = ref CelebrationRockets[i];
                r.Life -= dt;
                r.Vy += 180.0 * dt;
                r.X += r.Vx * dt;
                r.Y += r.Vy * dt;

                // Push fizz trail spark
                if (r.TrailCount < r.Trail.Length)
                {
                    r.Trail[r.TrailCount++] = new RocketTrailSpark
                    {
                        X = r.X + (JsRandom.Global.NextDouble() - 0.5) * 6.0,
                        Y = r.Y + 6.0,
                        Alpha = 0.95,
                        Size = JsRandom.Global.NextDouble() * 3.5 + 2.0
                    };
                }
                else
                {
                    // Shift trail
                    for (int t = 0; t < r.Trail.Length - 1; t++)
                        r.Trail[t] = r.Trail[t + 1];
                    r.Trail[r.Trail.Length - 1] = new RocketTrailSpark
                    {
                        X = r.X + (JsRandom.Global.NextDouble() - 0.5) * 6.0,
                        Y = r.Y + 6.0,
                        Alpha = 0.95,
                        Size = JsRandom.Global.NextDouble() * 3.5 + 2.0
                    };
                }

                // Render fizzing trail
                ctx.Save();
                for (int t = 0; t < r.TrailCount; t++)
                {
                    r.Trail[t].Alpha -= dt * 2.8;
                    if (r.Trail[t].Alpha > 0)
                    {
                        GameDraw.Alpha(ctx, JsMath.Max(0.0, JsMath.Min(1.0, r.Trail[t].Alpha)));
                        ctx.FillStyle = t % 2 == 0 ? "#fde047" : r.Color;
                        ctx.BeginPath();
                        ctx.Arc(r.Trail[t].X, r.Trail[t].Y, r.Trail[t].Size, 0, JsMath.PI * 2.0);
                        ctx.Fill();
                    }
                }
                ctx.Restore();

                // Render rocket head
                ctx.Save();
                ctx.FillStyle = r.Color;
                ctx.StrokeStyle = "#1e1b4b";
                ctx.LineWidth = 1.8;
                ctx.BeginPath();
                ctx.Arc(r.X, r.Y, 5.5, 0, JsMath.PI * 2.0);
                ctx.Fill();
                ctx.Stroke();

                ctx.FillStyle = "#ffffff";
                ctx.BeginPath();
                ctx.Arc(r.X, r.Y, 2.8, 0, JsMath.PI * 2.0);
                ctx.Fill();
                ctx.Restore();

                // Detonate at apex or when reaching target height
                if (r.Y <= r.TargetY || r.Vy >= -40.0 || r.Life <= 0)
                {
                    ExplodeFirework(r.X, r.Y, r.Color);
                    r.Active = false;
                }
            }

            // 0b. Active Firework Burst Sparks & Twinkling 4-Point Stars
            for (int i = 0; i < CelebrationSparks.Length; i++)
            {
                if (!CelebrationSparks[i].Active) continue;

                ref var p = ref CelebrationSparks[i];
                p.Life -= dt;
                if (p.Life <= 0)
                {
                    p.Active = false;
                    continue;
                }

                p.PrevX = p.X;
                p.PrevY = p.Y;
                p.Vx *= p.Drag;
                p.Vy = p.Vy * p.Drag + p.Gravity * dt;
                p.X += p.Vx * dt;
                p.Y += p.Vy * dt;
                p.Rot += p.VRot * dt;

                double alphaProgress = JsMath.Max(0.0, p.Life / p.MaxLife);
                double twinkle = 0.8 + 0.2 * JsMath.Sin(p.Life * p.TwinkleRate);
                double finalAlpha = JsMath.Max(0.0, JsMath.Min(1.0, alphaProgress * twinkle));

                ctx.Save();
                GameDraw.Alpha(ctx, finalAlpha);

                if (p.IsStar)
                {
                    ctx.FillStyle = p.Color;
                    ctx.Translate(p.X, p.Y);
                    ctx.Rotate(p.Rot);
                    DrawSparkleStar(ctx, 0, 0, p.Size);
                }
                else
                {
                    // Glowing spark with motion streak tail
                    ctx.StrokeStyle = p.Color;
                    ctx.LineWidth = p.Size;
                    ctx.BeginPath();
                    ctx.MoveTo(p.PrevX, p.PrevY);
                    ctx.LineTo(p.X, p.Y);
                    ctx.Stroke();

                    // White core bead with dark comic outline
                    ctx.FillStyle = "#ffffff";
                    ctx.StrokeStyle = "#1e1b4b";
                    ctx.LineWidth = 1.0;
                    ctx.BeginPath();
                    ctx.Arc(p.X, p.Y, p.Size * 0.6, 0, JsMath.PI * 2.0);
                    ctx.Fill();
                    ctx.Stroke();
                }
                ctx.Restore();
            }

            // 0c. Active Celebration Confetti Cannon Particles (Foil Strips, Shimmer Stars, Metallic Sequins)
            for (int i = 0; i < CelebrationConfetti.Length; i++)
            {
                if (!CelebrationConfetti[i].Active) continue;

                ref var p = ref CelebrationConfetti[i];
                p.Life -= dt;
                if (p.Life <= 0)
                {
                    p.Active = false;
                    continue;
                }

                // Ballistic air-drag physics
                p.Vy += p.Gravity * dt;
                p.Vx *= p.Drag;
                p.Vy *= p.Drag;

                // Gentle aerodynamic fluttering drift
                double flutterOffset = JsMath.Sin(p.Life * p.FlutterFreq + p.FlutterPhase) * p.FlutterAmp * dt;
                p.X += p.Vx * dt + flutterOffset;
                p.Y += p.Vy * dt;

                // 3D Tumbling rotations
                p.Rot += p.VRot * dt;
                p.Pitch += p.VPitch * dt;

                double alpha = JsMath.Min(1.0, p.Life / 0.45);

                ctx.Save();
                GameDraw.Alpha(ctx, JsMath.Max(0.0, JsMath.Min(1.0, alpha)));
                ctx.Translate(p.X, p.Y);
                ctx.Rotate(p.Rot);

                if (p.Shape == CelebrationConfettiShape.Strip)
                {
                    // Tumbling metallic foil strip (3D projection via cos(pitch))
                    double scaleY = JsMath.Cos(p.Pitch);
                    ctx.Scale(1.0, scaleY);

                    ctx.FillStyle = p.Color;
                    ctx.StrokeStyle = "#1e1b4b";
                    ctx.LineWidth = 1.0;
                    ctx.FillRect(-p.W * 0.5, -p.H * 0.5, p.W, p.H);
                    ctx.StrokeRect(-p.W * 0.5, -p.H * 0.5, p.W, p.H);

                    // Subtle specular highlight line down center
                    if (JsMath.Abs(scaleY) > 0.4)
                    {
                        ctx.StrokeStyle = "rgba(255, 255, 255, 0.45)";
                        ctx.LineWidth = 1.0;
                        ctx.BeginPath();
                        ctx.MoveTo(0, -p.H * 0.4);
                        ctx.LineTo(0, p.H * 0.4);
                        ctx.Stroke();
                    }
                }
                else if (p.Shape == CelebrationConfettiShape.Star)
                {
                    // Shimmering 4-point gold/silver star
                    ctx.FillStyle = p.Color;
                    DrawSparkleStar(ctx, 0, 0, p.W);
                }
                else if (p.Shape == CelebrationConfettiShape.Disc)
                {
                    // Flipping circular foil sequin
                    double scaleY = JsMath.Cos(p.Pitch);
                    ctx.Scale(1.0, scaleY);
                    ctx.FillStyle = p.Color;
                    ctx.StrokeStyle = "#1e1b4b";
                    ctx.LineWidth = 0.9;
                    ctx.BeginPath();
                    ctx.Arc(0, 0, p.W, 0, JsMath.PI * 2.0);
                    ctx.Fill();
                    ctx.Stroke();
                }

                ctx.Restore();
            }
        }
    }
}

