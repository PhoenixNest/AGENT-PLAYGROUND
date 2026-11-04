using System;
using System.Collections.Generic;
using System.Text;
using OrbitalSalvage.Loc;
using UnityEngine;

namespace OrbitalSalvage.Render
{
    /// <summary>
    /// Layout for a baked ar/hi/km string. Advances scale by <c>px / unitsPerEm</c>.
    /// CSS letter-spacing is added once between clusters, not between a base and its
    /// mark, and not after the last cluster of a glyph run. Latin, digits, symbols,
    /// and emoji are measured with the existing cmap path (Fredoka or Nunito, then
    /// the symbol faces), not with the script face.
    /// </summary>
    public static class ShapedText
    {
        public sealed class Piece
        {
            public int Kind;
            public int Level;
            public ShapedTable.Glyph[] Glyphs;
            public string Text;
        }

        public sealed class Line
        {
            public string Text;
            public double Width;
            public Piece[] Pieces;
        }

        public sealed class Fit
        {
            public Line[] Lines;
            public string[] Texts;
            public double[] Widths;
            public bool Ellipsis;
            public bool Extra;
            public bool RightAlign;
        }

        static readonly Dictionary<string, ShapedTable> tables = new Dictionary<string, ShapedTable>();

        public static void Clear()
        {
            tables.Clear();
        }

        public static bool IsComplex(string lang)
        {
            return lang == "ar" || lang == "hi" || lang == "km";
        }

        /// <summary>
        /// Shrink <paramref name="fontPx"/> so the script em box fits <paramref name="box"/>
        /// divided by <paramref name="lines"/>. The captured line box is the Latin em.
        /// Cairo, Baloo 2, and Kantumruy are taller, and a mark that does not fit is clipped.
        /// </summary>
        public static double PxForBox(string lang, double fontPx, double box, int lines)
        {
            if (!IsComplex(lang) || fontPx <= 0 || box <= 1) return fontPx;
            int n = lines < 1 ? 1 : lines;
            ShapedTable table = Table(lang);
            if (table == null || table.Refused) return fontPx;
            TrueTypeFont face;
            try
            {
                face = OutlineFonts.Load(table.FontName);
            }
            catch (InvalidOperationException)
            {
                return fontPx;
            }
            if (face == null || face.unitsPerEm <= 0) return fontPx;
            double em = (face.ascender - face.descender) / (double)face.unitsPerEm;
            if (em <= 0) return fontPx;
            double per = box / n;
            if (fontPx * em <= per + 0.5) return fontPx;
            return per / em;
        }

        /// <summary>Language whose baked table should be used, or null. English with no per-run override returns null.</summary>
        public static string ActiveLang(bool englishActive, string currentLanguage, string shapeCode)
        {
            if (IsComplex(shapeCode)) return shapeCode;
            if (englishActive) return null;
            if (IsComplex(currentLanguage)) return currentLanguage;
            return null;
        }

        public static ShapedTable Table(string lang)
        {
            if (!IsComplex(lang)) return ShapedTable.Empty("");
            ShapedTable table;
            if (tables.TryGetValue(lang, out table)) return table;
            TextAsset text = Resources.Load<TextAsset>("Localization/shaped/" + lang);
            if (text == null || text.bytes == null || text.bytes.Length == 0)
            {
                table = ShapedTable.Empty("");
                tables[lang] = table;
                return table;
            }
            ShapedTable parsed;
            try
            {
                parsed = ShapedTable.Parse(text.bytes);
            }
            catch (InvalidOperationException)
            {
                table = ShapedTable.Empty("");
                tables[lang] = table;
                return table;
            }
            TextAsset font = Resources.Load<TextAsset>("Fonts/Ttf/" + parsed.FontName);
            if (font == null || font.bytes == null || !ShapedTable.HashEquals(parsed.FontHash, ShapedTable.Sha256(font.bytes)))
            {
                table = ShapedTable.Refuse(parsed.FontName);
                tables[lang] = table;
                return table;
            }
            tables[lang] = parsed;
            return parsed;
        }

        public static bool TryFit(Canvas2D canvas, string text, string lang, double width, double px, double letterSpacing,
            TextFit.Action action, int maxLines, out Fit fit)
        {
            fit = null;
            ShapedTable.Resolved resolved;
            if (!TryResolved(lang, text, out resolved)) return false;
            ShapedTable table = Table(lang);
            Func<string, double> measure = s => MeasurePassthrough(canvas, s, px, letterSpacing);
            fit = Build(resolved, measure, px, table.UnitsPerEm, width, letterSpacing, action, maxLines);
            return true;
        }

        public static bool TryResolved(string lang, string text, out ShapedTable.Resolved resolved)
        {
            resolved = null;
            if (!IsComplex(lang) || string.IsNullOrEmpty(text)) return false;
            ShapedTable table = Table(lang);
            if (table == null || table.Refused || table.Count == 0) return false;
            return table.TryResolve(text, out resolved);
        }

        public static Fit Build(ShapedTable.Resolved resolved, Func<string, double> measurePassthrough, double px, int upem, double width,
            double letterSpacing, TextFit.Action action, int maxLines)
        {
            var atoms = Atoms(resolved, measurePassthrough, px, upem, 0);
            var fit = new Fit { RightAlign = resolved.RightAlign };
            if (action == TextFit.Action.Ellipsis)
            {
                fit.Lines = new[] { Ellipsize(atoms, width, letterSpacing, measurePassthrough, px) };
                fit.Ellipsis = true;
            }
            else if (action == TextFit.Action.Wrap)
            {
                List<List<Atom>> groups = WrapAtoms(atoms, width, letterSpacing);
                int max = maxLines < 1 ? 1 : maxLines;
                if (groups.Count > max)
                {
                    fit.Extra = true;
                    fit.Ellipsis = true;
                    while (groups.Count > max) groups.RemoveAt(groups.Count - 1);
                    if (groups.Count > 0) groups[groups.Count - 1] = EllipsisAtoms(groups[groups.Count - 1], width, letterSpacing, measurePassthrough, px);
                }
                fit.Lines = new Line[groups.Count];
                for (int i = 0; i < groups.Count; i++) fit.Lines[i] = LineFrom(groups[i], letterSpacing);
            }
            else
            {
                fit.Lines = new[] { LineFrom(atoms, letterSpacing) };
            }
            fit.Texts = new string[fit.Lines.Length];
            fit.Widths = new double[fit.Lines.Length];
            for (int i = 0; i < fit.Lines.Length; i++)
            {
                fit.Texts[i] = fit.Lines[i].Text ?? "";
                fit.Widths[i] = fit.Lines[i].Width;
            }
            return fit;
        }

        static List<Atom> Atoms(ShapedTable.Resolved resolved, Func<string, double> measure, double px, int upem, int depth)
        {
            var atoms = new List<Atom>();
            double k = px / (upem > 0 ? upem : 1000);
            ShapedTable.Run[] runs = resolved.Entry.Runs;
            for (int r = 0; r < runs.Length; r++)
            {
                ShapedTable.Run run = runs[r];
                if (run.Kind == ShapedTable.KindGlyph)
                {
                    var order = new List<int>();
                    var seen = new HashSet<int>();
                    for (int g = 0; g < run.Glyphs.Length; g++)
                    {
                        if (seen.Add(run.Glyphs[g].Cluster)) order.Add(run.Glyphs[g].Cluster);
                    }
                    order.Sort();
                    for (int c = 0; c < order.Count; c++)
                    {
                        int cluster = order[c];
                        var glyphs = new List<ShapedTable.Glyph>();
                        bool brk = false;
                        double adv = 0;
                        for (int g = 0; g < run.Glyphs.Length; g++)
                        {
                            if (run.Glyphs[g].Cluster != cluster) continue;
                            glyphs.Add(run.Glyphs[g]);
                            adv += run.Glyphs[g].Adv;
                            if (run.Glyphs[g].BreakBefore) brk = true;
                        }
                        int next = c + 1 < order.Count ? order[c + 1] : (resolved.Entry.Template == null ? cluster + 1 : resolved.Entry.Template.Length);
                        string slice = Slice(resolved.Entry.Template, cluster, next);
                        atoms.Add(new Atom
                        {
                            Level = run.Level,
                            BreakBefore = brk,
                            Glyph = true,
                            Glyphs = glyphs.ToArray(),
                            Text = slice,
                            Width = adv * k,
                            TrailingSpace = false
                        });
                    }
                }
                else
                {
                    string show = run.Kind == ShapedTable.KindSlot ? resolved.SlotText(run.Slot) : run.Text;
                    if (run.Kind == ShapedTable.KindSlot && depth < 3 && resolved.Source != null)
                    {
                        ShapedTable.Entry nested;
                        if (resolved.Source.TryGet(show, out nested) && nested != null && !nested.HasSlot)
                        {
                            var inner = new ShapedTable.Resolved(nested, Array.Empty<string>(), resolved.Source);
                            atoms.AddRange(Atoms(inner, measure, px, upem, depth + 1));
                            continue;
                        }
                    }
                    if (string.IsNullOrEmpty(show)) continue;
                    double w = measure != null ? measure(show) : 0;
                    atoms.Add(new Atom
                    {
                        Level = run.Level,
                        BreakBefore = atoms.Count > 0,
                        Glyph = false,
                        Kind = run.Kind,
                        Text = show,
                        Width = w,
                        TrailingSpace = true
                    });
                }
            }
            return atoms;
        }

        static string Slice(string template, int start, int end)
        {
            if (string.IsNullOrEmpty(template) || start < 0 || start >= template.Length) return "";
            if (end <= start) end = start + 1;
            if (end > template.Length) end = template.Length;
            return template.Substring(start, end - start);
        }

        static List<List<Atom>> WrapAtoms(List<Atom> atoms, double width, double letterSpacing)
        {
            var lines = new List<List<Atom>>();
            var cur = new List<Atom>();
            double used = 0;
            bool trailing = false;
            for (int i = 0; i < atoms.Count; i++)
            {
                Atom atom = atoms[i];
                double gap = cur.Count > 0 && !trailing ? letterSpacing : 0;
                if (atom.BreakBefore && cur.Count > 0 && used + gap + atom.Width > width + 0.5)
                {
                    lines.Add(cur);
                    cur = new List<Atom>();
                    used = 0;
                    trailing = false;
                    gap = 0;
                }
                cur.Add(atom);
                used += gap + atom.Width;
                trailing = atom.TrailingSpace;
            }
            if (cur.Count > 0) lines.Add(cur);
            if (lines.Count == 0) lines.Add(atoms);
            return lines;
        }

        static Line Ellipsize(List<Atom> atoms, double width, double letterSpacing, Func<string, double> measure, double px)
        {
            return LineFrom(EllipsisAtoms(atoms, width, letterSpacing, measure, px), letterSpacing);
        }

        static List<Atom> EllipsisAtoms(List<Atom> atoms, double width, double letterSpacing, Func<string, double> measure, double px)
        {
            double ell = measure != null ? measure(TextFit.Ellipsis) : px;
            var kept = new List<Atom>();
            double used = 0;
            bool trailing = false;
            for (int i = 0; i < atoms.Count; i++)
            {
                double gap = kept.Count > 0 && !trailing ? letterSpacing : 0;
                if (kept.Count > 0 && used + gap + atoms[i].Width + ell > width + 0.5) break;
                kept.Add(atoms[i]);
                used += gap + atoms[i].Width;
                trailing = atoms[i].TrailingSpace;
            }
            if (kept.Count == 0 && atoms.Count > 0) kept.Add(atoms[0]);
            kept.Add(new Atom
            {
                Level = 0,
                Glyph = false,
                Kind = ShapedTable.KindLatin,
                Text = TextFit.Ellipsis,
                Width = ell,
                TrailingSpace = true,
                BreakBefore = true
            });
            return kept;
        }

        static Line LineFrom(List<Atom> logical, double letterSpacing)
        {
            var pieces = Reorder(logical);
            double pen = 0;
            bool trailing = false;
            var text = new StringBuilder();
            for (int i = 0; i < logical.Count; i++) text.Append(logical[i].Text);
            var visual = new Piece[pieces.Count];
            for (int i = 0; i < pieces.Count; i++)
            {
                if (i > 0 && !trailing && letterSpacing != 0 && !(pieces[i].Glyph && pieces[i - 1].Glyph)) pen += letterSpacing;
                visual[i] = pieces[i].ToPiece();
                pen += pieces[i].Width;
                trailing = pieces[i].TrailingSpace;
            }
            // Width of glyph atoms ignored letter-spacing inside the atom (one cluster).
            // Between-atom spacing is in pen. Passthrough width already includes its own tracking.
            return new Line { Text = text.ToString(), Width = pen, Pieces = visual };
        }

        static List<Atom> Reorder(List<Atom> logical)
        {
            var seq = new List<Atom>(logical);
            int max = 0;
            for (int i = 0; i < seq.Count; i++) if (seq[i].Level > max) max = seq[i].Level;
            for (int level = max; level >= 1; level--)
            {
                int i = 0;
                while (i < seq.Count)
                {
                    if (seq[i].Level < level) { i++; continue; }
                    int j = i + 1;
                    while (j < seq.Count && seq[j].Level >= level) j++;
                    seq.Reverse(i, j - i);
                    i = j;
                }
            }
            return seq;
        }

        public static double MeasurePassthrough(Canvas2D canvas, string text, double px, double letterSpacing)
        {
            if (canvas == null || string.IsNullOrEmpty(text)) return 0;
            int weight;
            string family;
            OutlineFonts.SplitCssFont(canvas.FontString(), out weight, out family);
            TrueTypeFont primary = OutlineFonts.Primary(family, weight);
            TrueTypeFont[] chain = OutlineFonts.Chain(primary);
            double saved = canvas.LetterSpacingValue();
            canvas.SetLetterSpacing(letterSpacing);
            double width = canvas.LayoutPublic(text, px, primary, chain);
            canvas.SetLetterSpacing(saved);
            return width;
        }

        sealed class Atom
        {
            public int Level;
            public bool BreakBefore;
            public bool Glyph;
            public bool TrailingSpace;
            public int Kind;
            public ShapedTable.Glyph[] Glyphs;
            public string Text;
            public double Width;

            public Piece ToPiece()
            {
                return new Piece
                {
                    Kind = Glyph ? ShapedTable.KindGlyph : Kind,
                    Level = Level,
                    Glyphs = Glyphs,
                    Text = Text
                };
            }
        }

    }
}
