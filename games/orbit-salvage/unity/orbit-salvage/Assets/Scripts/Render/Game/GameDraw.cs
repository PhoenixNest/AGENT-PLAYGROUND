using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Logic;

namespace OrbitalSalvage.Render
{
    public sealed class StarSprite
    {
        public double X, Y, Size, Speed;
        public string Type;
        public string Color;
    }

    public sealed class PlanetSprite
    {
        public double X, Y, Radius;
        public string Color;
        public bool Ring;
        public string RingColor;
    }

    public sealed class SingularityParticle
    {
        public double RRatio, Angle, Speed, InwardRate, Size, Twinkle, TwinkleSpeed;
        public string Color;
    }

    public sealed class ConfettiParticle
    {
        public double X, Y, Vx, Vy, Gravity, Rot, VRot, Life, MaxLife, W, H;
        public string Color;
    }

    public sealed class FloatingText
    {
        public double X, Y, Alpha, Vy, Scale, Life;
        public string Text;
        public string Color;
    }

    public sealed class PopRing
    {
        public double X, Y, Radius, RadiusMax, Alpha;
        public string Color;
        public double? LineWidth;
        public double? Speed;
        public double? FadeRate;
    }

    public sealed class ThrusterPuff
    {
        public double X, Y, Vx, Vy, Radius, Life, MaxLife;
        public string Color;
    }

    /// <summary>Progress passed to <see cref="GameRenderer.Render"/> (web <c>progressInfo</c>).</summary>
    public sealed class RenderProgress
    {
        public double CollectedMass;
        public double Quota;
    }

    static class GameDraw
    {
        public static double Rand()
        {
            return JsRandom.Global.NextDouble();
        }

        public static string Or(string value, string fallback)
        {
            return string.IsNullOrEmpty(value) ? fallback : value;
        }

        public static double Or1(double v)
        {
            return (v == 0 || double.IsNaN(v)) ? 1 : v;
        }

        public static double OrNum(double v, double fallback)
        {
            return (v == 0 || double.IsNaN(v)) ? fallback : v;
        }

        public static string Rgba(int r, int g, int b, double a)
        {
            return "rgba(" + r.ToString(System.Globalization.CultureInfo.InvariantCulture) + ", "
                + g.ToString(System.Globalization.CultureInfo.InvariantCulture) + ", "
                + b.ToString(System.Globalization.CultureInfo.InvariantCulture) + ", "
                + JsNumber.ToString(a) + ")";
        }

        public static void Alpha(Canvas2D ctx, double a)
        {
            if (a < 0) a = 0;
            else if (a > 1) a = 1;
            ctx.GlobalAlpha = a;
        }

        public static CosmeticItem FindSkin(CosmeticItem[] items, string id)
        {
            if (items == null || items.Length == 0) return null;
            if (!string.IsNullOrEmpty(id))
            {
                for (int i = 0; i < items.Length; i++)
                    if (items[i].Id == id) return items[i];
            }
            return items[0];
        }

        public static string SkyStop(CosmeticItem skin, int index, string fallback)
        {
            if (skin == null || skin.BgGradient == null || index >= skin.BgGradient.Length) return fallback;
            return Or(skin.BgGradient[index], fallback);
        }
    }
}
