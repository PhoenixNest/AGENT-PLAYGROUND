using System;
using System.Collections.Generic;
using System.Globalization;

namespace OrbitalSalvage.Render
{
    /// <summary>Straight (non-premultiplied) RGBA, components in 0..1.</summary>
    public struct Rgba
    {
        public float r, g, b, a;
        public Rgba(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static readonly Rgba Transparent = new Rgba(0, 0, 0, 0);
        public static readonly Rgba Black = new Rgba(0, 0, 0, 1);
        public static readonly Rgba White = new Rgba(1, 1, 1, 1);
        public static readonly Rgba Green = new Rgba(0.29f, 0.87f, 0.49f, 1);
        public static readonly Rgba Yellow = new Rgba(0.98f, 0.8f, 0.08f, 1);
    }

    public static class CssColor
    {
        static readonly Dictionary<string, uint> Named = new Dictionary<string, uint>
        {
            { "black", 0x000000 }, { "white", 0xffffff }, { "red", 0xff0000 }, { "lime", 0x00ff00 }, { "green", 0x008000 },
            { "blue", 0x0000ff }, { "yellow", 0xffff00 }, { "cyan", 0x00ffff }, { "aqua", 0x00ffff }, { "magenta", 0xff00ff },
            { "fuchsia", 0xff00ff }, { "gray", 0x808080 }, { "grey", 0x808080 }, { "silver", 0xc0c0c0 }, { "maroon", 0x800000 },
            { "olive", 0x808000 }, { "navy", 0x000080 }, { "purple", 0x800080 }, { "teal", 0x008080 }, { "orange", 0xffa500 },
            { "pink", 0xffc0cb }, { "gold", 0xffd700 }, { "brown", 0xa52a2a }, { "indigo", 0x4b0082 }, { "violet", 0xee82ee },
            { "coral", 0xff7f50 }, { "crimson", 0xdc143c }, { "tomato", 0xff6347 }, { "salmon", 0xfa8072 }, { "khaki", 0xf0e68c },
        };

        public static bool TryParse(string s, out Rgba c)
        {
            c = Rgba.Black;
            if (s == null || s.Length == 0) return false;
            if (s.IndexOf('(') < 0 && Cache.TryGetValue(s, out c)) return true;
            string raw = s;
            s = s.Trim();
            if (s.Length == 0) return false;
            bool ok = false;
            if (s[0] == '#') ok = ParseHex(s, out c);
            else
            {
                string lower = s.ToLowerInvariant();
                if (lower == "transparent") { c = Rgba.Transparent; ok = true; }
                else if (Named.TryGetValue(lower, out uint rgb))
                {
                    c = new Rgba(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, 1f);
                    ok = true;
                }
                else
                {
                    int paren = lower.IndexOf('(');
                    if (paren > 0 && lower.EndsWith(")"))
                    {
                        string fn = lower.Substring(0, paren).Trim();
                        string body = lower.Substring(paren + 1, lower.Length - paren - 2);
                        var parts = Split(body);
                        if (parts != null && (fn == "rgb" || fn == "rgba")) ok = ParseRgb(parts, out c);
                        else if (parts != null && (fn == "hsl" || fn == "hsla")) ok = ParseHsl(parts, out c);
                    }
                }
            }
            if (ok && raw.IndexOf('(') < 0 && raw.Length <= 32) Cache[raw] = c;
            return ok;
        }

        static readonly Dictionary<string, Rgba> Cache = new Dictionary<string, Rgba>(64);

        public static Rgba Parse(string s, Rgba fallback) => TryParse(s, out var c) ? c : fallback;

        static bool ParseHex(string s, out Rgba c)
        {
            c = Rgba.Black;
            string h = s.Substring(1);
            foreach (char ch in h) if (Uri.IsHexDigit(ch) == false) return false;
            int V(string str) => int.Parse(str, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            switch (h.Length)
            {
                case 3:
                    c = new Rgba(V(h.Substring(0, 1)) * 17 / 255f, V(h.Substring(1, 1)) * 17 / 255f, V(h.Substring(2, 1)) * 17 / 255f, 1);
                    return true;
                case 4:
                    c = new Rgba(V(h.Substring(0, 1)) * 17 / 255f, V(h.Substring(1, 1)) * 17 / 255f, V(h.Substring(2, 1)) * 17 / 255f, V(h.Substring(3, 1)) * 17 / 255f);
                    return true;
                case 6:
                    c = new Rgba(V(h.Substring(0, 2)) / 255f, V(h.Substring(2, 2)) / 255f, V(h.Substring(4, 2)) / 255f, 1);
                    return true;
                case 8:
                    c = new Rgba(V(h.Substring(0, 2)) / 255f, V(h.Substring(2, 2)) / 255f, V(h.Substring(4, 2)) / 255f, V(h.Substring(6, 2)) / 255f);
                    return true;
            }
            return false;
        }

        static List<string> Split(string body)
        {
            var parts = new List<string>(4);
            var sb = new System.Text.StringBuilder();
            foreach (char ch in body)
            {
                if (ch == ',' || ch == ' ' || ch == '/' || ch == '\t')
                {
                    if (sb.Length > 0) { parts.Add(sb.ToString()); sb.Length = 0; }
                }
                else sb.Append(ch);
            }
            if (sb.Length > 0) parts.Add(sb.ToString());
            return parts.Count >= 3 && parts.Count <= 4 ? parts : null;
        }

        static bool Num(string t, out double v, out bool pct)
        {
            pct = t.EndsWith("%");
            if (pct) t = t.Substring(0, t.Length - 1);
            else if (t.EndsWith("deg")) t = t.Substring(0, t.Length - 3);
            return double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out v);
        }

        static bool ParseAlpha(List<string> parts, out float a)
        {
            a = 1f;
            if (parts.Count < 4) return true;
            if (!Num(parts[3], out double v, out bool pct)) return false;
            if (pct) v /= 100.0;
            a = (float)Math.Max(0, Math.Min(1, v));
            return true;
        }

        static bool ParseRgb(List<string> parts, out Rgba c)
        {
            c = Rgba.Black;
            var ch = new float[3];
            for (int i = 0; i < 3; i++)
            {
                if (!Num(parts[i], out double v, out bool pct)) return false;
                if (pct) v = v * 255.0 / 100.0;
                ch[i] = (float)(Math.Max(0, Math.Min(255, Math.Round(v))) / 255.0);
            }
            if (!ParseAlpha(parts, out float a)) return false;
            c = new Rgba(ch[0], ch[1], ch[2], a);
            return true;
        }

        static bool ParseHsl(List<string> parts, out Rgba c)
        {
            c = Rgba.Black;
            if (!Num(parts[0], out double h, out _) || !Num(parts[1], out double s, out _) || !Num(parts[2], out double l, out _)) return false;
            h = ((h % 360) + 360) % 360 / 360.0;
            s = Math.Max(0, Math.Min(1, s / 100.0));
            l = Math.Max(0, Math.Min(1, l / 100.0));
            double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            double p = 2 * l - q;
            double Hue(double t)
            {
                if (t < 0) t += 1;
                if (t > 1) t -= 1;
                if (t < 1.0 / 6) return p + (q - p) * 6 * t;
                if (t < 0.5) return q;
                if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
                return p;
            }
            if (!ParseAlpha(parts, out float a)) return false;
            c = new Rgba((float)Hue(h + 1.0 / 3), (float)Hue(h), (float)Hue(h - 1.0 / 3), a);
            if (s == 0) c = new Rgba((float)l, (float)l, (float)l, a);
            return true;
        }
    }
}
