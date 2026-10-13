using System;
using System.Collections.Generic;
using OrbitalSalvage.Loc;
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
        static readonly Dictionary<string, int> ChainStamps = new Dictionary<string, int>();
        static readonly HashSet<string> MissingFiles = new HashSet<string>();
        static int nextStamp = 1;
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
        /// Language face list from <see cref="LocFonts"/>. <paramref name="language"/> <c>en</c> returns
        /// <see cref="Chain(TrueTypeFont)"/> unchanged (same font objects). A missing file is skipped.
        /// A missing script face falls back to the German Latin chain. Never throws.
        /// </summary>
        public static TrueTypeFont[] ChainFor(string language, string family, int cssWeight)
        {
            EnsureManifestLoader();
            string key = ChainKey(language, family, cssWeight);
            if (Chains.TryGetValue(key, out var cached)) return cached;
            TrueTypeFont[] chain;
            if (string.IsNullOrEmpty(language) || language == LanguageRegistry.DefaultCode)
                chain = Chain(Primary(family, cssWeight));
            else
            {
                string[] names = LocFonts.GetChain(language, family, cssWeight);
                chain = LoadNames(names);
                if (LocFonts.NeedsLatinFallback(language, names, NamesOf(chain)))
                    chain = LoadNames(LocFonts.GetChain("de", family, cssWeight));
            }
            Chains[key] = chain;
            if (!ChainStamps.ContainsKey(key)) ChainStamps[key] = nextStamp++;
            return chain;
        }

        public static int ChainStamp(string language, string family, int cssWeight)
        {
            string key = ChainKey(language, family, cssWeight);
            if (!ChainStamps.ContainsKey(key)) ChainFor(language, family, cssWeight);
            int stamp;
            return ChainStamps.TryGetValue(key, out stamp) ? stamp : 1;
        }

        /// <summary>First integer and the family after <c>px</c> in a paint font string such as <c>800 14px Fredoka</c>.</summary>
        public static void SplitCssFont(string font, out int weight, out string family)
        {
            weight = 400;
            family = ParityFonts.Fredoka;
            if (string.IsNullOrEmpty(font)) return;
            int space = font.IndexOf(' ');
            if (space > 0)
            {
                int parsed;
                if (int.TryParse(font.Substring(0, space), out parsed)) weight = parsed;
            }
            int px = font.IndexOf("px", StringComparison.Ordinal);
            if (px < 0 || px + 2 > font.Length) return;
            string rest = font.Substring(px + 2).Trim();
            if (rest.Length > 0) family = rest;
        }

        /// <summary>
        /// Group hook kept for callers that only know a font group. Language selection goes through
        /// <see cref="ChainFor"/>. An empty group is the Latin chain.
        /// </summary>
        public static TrueTypeFont[] Chain(TrueTypeFont primary, string fontGroup)
        {
            if (string.IsNullOrEmpty(fontGroup)) return Chain(primary);
            string key = primary.name + "\n" + fontGroup;
            if (Chains.TryGetValue(key, out var c)) return c;
            c = new[] { primary, Load(SymbolChain[0]), Load(SymbolChain[1]) };
            Chains[key] = c;
            return c;
        }

        public static void Clear()
        {
            Fonts.Clear(); Ids.Clear(); Chains.Clear(); ChainStamps.Clear(); MissingFiles.Clear();
            Cache.Clear(); Raw.Clear();
            nextStamp = 1;
            ShapedText.Clear();
        }

        static void EnsureManifestLoader()
        {
            if (LocFonts.HasLoader) return;
            LocFonts.SetLoader(new ResourcesFontManifestLoader());
        }

        static string ChainKey(string language, string family, int cssWeight)
        {
            return (language ?? "") + "\n" + (family ?? "") + "\n" + cssWeight;
        }

        static TrueTypeFont[] LoadNames(string[] names)
        {
            if (names == null || names.Length == 0) return Array.Empty<TrueTypeFont>();
            var list = new List<TrueTypeFont>(names.Length);
            for (int i = 0; i < names.Length; i++)
            {
                TrueTypeFont font;
                if (TryLoad(names[i], out font)) list.Add(font);
            }
            return list.ToArray();
        }

        static string[] NamesOf(TrueTypeFont[] fonts)
        {
            if (fonts == null || fonts.Length == 0) return Array.Empty<string>();
            var names = new string[fonts.Length];
            for (int i = 0; i < fonts.Length; i++) names[i] = fonts[i].name;
            return names;
        }

        static bool TryLoad(string file, out TrueTypeFont font)
        {
            font = null;
            if (string.IsNullOrEmpty(file)) return false;
            if (Fonts.TryGetValue(file, out font)) return true;
            if (MissingFiles.Contains(file)) return false;
            var asset = Resources.Load<TextAsset>(ResourcesDir + file);
            if (asset == null || asset.bytes == null || asset.bytes.Length == 0)
            {
                MissingFiles.Add(file);
                return false;
            }
            try
            {
                font = new TrueTypeFont(file, asset.bytes);
            }
            catch (Exception)
            {
                MissingFiles.Add(file);
                font = null;
                return false;
            }
            Fonts[file] = font;
            Ids[font] = Ids.Count;
            return true;
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
