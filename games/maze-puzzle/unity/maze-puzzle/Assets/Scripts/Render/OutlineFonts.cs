using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitalSalvage.Render
{
    /// <summary>A glyph outline flattened to polylines in CSS px (y down, origin at the glyph origin).</summary>
    public sealed class FlatGlyph
    {
        public float[] pts = Array.Empty<float>(); // x0,y0,x1,y1,...
        public int[] starts = Array.Empty<int>();  // point index (not float index) where each contour starts
        public int contourCount;
    }

    /// <summary>
    /// Font registry for the outline text rasteriser (SPEC 6.4, version 1.6). Reads the same static TTFs as the web
    /// harness (shipped as TextAssets under Resources/Fonts/Ttf), selects faces with <see cref="ParityFonts"/>'s CSS
    /// weight matching and caches flattened glyph outlines per (font, glyph, quantised size).
    /// </summary>
    public static class OutlineFonts
    {
        public const string ResourcesDir = "Fonts/Ttf/";
        public static readonly string[] SymbolChain = { "NotoSansSymbols2-Regular", "NotoSansSymbols-Regular" };

        /// <summary>Flattening tolerance in device pixels.</summary>
        public static double Tolerance = 0.02;

        static readonly Dictionary<string, TrueTypeFont> Fonts = new Dictionary<string, TrueTypeFont>();
        static readonly Dictionary<TrueTypeFont, int> Ids = new Dictionary<TrueTypeFont, int>();
        static readonly Dictionary<string, TrueTypeFont[]> Chains = new Dictionary<string, TrueTypeFont[]>();
        static readonly Dictionary<ulong, FlatGlyph> Cache = new Dictionary<ulong, FlatGlyph>();
        static readonly Dictionary<ulong, GlyphOutline> Raw = new Dictionary<ulong, GlyphOutline>();
        static readonly List<float> scratch = new List<float>(256);
        static readonly List<int> scratchStarts = new List<int>(8);

        public static TrueTypeFont Load(string file)
        {
            if (Fonts.TryGetValue(file, out var f)) return f;
            var asset = Resources.Load<TextAsset>(ResourcesDir + file);
            if (asset == null) throw new InvalidOperationException($"[OutlineFonts] missing Resources/{ResourcesDir}{file}.bytes (run ParityAssetBuilder.BuildAll)");
            f = new TrueTypeFont(file, asset.bytes);
            Fonts[file] = f;
            Ids[f] = Ids.Count;
            return f;
        }

        /// <summary>Primary face for a CSS (family, weight): Nunito 400 draws Nunito 700, Fredoka 800/900 draw Fredoka 700.</summary>
        public static TrueTypeFont Primary(string family, int cssWeight)
        {
            int face = ParityFonts.ResolveWeight(family, cssWeight);
            if (face < 0) face = 700;
            string fam = string.Equals(family, ParityFonts.Fredoka, StringComparison.OrdinalIgnoreCase) ? ParityFonts.Fredoka : ParityFonts.Nunito;
            return Load(fam + "-" + face);
        }

        /// <summary>Primary face followed by Noto Sans Symbols 2 and Noto Sans Symbols (SPEC 5.1).</summary>
        public static TrueTypeFont[] Chain(TrueTypeFont primary)
        {
            if (Chains.TryGetValue(primary.name, out var c)) return c;
            c = new[] { primary, Load(SymbolChain[0]), Load(SymbolChain[1]) };
            Chains[primary.name] = c;
            return c;
        }

        /// <summary>
        /// Same faces as <see cref="Chain(TrueTypeFont)"/> today. A later task prepends language faces
        /// for <paramref name="fontGroup"/> (<c>cjk-sc</c>, <c>arabic</c>, …). The cache key is
        /// (primary, group). An empty group uses the Latin chain unchanged.
        /// </summary>
        public static TrueTypeFont[] Chain(TrueTypeFont primary, string fontGroup)
        {
            if (string.IsNullOrEmpty(fontGroup)) return Chain(primary);
            string key = primary.name + "\n" + fontGroup;
            if (Chains.TryGetValue(key, out var c)) return c;
            // TODO: prepend the faces registered for fontGroup. None are shipped yet.
            c = new[] { primary, Load(SymbolChain[0]), Load(SymbolChain[1]) };
            Chains[key] = c;
            return c;
        }

        public static void Clear()
        {
            Fonts.Clear(); Ids.Clear(); Chains.Clear(); Cache.Clear(); Raw.Clear();
        }

        /// <summary>
        /// Flattened outline of a glyph at <paramref name="px"/> CSS px. <paramref name="devScale"/> is the maximum
        /// stretch of the CTM and only controls the flattening tolerance.
        /// </summary>
        public static FlatGlyph Flat(TrueTypeFont font, int gid, double px, double devScale)
        {
            int sk = (int)Math.Min(0xFFFFFF, Math.Max(1, Math.Round(devScale * 8)));
            int pk = (int)Math.Min(0xFFFFF, Math.Round(px * 64));
            ulong key = ((ulong)Ids[font] << 60) | ((ulong)gid << 44) | ((ulong)pk << 24) | (uint)sk;
            PaintProfile.Begin(PaintProfile.GlyphFlat);
            if (Cache.TryGetValue(key, out var g)) { PaintProfile.End(PaintProfile.GlyphFlat); return g; }
            PaintProfile.FlatMisses++;
            ulong rawKey = ((ulong)Ids[font] << 32) | (uint)gid;
            if (!Raw.TryGetValue(rawKey, out var o)) { o = font.Outline(gid); Raw[rawKey] = o; }
            g = Flatten(o, px / font.unitsPerEm, Tolerance / Math.Max(devScale, 1e-6));
            Cache[key] = g;
            PaintProfile.End(PaintProfile.GlyphFlat);
            return g;
        }

        static FlatGlyph Flatten(GlyphOutline o, double scale, double tol)
        {
            scratch.Clear(); scratchStarts.Clear();
            int s = 0;
            foreach (int e in o.ends)
            {
                int n = e - s + 1;
                if (n >= 3) FlattenContour(o, s, e, scale, tol);
                s = e + 1;
            }
            return new FlatGlyph { pts = scratch.ToArray(), starts = scratchStarts.ToArray(), contourCount = scratchStarts.Count };
        }

        struct P { public double x, y; public bool on; }
        static readonly List<P> ring = new List<P>(64);

        static void FlattenContour(GlyphOutline o, int s, int e, double sc, double tol)
        {
            ring.Clear();
            P At(int i) => new P { x = o.xs[i] * sc, y = -o.ys[i] * sc, on = o.on[i] };
            if (o.on[s]) for (int i = s; i <= e; i++) ring.Add(At(i));
            else if (o.on[e]) { ring.Add(At(e)); for (int i = s; i < e; i++) ring.Add(At(i)); }
            else
            {
                var a = At(s); var b = At(e);
                ring.Add(new P { x = (a.x + b.x) * 0.5, y = (a.y + b.y) * 0.5, on = true });
                for (int i = s; i <= e; i++) ring.Add(At(i));
            }
            int m = ring.Count;
            scratchStarts.Add(scratch.Count / 2);
            var cur = ring[0];
            scratch.Add((float)cur.x); scratch.Add((float)cur.y);
            int k = 1;
            while (k <= m)
            {
                var p = ring[k % m];
                if (p.on) { AddPt(p); cur = p; k++; continue; }
                var nx = ring[(k + 1) % m];
                if (nx.on) { Quad(cur, p, nx, tol); cur = nx; k += 2; }
                else
                {
                    var mid = new P { x = (p.x + nx.x) * 0.5, y = (p.y + nx.y) * 0.5, on = true };
                    Quad(cur, p, mid, tol); cur = mid; k++;
                }
            }
            // drop the closing duplicate of the start point
            int cnt = scratch.Count;
            if (cnt >= 4 && scratch[cnt - 2] == scratch[scratchStarts[scratchStarts.Count - 1] * 2] && scratch[cnt - 1] == scratch[scratchStarts[scratchStarts.Count - 1] * 2 + 1])
                scratch.RemoveRange(cnt - 2, 2);
        }

        static void AddPt(P p) { scratch.Add((float)p.x); scratch.Add((float)p.y); }

        static void Quad(P a, P c, P b, double tol)
        {
            double dx = a.x - 2 * c.x + b.x, dy = a.y - 2 * c.y + b.y;
            double dd = Math.Sqrt(dx * dx + dy * dy);
            int n = Math.Max(1, Math.Min(64, (int)Math.Ceiling(Math.Sqrt(dd / (4 * tol)))));
            for (int i = 1; i <= n; i++)
            {
                double t = (double)i / n, u = 1 - t;
                scratch.Add((float)(u * u * a.x + 2 * u * t * c.x + t * t * b.x));
                scratch.Add((float)(u * u * a.y + 2 * u * t * c.y + t * t * b.y));
            }
        }
    }
}
