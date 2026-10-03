using System.Collections.Generic;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Logic;

namespace OrbitalSalvage.Render
{
    public sealed partial class GameRenderer
    {
        void RenderNibblesBlackHole(Canvas2D ctx, PhysicsEngine physics, CosmeticItem skin)
        {
            var bh = physics.BlackHole;
            double time = CosmicTime * 2.5;
            string halo = GameDraw.Or(skin != null ? skin.HaloColor : null, "#818cf8");
            string core = GameDraw.Or(skin != null ? skin.CoreColor : null, "#0f172a");

            double funnelTopY = JsMath.Min(bh.Y - 120, physics.Center.Y + physics.RingRadius - 15);
            double funnelHeight = bh.Y - funnelTopY;
            const double topHalfW = 95;
            const double bottomHalfW = 46;

            var funnelGrad = ctx.CreateLinearGradient(bh.X, bh.Y, bh.X, funnelTopY);
            funnelGrad.AddColorStop(0, halo + "55");
            funnelGrad.AddColorStop(0.55, halo + "22");
            funnelGrad.AddColorStop(1, "rgba(56, 189, 248, 0.04)");

            ctx.Save();
            ctx.FillStyle = funnelGrad;
            ctx.BeginPath();
            ctx.MoveTo(bh.X - bottomHalfW, bh.Y);
            ctx.LineTo(bh.X - topHalfW, funnelTopY);
            ctx.LineTo(bh.X + topHalfW, funnelTopY);
            ctx.LineTo(bh.X + bottomHalfW, bh.Y);
            ctx.ClosePath();
            ctx.Fill();

            string railColor = GameDraw.Or(skin != null ? skin.HaloColor : null, "#38bdf8");
            ctx.Save();
            ctx.StrokeStyle = railColor;
            ctx.LineWidth = 3.5;
            ctx.LineCap = "round";
            ctx.SetLineDash(Dash12_8);
            ctx.LineDashOffset = -CosmicTime * 55;
            ctx.ShadowColor = railColor;
            ctx.ShadowBlur = 8;
            ctx.BeginPath();
            ctx.MoveTo(bh.X - topHalfW, funnelTopY);
            ctx.LineTo(bh.X - bottomHalfW, bh.Y);
            ctx.Stroke();
            ctx.BeginPath();
            ctx.MoveTo(bh.X + topHalfW, funnelTopY);
            ctx.LineTo(bh.X + bottomHalfW, bh.Y);
            ctx.Stroke();
            ctx.Restore();

            ctx.Save();
            ctx.FillStyle = "#38bdf8";
            ctx.ShadowColor = "#38bdf8";
            ctx.ShadowBlur = 6;
            ctx.BeginPath();
            ctx.Arc(bh.X - topHalfW, funnelTopY, 4, 0, JsMath.PI * 2);
            ctx.Arc(bh.X + topHalfW, funnelTopY, 4, 0, JsMath.PI * 2);
            ctx.Fill();
            ctx.Restore();

            for (int b = 0; b < 4; b++)
            {
                double offset = JsMath.Mod(CosmicTime * 0.75 + b * 0.25, 1.0);
                double bY = funnelTopY + offset * funnelHeight;
                double t = offset;
                double bW = (topHalfW * (1 - t) + bottomHalfW * t) * 1.6;
                double bAlpha = JsMath.Sin(offset * JsMath.PI) * 0.45;
                ctx.Save();
                ctx.StrokeStyle = GameDraw.Rgba(255, 255, 255, bAlpha);
                ctx.LineWidth = 2.4;
                ctx.LineCap = "round";
                ctx.BeginPath();
                ctx.MoveTo(bh.X - bW * 0.5, bY - 6);
                ctx.LineTo(bh.X, bY + 3);
                ctx.LineTo(bh.X + bW * 0.5, bY - 6);
                ctx.Stroke();
                ctx.Restore();
            }
            ctx.Restore();

            ctx.Save();
            ctx.Translate(bh.X, bh.Y);
            ctx.Scale(CurrentBhScale, CurrentBhScale);
            ctx.Translate(-bh.X, -bh.Y);

            double rHorizon = bh.EventHorizonRadius;
            double haloRadius = rHorizon * 2.8 + JsMath.Sin(time * 1.6) * 3;
            ctx.Save();
            ctx.Translate(bh.X, bh.Y);

            var haloGrad = ctx.CreateRadialGradient(0, 0, rHorizon * 0.7, 0, 0, haloRadius);
            haloGrad.AddColorStop(0, halo + "66");
            haloGrad.AddColorStop(0.45, halo + "28");
            haloGrad.AddColorStop(1, "rgba(0, 0, 0, 0)");
            ctx.FillStyle = haloGrad;
            ctx.BeginPath();
            ctx.Arc(0, 0, haloRadius, 0, JsMath.PI * 2);
            ctx.Fill();

            ctx.Save();
            ctx.Rotate(time * 0.4);
            ctx.SetLineDash(Dash14_16);
            ctx.LineCap = "round";
            ctx.LineWidth = 4;
            ctx.StrokeStyle = halo + "aa";
            ctx.BeginPath();
            ctx.Ellipse(0, 0, rHorizon * 2.35, rHorizon * 1.95, 0, 0, JsMath.PI * 2);
            ctx.Stroke();
            ctx.Restore();

            ctx.Save();
            ctx.Rotate(-time * 0.85);
            ctx.SetLineDash(Dash10_14);
            ctx.LineCap = "round";
            ctx.LineWidth = 4.5;
            ctx.StrokeStyle = "#fde047dd";
            ctx.BeginPath();
            ctx.Ellipse(0, 0, rHorizon * 1.75, rHorizon * 1.48, 0, 0, JsMath.PI * 2);
            ctx.Stroke();
            ctx.Restore();

            ctx.Save();
            ctx.Rotate(time * 1.8);
            ctx.LineWidth = 3.2;
            ctx.StrokeStyle = "#38bdf8ee";
            ctx.SetLineDash(Dash6_8);
            ctx.LineCap = "round";
            ctx.BeginPath();
            ctx.Arc(0, 0, rHorizon + 6.5, 0, JsMath.PI * 2);
            ctx.Stroke();
            ctx.Restore();

            for (int i = 0; i < SingularityParticles.Count; i++)
            {
                var p = SingularityParticles[i];
                double pr = rHorizon * p.RRatio;
                double px = JsMath.Cos(p.Angle) * pr;
                double py = JsMath.Sin(p.Angle) * (pr * 0.82);
                double horizonDist = p.RRatio - 1.0;
                if (horizonDist < 0) continue;
                double alpha = JsMath.Min(1, horizonDist * 2.2) * (0.55 + JsMath.Sin(p.Twinkle) * 0.45);
                ctx.Save();
                GameDraw.Alpha(ctx, alpha);
                ctx.FillStyle = p.Color;
                ctx.Translate(px, py);
                ctx.BeginPath();
                ctx.Arc(0, 0, p.Size, 0, JsMath.PI * 2);
                ctx.Fill();
                ctx.FillStyle = "#ffffff";
                ctx.BeginPath();
                ctx.Arc(0, 0, p.Size * 0.45, 0, JsMath.PI * 2);
                ctx.Fill();
                double tanAngle = p.Angle + JsMath.PI * 0.5;
                ctx.StrokeStyle = p.Color;
                ctx.LineWidth = 1.3;
                ctx.BeginPath();
                ctx.MoveTo(0, 0);
                ctx.LineTo(-JsMath.Cos(tanAngle) * (p.Size * 2.2), -JsMath.Sin(tanAngle) * (p.Size * 2.2));
                ctx.Stroke();
                ctx.Restore();
            }
            ctx.Restore();

            ctx.Save();
            ctx.Translate(bh.X, bh.Y);
            ctx.LineWidth = 4.5;
            ctx.StrokeStyle = "#1e1b4b";
            ctx.FillStyle = core;
            ctx.BeginPath();
            ctx.Arc(0, 0, bh.EventHorizonRadius + 3, 0, JsMath.PI * 2);
            ctx.Fill();
            ctx.Stroke();

            ctx.FillStyle = "rgba(244, 114, 182, 0.6)";
            ctx.BeginPath();
            ctx.Arc(-bh.EventHorizonRadius * 0.58, bh.EventHorizonRadius * 0.15, 6, 0, JsMath.PI * 2);
            ctx.Arc(bh.EventHorizonRadius * 0.58, bh.EventHorizonRadius * 0.15, 6, 0, JsMath.PI * 2);
            ctx.Fill();

            double eyeOffsetX = bh.EventHorizonRadius * 0.38;
            double eyeOffsetY = -bh.EventHorizonRadius * 0.22;
            const double eyeRadius = 7.5;
            double lookDx = 0;
            double lookDy = -1;
            double minD = 9999;
            var debris = physics.DebrisList;
            for (int i = 0; i < debris.Count; i++)
            {
                var d = debris[i];
                double dist = JsMath.Hypot(d.X - bh.X, d.Y - bh.Y);
                if (dist < minD)
                {
                    minD = dist;
                    lookDx = (d.X - bh.X) / (dist + 0.1);
                    lookDy = (d.Y - bh.Y) / (dist + 0.1);
                }
            }

            double[] eyes = { -eyeOffsetX, eyeOffsetX };
            for (int e = 0; e < eyes.Length; e++)
            {
                ctx.Save();
                ctx.Translate(eyes[e], eyeOffsetY);
                if (IsBlinking)
                {
                    ctx.StrokeStyle = "#ffffff";
                    ctx.LineWidth = 2.5;
                    ctx.BeginPath();
                    ctx.Arc(0, 0, eyeRadius * 0.8, 0, JsMath.PI);
                    ctx.Stroke();
                }
                else
                {
                    ctx.FillStyle = "#ffffff";
                    ctx.StrokeStyle = "#1e1b4b";
                    ctx.LineWidth = 2;
                    ctx.BeginPath();
                    ctx.Arc(0, 0, eyeRadius, 0, JsMath.PI * 2);
                    ctx.Fill();
                    ctx.Stroke();
                    double pX = lookDx * 2.5;
                    double pY = lookDy * 2.5;
                    ctx.FillStyle = "#1e1b4b";
                    ctx.BeginPath();
                    ctx.Arc(pX, pY, eyeRadius * 0.58, 0, JsMath.PI * 2);
                    ctx.Fill();
                    ctx.FillStyle = "#ffffff";
                    ctx.BeginPath();
                    ctx.Arc(pX - 1.5, pY - 1.8, 2.0, 0, JsMath.PI * 2);
                    ctx.Arc(pX + 1.5, pY + 1.2, 1.0, 0, JsMath.PI * 2);
                    ctx.Fill();
                }
                ctx.Restore();
            }

            ctx.Save();
            ctx.Translate(0, bh.EventHorizonRadius * 0.28);
            if (MouthOpenAmount > 0.2)
            {
                double openH = 6 + MouthOpenAmount * 11;
                double openW = 8 + MouthOpenAmount * 6;
                ctx.FillStyle = "#e11d48";
                ctx.StrokeStyle = "#1e1b4b";
                ctx.LineWidth = 2.2;
                ctx.BeginPath();
                ctx.Ellipse(0, 0, openW, openH, 0, 0, JsMath.PI * 2);
                ctx.Fill();
                ctx.Stroke();
                ctx.FillStyle = "#fb7185";
                ctx.BeginPath();
                ctx.Arc(0, openH * 0.45, openW * 0.55, 0, JsMath.PI);
                ctx.Fill();
            }
            else
            {
                ctx.StrokeStyle = "#ffffff";
                ctx.LineWidth = 2.2;
                ctx.BeginPath();
                ctx.Arc(-3.5, 0, 3.5, 0, JsMath.PI);
                ctx.Arc(3.5, 0, 3.5, 0, JsMath.PI);
                ctx.Stroke();
            }
            ctx.Restore();
            ctx.Restore();
            ctx.Restore();
        }

        void RenderOneWayFlappers(Canvas2D ctx, PhysicsEngine physics)
        {
            if (physics.OneWayFlaps == null || physics.OneWayFlaps.Count == 0) return;
            double cx = physics.Center.X;
            double cy = physics.Center.Y;
            ctx.Save();
            ctx.Translate(cx, cy);
            ctx.Rotate(physics.RingAngle);
            for (int i = 0; i < physics.OneWayFlaps.Count; i++)
            {
                var flap = physics.OneWayFlaps[i];
                double rot = (flap.IsInner && physics.ConcentricRings) ? (physics.InnerRingAngle - physics.RingAngle) : 0;
                double sign = GameDraw.Or1(flap.NormalSign);
                double baseAngle = flap.Angle + rot;
                double currentAngle = baseAngle + flap.Swing * sign;
                bool isLocked = flap.Swing <= 0.05;
                double cosCur = JsMath.Cos(currentAngle);
                double sinCur = JsMath.Sin(currentAngle);
                double stopAng = baseAngle - 0.08 * sign;
                double stopX = JsMath.Cos(stopAng) * (flap.RMin + 14);
                double stopY = JsMath.Sin(stopAng) * (flap.RMin + 14);
                ctx.FillStyle = "#64748b";
                ctx.StrokeStyle = "#1e1b4b";
                ctx.LineWidth = 2;
                ctx.BeginPath();
                ctx.Arc(stopX, stopY, 4, 0, JsMath.PI * 2);
                ctx.Fill();
                ctx.Stroke();

                double x1 = cosCur * flap.RMin;
                double y1 = sinCur * flap.RMin;
                double x2 = cosCur * flap.RMax;
                double y2 = sinCur * flap.RMax;
                ctx.LineWidth = 8;
                ctx.LineCap = "round";
                ctx.StrokeStyle = "#1e1b4b";
                ctx.BeginPath();
                ctx.MoveTo(x1, y1);
                ctx.LineTo(x2, y2);
                ctx.Stroke();
                ctx.LineWidth = 4;
                ctx.StrokeStyle = isLocked ? "#f59e0b" : "#22c55e";
                ctx.BeginPath();
                ctx.MoveTo(x1, y1);
                ctx.LineTo(x2, y2);
                ctx.Stroke();

                ctx.FillStyle = "#e2e8f0";
                ctx.StrokeStyle = "#1e1b4b";
                ctx.LineWidth = 2.5;
                ctx.BeginPath();
                ctx.Arc(x1, y1, 7, 0, JsMath.PI * 2);
                ctx.Fill();
                ctx.Stroke();
                ctx.FillStyle = isLocked ? "#f59e0b" : "#4ade80";
                ctx.BeginPath();
                ctx.Arc(x1, y1, 2.8, 0, JsMath.PI * 2);
                ctx.Fill();
                ctx.FillStyle = "#38bdf8";
                ctx.StrokeStyle = "#1e1b4b";
                ctx.LineWidth = 1.5;
                ctx.BeginPath();
                ctx.Arc(x2, y2, 4, 0, JsMath.PI * 2);
                ctx.Fill();
                ctx.Stroke();
            }
            ctx.Restore();
        }

        void RenderCartoonBumpers(Canvas2D ctx, PhysicsEngine physics)
        {
            if (physics.DeflectorPins == null || physics.DeflectorPins.Count == 0) return;
            ctx.Save();
            ctx.Translate(physics.Center.X, physics.Center.Y);
            ctx.Rotate(physics.RingAngle);
            for (int i = 0; i < physics.DeflectorPins.Count; i++)
            {
                var pin = physics.DeflectorPins[i];
                double rot = (pin.IsInner && physics.ConcentricRings) ? (physics.InnerRingAngle - physics.RingAngle) : 0;
                double finalAngle = pin.Angle + rot;
                double px = JsMath.Cos(finalAngle) * pin.R;
                double py = JsMath.Sin(finalAngle) * pin.R;
                double r = GameDraw.OrNum(pin.Radius, 9.5);
                bool isHit = pin.HitTimer > 0;
                double hitProgress = isHit ? pin.HitTimer / 0.25 : 0;
                double scale = isHit ? 1 + JsMath.Sin(hitProgress * JsMath.PI) * 0.35 : 1;
                ctx.Save();
                ctx.Translate(px, py);
                ctx.Scale(scale, scale);
                ctx.FillStyle = "rgba(15, 23, 42, 0.4)";
                ctx.BeginPath();
                ctx.Arc(0, 3, r + 2, 0, JsMath.PI * 2);
                ctx.Fill();
                ctx.LineWidth = 3;
                ctx.StrokeStyle = "#1e1b4b";
                ctx.FillStyle = isHit ? "#fef08a" : GameDraw.Or(pin.Color, "#ec4899");
                ctx.BeginPath();
                ctx.Arc(0, 0, r + 1, 0, JsMath.PI * 2);
                ctx.Fill();
                ctx.Stroke();
                var dome = ctx.CreateRadialGradient(-r * 0.3, -r * 0.3, 1, 0, 0, r);
                dome.AddColorStop(0, "#ffffff");
                dome.AddColorStop(0.45, isHit ? "#fbbf24" : GameDraw.Or(pin.Accent, "#f43f5e"));
                dome.AddColorStop(1, isHit ? "#f59e0b" : "#be123c");
                ctx.FillStyle = dome;
                ctx.BeginPath();
                ctx.Arc(0, 0, r * 0.8, 0, JsMath.PI * 2);
                ctx.Fill();
                ctx.FillStyle = "rgba(255, 255, 255, 0.85)";
                ctx.BeginPath();
                ctx.Arc(-r * 0.28, -r * 0.28, r * 0.26, 0, JsMath.PI * 2);
                ctx.Fill();
                if (isHit)
                {
                    double shockRadius = r + (1 - hitProgress) * 16;
                    ctx.LineWidth = 2.5 * hitProgress;
                    ctx.StrokeStyle = GameDraw.Rgba(254, 240, 138, hitProgress);
                    ctx.BeginPath();
                    ctx.Arc(0, 0, shockRadius, 0, JsMath.PI * 2);
                    ctx.Stroke();
                }
                ctx.Restore();
            }
            ctx.Restore();
        }
    }
}
