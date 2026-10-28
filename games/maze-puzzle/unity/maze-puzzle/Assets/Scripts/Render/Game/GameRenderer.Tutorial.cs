using System;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Loc;
using OrbitalSalvage.Logic;

namespace OrbitalSalvage.Render
{
    using Loc = OrbitalSalvage.Loc.Loc;

    public sealed partial class GameRenderer
    {
        public void RenderTutorialHud(Canvas2D ctx, PhysicsEngine physics, TutorialPhase phase, RenderProgress progress)
        {
            if (ctx == null || physics == null || phase == TutorialPhase.None || phase == TutorialPhase.Done) return;

            // Dimensions matching web prototype: docked at bottom with breathing space
            double cardW = JsMath.Min(380, Width > 40 ? Width - 24 : 350);
            double cardH = 68;
            double cardX = physics.Center.X - cardW * 0.5;
            double cardY = Height > 100 ? Height - cardH - 22 : physics.BlackHole.Y + 45;

            switch (phase)
            {
                case TutorialPhase.Swipe:
                    RenderSwipeGuide(ctx, physics);
                    DrawCommsCard(ctx, cardX, cardY, cardW, cardH, "😋", "RADIO COMMS • NIBBLES", Loc.Tr("intro.step.rotate"), "#facc15", 0);
                    break;

                case TutorialPhase.Gate:
                    RenderGateBeacon(ctx, physics);
                    DrawCommsCard(ctx, cardX, cardY, cardW, cardH, "⚡", "ENERGY WARNING • MULTIPLIER", Loc.Tr("intro.step.multiply"), "#38bdf8", 1);
                    break;

                case TutorialPhase.Feed:
                    RenderFeedGuide(ctx, physics);
                    double quota = progress != null ? progress.Quota : 20;
                    DrawCommsCard(ctx, cardX, cardY, cardW, cardH, "😋", "HUNGRY VOID • FEED TIME", Loc.Tr("intro.step.feed", quota), "#4ade80", 2);
                    break;
            }
        }

        void RenderSwipeGuide(Canvas2D ctx, PhysicsEngine physics)
        {
            double cx = physics.Center.X;
            double cy = physics.Center.Y;
            double rOrbit = physics.RingRadius + 14;

            // 1. Top floating badge: "↻ SWIPE TO ROTATE"
            double badgeW = 142;
            double badgeH = 24;
            double badgeX = cx - badgeW * 0.5;
            double badgeY = cy - physics.RingRadius - 32;

            // Badge outer glow
            ctx.Save();
            ctx.StrokeStyle = "#38bdf8";
            GameDraw.Alpha(ctx, 0.25);
            ctx.LineWidth = 5;
            DrawRoundedRect(ctx, badgeX - 1, badgeY - 1, badgeW + 2, badgeH + 2, 12);
            ctx.Stroke();
            ctx.Restore();

            // Badge pill body & border
            ctx.Save();
            ctx.FillStyle = "#0f172a";
            ctx.StrokeStyle = "#38bdf8";
            ctx.LineWidth = 2;
            DrawRoundedRect(ctx, badgeX, badgeY, badgeW, badgeH, 12);
            ctx.Fill();
            ctx.Stroke();

            ctx.Font = "bold 10px Fredoka, sans-serif";
            ctx.FillStyle = "#facc15";
            ctx.TextAlign = "center";
            ctx.TextBaseline = "middle";
            ctx.FillText("↻ SWIPE TO ROTATE", cx, badgeY + badgeH * 0.5);
            ctx.Restore();

            // 2. Multi-layer neon orbit sweep track (Zero ShadowBlur, hardware-batched)
            // Outer glow arc
            ctx.Save();
            ctx.StrokeStyle = "#38bdf8";
            GameDraw.Alpha(ctx, 0.22);
            ctx.LineWidth = 7;
            ctx.BeginPath();
            ctx.Arc(cx, cy, rOrbit, 0, JsMath.PI * 2);
            ctx.Stroke();
            ctx.Restore();

            // Inner dashed animated orbit sweep
            ctx.Save();
            ctx.StrokeStyle = "#38bdf8";
            GameDraw.Alpha(ctx, 0.95);
            ctx.LineWidth = 2.8;
            ctx.LineCap = "round";
            ctx.SetLineDash(new[] { 14.0, 10.0 });
            ctx.LineDashOffset = -CosmicTime * 45;
            ctx.BeginPath();
            ctx.Arc(cx, cy, rOrbit, 0, JsMath.PI * 2);
            ctx.Stroke();
            ctx.Restore();

            // 3. Animated Hand Finger Pointer with expanding wave ripple
            double sweepA = JsMath.PI * 0.5 + JsMath.Sin(CosmicTime * 2.8) * 0.6;
            double hx = cx + JsMath.Cos(sweepA) * rOrbit;
            double hy = cy + JsMath.Sin(sweepA) * rOrbit;

            // Expanding ripple wave
            double rippleR = 10 + ((CosmicTime * 22) % 18);
            double rippleA = 1 - (rippleR - 10) / 18;
            ctx.Save();
            ctx.StrokeStyle = "#38bdf8";
            GameDraw.Alpha(ctx, rippleA * 0.85);
            ctx.LineWidth = 2;
            ctx.BeginPath();
            ctx.Arc(hx, hy, rippleR, 0, JsMath.PI * 2);
            ctx.Stroke();
            ctx.Restore();

            // Finger emoji indicator
            ctx.Save();
            ctx.Font = "22px Fredoka, sans-serif";
            ctx.TextAlign = "center";
            ctx.TextBaseline = "middle";
            ctx.FillText("👆", hx, hy + 4);
            ctx.Restore();
        }

        void RenderGateBeacon(Canvas2D ctx, PhysicsEngine physics)
        {
            GateRuntime targetGate = null;
            var gates = physics.Gates;
            if (gates != null)
            {
                for (int g = 0; g < gates.Count; g++)
                {
                    if (gates[g].Type == "MULTIPLY") { targetGate = gates[g]; break; }
                }
                if (targetGate == null && gates.Count > 0) targetGate = gates[0];
            }
            if (targetGate == null) return;

            double rot = targetGate.IsInner && physics.ConcentricRings ? physics.InnerRingAngle - physics.RingAngle : 0;
            double rMid = (targetGate.RMin + targetGate.RMax) * 0.5;
            double ang = targetGate.Angle + rot + physics.RingAngle;
            double gx = physics.Center.X + JsMath.Cos(ang) * rMid;
            double gy = physics.Center.Y + JsMath.Sin(ang) * rMid;

            double pulseR = 26 + JsMath.Sin(CosmicTime * 5) * 4;

            // 1. Multi-layer pulse reticle ring
            // Outer glow ring
            ctx.Save();
            ctx.StrokeStyle = "#facc15";
            GameDraw.Alpha(ctx, 0.25);
            ctx.LineWidth = 7;
            ctx.BeginPath();
            ctx.Arc(gx, gy, pulseR, 0, JsMath.PI * 2);
            ctx.Stroke();
            ctx.Restore();

            // Core dashed pulsing reticle
            ctx.Save();
            ctx.StrokeStyle = "#facc15";
            GameDraw.Alpha(ctx, 0.95);
            ctx.LineWidth = 2.5;
            ctx.SetLineDash(new[] { 8.0, 6.0 });
            ctx.LineDashOffset = CosmicTime * 40;
            ctx.BeginPath();
            ctx.Arc(gx, gy, pulseR, 0, JsMath.PI * 2);
            ctx.Stroke();
            ctx.Restore();

            // Precision crosshairs
            ctx.Save();
            ctx.StrokeStyle = "#fde047";
            ctx.LineWidth = 1.8;
            double chIn = pulseR - 6;
            double chOut = pulseR + 6;
            ctx.BeginPath();
            ctx.MoveTo(gx - chOut, gy); ctx.LineTo(gx - chIn, gy);
            ctx.MoveTo(gx + chIn, gy); ctx.LineTo(gx + chOut, gy);
            ctx.MoveTo(gx, gy - chOut); ctx.LineTo(gx, gy - chIn);
            ctx.MoveTo(gx, gy + chIn); ctx.LineTo(gx, gy + chOut);
            ctx.Stroke();
            ctx.Restore();

            // 2. Gate Badge: "⚡ ×2 GATE"
            double badgeW = 76;
            double badgeH = 20;
            double badgeX = gx - badgeW * 0.5;
            double badgeY = gy - pulseR - 14;

            ctx.Save();
            ctx.FillStyle = "#38bdf8";
            ctx.StrokeStyle = "#ffffff";
            ctx.LineWidth = 1.5;
            DrawRoundedRect(ctx, badgeX, badgeY, badgeW, badgeH, 10);
            ctx.Fill();
            ctx.Stroke();

            ctx.Font = "bold 9px Fredoka, sans-serif";
            ctx.FillStyle = "#0f172a";
            ctx.TextAlign = "center";
            ctx.TextBaseline = "middle";
            ctx.FillText("⚡ ×2 GATE", gx, badgeY + badgeH * 0.5);
            ctx.Restore();

            // 3. Pointing Hand Gesture: "👉"
            double bounce = JsMath.Sin(CosmicTime * 5) * 4;
            ctx.Save();
            ctx.Font = "20px Fredoka, sans-serif";
            ctx.TextAlign = "center";
            ctx.TextBaseline = "middle";
            ctx.FillText("👆", gx, gy + pulseR + 18 - bounce);
            ctx.Restore();
        }

        void RenderFeedGuide(Canvas2D ctx, PhysicsEngine physics)
        {
            double topY = physics.Center.Y + physics.RingRadius * 0.55;
            double botY = physics.BlackHole.Y - 45;
            double span = botY - topY;
            if (span <= 20) return;

            // 1. Cascading Chevrons stream (Zero ShadowBlur)
            for (int c = 0; c < 3; c++)
            {
                double progressOffset = ((CosmicTime * 1.5 + c * 0.33) % 1.0);
                double cy = topY + span * progressOffset;
                double cx = physics.BlackHole.X;
                double alpha = JsMath.Sin(progressOffset * JsMath.PI);

                // Outer glow chevron
                ctx.Save();
                ctx.StrokeStyle = "#4ade80";
                GameDraw.Alpha(ctx, alpha * 0.28);
                ctx.LineWidth = 7;
                ctx.LineCap = "round";
                ctx.BeginPath();
                ctx.MoveTo(cx - 16, cy - 8);
                ctx.LineTo(cx, cy);
                ctx.LineTo(cx + 16, cy - 8);
                ctx.Stroke();
                ctx.Restore();

                // Core chevron
                ctx.Save();
                ctx.StrokeStyle = "#4ade80";
                GameDraw.Alpha(ctx, alpha * 0.95);
                ctx.LineWidth = 3;
                ctx.LineCap = "round";
                ctx.BeginPath();
                ctx.MoveTo(cx - 16, cy - 8);
                ctx.LineTo(cx, cy);
                ctx.LineTo(cx + 16, cy - 8);
                ctx.Stroke();
                ctx.Restore();
            }

            // 2. Target Funnel Ring above Nibbles
            double targetR = 36 + JsMath.Sin(CosmicTime * 4) * 3;
            double ringAlpha = 0.45 + JsMath.Sin(CosmicTime * 4) * 0.25;

            ctx.Save();
            ctx.StrokeStyle = "#4ade80";
            GameDraw.Alpha(ctx, ringAlpha);
            ctx.LineWidth = 2.2;
            ctx.SetLineDash(new[] { 6.0, 6.0 });
            ctx.LineDashOffset = -CosmicTime * 30;
            ctx.BeginPath();
            ctx.Arc(physics.BlackHole.X, physics.BlackHole.Y - 20, targetR, 0, JsMath.PI * 2);
            ctx.Stroke();
            ctx.Restore();
        }

        void DrawCommsCard(Canvas2D ctx, double x, double y, double w, double h, string avatarFace, string senderTag, string prompt, string accentColor, int activePipIndex)
        {
            // 1. Acrylic Hologram Card Outer Glow & Background
            ctx.Save();
            ctx.StrokeStyle = accentColor;
            GameDraw.Alpha(ctx, 0.24);
            ctx.LineWidth = 6;
            DrawRoundedRect(ctx, x - 1, y - 1, w + 2, h + 2, 20);
            ctx.Stroke();
            ctx.Restore();

            ctx.Save();
            ctx.FillStyle = "#0f172a";
            ctx.StrokeStyle = accentColor;
            ctx.LineWidth = 2.2;
            DrawRoundedRect(ctx, x, y, w, h, 20);
            ctx.Fill();
            ctx.Stroke();
            ctx.Restore();

            // 2. Glassmorphic Mascot Avatar Container (Left)
            double avatarSize = 48;
            double avatarX = x + 10;
            double avatarY = y + (h - avatarSize) * 0.5;

            ctx.Save();
            ctx.FillStyle = "#ffffff";
            GameDraw.Alpha(ctx, 0.08);
            DrawRoundedRect(ctx, avatarX, avatarY, avatarSize, avatarSize, 14);
            ctx.Fill();
            ctx.StrokeStyle = "#ffffff";
            GameDraw.Alpha(ctx, 0.22);
            ctx.LineWidth = 1.5;
            ctx.Stroke();
            ctx.Restore();

            // Mascot Emoji Face with subtle playful wobble
            ctx.Save();
            double wobble = JsMath.Sin(CosmicTime * 3) * 0.06;
            ctx.Translate(avatarX + avatarSize * 0.5, avatarY + avatarSize * 0.5);
            ctx.Rotate(wobble);
            ctx.Font = "24px Fredoka, sans-serif";
            ctx.TextAlign = "center";
            ctx.TextBaseline = "middle";
            ctx.FillText(avatarFace, 0, 1);
            ctx.Restore();

            // Live Comms Pulsing Green Indicator Dot
            double dotX = avatarX + avatarSize - 4;
            double dotY = avatarY + 4;
            double pulseA = 0.35 + JsMath.Sin(CosmicTime * 6) * 0.35;

            ctx.Save();
            ctx.StrokeStyle = "#22c55e";
            GameDraw.Alpha(ctx, pulseA);
            ctx.LineWidth = 2;
            ctx.BeginPath();
            ctx.Arc(dotX, dotY, 5, 0, JsMath.PI * 2);
            ctx.Stroke();

            ctx.FillStyle = "#22c55e";
            GameDraw.Alpha(ctx, 1.0);
            ctx.BeginPath();
            ctx.Arc(dotX, dotY, 3, 0, JsMath.PI * 2);
            ctx.Fill();
            ctx.Restore();

            // 3. Comms Header Row: Sender Tag + 3 Step Pills
            double bodyX = avatarX + avatarSize + 12;
            double headerY = y + 16;

            ctx.Save();
            ctx.Font = "bold 9px Fredoka, sans-serif";
            ctx.FillStyle = accentColor;
            ctx.TextAlign = "left";
            ctx.TextBaseline = "middle";
            ctx.FillText(senderTag, bodyX, headerY);
            ctx.Restore();

            // 3 Step Progress Pills: "1. SPIN", "2. MULTIPLY", "3. FEED"
            string[] pipLabels = { "1. SPIN", "2. MULTIPLY", "3. FEED" };
            double pipW = 44;
            double pipH = 14;
            double pipSpacing = 4;
            double totalPipsW = pipLabels.Length * pipW + (pipLabels.Length - 1) * pipSpacing;
            double pipsStartX = x + w - totalPipsW - 14;

            for (int i = 0; i < pipLabels.Length; i++)
            {
                double px = pipsStartX + i * (pipW + pipSpacing);
                double py = headerY - pipH * 0.5;
                bool isActive = (i == activePipIndex);

                ctx.Save();
                if (isActive)
                {
                    ctx.FillStyle = accentColor;
                    DrawRoundedRect(ctx, px, py, pipW, pipH, 5);
                    ctx.Fill();

                    ctx.Font = "bold 8px Fredoka, sans-serif";
                    ctx.FillStyle = "#0f172a";
                    ctx.TextAlign = "center";
                    ctx.TextBaseline = "middle";
                    ctx.FillText(pipLabels[i], px + pipW * 0.5, py + pipH * 0.5);
                }
                else
                {
                    ctx.FillStyle = "#ffffff";
                    GameDraw.Alpha(ctx, 0.1);
                    DrawRoundedRect(ctx, px, py, pipW, pipH, 5);
                    ctx.Fill();

                    ctx.Font = "bold 8px Fredoka, sans-serif";
                    ctx.FillStyle = "#94a3b8";
                    GameDraw.Alpha(ctx, 0.85);
                    ctx.TextAlign = "center";
                    ctx.TextBaseline = "middle";
                    ctx.FillText(pipLabels[i], px + pipW * 0.5, py + pipH * 0.5);
                }
                ctx.Restore();
            }

            // 4. Comms Body Prompt Text
            ctx.Save();
            ctx.Font = "bold 11px Fredoka, sans-serif";
            ctx.FillStyle = "#ffffff";
            ctx.TextAlign = "left";
            ctx.TextBaseline = "bottom";
            ctx.FillText(prompt, bodyX, y + h - 14);
            ctx.Restore();
        }

        static void DrawRoundedRect(Canvas2D ctx, double x, double y, double w, double h, double r)
        {
            ctx.BeginPath();
            ctx.MoveTo(x + r, y);
            ctx.LineTo(x + w - r, y);
            ctx.Arc(x + w - r, y + r, r, -JsMath.PI * 0.5, 0);
            ctx.LineTo(x + w, y + h - r);
            ctx.Arc(x + w - r, y + h - r, r, 0, JsMath.PI * 0.5);
            ctx.LineTo(x + r, y + h);
            ctx.Arc(x + r, y + h - r, r, JsMath.PI * 0.5, JsMath.PI);
            ctx.LineTo(x, y + r);
            ctx.Arc(x + r, y + r, r, JsMath.PI, JsMath.PI * 1.5);
            ctx.ClosePath();
        }
    }
}
