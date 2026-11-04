using System;
using System.Collections.Generic;
using OrbitalSalvage.Loc;

namespace OrbitalSalvage.Render
{
    using Loc = OrbitalSalvage.Loc.Loc;

    public sealed partial class Canvas2D
    {
        /// <summary>
        /// When this is <c>ar</c>, <c>hi</c>, or <c>km</c>, outline text looks up the baked table
        /// even if English is the active language. The language picker sets it for one button.
        /// Empty means "use <see cref="Loc.CurrentLanguage"/>, and English stays on the cmap path".
        /// </summary>
        public string ShapeCode;

        /// <summary>
        /// One already-wrapped visual line. <see cref="CssBox.DrawText"/> draws shadows by calling
        /// <c>FillText</c> again, so the line has to stay set for the whole call.
        /// </summary>
        public ShapedText.Line PreparedLine;

        TrueTypeFont shapedFace;

        public struct ShapedMark
        {
            public int Gid;
            public double Pen;
            public double Dx;
            public double Dy;
            public int Cluster;
            public string Font;
        }

        internal string FontString()
        {
            return state.font;
        }

        internal double LetterSpacingValue()
        {
            return state.letterSpacing;
        }

        internal void SetLetterSpacing(double value)
        {
            if (Finite(value)) state.letterSpacing = value;
        }

        internal double LayoutPublic(string text, double px, TrueTypeFont primary, TrueTypeFont[] chain)
        {
            return Layout(text, px, primary, chain, true);
        }

        /// <summary>Lays a baked string and copies the glyph origins. Returns false when the cmap path should run.</summary>
        public bool LayoutShapedMarks(string text, List<ShapedMark> marks, out double width)
        {
            var spec = ParseFont(state.font);
            if (!TryConsumeShaped(text, spec.px, false, out width)) return false;
            if (marks != null)
            {
                marks.Clear();
                for (int i = 0; i < laid.Count; i++)
                {
                    LaidGlyph g = laid[i];
                    marks.Add(new ShapedMark
                    {
                        Gid = g.gid,
                        Pen = g.pen,
                        Dx = g.dx,
                        Dy = g.dy,
                        Cluster = g.cluster,
                        Font = g.font == null ? "" : g.font.name
                    });
                }
            }
            return true;
        }

        bool TryConsumeShaped(string text, double px, bool measureOnly, out double width)
        {
            width = 0;
            shapedFace = null;
            if (PreparedLine != null)
            {
                string preparedLang = ShapedText.ActiveLang(Loc.EnglishActive, Loc.CurrentLanguage, ShapeCode);
                if (string.IsNullOrEmpty(preparedLang)) preparedLang = "ar";
                width = LayPieces(PreparedLine.Pieces, px, preparedLang);
                if (measureOnly) ClearLaid();
                return true;
            }
            string lang = ShapedText.ActiveLang(Loc.EnglishActive, Loc.CurrentLanguage, ShapeCode);
            if (lang == null) return false;
            ShapedTable table = ShapedText.Table(lang);
            if (table == null || table.Refused || table.Count == 0 || !table.TryResolve(text, out ShapedTable.Resolved resolved))
            {
                if (ShapedTable.ContainsScript(text, lang))
                {
                    ClearLaid();
                    width = 0;
                    return true;
                }
                return false;
            }
            ShapedText.Fit fit = ShapedText.Build(resolved, s => ShapedText.MeasurePassthrough(this, s, px, state.letterSpacing),
                px, table.UnitsPerEm, double.MaxValue, state.letterSpacing, TextFit.Action.Single, 1);
            if (fit.Lines == null || fit.Lines.Length == 0)
            {
                ClearLaid();
                return true;
            }
            width = LayPieces(fit.Lines[0].Pieces, px, lang);
            if (measureOnly) ClearLaid();
            return true;
        }

        double LayPieces(ShapedText.Piece[] pieces, double px, string lang)
        {
            ClearLaid();
            if (pieces == null || pieces.Length == 0) return 0;
            ShapedTable table = ShapedText.Table(lang);
            TrueTypeFont script = ScriptFace(lang, table == null ? "" : table.FontName);
            if (script != null && table != null && table.UnitsPerEm > 0 && script.unitsPerEm != table.UnitsPerEm)
                script = null;
            shapedFace = null;
            double pen = 0;
            bool trailing = false;
            bool needScript = false;
            for (int i = 0; i < pieces.Length; i++)
                if (pieces[i] != null && pieces[i].Glyphs != null && pieces[i].Glyphs.Length > 0) needScript = true;
            if (needScript && script == null)
            {
                ClearLaid();
                shapedFace = null;
                return 0;
            }
            if (needScript) shapedFace = script;
            double k = script == null ? 0 : px / script.unitsPerEm;
            for (int i = 0; i < pieces.Length; i++)
            {
                ShapedText.Piece piece = pieces[i];
                if (piece == null) continue;
                if (pen > 0 && !trailing && state.letterSpacing != 0) pen += state.letterSpacing;
                if (piece.Glyphs != null)
                {
                    int prev = int.MinValue;
                    bool any = false;
                    for (int g = 0; g < piece.Glyphs.Length; g++)
                    {
                        ShapedTable.Glyph glyph = piece.Glyphs[g];
                        if (any && glyph.Cluster != prev && state.letterSpacing != 0) pen += state.letterSpacing;
                        laid.Add(new LaidGlyph
                        {
                            font = script,
                            gid = glyph.Gid,
                            pen = pen,
                            dx = glyph.Dx * k,
                            dy = -glyph.Dy * k,
                            cluster = glyph.Cluster
                        });
                        pen += glyph.Adv * k;
                        prev = glyph.Cluster;
                        any = true;
                    }
                    trailing = false;
                }
                else
                {
                    pen += AppendPassthrough(piece.Text, px, pen);
                    trailing = true;
                }
            }
            return pen;
        }

        double AppendPassthrough(string text, double px, double origin)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            LaidGlyph[] keepG = laid.ToArray();
            double[] keepP = emojiPens.ToArray();
            CanvasImage[] keepI = emojiImages.ToArray();
            int weight;
            string family;
            OutlineFonts.SplitCssFont(state.font, out weight, out family);
            TrueTypeFont primary = OutlineFonts.Primary(family, weight);
            double width = Layout(text, px, primary, null, false);
            LaidGlyph[] extraG = laid.ToArray();
            double[] extraP = emojiPens.ToArray();
            CanvasImage[] extraI = emojiImages.ToArray();
            laid.Clear();
            for (int i = 0; i < keepG.Length; i++) laid.Add(keepG[i]);
            for (int i = 0; i < extraG.Length; i++)
            {
                LaidGlyph g = extraG[i];
                g.pen += origin;
                laid.Add(g);
            }
            emojiPens.Clear();
            emojiImages.Clear();
            for (int i = 0; i < keepP.Length; i++) emojiPens.Add(keepP[i]);
            for (int i = 0; i < keepI.Length; i++) emojiImages.Add(keepI[i]);
            for (int i = 0; i < extraP.Length; i++) emojiPens.Add(extraP[i] + origin);
            for (int i = 0; i < extraI.Length; i++) emojiImages.Add(extraI[i]);
            return width;
        }

        TrueTypeFont ScriptFace(string lang, string fontName)
        {
            if (string.IsNullOrEmpty(fontName)) return null;
            if (TextChain != null)
            {
                for (int i = 0; i < TextChain.Length; i++)
                    if (TextChain[i] != null && TextChain[i].name == fontName) return TextChain[i];
            }
            int weight;
            string family;
            OutlineFonts.SplitCssFont(state.font, out weight, out family);
            TrueTypeFont[] chain = OutlineFonts.ChainFor(lang, family, weight);
            for (int i = 0; i < chain.Length; i++)
                if (chain[i] != null && chain[i].name == fontName) return chain[i];
            try
            {
                return OutlineFonts.Load(fontName);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        void ClearLaid()
        {
            laid.Clear();
            emojiPens.Clear();
            emojiImages.Clear();
        }
    }
}
