using System.Collections.Generic;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Logic;

namespace OrbitalSalvage.Render
{
    public sealed partial class GameRenderer
    {
        void RenderSawbladeGate(Canvas2D ctx, double x1, double y1, double x2, double y2, double gx, double gy, double gAngle, GateRuntime gate)
        {
            const string mainColor = "#f97316";
            ctx.LineWidth = 6;
            ctx.StrokeStyle = "#1e1b4b";
            ctx.BeginPath();
            ctx.MoveTo(x1, y1);
            ctx.LineTo(x2, y2);
            ctx.Stroke();
            ctx.LineWidth = 3.5;
            ctx.StrokeStyle = mainColor;
            ctx.BeginPath();
            ctx.MoveTo(x1, y1);
            ctx.LineTo(x2, y2);
            ctx.Stroke();
            ctx.LineWidth = 1.4;
            ctx.StrokeStyle = "#fef08a";
            ctx.BeginPath();
            ctx.MoveTo(x1, y1);
            ctx.LineTo(x2, y2);
            ctx.Stroke();
            double spin = CosmicTime * 7;
            RenderSpinningGear(ctx, x1, y1, 5.5, spin, mainColor);
            RenderSpinningGear(ctx, x2, y2, 5.5, -spin, mainColor);

            ctx.Save();
            ctx.Translate(gx, gy);
            ctx.Rotate(gAngle + JsMath.PI * 0.5);
            const double bw = 17;
            const double bh = 8.5;
            ctx.FillStyle = "#1e1b4b";
            ctx.BeginPath();
            ctx.MoveTo(-bw - 4, 2);
            ctx.LineTo(-bw, -bh + 2);
            ctx.LineTo(bw, -bh + 2);
            ctx.LineTo(bw + 4, 2);
            ctx.LineTo(bw, bh + 2);
            ctx.LineTo(-bw, bh + 2);
            ctx.ClosePath();
            ctx.Fill();
            ctx.FillStyle = "#ffffff";
            ctx.StrokeStyle = mainColor;
            ctx.LineWidth = 2.2;
            ctx.BeginPath();
            ctx.MoveTo(-bw - 4, 0);
            ctx.LineTo(-bw, -bh);
            ctx.LineTo(bw, -bh);
            ctx.LineTo(bw + 4, 0);
            ctx.LineTo(bw, bh);
            ctx.LineTo(-bw, bh);
            ctx.ClosePath();
            ctx.Fill();
            ctx.Stroke();
            ctx.FillStyle = "#c2410c";
            ctx.Font = "bold 10px Fredoka, sans-serif";
            ctx.TextAlign = "center";
            ctx.TextBaseline = "middle";
            ctx.FillText(_labels.Zap(gate.Value), 0, 0.5);
            ctx.Restore();
        }

        void RenderSuperShredderGate(Canvas2D ctx, double x1, double y1, double x2, double y2, double gx, double gy, double gAngle, GateRuntime gate)
        {
            const string mainColor = "#ec4899";
            ctx.LineWidth = 8;
            ctx.StrokeStyle = "rgba(236, 72, 153, 0.4)";
            ctx.BeginPath();
            ctx.MoveTo(x1, y1);
            ctx.LineTo(x2, y2);
            ctx.Stroke();
            double dx = x2 - x1;
            double dy = y2 - y1;
            const int segCount = 6;
            ctx.LineWidth = 2.5;
            ctx.StrokeStyle = "#ffffff";
            ctx.BeginPath();
            ctx.MoveTo(x1, y1);
            double hyp = JsMath.Hypot(dx, dy) + 0.001;
            for (int i = 1; i < segCount; i++)
            {
                double frac = (double)i / segCount;
                double px = x1 + dx * frac;
                double py = y1 + dy * frac;
                double perpX = -dy / hyp;
                double perpY = dx / hyp;
                double jitter = JsMath.Sin(CosmicTime * 32 + i * 2) * 3;
                ctx.LineTo(px + perpX * jitter, py + perpY * jitter);
            }
            ctx.LineTo(x2, y2);
            ctx.Stroke();

            double[] px2 = { x1, x2 };
            double[] py2 = { y1, y2 };
            for (int i = 0; i < 2; i++)
            {
                ctx.Save();
                ctx.Translate(px2[i], py2[i]);
                ctx.Rotate(CosmicTime * 5);
                ctx.FillStyle = mainColor;
                ctx.StrokeStyle = "#1e1b4b";
                ctx.LineWidth = 1.6;
                ctx.BeginPath();
                ctx.MoveTo(0, -7.5);
                ctx.LineTo(2.5, -2.5);
                ctx.LineTo(7.5, 0);
                ctx.LineTo(2.5, 2.5);
                ctx.LineTo(0, 7.5);
                ctx.LineTo(-2.5, 2.5);
                ctx.LineTo(-7.5, 0);
                ctx.LineTo(-2.5, -2.5);
                ctx.ClosePath();
                ctx.Fill();
                ctx.Stroke();
                ctx.FillStyle = "#ffffff";
                ctx.BeginPath();
                ctx.Arc(0, 0, 2.2, 0, JsMath.PI * 2);
                ctx.Fill();
                ctx.Restore();
            }

            ctx.Save();
            ctx.Translate(gx, gy);
            ctx.Rotate(gAngle + JsMath.PI * 0.5);
            const double bw = 19;
            const double bh = 9;
            ctx.FillStyle = "rgba(236, 72, 153, 0.4)";
            ctx.BeginPath();
            ctx.Arc(0, 0, 16, 0, JsMath.PI * 2);
            ctx.Fill();
            ctx.FillStyle = "#1e1b4b";
            ctx.BeginPath();
            ctx.RoundRect(-bw, -bh + 2, bw * 2, bh * 2, 5);
            ctx.Fill();
            ctx.FillStyle = "#ec4899";
            ctx.StrokeStyle = "#ffffff";
            ctx.LineWidth = 2;
            ctx.BeginPath();
            ctx.RoundRect(-bw, -bh, bw * 2, bh * 2, 5);
            ctx.Fill();
            ctx.Stroke();
            ctx.FillStyle = "#ffffff";
            ctx.Font = "900 10.5px Fredoka, sans-serif";
            ctx.TextAlign = "center";
            ctx.TextBaseline = "middle";
            ctx.FillText(_labels.Shred(gate.Value), 0, 0.5);
            ctx.Restore();
        }

        void RenderCompactorGate(Canvas2D ctx, double x1, double y1, double x2, double y2, double gx, double gy, double gAngle, GateRuntime gate)
        {
            double perpAngle = gAngle + JsMath.PI * 0.5;
            double offX = JsMath.Cos(perpAngle) * 2.5;
            double offY = JsMath.Sin(perpAngle) * 2.5;
            ctx.LineWidth = 3;
            ctx.StrokeStyle = "#0891b2";
            ctx.BeginPath();
            ctx.MoveTo(x1 - offX, y1 - offY);
            ctx.LineTo(x2 - offX, y2 - offY);
            ctx.MoveTo(x1 + offX, y1 + offY);
            ctx.LineTo(x2 + offX, y2 + offY);
            ctx.Stroke();

            double dx = x2 - x1;
            double dy = y2 - y1;
            const int waveCount = 3;
            for (int i = 0; i < waveCount; i++)
            {
                double wavePhase = JsMath.Mod(CosmicTime * 1.8 + (double)i / waveCount, 1.0);
                double frac = wavePhase < 0.5 ? wavePhase : 1.0 - (wavePhase - 0.5);
                double wx = x1 + dx * frac;
                double wy = y1 + dy * frac;
                ctx.StrokeStyle = GameDraw.Rgba(6, 182, 212, 0.8 - frac * 0.6);
                ctx.LineWidth = 1.8;
                ctx.BeginPath();
                ctx.Arc(wx, wy, 3.5 + wavePhase * 5, 0, JsMath.PI * 2);
                ctx.Stroke();
            }

            double[] px = { x1, x2 };
            double[] py = { y1, y2 };
            for (int i = 0; i < 2; i++)
            {
                ctx.Save();
                ctx.Translate(px[i], py[i]);
                ctx.FillStyle = "#0284c7";
                ctx.StrokeStyle = "#1e1b4b";
                ctx.LineWidth = 1.8;
                ctx.BeginPath();
                ctx.RoundRect(-4.5, -4.5, 9, 9, 2.5);
                ctx.Fill();
                ctx.Stroke();
                ctx.FillStyle = "#67e8f9";
                ctx.BeginPath();
                ctx.Arc(0, 0, 2.2, 0, JsMath.PI * 2);
                ctx.Fill();
                ctx.Restore();
            }

            ctx.Save();
            ctx.Translate(gx, gy);
            ctx.Rotate(gAngle + JsMath.PI * 0.5);
            const double bw = 17;
            const double bh = 8.5;
            ctx.FillStyle = "#1e1b4b";
            ctx.BeginPath();
            ctx.RoundRect(-bw, -bh + 2, bw * 2, bh * 2, 8.5);
            ctx.Fill();
            ctx.FillStyle = "#ffffff";
            ctx.StrokeStyle = "#0891b2";
            ctx.LineWidth = 2.2;
            ctx.BeginPath();
            ctx.RoundRect(-bw, -bh, bw * 2, bh * 2, 8.5);
            ctx.Fill();
            ctx.Stroke();
            ctx.FillStyle = "#0369a1";
            ctx.Font = "bold 10px Fredoka, sans-serif";
            ctx.TextAlign = "center";
            ctx.TextBaseline = "middle";
            ctx.FillText(_labels.Magnet(gate.Value), 0, 0.5);
            ctx.Restore();
        }

        void RenderBoosterGate(Canvas2D ctx, double x1, double y1, double x2, double y2, double gx, double gy, double gAngle, GateRuntime gate)
        {
            ctx.LineWidth = 6;
            ctx.StrokeStyle = "rgba(132, 204, 22, 0.35)";
            ctx.BeginPath();
            ctx.MoveTo(x1, y1);
            ctx.LineTo(x2, y2);
            ctx.Stroke();
            ctx.LineWidth = 3;
            ctx.StrokeStyle = "#65a30d";
            ctx.BeginPath();
            ctx.MoveTo(x1, y1);
            ctx.LineTo(x2, y2);
            ctx.Stroke();

            double[] px = { x1, x2 };
            double[] py = { y1, y2 };
            for (int i = 0; i < 2; i++)
            {
                ctx.Save();
                ctx.Translate(px[i], py[i]);
                ctx.FillStyle = "#84cc16";
                ctx.StrokeStyle = "#1e1b4b";
                ctx.LineWidth = 1.8;
                ctx.BeginPath();
                ctx.Arc(0, 0, 4.5, 0, JsMath.PI * 2);
                ctx.Fill();
                ctx.Stroke();
                ctx.FillStyle = "#facc15";
                ctx.BeginPath();
                ctx.Arc(0, 0, 2.2, 0, JsMath.PI * 2);
                ctx.Fill();
                ctx.Restore();
            }

            ctx.Save();
            ctx.Translate(gx, gy);
            ctx.Rotate(gAngle + JsMath.PI * 0.5);
            ctx.Transform(1, 0, -0.22, 1, 0, 0);
            const double bw = 18;
            const double bh = 8.5;
            ctx.FillStyle = "#1e1b4b";
            ctx.BeginPath();
            ctx.RoundRect(-bw, -bh + 2, bw * 2, bh * 2, 4.5);
            ctx.Fill();
            ctx.FillStyle = "#84cc16";
            ctx.StrokeStyle = "#1e1b4b";
            ctx.LineWidth = 2;
            ctx.BeginPath();
            ctx.RoundRect(-bw, -bh, bw * 2, bh * 2, 4.5);
            ctx.Fill();
            ctx.Stroke();
            ctx.FillStyle = "#ffffff";
            ctx.Font = "bold 9.5px Fredoka, sans-serif";
            ctx.TextAlign = "center";
            ctx.TextBaseline = "middle";
            ctx.FillText(_labels.Boost(), 0, 0.5);
            ctx.Restore();
        }

        void RenderCartoonGates(Canvas2D ctx, PhysicsEngine physics)
        {
            ctx.Save();
            ctx.Translate(physics.Center.X, physics.Center.Y);
            ctx.Rotate(physics.RingAngle);
            var gates = physics.Gates;
            for (int i = 0; i < gates.Count; i++)
            {
                var gate = gates[i];
                double rot = (gate.IsInner && physics.ConcentricRings) ? (physics.InnerRingAngle - physics.RingAngle) : 0;
                double gAngle = gate.Angle + rot;
                double rMin = gate.RMin;
                double rMax = gate.RMax;
                double rBadge = rMin + (rMax - rMin) * 0.82;
                double cosA = JsMath.Cos(gAngle);
                double sinA = JsMath.Sin(gAngle);
                double x1 = cosA * rMin;
                double y1 = sinA * rMin;
                double x2 = cosA * rMax;
                double y2 = sinA * rMax;
                double gx = cosA * rBadge;
                double gy = sinA * rBadge;
                if (gate.Type == "BOOST") RenderBoosterGate(ctx, x1, y1, x2, y2, gx, gy, gAngle, gate);
                else if (gate.Type == "ADD") RenderCompactorGate(ctx, x1, y1, x2, y2, gx, gy, gAngle, gate);
                else if (gate.Value >= 4) RenderSuperShredderGate(ctx, x1, y1, x2, y2, gx, gy, gAngle, gate);
                else RenderSawbladeGate(ctx, x1, y1, x2, y2, gx, gy, gAngle, gate);
            }
            ctx.Restore();
        }

        void RenderCartoonDebris(Canvas2D ctx, List<Debris> debrisList)
        {
            ctx.Save();
            // Phone path: put the transform back by hand. Save cloned a state object per piece.
            bool phone = RenderOpts.FastShape;
            Matrix2D keep = ctx.CurrentTransform;
            for (int i = 0; i < debrisList.Count; i++)
            {
                var d = debrisList[i];
                if (d.State == DebrisState.Removed) continue;
                if (!phone) ctx.Save();
                ctx.Translate(d.X, d.Y);
                if (d.Type == "solar")
                {
                    ctx.FillStyle = "#38bdf8";
                    ctx.StrokeStyle = "#1e1b4b";
                    ctx.LineWidth = 1.8;
                    ctx.BeginPath();
                    ctx.RoundRect(-d.Radius, -d.Radius * 0.75, d.Radius * 2, d.Radius * 1.5, 3);
                    ctx.Fill();
                    ctx.Stroke();
                    ctx.FillStyle = "#facc15";
                    ctx.FillRect(-d.Radius - 5, -3, 4, 6);
                    ctx.FillRect(d.Radius + 1, -3, 4, 6);
                    ctx.StrokeStyle = "#1e1b4b";
                    ctx.BeginPath();
                    ctx.MoveTo(0, -d.Radius * 0.75);
                    ctx.LineTo(0, -d.Radius * 0.75 - 4);
                    ctx.Stroke();
                    ctx.FillStyle = "#f472b6";
                    ctx.BeginPath();
                    ctx.Arc(0, -d.Radius * 0.75 - 5, 2, 0, JsMath.PI * 2);
                    ctx.Fill();
                }
                else if (d.Type == "volatile")
                {
                    ctx.FillStyle = "#ef4444";
                    ctx.StrokeStyle = "#1e1b4b";
                    ctx.LineWidth = 2;
                    ctx.BeginPath();
                    ctx.RoundRect(-d.Radius, -d.Radius * 1.1, d.Radius * 2, d.Radius * 2.2, 4);
                    ctx.Fill();
                    ctx.Stroke();
                    ctx.FillStyle = "#facc15";
                    ctx.FillRect(-3, -d.Radius * 1.1 - 3, 6, 3);
                    ctx.FillStyle = "#38bdf8";
                    ctx.BeginPath();
                    ctx.Arc(d.Radius * 0.6, -d.Radius * 0.7, 2, 0, JsMath.PI * 2);
                    ctx.Fill();
                }
                else
                {
                    ctx.FillStyle = "#facc15";
                    ctx.StrokeStyle = "#1e1b4b";
                    ctx.LineWidth = 2;
                    ctx.BeginPath();
                    ctx.Arc(0, 0, d.Radius, 0, JsMath.PI * 2);
                    ctx.Fill();
                    ctx.Stroke();
                    ctx.FillStyle = "#1e1b4b";
                    if (RenderOpts.FastShape)
                    {
                        // Phone path: one circle per fill. A two-circle path cannot use the analytic fast fill, and every
                        // piece in a big pile paid a full-size target switch for it. The eyes do not overlap, so the picture is the same.
                        ctx.BeginPath();
                        ctx.Arc(-2, -1, 1.2, 0, JsMath.PI * 2);
                        ctx.Fill();
                        ctx.BeginPath();
                        ctx.Arc(2, -1, 1.2, 0, JsMath.PI * 2);
                        ctx.Fill();
                    }
                    else
                    {
                        ctx.BeginPath();
                        ctx.Arc(-2, -1, 1.2, 0, JsMath.PI * 2);
                        ctx.Arc(2, -1, 1.2, 0, JsMath.PI * 2);
                        ctx.Fill();
                    }
                    ctx.BeginPath();
                    ctx.Arc(0, 1.5, 2, 0, JsMath.PI);
                    ctx.Stroke();
                }
                if (phone) ctx.SetTransform(keep.a, keep.b, keep.c, keep.d, keep.e, keep.f);
                else ctx.Restore();
            }
            ctx.Restore();
        }

        void RenderVfx(Canvas2D ctx, double dt)
        {
            for (int i = Particles.Count - 1; i >= 0; i--)
            {
                var p = Particles[i];
                p.Life -= dt;
                if (p.Life <= 0)
                {
                    Particles.RemoveAt(i);
                    continue;
                }
                p.Vy += p.Gravity * dt;
                p.X += p.Vx * dt;
                p.Y += p.Vy * dt;
                p.Rot += p.VRot * dt;
                ctx.Save();
                ctx.Translate(p.X, p.Y);
                ctx.Rotate(p.Rot);
                ctx.FillStyle = p.Color;
                ctx.FillRect(-p.W * 0.5, -p.H * 0.5, p.W, p.H);
                ctx.Restore();
            }

            for (int i = PopRings.Count - 1; i >= 0; i--)
            {
                var r = PopRings[i];
                r.Radius += dt * (r.Speed ?? 80);
                r.Alpha -= dt * (r.FadeRate ?? 2.2);
                if (r.Alpha <= 0 || r.Radius >= r.RadiusMax)
                {
                    PopRings.RemoveAt(i);
                    continue;
                }
                ctx.Save();
                ctx.StrokeStyle = r.Color;
                GameDraw.Alpha(ctx, r.Alpha);
                ctx.LineWidth = r.LineWidth ?? 3;
                ctx.BeginPath();
                ctx.Arc(r.X, r.Y, r.Radius, 0, JsMath.PI * 2);
                ctx.Stroke();
                ctx.Restore();
            }

            for (int i = ThrusterPuffs.Count - 1; i >= 0; i--)
            {
                var puff = ThrusterPuffs[i];
                double alpha = (puff.Life / puff.MaxLife) * 0.85;
                ctx.Save();
                GameDraw.Alpha(ctx, alpha);
                ctx.FillStyle = GameDraw.Or(puff.Color, "#ffffff");
                ctx.StrokeStyle = "#1e1b4b";
                ctx.LineWidth = 2;
                ctx.BeginPath();
                ctx.Arc(puff.X, puff.Y, puff.Radius, 0, JsMath.PI * 2);
                ctx.Fill();
                ctx.Stroke();
                ctx.Restore();
            }

            for (int i = FloatingTexts.Count - 1; i >= 0; i--)
            {
                var ft = FloatingTexts[i];
                ft.Life -= dt;
                if (ft.Life <= 0)
                {
                    FloatingTexts.RemoveAt(i);
                    continue;
                }
                ft.Y += ft.Vy * dt;
                ft.Alpha = ft.Life / 0.85;
                ctx.Save();
                ctx.Translate(ft.X, ft.Y);
                ctx.Scale(ft.Scale, ft.Scale);
                ctx.Font = "900 16px Fredoka, sans-serif";
                ctx.TextAlign = "center";
                ctx.StrokeStyle = "#1e1b4b";
                ctx.LineWidth = 3.5;
                ctx.StrokeText(ft.Text, 0, 0);
                ctx.FillStyle = ft.Color;
                ctx.FillText(ft.Text, 0, 0);
                ctx.Restore();
            }
            ctx.GlobalAlpha = 1;
        }
    }
}
