using System;
using System.Globalization;

namespace OrbitalSalvage.Render
{
    /// <summary>
    /// The game still bakes <c>#gesture-hint</c> after <c>.hidden</c> (opacity 0, scale 0.9, translate -60%).
    /// The web shows that card until 3.5s, then eases it shut over 400ms, while <c>.hand-swipe-anim</c> loops.
    /// This rebuilds the live border boxes from that baked end pose. Rects are CSS pixels.
    /// </summary>
    public static class GestureHintPose
    {
        public const double DismissMs = 3500;
        public const double FadeMs = 400;
        public const double HandMs = 1800;

        public struct Box
        {
            public double X, Y, W, H;
            public Box(double x, double y, double w, double h) { X = x; Y = y; W = w; H = h; }
        }

        public struct Result
        {
            /// <summary>False once the fade has finished: the baked hidden pose is already correct.</summary>
            public bool Live;
            public double Opacity;
            public double Scale;
            public double CardTx, CardTy;
            public Box Card, Hand, Text, Emoji;
            public double HandA, HandB, HandC, HandD, HandTx, HandTy;
        }

        /// <summary>
        /// <paramref name="card"/>, <paramref name="handLayout"/>, <paramref name="text"/> and <paramref name="emoji"/>
        /// are the baked hidden-end boxes. <paramref name="handLayout"/> is the hand border box before its own transform.
        /// <paramref name="captureA"/>/<paramref name="captureB"/>/<paramref name="captureTx"/> are that hand matrix.
        /// </summary>
        public static Result At(
            double ageMs, double cssW, double cssH,
            Box card, double hiddenScale, double hiddenTx, double hiddenTy,
            Box handLayout, double captureA, double captureB, double captureTx,
            Box text, Box emoji)
        {
            var result = new Result();
            if (ageMs < 0 || hiddenScale < 1e-6) return result;
            double u;
            if (ageMs < DismissMs) u = 0;
            else if (ageMs >= DismissMs + FadeMs) return result;
            else u = CssEasing.Ease("ease", (ageMs - DismissMs) / FadeMs);

            double width = card.W / hiddenScale;
            double height = card.H / hiddenScale;
            double layoutL = cssW * 0.5;
            double layoutT = cssH * 0.5;
            double ox = layoutL + width * 0.5;
            double oy = layoutT + height * 0.5;
            double scale = 1 - (1 - hiddenScale) * u;
            double tx = hiddenTx;
            double tyVisible = -0.5 * height;
            double ty = tyVisible + (hiddenTy - tyVisible) * u;

            result.Live = true;
            result.Opacity = 1 - u;
            result.Scale = scale;
            result.CardTx = tx;
            result.CardTy = ty;
            result.Card = MapCorner(layoutL, layoutT, width, height, ox, oy, scale, tx, ty);

            HandLocal(ageMs, out double localTx, out double rotDeg);
            Box handPre = MapBox(handLayout, ox, oy, hiddenScale, hiddenTx, hiddenTy, scale, tx, ty);
            double preCx = handPre.X + handPre.W * 0.5;
            double preCy = handPre.Y + handPre.H * 0.5;
            double localW = handLayout.W / hiddenScale;
            double localH = handLayout.H / hiddenScale;
            double rad = rotDeg * Math.PI / 180.0;
            double cos = Math.Cos(rad), sin = Math.Sin(rad);
            result.Hand = Aabb(preCx + localTx * scale, preCy, localW * scale, localH * scale, cos, sin);
            result.HandA = cos;
            result.HandB = sin;
            result.HandC = -sin;
            result.HandD = cos;
            result.HandTx = localTx;
            result.HandTy = 0;

            result.Text = MapBox(text, ox, oy, hiddenScale, hiddenTx, hiddenTy, scale, tx, ty);

            double capRad = Math.Atan2(captureB, captureA);
            double capCos = Math.Cos(capRad), capSin = Math.Sin(capRad);
            Box emojiAtHidden = emoji;
            double emojiSide = emojiAtHidden.W / (hiddenScale * (Math.Abs(capCos) + Math.Abs(capSin)));
            // Undo the parent, then undo the capture hand transform, so the emoji's rest offset is in hand-local pixels.
            double ecx = emoji.X + emoji.W * 0.5;
            double ecy = emoji.Y + emoji.H * 0.5;
            double epx = ox + (ecx - ox - hiddenTx) / hiddenScale;
            double epy = oy + (ecy - oy - hiddenTy) / hiddenScale;
            double hcx = handLayout.X + handLayout.W * 0.5;
            double hcy = handLayout.Y + handLayout.H * 0.5;
            double hpx = ox + (hcx - ox - hiddenTx) / hiddenScale;
            double hpy = oy + (hcy - oy - hiddenTy) / hiddenScale;
            double dx = epx - hpx - captureTx;
            double dy = epy - hpy;
            // Inverse of rotate-then-translate: local = R^-1 * (point - translation).
            double localEx = capCos * dx + capSin * dy;
            double localEy = -capSin * dx + capCos * dy;
            double liveEx = hpx + localTx + cos * localEx - sin * localEy;
            double liveEy = hpy + sin * localEx + cos * localEy;
            double visEx = ox + scale * (liveEx - ox) + tx;
            double visEy = oy + scale * (liveEy - oy) + ty;
            double side = emojiSide * scale * (Math.Abs(cos) + Math.Abs(sin));
            result.Emoji = new Box(visEx - side * 0.5, visEy - side * 0.5, side, side);
            return result;
        }

        public static string Matrix(double a, double b, double c, double d, double tx, double ty)
        {
            return string.Format(CultureInfo.InvariantCulture, "matrix({0}, {1}, {2}, {3}, {4}, {5})", a, b, c, d, tx, ty);
        }

        static Box MapCorner(double x, double y, double w, double h, double ox, double oy, double scale, double tx, double ty)
        {
            double vx = ox + scale * (x - ox) + tx;
            double vy = oy + scale * (y - oy) + ty;
            return new Box(vx, vy, w * scale, h * scale);
        }

        static Box MapBox(Box hidden, double ox, double oy, double hiddenScale, double hiddenTx, double hiddenTy, double scale, double tx, double ty)
        {
            double cx = hidden.X + hidden.W * 0.5;
            double cy = hidden.Y + hidden.H * 0.5;
            double px = ox + (cx - ox - hiddenTx) / hiddenScale;
            double py = oy + (cy - oy - hiddenTy) / hiddenScale;
            double pw = hidden.W / hiddenScale;
            double ph = hidden.H / hiddenScale;
            double vx = ox + scale * (px - ox) + tx;
            double vy = oy + scale * (py - oy) + ty;
            return new Box(vx - pw * scale * 0.5, vy - ph * scale * 0.5, pw * scale, ph * scale);
        }

        static Box Aabb(double cx, double cy, double w, double h, double cos, double sin)
        {
            double aw = w * Math.Abs(cos) + h * Math.Abs(sin);
            double ah = w * Math.Abs(sin) + h * Math.Abs(cos);
            return new Box(cx - aw * 0.5, cy - ah * 0.5, aw, ah);
        }

        /// <summary>handSwipe 1.8s ease-in-out, infinite. 0% and 100% are the same pose.</summary>
        public static void HandLocal(double ageMs, out double tx, out double rotDeg)
        {
            double t = ageMs % HandMs;
            if (t < 0) t += HandMs;
            double p = t / HandMs;
            double e;
            if (p <= 0.5)
            {
                e = CssEasing.Ease("ease-in-out", p / 0.5);
                tx = -24 + 48 * e;
                rotDeg = -12 + 24 * e;
            }
            else
            {
                e = CssEasing.Ease("ease-in-out", (p - 0.5) / 0.5);
                tx = 24 - 48 * e;
                rotDeg = 12 - 24 * e;
            }
        }
    }
}
