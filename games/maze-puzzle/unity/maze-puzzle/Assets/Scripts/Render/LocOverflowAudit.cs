using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using OrbitalSalvage.Loc;

namespace OrbitalSalvage.Render
{
    using Loc = OrbitalSalvage.Loc.Loc;

    /// <summary>
    /// Paint-free overflow check. Uses <see cref="TextFit"/> and <see cref="LineBreaker"/> with a real
    /// <see cref="Canvas2D"/> measure. The caller sets <see cref="Loc.CurrentLanguage"/> first
    /// (<c>qps</c> is the usual choice). Hidden, zero-size, and off-screen sheet copies are skipped.
    /// </summary>
    public static class LocOverflowAudit
    {
        public const double HomeIndicatorY = TextFit.HomeIndicatorY;

        public sealed class Finding
        {
            public string File;
            public string ElementId;
            public string Kind;
            public string Detail;
        }

        public sealed class Report
        {
            public readonly List<Finding> Findings = new List<Finding>();
            public int Files;
            public int Runs;
            public int FragmentElements;
            public int HardOutside, HardOverlap, HardBand, HardWider, HardWrapped, HardMissing, HardIconOverlap;
            public int SoftShrink, SoftEllipsis, SoftWrap, SoftControlEllipsis;
            public string Text = "";

            public int Hard
            {
                get { return HardOutside + HardOverlap + HardBand + HardWider + HardWrapped + HardMissing + HardIconOverlap; }
            }

            public int Soft
            {
                get { return SoftShrink + SoftEllipsis + SoftWrap + SoftControlEllipsis; }
            }
        }

        public static Report Run(string specDir)
        {
            var report = new Report();
            var canvas = new Canvas2D(16, 16);
            try
            {
                string[] files = Directory.GetFiles(specDir, "*.json");
                Array.Sort(files, StringComparer.Ordinal);
                for (int f = 0; f < files.Length; f++)
                {
                    string name = Path.GetFileName(files[f]);
                    if (name.IndexOf("__zero.json", StringComparison.Ordinal) < 0
                        && name.IndexOf("__iphone.json", StringComparison.Ordinal) < 0) continue;
                    report.Files++;
                    ScanFile(files[f], name, canvas, report);
                }
            }
            finally
            {
                canvas.Dispose();
            }
            report.Text = Format(report);
            return report;
        }

        static void ScanFile(string path, string name, Canvas2D canvas, Report report)
        {
            var root = MiniJson.Parse(File.ReadAllText(path)) as Dictionary<string, object>;
            if (root == null || !(root["elements"] is List<object> raw)) return;
            bool iphone = name.IndexOf("__iphone.json", StringComparison.Ordinal) >= 0;
            var els = new List<Dictionary<string, object>>(raw.Count);
            for (int i = 0; i < raw.Count; i++)
                if (raw[i] is Dictionary<string, object> el) els.Add(el);
            var skip = new bool[els.Count];
            var byI = new Dictionary<int, int>();
            for (int i = 0; i < els.Count; i++) byI[(int)Num(els[i], "i", i)] = i;
            for (int i = 0; i < els.Count; i++)
            {
                int p = (int)Num(els[i], "p", -1);
                int pk;
                bool parentSkip = p >= 0 && byI.TryGetValue(p, out pk) && skip[pk];
                skip[i] = parentSkip || Hidden(els[i]);
            }
            for (int i = 0; i < els.Count; i++)
            {
                if (skip[i]) continue;
                var e = els[i];
                if (!(e.TryGetValue("runs", out var rv) && rv is List<object> runs) || runs.Count == 0) continue;
                double[] er = Box(e);
                if (er == null || er[2] <= 0 || er[3] <= 0 || er[1] >= 932) continue;
                bool fragment = runs.Count >= 2;
                bool countedFragment = false;
                var order = new int[runs.Count];
                for (int ri = 0; ri < runs.Count; ri++) order[ri] = ri;
                Array.Sort(order, (a, b) => RunTop(runs, a).CompareTo(RunTop(runs, b)));
                double blockBottom = BlockBottom(els, e, er, iphone);
                double extra = 0;
                for (int n = 0; n < order.Length; n++)
                {
                    int ri = order[n];
                    if (!(runs[ri] is Dictionary<string, object> run)) continue;
                    double[] rr = Box(run);
                    if (rr == null || rr[2] <= 0 || rr[3] <= 0) continue;
                    string source = Collapse(Str(run, "s"));
                    string key = LocText.Normalize(source);
                    if (!LocText.HasLetter(key)) continue;
                    string translated;
                    if (!Loc.TryTranslateSpecText(source, out translated) || string.IsNullOrEmpty(translated)) continue;
                    report.Runs++;
                    if (fragment && !countedFragment)
                    {
                        report.FragmentElements++;
                        countedFragment = true;
                    }
                    string shown = Str(e, "tt") == "uppercase" ? LocCase.Upper(translated, Loc.CurrentLanguage) : translated;
                    double ceiling = blockBottom;
                    double reserved = 0;
                    for (int j = 0; j < runs.Count; j++)
                    {
                        double top = RunTop(runs, j);
                        if (top > rr[1] + 0.5) reserved += RunHeight(runs, j);
                    }
                    ceiling -= reserved;
                    if (ceiling < rr[1] + 1) ceiling = rr[1] + 1;
                    var icons = IconsOf(els, e, rr);
                    AddRowIcons(els, e, rr, ref icons);
                    double parentX, parentW;
                    ParentBox(els, e, out parentX, out parentW);
                    extra += AuditRun(name, e, er, rr, Str(run, "s"), shown, icons, iphone, rr[1] + extra, ceiling, parentX, parentW, canvas, report);
                }
            }
        }

        static double AuditRun(string file, Dictionary<string, object> e, double[] er, double[] rr, string raw, string shown,
            List<TextFit.Icon> icons, bool iphone, double drawY, double ceiling, double parentX, double parentW, Canvas2D canvas, Report report)
        {
            string font = Str(e, "fw") + " " + Str(e, "fz") + " " + Family(Str(e, "ff"));
            double ls = LocTextStyle.LetterSpacing(Loc.CurrentLanguage, CssLength(Str(e, "ls")));
            double fontPx = CssLength(Str(e, "fz"));
            string id = Str(e, "id");
            if (id.Length == 0) id = Str(e, "c");
            bool buttonLike = TextFit.LooksLikeControl(Str(e, "t"), Str(e, "id"), Str(e, "c"));
            var input = new TextFit.Input
            {
                RunX = rr[0], RunY = drawY, RunW = rr[2], RunH = rr[3],
                ElX = er[0], ElY = er[1], ElW = er[2], ElH = er[3],
                CenteredBlock = Centered(rr, er),
                IsButton = Str(e, "t") == "button",
                ButtonLike = buttonLike,
                FontPx = fontPx,
                LetterSpacing = ls,
                LinePitch = LocTextStyle.LinePitchOverride(Loc.CurrentLanguage, raw, rr[3]),
                OriginalMultiLine = fontPx > 0 && rr[3] > TextFit.SingleLineFactor * fontPx + TextFit.SingleLineSlack,
                SidePad = TextFit.SidePadding,
                Icons = icons == null || icons.Count == 0 ? null : icons.ToArray(),
                BandY = iphone ? HomeIndicatorY : 0,
                CeilingY = ceiling,
                ParentX = parentX,
                ParentW = parentW
            };
            string leadSpaces = TextFit.EdgeSpaces(raw, true);
            string trailSpaces = TextFit.EdgeSpaces(raw, false);
            if (leadSpaces.Length > 0) input.LeadPad = Measure(canvas, font, leadSpaces, fontPx, ls);
            if (trailSpaces.Length > 0) input.TrailPad = Measure(canvas, font, trailSpaces, fontPx, ls);
            NoteMissingGlyphs(file, id, shown, font, report);
            TextFit.Layout plan = TextFit.Plan(input, (px, spacing) => Measure(canvas, font, shown, px, spacing));
            var lines = new List<string>();
            Func<string, double> measureLine = s => Measure(canvas, font, s, plan.FontPx, plan.LetterSpacing);
            bool ellipsis = plan.Action == TextFit.Action.Ellipsis;
            bool extra = false;
            if (plan.Action == TextFit.Action.Wrap)
            {
                LineBreaker.For(Loc.CurrentLanguage).Wrap(shown, plan.Available, 0.5, measureLine, lines);
                int max = plan.MaxLines < 1 ? 1 : plan.MaxLines;
                if (lines.Count > max)
                {
                    extra = true;
                    ellipsis = true;
                    while (lines.Count > max) lines.RemoveAt(lines.Count - 1);
                    if (lines.Count > 0) lines[lines.Count - 1] = TextFit.Ellipsize(lines[lines.Count - 1], plan.Available, measureLine);
                }
                for (int i = 0; i < lines.Count; i++)
                {
                    if (measureLine(lines[i]) <= plan.Available + 0.5) continue;
                    lines[i] = TextFit.Ellipsize(lines[i], plan.Available, measureLine);
                    ellipsis = true;
                }
            }
            else if (ellipsis)
            {
                lines.Add(TextFit.Ellipsize(shown, plan.Available, measureLine));
            }
            else lines.Add(shown);

            if (plan.SoftShrink || (plan.Action == TextFit.Action.Shrink && plan.Scale < TextFit.SoftShrink - 1e-9))
            {
                report.SoftShrink++;
                Add(report, file, id, "soft-shrink", "scale=" + plan.Scale.ToString("0.00", CultureInfo.InvariantCulture));
            }
            bool oneLine = TextFit.LocksToOneLine(input);
            if (oneLine && lines.Count > 1)
            {
                report.HardWrapped++;
                Add(report, file, id, "wrapped-in-control", shown);
            }
            if (oneLine && ellipsis)
            {
                report.SoftControlEllipsis++;
                Add(report, file, id, "soft-control-ellipsis", shown);
            }
            else if (ellipsis)
            {
                report.SoftEllipsis++;
                Add(report, file, id, "soft-ellipsis", shown);
            }
            if (extra)
            {
                report.SoftWrap++;
                Add(report, file, id, "soft-wrap", "max=" + plan.MaxLines.ToString(CultureInfo.InvariantCulture));
            }

            double pitch = plan.LinePitch > 0 ? plan.LinePitch : Math.Max(1, fontPx);
            double inkH = fontPx > 0 ? Math.Min(pitch, fontPx * 1.2) : pitch;
            for (int i = 0; i < lines.Count; i++)
            {
                double tw = Measure(canvas, font, lines[i], plan.FontPx, plan.LetterSpacing);
                double x = TextFit.PlaceX(plan.Anchor, er[0], er[2], tw, plan.Left, plan.Right);
                if (lines.Count == 1 && (plan.IconIndex >= 0 || plan.KeepX)) x = plan.X;
                double top = drawY + i * pitch;
                double bottom = top + pitch;
                if (x < er[0] - TextFit.OutsideSlop - 0.01 || x + tw > er[0] + er[2] + TextFit.OutsideSlop + 0.01)
                {
                    report.HardOutside++;
                    Add(report, file, id, "hard-outside", "x=" + x.ToString("0.0", CultureInfo.InvariantCulture) + " w=" + tw.ToString("0.0", CultureInfo.InvariantCulture));
                }
                if (tw > plan.Available + 0.5)
                {
                    report.HardWider++;
                    Add(report, file, id, "hard-wider", "w=" + tw.ToString("0.0", CultureInfo.InvariantCulture) + " avail=" + plan.Available.ToString("0.0", CultureInfo.InvariantCulture));
                }
                if (iphone && bottom > HomeIndicatorY + 0.01 && rr[1] + rr[3] <= HomeIndicatorY + 0.01)
                {
                    report.HardBand++;
                    Add(report, file, id, "hard-band", "bottom=" + bottom.ToString("0.0", CultureInfo.InvariantCulture));
                }
                if (icons != null)
                {
                    for (int n = 0; n < icons.Count; n++)
                    {
                        var icon = icons[n];
                        double ix = icon.X + (plan.IconIndex == icon.Index ? plan.IconShift : 0);
                        if (!Overlaps(x, top, tw, inkH, ix, icon.Y, icon.W, icon.H)) continue;
                        // A dock badge is captured on top of its icon. That overlap is the layout.
                        // Flag it only when the translation starts further left than the English run.
                        if (icon.RowSibling && x >= rr[0] - 0.5 && Overlaps(rr[0], rr[1], rr[2], Math.Min(rr[3], inkH), icon.X, icon.Y, icon.W, icon.H))
                            continue;
                        report.HardIconOverlap++;
                        Add(report, file, id, "icon-overlap", "icon=" + icon.Index.ToString(CultureInfo.InvariantCulture));
                        if (!plan.KeepX)
                        {
                            report.HardOverlap++;
                            Add(report, file, id, "hard-overlap", "icon=" + icon.Index.ToString(CultureInfo.InvariantCulture));
                        }
                    }
                }
            }
            double used = lines.Count * pitch;
            return used > rr[3] ? used - rr[3] : 0;
        }

        static bool Overlaps(double x, double y, double w, double h, double ix, double iy, double iw, double ih)
        {
            double x0 = Math.Max(x, ix);
            double x1 = Math.Min(x + w, ix + iw);
            double y0 = Math.Max(y, iy);
            double y1 = Math.Min(y + h, iy + ih);
            return x1 - x0 > 0.5 && y1 - y0 > 0.5;
        }

        static void NoteMissingGlyphs(string file, string id, string shown, string font, Report report)
        {
            if (string.IsNullOrEmpty(shown) || Loc.EnglishActive) return;
            int weight;
            string family;
            OutlineFonts.SplitCssFont(font, out weight, out family);
            string lang = Loc.CurrentLanguage;
            if (id.StartsWith(LanguageMarker.ButtonPrefix, StringComparison.Ordinal))
            {
                string code = id.Substring(LanguageMarker.ButtonPrefix.Length);
                if (LanguageRegistry.IsSupported(code)) lang = code;
            }
            TrueTypeFont[] chain = OutlineFonts.ChainFor(lang, family, weight);
            var missing = new List<int>();
            LocGlyphCoverage.CollectString(shown, cp =>
            {
                for (int i = 0; i < chain.Length; i++)
                    if (chain[i] != null && chain[i].GlyphIndex(cp) != 0) return true;
                return false;
            }, CanvasEmoji.IsEmoji, missing);
            var seen = new HashSet<int>();
            for (int i = 0; i < missing.Count; i++)
            {
                if (!seen.Add(missing[i])) continue;
                report.HardMissing++;
                Add(report, file, id, "missing-glyph", "U+" + missing[i].ToString("X4", CultureInfo.InvariantCulture));
            }
        }

        static double Measure(Canvas2D canvas, string font, string text, double px, double ls)
        {
            canvas.Font = FontSized(font, px);
            canvas.LetterSpacing = ls;
            if (Loc.EnglishActive)
            {
                canvas.TextChain = null;
                canvas.TextChainStamp = 0;
            }
            else
            {
                int weight;
                string family;
                OutlineFonts.SplitCssFont(font, out weight, out family);
                canvas.TextChain = OutlineFonts.ChainFor(Loc.CurrentLanguage, family, weight);
                canvas.TextChainStamp = OutlineFonts.ChainStamp(Loc.CurrentLanguage, family, weight);
            }
            return canvas.MeasureText(text ?? "");
        }

        static string FontSized(string font, double px)
        {
            int pxAt = font.IndexOf("px", StringComparison.Ordinal);
            if (pxAt < 0) return font;
            int start = pxAt;
            while (start > 0 && (char.IsDigit(font[start - 1]) || font[start - 1] == '.')) start--;
            return font.Substring(0, start) + px.ToString("0.##", CultureInfo.InvariantCulture) + font.Substring(pxAt);
        }

        static void Add(Report report, string file, string id, string kind, string detail)
        {
            report.Findings.Add(new Finding { File = file, ElementId = id, Kind = kind, Detail = detail });
        }

        static string Format(Report report)
        {
            var sb = new StringBuilder();
            sb.Append("language=").Append(Loc.CurrentLanguage);
            sb.Append(" files=").Append(report.Files.ToString(CultureInfo.InvariantCulture));
            sb.Append(" runs=").Append(report.Runs.ToString(CultureInfo.InvariantCulture));
            sb.Append(" fragments=").Append(report.FragmentElements.ToString(CultureInfo.InvariantCulture));
            sb.Append(" hard=").Append(report.Hard.ToString(CultureInfo.InvariantCulture));
            sb.Append(" soft=").Append(report.Soft.ToString(CultureInfo.InvariantCulture));
            sb.Append(" hard_outside=").Append(report.HardOutside.ToString(CultureInfo.InvariantCulture));
            sb.Append(" hard_overlap=").Append(report.HardOverlap.ToString(CultureInfo.InvariantCulture));
            sb.Append(" hard_band=").Append(report.HardBand.ToString(CultureInfo.InvariantCulture));
            sb.Append(" hard_wider=").Append(report.HardWider.ToString(CultureInfo.InvariantCulture));
            sb.Append(" hard_wrapped=").Append(report.HardWrapped.ToString(CultureInfo.InvariantCulture));
            sb.Append(" icon_overlap=").Append(report.HardIconOverlap.ToString(CultureInfo.InvariantCulture));
            sb.Append(" hard_missing=").Append(report.HardMissing.ToString(CultureInfo.InvariantCulture));
            sb.Append(" soft_shrink=").Append(report.SoftShrink.ToString(CultureInfo.InvariantCulture));
            sb.Append(" soft_ellipsis=").Append(report.SoftEllipsis.ToString(CultureInfo.InvariantCulture));
            sb.Append(" soft_wrap=").Append(report.SoftWrap.ToString(CultureInfo.InvariantCulture));
            sb.Append(" soft_control_ellipsis=").Append(report.SoftControlEllipsis.ToString(CultureInfo.InvariantCulture));
            sb.Append('\n');
            // Per-run translation. Extra lines shift the next run down; there is no group key yet.
            sb.Append("fragments are per-run translations; joining them is a later task\n");
            for (int i = 0; i < report.Findings.Count; i++)
            {
                Finding f = report.Findings[i];
                sb.Append(f.Kind).Append('\t').Append(f.File).Append('\t').Append(f.ElementId).Append('\t').Append(f.Detail).Append('\n');
            }
            return sb.ToString();
        }

        static double RunTop(List<object> runs, int index)
        {
            if (index < 0 || index >= runs.Count || !(runs[index] is Dictionary<string, object> run)) return 0;
            double[] r = Box(run);
            return r == null ? 0 : r[1];
        }

        static double RunHeight(List<object> runs, int index)
        {
            if (index < 0 || index >= runs.Count || !(runs[index] is Dictionary<string, object> run)) return 0;
            double[] r = Box(run);
            return r == null ? 0 : r[3];
        }

        static double BlockBottom(List<Dictionary<string, object>> els, Dictionary<string, object> self, double[] er, bool iphone)
        {
            double bottom = er[1] + er[3];
            if (iphone) bottom = Math.Min(bottom, HomeIndicatorY);
            int parent = (int)Num(self, "p", -1);
            if (parent < 0) return bottom;
            for (int i = 0; i < els.Count; i++)
            {
                if (ReferenceEquals(els[i], self)) continue;
                if ((int)Num(els[i], "p", -2) != parent) continue;
                double[] box = Box(els[i]);
                if (box == null || box[2] <= 0 || box[3] <= 0 || box[1] >= 932) continue;
                if (box[1] <= er[1] + 1) continue;
                if (box[1] < bottom) bottom = box[1];
            }
            return bottom;
        }

        static void AddRowIcons(List<Dictionary<string, object>> els, Dictionary<string, object> self, double[] rr, ref List<TextFit.Icon> list)
        {
            if (rr == null) return;
            int parent = (int)Num(self, "p", -1);
            int selfId = (int)Num(self, "i", -1);
            if (parent < 0) return;
            for (int i = 0; i < els.Count; i++)
            {
                if ((int)Num(els[i], "i", -2) == selfId) continue;
                if ((int)Num(els[i], "p", -2) != parent) continue;
                int iconAt = i;
                double[] box = Box(els[i]);
                if (box == null || box[2] <= 0 || box[3] <= 0) continue;
                bool emoji = Str(els[i], "emoji").Length > 0;
                if (!emoji)
                {
                    if (!NestedEmoji(els, els[i], out iconAt, out box)) continue;
                }
                if (box[1] + box[3] < rr[1] - 2 || box[1] > rr[1] + rr[3] + 2) continue;
                if (list != null)
                {
                    bool seen = false;
                    for (int n = 0; n < list.Count; n++)
                        if (list[n].Index == iconAt) { seen = true; break; }
                    if (seen) continue;
                }
                else list = new List<TextFit.Icon>();
                list.Add(new TextFit.Icon { Index = iconAt, X = box[0], Y = box[1], W = box[2], H = box[3], RowSibling = true });
            }
        }

        static List<TextFit.Icon> IconsOf(List<Dictionary<string, object>> els, Dictionary<string, object> parent, double[] rr)
        {
            int id = (int)Num(parent, "i", -1);
            List<TextFit.Icon> list = null;
            if (rr == null) return null;
            for (int i = 0; i < els.Count; i++)
            {
                if ((int)Num(els[i], "p", -2) != id) continue;
                double[] box = Box(els[i]);
                if (box == null || box[2] <= 0) continue;
                bool emoji = Str(els[i], "emoji").Length > 0;
                bool symbol = false;
                if (!emoji && els[i].TryGetValue("runs", out var cr) && cr is List<object> cRuns && cRuns.Count == 1 && cRuns[0] is Dictionary<string, object> cRun)
                {
                    string s = Collapse(Str(cRun, "s")).Trim();
                    symbol = s.Length > 0 && s.Length <= 2 && !LocText.HasLetter(s) && box[2] <= 28;
                }
                int iconAt = i;
                double[] iconBox = box;
                if (!emoji && !symbol)
                {
                    if (!NestedEmoji(els, els[i], out iconAt, out iconBox)) continue;
                }
                double iconRight = iconBox[0] + iconBox[2];
                double runRight = rr[0] + rr[2];
                bool lead = iconRight <= rr[0] + 2 && rr[0] - iconRight <= TextFit.IconGapSlop;
                bool trail = iconBox[0] >= runRight - 2 && iconBox[0] - runRight <= TextFit.IconGapSlop;
                bool overlap = iconBox[0] < runRight - 0.5 && iconRight > rr[0] + 0.5
                    && iconBox[1] < rr[1] + rr[3] - 0.5 && iconBox[1] + iconBox[3] > rr[1] + 0.5;
                if (!lead && !trail && !overlap) continue;
                if (list == null) list = new List<TextFit.Icon>();
                list.Add(new TextFit.Icon { Index = iconAt, X = iconBox[0], Y = iconBox[1], W = iconBox[2], H = iconBox[3] });
            }
            return list;
        }

        static bool NestedEmoji(List<Dictionary<string, object>> els, Dictionary<string, object> child, out int index, out double[] box)
        {
            index = -1;
            box = null;
            if (child.TryGetValue("runs", out var rv) && rv is List<object> runs)
            {
                for (int n = 0; n < runs.Count; n++)
                {
                    if (runs[n] is Dictionary<string, object> run && LocText.HasLetter(LocText.Normalize(Collapse(Str(run, "s")))))
                        return false;
                }
            }
            int id = (int)Num(child, "i", -1);
            if (id < 0) return false;
            for (int i = 0; i < els.Count; i++)
            {
                if ((int)Num(els[i], "p", -2) != id) continue;
                if (Str(els[i], "emoji").Length == 0) continue;
                double[] r = Box(els[i]);
                if (r == null || r[2] <= 0 || r[3] <= 0) continue;
                index = i;
                box = r;
                return true;
            }
            return false;
        }

        static void ParentBox(List<Dictionary<string, object>> els, Dictionary<string, object> e, out double x, out double w)
        {
            x = 0;
            w = 0;
            int parent = (int)Num(e, "p", -1);
            if (parent < 0) return;
            for (int i = 0; i < els.Count; i++)
            {
                if ((int)Num(els[i], "i", -2) != parent) continue;
                double[] box = Box(els[i]);
                if (box == null || box[2] <= 1) return;
                x = box[0];
                w = box[2];
                return;
            }
        }

        static bool Centered(double[] rr, double[] er)
        {
            if (er[2] <= 0 || rr[2] <= 0) return false;
            double left = rr[0] - er[0];
            double right = (er[0] + er[2]) - (rr[0] + rr[2]);
            return left >= 1 && right >= 1 && Math.Abs(left - right) <= 1.5;
        }

        static bool Hidden(Dictionary<string, object> e)
        {
            if (Num(e, "v", 1) == 0) return true;
            if (Str(e, "vis") == "hidden") return true;
            if (Num(e, "o", 1) <= 0) return true;
            string c = " " + Str(e, "c") + " ";
            return c.IndexOf(" page-hidden ", StringComparison.Ordinal) >= 0;
        }

        static double[] Box(Dictionary<string, object> e)
        {
            if (!e.TryGetValue("r", out var v) || !(v is List<object> l) || l.Count < 4) return null;
            return new[] { NumOf(l[0]), NumOf(l[1]), NumOf(l[2]), NumOf(l[3]) };
        }

        static string Collapse(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder(s.Length);
            bool space = false;
            for (int i = 0; i < s.Length; i++)
            {
                if (char.IsWhiteSpace(s[i])) { space = sb.Length > 0; continue; }
                if (space) sb.Append(' ');
                space = false;
                sb.Append(s[i]);
            }
            return sb.ToString();
        }

        static string Family(string ff)
        {
            if (string.IsNullOrEmpty(ff)) return "Fredoka";
            int comma = ff.IndexOf(',');
            string first = (comma >= 0 ? ff.Substring(0, comma) : ff).Trim().Trim('"', '\'');
            return first.Length == 0 ? "Fredoka" : first;
        }

        static double CssLength(string css)
        {
            if (string.IsNullOrEmpty(css) || css == "normal") return 0;
            return CssBox.ParseLength(css);
        }

        static double Num(Dictionary<string, object> e, string key, double fallback)
        {
            object v;
            if (!e.TryGetValue(key, out v) || v == null) return fallback;
            return NumOf(v);
        }

        static double NumOf(object v)
        {
            if (v is double) return (double)v;
            if (v is long) return (long)v;
            if (v is int) return (int)v;
            double n;
            return double.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture, out n) ? n : 0;
        }

        static string Str(Dictionary<string, object> e, string key)
        {
            object v;
            if (!e.TryGetValue(key, out v) || v == null) return "";
            return v as string ?? "";
        }
    }
}
