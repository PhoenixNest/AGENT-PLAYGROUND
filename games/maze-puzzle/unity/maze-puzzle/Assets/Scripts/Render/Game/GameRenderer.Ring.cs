using System.Collections.Generic;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Logic;

namespace OrbitalSalvage.Render
{
    public sealed partial class GameRenderer
    {
        struct StreakTrack
        {
            public double R, Width, Arc;
            public int Count;
            public string Color;
        }

        void RenderCartoonRing(Canvas2D ctx, PhysicsEngine physics, CosmeticItem ringSkin)
        {
            double cx = physics.Center.X;
            double cy = physics.Center.Y;
            double rOut = physics.RingRadius;
            double rIn = physics.InnerRadius;
            double angle = physics.RingAngle;
            double apAngle = physics.ApertureAngle;
            string color = ringSkin != null ? ringSkin.Color : "#f472b6";
            string accent = ringSkin != null ? ringSkin.Accent : "#fde047";
            string id = ringSkin != null ? ringSkin.Id : null;

            ctx.Save();
            ctx.Translate(cx, cy);
            RenderRotationBoostVFX(ctx, physics, rOut, rIn, angle, ringSkin);
            ctx.Rotate(angle);

            double apStart = JsMath.PI * 0.5 + apAngle * 0.5;
            double apEnd = JsMath.PI * 0.5 - apAngle * 0.5;

            ctx.LineWidth = 18;
            ctx.StrokeStyle = "rgba(15, 23, 42, 0.45)";
            ctx.BeginPath();
            ctx.Arc(0, 4, rOut, apStart, apEnd);
            ctx.Stroke();

            ctx.LineWidth = 14;
            ctx.StrokeStyle = color;
            if (physics.RcsThrusting)
            {
                ctx.ShadowBlur = 18;
                ctx.ShadowColor = "#38bdf8";
            }
            ctx.LineCap = "round";
            ctx.BeginPath();
            ctx.Arc(0, 0, rOut, apStart, apEnd);
            ctx.Stroke();
            ctx.ShadowBlur = 0;

            ctx.LineWidth = 3;
            ctx.StrokeStyle = "#1e1b4b";
            ctx.BeginPath();
            ctx.Arc(0, 0, rOut + 7, apStart, apEnd);
            ctx.Stroke();
            ctx.BeginPath();
            ctx.Arc(0, 0, rOut - 7, apStart, apEnd);
            ctx.Stroke();

            ctx.LineWidth = 3;
            ctx.StrokeStyle = "rgba(255, 255, 255, 0.65)";
            ctx.BeginPath();
            ctx.Arc(0, 0, rOut - 4, JsMath.PI * 1.1, JsMath.PI * 1.9);
            ctx.Stroke();

            for (int i = 0; i < 10; i++)
            {
                double a = (JsMath.PI * 2 * i) / 10;
                if (a > apEnd && a < apStart) continue;
                double bx = JsMath.Cos(a) * (rOut - 1);
                double by = JsMath.Sin(a) * (rOut - 1);
                ctx.FillStyle = accent;
                ctx.BeginPath();
                ctx.Arc(bx, by, 3.5, 0, JsMath.PI * 2);
                ctx.Fill();
            }

            RenderCartoonChevrons(ctx, rOut);

            ctx.Save();
            if (physics.ConcentricRings) ctx.Rotate(physics.InnerRingAngle - physics.RingAngle);
            ctx.LineWidth = 10;
            ctx.StrokeStyle = "#1e1b4b";
            ctx.FillStyle = color;
            ctx.BeginPath();
            ctx.Arc(0, 0, rIn, 0, JsMath.PI * 2);
            ctx.Fill();
            ctx.Stroke();
            ctx.FillStyle = "#ffffff";
            ctx.BeginPath();
            ctx.Arc(0, 0, rIn - 8, 0, JsMath.PI * 2);
            ctx.Fill();
            ctx.FillStyle = "#1e1b4b";
            ctx.Font = "bold 13px Fredoka, sans-serif";
            ctx.TextAlign = "center";
            ctx.TextBaseline = "middle";
            string hubEmoji;
            string hubSubtitle;
            _labels.Hub(id, physics.ConcentricRings, out hubEmoji, out hubSubtitle);
            ctx.FillText(hubEmoji, 0, -7);
            ctx.Font = "11px Nunito, sans-serif";
            ctx.FillStyle = "#64748b";
            ctx.FillText(hubSubtitle, 0, 9);
            ctx.Restore();

            if (physics.TrackWalls != null && physics.TrackWalls.Count > 0)
            {
                for (int w = 0; w < physics.TrackWalls.Count; w++)
                {
                    var tw = physics.TrackWalls[w];
                    double gapHalf = GameDraw.OrNum(tw.GapSize, 0.45) * 0.5;
                    double gapAng = tw.GapAngle;
                    double startA = gapAng + gapHalf;
                    double endA = gapAng - gapHalf + JsMath.PI * 2;
                    ctx.LineWidth = 8;
                    ctx.StrokeStyle = "#1e1b4b";
                    ctx.BeginPath();
                    ctx.Arc(0, 0, tw.R, startA, endA);
                    ctx.Stroke();
                    ctx.LineWidth = 4;
                    ctx.StrokeStyle = accent;
                    ctx.BeginPath();
                    ctx.Arc(0, 0, tw.R, startA, endA);
                    ctx.Stroke();
                    if (physics.ConcentricRings)
                    {
                        const int numCogs = 24;
                        double cogStep = (JsMath.PI * 2) / numCogs;
                        double cogRot = (physics.InnerRingAngle - physics.RingAngle) * 0.6;
                        ctx.FillStyle = "#facc15";
                        ctx.StrokeStyle = "#1e1b4b";
                        ctx.LineWidth = 1.5;
                        for (int i = 0; i < numCogs; i++)
                        {
                            double cogAng = i * cogStep + cogRot;
                            double diff = cogAng - gapAng;
                            while (diff > JsMath.PI) diff -= JsMath.PI * 2;
                            while (diff < -JsMath.PI) diff += JsMath.PI * 2;
                            if (JsMath.Abs(diff) < gapHalf + 0.1) continue;
                            double cx1 = JsMath.Cos(cogAng) * (tw.R - 4);
                            double cy1 = JsMath.Sin(cogAng) * (tw.R - 4);
                            ctx.BeginPath();
                            ctx.Arc(cx1, cy1, 2.5, 0, JsMath.PI * 2);
                            ctx.Fill();
                            ctx.Stroke();
                        }
                    }
                    double[] offs = { -gapHalf, gapHalf };
                    for (int o = 0; o < offs.Length; o++)
                    {
                        double capAng = gapAng + offs[o];
                        ctx.FillStyle = "#facc15";
                        ctx.StrokeStyle = "#1e1b4b";
                        ctx.LineWidth = 2;
                        ctx.BeginPath();
                        ctx.Arc(JsMath.Cos(capAng) * tw.R, JsMath.Sin(capAng) * tw.R, 4.5, 0, JsMath.PI * 2);
                        ctx.Fill();
                        ctx.Stroke();
                    }
                }
            }

            ctx.LineWidth = 7;
            ctx.LineCap = "round";
            var dividers = physics.Dividers;
            for (int i = 0; i < dividers.Count; i++)
            {
                var div = dividers[i];
                double rot = (div.IsInner && physics.ConcentricRings) ? (physics.InnerRingAngle - physics.RingAngle) : 0;
                double finalAngle = div.Angle + rot;
                double dx1 = JsMath.Cos(finalAngle) * div.RMin;
                double dy1 = JsMath.Sin(finalAngle) * div.RMin;
                double dx2 = JsMath.Cos(finalAngle) * div.RMax;
                double dy2 = JsMath.Sin(finalAngle) * div.RMax;
                ctx.StrokeStyle = "#1e1b4b";
                ctx.BeginPath();
                ctx.MoveTo(dx1, dy1);
                ctx.LineTo(dx2, dy2);
                ctx.Stroke();
                ctx.LineWidth = 3.5;
                ctx.StrokeStyle = accent;
                ctx.BeginPath();
                ctx.MoveTo(dx1, dy1);
                ctx.LineTo(dx2, dy2);
                ctx.Stroke();
                ctx.LineWidth = 7;
            }
            ctx.Restore();
        }

        void RenderRotationBoostVFX(Canvas2D ctx, PhysicsEngine physics, double rOut, double rIn, double angle, CosmeticItem ringSkin)
        {
            double absSpeed = JsMath.Abs(physics.AngularVelocity);
            bool isBoosting = physics.RcsThrusting;
            if (!(absSpeed > 0.15 || isBoosting)) return;
            double signed = physics.AngularVelocity;
            if (signed == 0 || double.IsNaN(signed)) signed = physics.RcsDirection;
            if (signed == 0 || double.IsNaN(signed)) signed = 1;
            double dir = JsMath.Sign(signed);
            double speedRatio = JsMath.Min(1, absSpeed / 5.0);
            double intensity = isBoosting ? JsMath.Max(0.9, speedRatio) : speedRatio;

            var tracks = new StreakTrack[]
            {
                new StreakTrack { R = rOut + 14, Color = "#38bdf8", Width = 5.5, Arc = 0.55 + intensity * 0.65, Count = 3 },
                new StreakTrack { R = rOut + 8, Color = "#fde047", Width = 4.0, Arc = 0.45 + intensity * 0.55, Count = 4 },
                new StreakTrack { R = rIn - 8, Color = "#f472b6", Width = 3.5, Arc = 0.40 + intensity * 0.50, Count = 3 }
            };

            ctx.Save();
            for (int t = 0; t < tracks.Length; t++)
            {
                var track = tracks[t];
                for (int i = 0; i < track.Count; i++)
                {
                    double baseOffset = (JsMath.PI * 2 * i) / track.Count + (angle * 0.35);
                    double headAngle = baseOffset;
                    double tailAngle = headAngle - track.Arc * dir;
                    ctx.Save();
                    ctx.StrokeStyle = track.Color;
                    GameDraw.Alpha(ctx, 0.35 + intensity * 0.55);
                    ctx.LineWidth = track.Width * (0.7 + intensity * 0.5);
                    ctx.LineCap = "round";
                    ctx.BeginPath();
                    if (dir > 0) ctx.Arc(0, 0, track.R, tailAngle, headAngle);
                    else ctx.Arc(0, 0, track.R, headAngle, tailAngle);
                    ctx.Stroke();
                    ctx.StrokeStyle = "#ffffff";
                    ctx.LineWidth = track.Width * 0.38;
                    GameDraw.Alpha(ctx, 0.65 + intensity * 0.35);
                    ctx.BeginPath();
                    if (dir > 0) ctx.Arc(0, 0, track.R, tailAngle + track.Arc * 0.35, headAngle);
                    else ctx.Arc(0, 0, track.R, headAngle, tailAngle - track.Arc * 0.35);
                    ctx.Stroke();
                    for (int d = 1; d <= 2; d++)
                    {
                        double dotAngle = dir > 0 ? tailAngle - d * 0.08 : tailAngle + d * 0.08;
                        ctx.FillStyle = track.Color;
                        GameDraw.Alpha(ctx, 0.5 - d * 0.15);
                        ctx.BeginPath();
                        ctx.Arc(JsMath.Cos(dotAngle) * track.R, JsMath.Sin(dotAngle) * track.R, track.Width * 0.45, 0, JsMath.PI * 2);
                        ctx.Fill();
                    }
                    ctx.Restore();
                }
            }

            if (absSpeed > 2.0 || isBoosting)
            {
                SparkEmitTimer = SparkEmitTimer + 1;
                if (SparkEmitTimer % 2 == 0)
                {
                    double emitAngle = GameDraw.Rand() * JsMath.PI * 2;
                    double ex = JsMath.Cos(emitAngle) * (rOut + 4);
                    double ey = JsMath.Sin(emitAngle) * (rOut + 4);
                    double tanAngle = emitAngle + JsMath.PI * 0.5 * dir;
                    double sparkSpeed = (120 + GameDraw.Rand() * 80) * intensity;
                    double vx = JsMath.Cos(tanAngle) * sparkSpeed;
                    double vy = JsMath.Sin(tanAngle) * sparkSpeed;
                    string col = SparkColors[(int)JsMath.Floor(GameDraw.Rand() * SparkColors.Length)];
                    AddRimSpark(physics.Center.X + ex, physics.Center.Y + ey, vx, vy, col);
                }
            }

            double plumeA = dir > 0 ? -1 : 1;
            double plumeB = dir > 0 ? 1 : -1;
            double[] tx = { -rOut, rOut };
            double[] plume = { plumeA, plumeB };
            for (int i = 0; i < 2; i++)
            {
                double flameLength = (16 + intensity * 28) * (isBoosting ? 1.5 : 1.0);
                double flameWidth = (8 + intensity * 8) * (isBoosting ? 1.4 : 1.0);
                ctx.Save();
                ctx.Translate(tx[i], 0);
                ctx.FillStyle = "#1e1b4b";
                ctx.StrokeStyle = "#38bdf8";
                ctx.LineWidth = 2;
                ctx.BeginPath();
                ctx.RoundRect(-5, -6, 10, 12, 3);
                ctx.Fill();
                ctx.Stroke();
                ctx.FillStyle = isBoosting ? "#f59e0b" : "#38bdf8";
                ctx.StrokeStyle = "#1e1b4b";
                ctx.LineWidth = 1.8;
                ctx.BeginPath();
                ctx.MoveTo(-flameWidth * 0.5, 0);
                ctx.LineTo(flameWidth * 0.5, 0);
                ctx.LineTo(0, plume[i] * flameLength);
                ctx.ClosePath();
                ctx.Fill();
                ctx.Stroke();
                ctx.FillStyle = "#ffffff";
                ctx.BeginPath();
                ctx.MoveTo(-flameWidth * 0.25, 0);
                ctx.LineTo(flameWidth * 0.25, 0);
                ctx.LineTo(0, plume[i] * flameLength * 0.65);
                ctx.ClosePath();
                ctx.Fill();
                if (isBoosting && GameDraw.Rand() > 0.4)
                {
                    AddThrusterPuff(
                        physics.Center.X + tx[i],
                        physics.Center.Y + plume[i] * (flameLength * 0.8),
                        (GameDraw.Rand() - 0.5) * 20,
                        plume[i] * 50,
                        5 + GameDraw.Rand() * 4,
                        "#fef08a");
                }
                ctx.Restore();
            }
            ctx.Restore();
        }

        void RenderCartoonChevrons(Canvas2D ctx, double rOut)
        {
            ctx.Save();
            double[] offsets = { -0.07, 0.07 };
            for (int i = 0; i < offsets.Length; i++)
            {
                double ang = JsMath.PI * 0.5 + offsets[i];
                double x = JsMath.Cos(ang) * (rOut + 4);
                double y = JsMath.Sin(ang) * (rOut + 4);
                ctx.FillStyle = "#facc15";
                ctx.StrokeStyle = "#1e1b4b";
                ctx.LineWidth = 2;
                ctx.BeginPath();
                ctx.Arc(x, y, 4.5, 0, JsMath.PI * 2);
                ctx.Fill();
                ctx.Stroke();
            }
            ctx.Restore();
        }

        void RenderSpinningGear(Canvas2D ctx, double x, double y, double radius, double rot, string color)
        {
            ctx.Save();
            ctx.Translate(x, y);
            ctx.Rotate(rot);
            const int numTeeth = 6;
            ctx.FillStyle = color;
            ctx.StrokeStyle = "#1e1b4b";
            ctx.LineWidth = 1.5;
            for (int i = 0; i < numTeeth; i++)
            {
                double a = (i * JsMath.PI * 2) / numTeeth;
                ctx.Save();
                ctx.Rotate(a);
                ctx.BeginPath();
                ctx.Rect(-radius * 0.35, -radius * 1.3, radius * 0.7, radius * 0.55);
                ctx.Fill();
                ctx.Stroke();
                ctx.Restore();
            }
            ctx.BeginPath();
            ctx.Arc(0, 0, radius, 0, JsMath.PI * 2);
            ctx.Fill();
            ctx.Stroke();
            ctx.FillStyle = "#ffffff";
            ctx.BeginPath();
            ctx.Arc(0, 0, radius * 0.38, 0, JsMath.PI * 2);
            ctx.Fill();
            ctx.StrokeStyle = "#1e1b4b";
            ctx.LineWidth = 1.2;
            ctx.Stroke();
            ctx.Restore();
        }
    }
}
