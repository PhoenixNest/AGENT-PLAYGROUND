using System;
using System.Collections.Generic;

namespace OrbitalSalvage.Render
{
    /// <summary>
    /// Glyph outline in font units, y up: points of every contour in one array. <c>ends[i]</c> is the index of the last
    /// point of contour i. Quadratic (TrueType) outlines only.
    /// </summary>
    public sealed class GlyphOutline
    {
        public readonly List<float> xs = new List<float>(), ys = new List<float>();
        public readonly List<bool> on = new List<bool>();
        public readonly List<int> ends = new List<int>();
        public int PointCount => xs.Count;
    }

    /// <summary>
    /// Minimal TrueType reader for the parity fonts: head, hhea, OS/2, maxp, cmap (formats 4 and 12), hmtx, loca, glyf
    /// (simple and compound glyphs), GPOS pair adjustment for the <c>kern</c> feature (lookup type 2 formats 1 and 2,
    /// extension type 9) and GSUB ligature substitution for <c>liga</c> (lookup type 4, extension type 7).
    /// Everything is read straight from the byte array; no native code, no allocations after load except caches.
    /// </summary>
    public sealed class TrueTypeFont
    {
        readonly byte[] d;
        readonly Dictionary<string, int> tableOffset = new Dictionary<string, int>();
        public readonly string name;
        public readonly int unitsPerEm;
        public readonly int ascender, descender, lineGap;
        public readonly int typoAscender, typoDescender;
        public readonly int numGlyphs;
        readonly int glyfOff, locaOff, locaFormat, hmtxOff, numHMetrics, cmap4 = -1, cmap12 = -1;

        // GPOS kern lookups: offsets of the lookup tables, in application order.
        readonly List<int> kernLookups = new List<int>();
        // GSUB liga lookups.
        readonly List<int> ligaLookups = new List<int>();
        readonly Dictionary<uint, float> kernCache = new Dictionary<uint, float>();
        readonly Dictionary<int, int> cmapCache = new Dictionary<int, int>();

        public TrueTypeFont(string name, byte[] data)
        {
            this.name = name;
            d = data;
            int numTables = U16(4);
            for (int i = 0; i < numTables; i++)
            {
                int r = 12 + i * 16;
                tableOffset[Tag(r)] = (int)U32(r + 8);
            }
            int head = Table("head");
            unitsPerEm = U16(head + 18);
            locaFormat = I16(head + 50);
            int hhea = Table("hhea");
            ascender = I16(hhea + 4);
            descender = I16(hhea + 6);
            lineGap = I16(hhea + 8);
            numHMetrics = U16(hhea + 34);
            typoAscender = ascender; typoDescender = descender;
            if (tableOffset.TryGetValue("OS/2", out int os2) && U16(os2) >= 0)
            {
                typoAscender = I16(os2 + 68);
                typoDescender = I16(os2 + 70);
            }
            numGlyphs = U16(Table("maxp") + 4);
            hmtxOff = Table("hmtx");
            locaOff = Table("loca");
            glyfOff = Table("glyf");

            int cm = Table("cmap");
            int n = U16(cm + 2);
            for (int i = 0; i < n; i++)
            {
                int plat = U16(cm + 4 + i * 8), enc = U16(cm + 6 + i * 8);
                int off = cm + (int)U32(cm + 8 + i * 8);
                int fmt = U16(off);
                bool unicode = plat == 0 || (plat == 3 && (enc == 1 || enc == 10));
                if (!unicode) continue;
                if (fmt == 12 && cmap12 < 0) cmap12 = off;
                else if (fmt == 4 && cmap4 < 0) cmap4 = off;
            }
            if (tableOffset.TryGetValue("GPOS", out int gpos)) CollectLookups(gpos, new[] { "kern" }, kernLookups);
            // HarfBuzz default-on substitution features that matter for Latin text, applied in lookup-index order.
            if (tableOffset.TryGetValue("GSUB", out int gsub)) CollectLookups(gsub, new[] { "ccmp", "liga" }, ligaLookups);
        }

        // ---- byte access -----------------------------------------------------------------------

        int U8(int o) => d[o];
        int U16(int o) => (d[o] << 8) | d[o + 1];
        int I16(int o) { int v = (d[o] << 8) | d[o + 1]; return v >= 0x8000 ? v - 0x10000 : v; }
        uint U32(int o) => ((uint)d[o] << 24) | ((uint)d[o + 1] << 16) | ((uint)d[o + 2] << 8) | d[o + 3];
        string Tag(int o) => new string(new[] { (char)d[o], (char)d[o + 1], (char)d[o + 2], (char)d[o + 3] });
        int Table(string t)
        {
            if (!tableOffset.TryGetValue(t, out int o)) throw new InvalidOperationException($"{name}: missing table '{t}'");
            return o;
        }

        // ---- cmap / metrics --------------------------------------------------------------------

        /// <summary>Glyph index of a code point, 0 when the font has no glyph.</summary>
        public int GlyphIndex(int cp)
        {
            if (cmapCache.TryGetValue(cp, out int cached)) return cached;
            int g = 0;
            if (cmap12 >= 0) g = Lookup12(cmap12, cp);
            if (g == 0 && cmap4 >= 0 && cp <= 0xFFFF) g = Lookup4(cmap4, cp);
            cmapCache[cp] = g;
            return g;
        }

        int Lookup12(int o, int cp)
        {
            int lo = 0, hi = (int)U32(o + 12) - 1;
            while (lo <= hi)
            {
                int mid = (lo + hi) >> 1;
                int g = o + 16 + mid * 12;
                uint s = U32(g), e = U32(g + 4);
                if (cp < s) hi = mid - 1;
                else if (cp > e) lo = mid + 1;
                else return (int)(U32(g + 8) + (uint)(cp - s));
            }
            return 0;
        }

        int Lookup4(int o, int cp)
        {
            int segX2 = U16(o + 6);
            int endOff = o + 14, startOff = endOff + segX2 + 2, deltaOff = startOff + segX2, rangeOff = deltaOff + segX2;
            for (int i = 0; i < segX2; i += 2)
            {
                if (U16(endOff + i) < cp) continue;
                int start = U16(startOff + i);
                if (cp < start) return 0;
                int delta = I16(deltaOff + i), ro = U16(rangeOff + i);
                if (ro == 0) return (cp + delta) & 0xFFFF;
                int gi = U16(rangeOff + i + ro + (cp - start) * 2);
                return gi == 0 ? 0 : (gi + delta) & 0xFFFF;
            }
            return 0;
        }

        /// <summary>Horizontal advance in font units.</summary>
        public int Advance(int gid)
        {
            int i = gid < numHMetrics ? gid : numHMetrics - 1;
            return U16(hmtxOff + i * 4);
        }

        // ---- outlines --------------------------------------------------------------------------

        public GlyphOutline Outline(int gid)
        {
            var o = new GlyphOutline();
            Load(gid, o, 1, 0, 0, 1, 0, 0, 0);
            return o;
        }

        bool GlyphRange(int gid, out int off, out int len)
        {
            off = len = 0;
            if (gid < 0 || gid >= numGlyphs) return false;
            int a, b;
            if (locaFormat == 0) { a = U16(locaOff + gid * 2) * 2; b = U16(locaOff + gid * 2 + 2) * 2; }
            else { a = (int)U32(locaOff + gid * 4); b = (int)U32(locaOff + gid * 4 + 4); }
            if (b <= a) return false;
            off = glyfOff + a; len = b - a;
            return true;
        }

        void Load(int gid, GlyphOutline dst, double a, double b, double c, double dd, double tx, double ty, int depth)
        {
            if (depth > 6 || !GlyphRange(gid, out int p, out _)) return;
            int nc = I16(p);
            p += 10;
            if (nc >= 0)
            {
                var ends = new int[nc];
                for (int i = 0; i < nc; i++) { ends[i] = U16(p); p += 2; }
                int npts = nc == 0 ? 0 : ends[nc - 1] + 1;
                int ilen = U16(p); p += 2 + ilen;
                var flags = new byte[npts];
                for (int i = 0; i < npts;)
                {
                    byte f = d[p++];
                    flags[i++] = f;
                    if ((f & 8) != 0)
                    {
                        int rep = d[p++];
                        while (rep-- > 0 && i < npts) flags[i++] = f;
                    }
                }
                var px = new int[npts]; var py = new int[npts];
                int v = 0;
                for (int i = 0; i < npts; i++)
                {
                    byte f = flags[i];
                    if ((f & 2) != 0) { int dx = d[p++]; v += (f & 16) != 0 ? dx : -dx; }
                    else if ((f & 16) == 0) { v += I16(p); p += 2; }
                    px[i] = v;
                }
                v = 0;
                for (int i = 0; i < npts; i++)
                {
                    byte f = flags[i];
                    if ((f & 4) != 0) { int dy = d[p++]; v += (f & 32) != 0 ? dy : -dy; }
                    else if ((f & 32) == 0) { v += I16(p); p += 2; }
                    py[i] = v;
                }
                int baseIdx = dst.PointCount;
                for (int i = 0; i < npts; i++)
                {
                    dst.xs.Add((float)(a * px[i] + c * py[i] + tx));
                    dst.ys.Add((float)(b * px[i] + dd * py[i] + ty));
                    dst.on.Add((flags[i] & 1) != 0);
                }
                for (int i = 0; i < nc; i++) dst.ends.Add(baseIdx + ends[i]);
                return;
            }

            // compound glyph
            while (true)
            {
                int flags = U16(p), comp = U16(p + 2);
                p += 4;
                double dx, dy;
                if ((flags & 1) != 0)
                {
                    if ((flags & 2) != 0) { dx = I16(p); dy = I16(p + 2); } else { dx = dy = 0; }
                    p += 4;
                }
                else
                {
                    if ((flags & 2) != 0) { dx = (sbyte)d[p]; dy = (sbyte)d[p + 1]; } else { dx = dy = 0; }
                    p += 2;
                }
                double ma = 1, mb = 0, mc = 0, md = 1;
                if ((flags & 8) != 0) { ma = md = I16(p) / 16384.0; p += 2; }
                else if ((flags & 0x40) != 0) { ma = I16(p) / 16384.0; md = I16(p + 2) / 16384.0; p += 4; }
                else if ((flags & 0x80) != 0)
                {
                    ma = I16(p) / 16384.0; mb = I16(p + 2) / 16384.0; mc = I16(p + 4) / 16384.0; md = I16(p + 6) / 16384.0; p += 8;
                }
                // component transform T = [ma mc; mb md] applied to points, then offset (dx, dy); parent transform on top
                double na = a * ma + c * mb, nb = b * ma + dd * mb;
                double nc2 = a * mc + c * md, nd = b * mc + dd * md;
                double ntx = a * dx + c * dy + tx, nty = b * dx + dd * dy + ty;
                Load(comp, dst, na, nb, nc2, nd, ntx, nty, depth + 1);
                if ((flags & 0x20) == 0) break;
            }
        }

        // ---- layout tables ---------------------------------------------------------------------

        void CollectLookups(int table, string[] features, List<int> into)
        {
            int scriptList = table + U16(table + 4), featureList = table + U16(table + 6), lookupList = table + U16(table + 8);
            int sc = U16(scriptList);
            int script = -1;
            for (int pass = 0; pass < 2 && script < 0; pass++)
            {
                string want = pass == 0 ? "latn" : "DFLT";
                for (int i = 0; i < sc; i++)
                    if (Tag(scriptList + 2 + i * 6) == want) { script = scriptList + U16(scriptList + 2 + i * 6 + 4); break; }
            }
            if (script < 0) return;
            int defLang = U16(script);
            if (defLang == 0) return;
            int ls = script + defLang;
            int fc = U16(ls + 4);
            var idx = new SortedSet<int>();
            for (int i = 0; i < fc; i++)
            {
                int fi = U16(ls + 6 + i * 2);
                int fr = featureList + 2 + fi * 6;
                if (Array.IndexOf(features, Tag(fr)) < 0) continue;
                int ft = featureList + U16(fr + 4);
                int lc = U16(ft + 2);
                for (int k = 0; k < lc; k++) idx.Add(U16(ft + 4 + k * 2));
            }
            foreach (int li in idx) into.Add(lookupList + U16(lookupList + 2 + li * 2));
        }

        int Coverage(int o, int gid)
        {
            int fmt = U16(o);
            int n = U16(o + 2);
            if (fmt == 1)
            {
                int lo = 0, hi = n - 1;
                while (lo <= hi)
                {
                    int mid = (lo + hi) >> 1, g = U16(o + 4 + mid * 2);
                    if (gid < g) hi = mid - 1; else if (gid > g) lo = mid + 1; else return mid;
                }
                return -1;
            }
            for (int i = 0; i < n; i++)
            {
                int r = o + 4 + i * 6;
                int s = U16(r), e = U16(r + 2);
                if (gid >= s && gid <= e) return U16(r + 4) + gid - s;
            }
            return -1;
        }

        int ClassOf(int o, int gid)
        {
            int fmt = U16(o);
            if (fmt == 1)
            {
                int start = U16(o + 2), n = U16(o + 4);
                return gid >= start && gid < start + n ? U16(o + 6 + (gid - start) * 2) : 0;
            }
            int rc = U16(o + 2);
            for (int i = 0; i < rc; i++)
            {
                int r = o + 4 + i * 6;
                if (gid >= U16(r) && gid <= U16(r + 2)) return U16(r + 4);
            }
            return 0;
        }

        static int PopCount(int v) { int c = 0; while (v != 0) { c += v & 1; v >>= 1; } return c; }

        /// <summary>Pair kerning in font units between two glyphs (GPOS kern feature, x advance of the first glyph).</summary>
        public float Kerning(int first, int second)
        {
            if (kernLookups.Count == 0) return 0;
            uint key = ((uint)first << 16) | (uint)second;
            if (kernCache.TryGetValue(key, out float cached)) return cached;
            float total = 0;
            foreach (int lk in kernLookups)
            {
                int type = U16(lk), sc = U16(lk + 4);
                for (int s = 0; s < sc; s++)
                {
                    int st = lk + U16(lk + 6 + s * 2);
                    int t = type;
                    if (t == 9) { t = U16(st + 2); st += (int)U32(st + 4); }
                    if (t != 2) continue;
                    if (PairPos(st, first, second, out float v)) { total += v; break; }
                }
            }
            kernCache[key] = total;
            return total;
        }

        bool PairPos(int st, int g1, int g2, out float adv)
        {
            adv = 0;
            int fmt = U16(st);
            int ci = Coverage(st + U16(st + 2), g1);
            if (ci < 0) return false;
            int vf1 = U16(st + 4), vf2 = U16(st + 6);
            int size1 = PopCount(vf1) * 2, size2 = PopCount(vf2) * 2;
            int xAdv1 = (vf1 & 4) != 0 ? PopCount(vf1 & 3) * 2 : -1;
            int xAdv2 = (vf2 & 4) != 0 ? PopCount(vf2 & 3) * 2 : -1;
            if (fmt == 1)
            {
                int ps = st + U16(st + 10 + ci * 2);
                int n = U16(ps), rec = 2 + size1 + size2;
                for (int i = 0; i < n; i++)
                {
                    int r = ps + 2 + i * rec;
                    if (U16(r) != g2) continue;
                    if (xAdv1 >= 0) adv += I16(r + 2 + xAdv1);
                    if (xAdv2 >= 0) adv += I16(r + 2 + size1 + xAdv2);
                    return true;
                }
                return false;
            }
            if (fmt == 2)
            {
                int cd1 = st + U16(st + 8), cd2 = st + U16(st + 10);
                int c1n = U16(st + 12), c2n = U16(st + 14);
                int c1 = ClassOf(cd1, g1), c2 = ClassOf(cd2, g2);
                if (c1 >= c1n || c2 >= c2n) return false;
                int rec = size1 + size2;
                int r = st + 16 + (c1 * c2n + c2) * rec;
                if (xAdv1 >= 0) adv += I16(r + xAdv1);
                if (xAdv2 >= 0) adv += I16(r + size1 + xAdv2);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries the GSUB <c>liga</c> lookups at <paramref name="at"/>. On success returns the ligature glyph and the
        /// number of input glyphs consumed.
        /// </summary>
        public bool HasSubstitutions => ligaLookups.Count > 0;

        /// <summary>
        /// Applies the GSUB lookups of the <c>ccmp</c> and <c>liga</c> features (latn default language system) to the glyph
        /// sequence in place, in lookup-index order like HarfBuzz: type 4 ligatures and type 2 multiple substitutions
        /// (Fredoka decomposes its <c>fi</c>/<c>fl</c> ligatures again in ccmp, which is why Chromium draws them unligated).
        /// Types 1, 3 and 6 are not applied (accent handling only).
        /// </summary>
        public void Substitute(List<int> gids)
        {
            foreach (int lk in ligaLookups)
            {
                int type = U16(lk), sc = U16(lk + 4);
                for (int at = 0; at < gids.Count; at++)
                {
                    for (int s = 0; s < sc; s++)
                    {
                        int st = lk + U16(lk + 6 + s * 2);
                        int t = type;
                        if (t == 7) { t = U16(st + 2); st += (int)U32(st + 4); }
                        if (t == 4)
                        {
                            if (!TryLigature(st, gids, at, out int lig, out int used)) continue;
                            gids[at] = lig;
                            gids.RemoveRange(at + 1, used - 1);
                            break;
                        }
                        if (t == 2)
                        {
                            int ci = Coverage(st + U16(st + 2), gids[at]);
                            if (ci < 0) continue;
                            int seq = st + U16(st + 6 + ci * 2);
                            int gc = U16(seq);
                            if (gc == 0) continue;
                            gids[at] = U16(seq + 2);
                            for (int k = 1; k < gc; k++) gids.Insert(at + k, U16(seq + 2 + k * 2));
                            at += gc - 1;
                            break;
                        }
                    }
                }
            }
        }

        bool TryLigature(int st, List<int> gids, int at, out int ligGid, out int consumed)
        {
            ligGid = 0; consumed = 0;
            int ci = Coverage(st + U16(st + 2), gids[at]);
            if (ci < 0) return false;
            int set = st + U16(st + 6 + ci * 2);
            int lc = U16(set);
            for (int i = 0; i < lc; i++)
            {
                int lig = set + U16(set + 2 + i * 2);
                int cc = U16(lig + 2);
                if (at + cc > gids.Count) continue;
                bool match = true;
                for (int k = 1; k < cc; k++) if (U16(lig + 4 + (k - 1) * 2) != gids[at + k]) { match = false; break; }
                if (!match) continue;
                ligGid = U16(lig); consumed = cc;
                return true;
            }
            return false;
        }
    }
}
