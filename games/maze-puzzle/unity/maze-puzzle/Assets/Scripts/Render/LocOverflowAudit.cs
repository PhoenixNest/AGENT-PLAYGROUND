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
            public int HardOutside, HardOverlap, HardBand, HardWider;
            public int SoftShrink, SoftEllipsis, SoftWrap;
            public string Text = "";

            public int Hard
            {
                get { return HardOutside + HardOverlap + HardBand + HardWider; }
            }

            public int Soft
            {
                get { return SoftShrink + SoftEllipsis + SoftWrap; }
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
                var icons = IconsOf(els, e, byI);
                bool fragment = runs.Count >= 2;
                bool stacked = false;
                if (runs.Count > 1 && runs[0] is Dictionary<string, object> first && runs[1] is Dictionary<string, object> second)
                {
                    double[] a = Box(first);
                    double[] b = Box(second);
                    if (a != null && b != null && b[1] > a[1] + 1) stacked = true;
                }
                bool countedFragment = false;
                for (int ri = 0; ri < runs.Count; ri++)
                {
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
                    AuditRun(name, e, er, rr, shown, icons, iphone, stacked, canvas, report);
                }
            }
        }

        static void AuditRun(string file, Dictionary<string, object> e, double[] er, double[] rr, string shown,
            List<TextFit.Icon> icons, bool iphone, bool stacked, Canvas2D canvas, Report report)
        {
            string font = Str(e, "fw") + " " + Str(e, "fz") + " " + Family(Str(e, "ff"));
            double ls = CssLength(Str(e, "ls"));
            double fontPx = CssLength(Str(e, "fz"));
            string id = Str(e, "id");
            if (id.Length == 0) id = Str(e, "c");
            var input = new TextFit.Input
            {
                RunX = rr[0], RunY = rr[1], RunW = rr[2], RunH = rr[3],
                ElX = er[0], ElY = er[1], ElW = er[2], ElH = er[3],
                CenteredBlock = Centered(rr, er),
                IsButton = Str(e, "t") == "button",
                FontPx = fontPx,
                LetterSpacing = ls,
                OriginalMultiLine = stacked || (fontPx > 0 && rr[3] >= TextFit.TwoLineFactor * fontPx),
                SidePad = TextFit.SidePadding,
                Icons = icons == null || icons.Count == 0 ? null : icons.ToArray(),
                BandY = iphone ? HomeIndicatorY : 0
            };
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
            if (ellipsis)
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
            for (int i = 0; i < lines.Count; i++)
            {
                double tw = Measure(canvas, font, lines[i], plan.FontPx, plan.LetterSpacing);
                double x = TextFit.PlaceX(plan.Anchor, er[0], er[2], tw, plan.Left, plan.Right);
                if (lines.Count == 1 && plan.IconIndex >= 0) x = plan.X;
                double top = rr[1] + i * pitch;
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
                        if (Overlaps(x, top, tw, pitch, ix, icon.Y, icon.W, icon.H))
                        {
                            report.HardOverlap++;
                            Add(report, file, id, "hard-overlap", "icon=" + icon.Index.ToString(CultureInfo.InvariantCulture));
                        }
                    }
                }
            }
        }

        static bool Overlaps(double x, double y, double w, double h, double ix, double iy, double iw, double ih)
        {
            double x0 = Math.Max(x, ix);
            double x1 = Math.Min(x + w, ix + iw);
            double y0 = Math.Max(y, iy);
            double y1 = Math.Min(y + h, iy + ih);
            return x1 - x0 > 0.5 && y1 - y0 > 0.5;
        }

        static double Measure(Canvas2D canvas, string font, string text, double px, double ls)
        {
            canvas.Font = FontSized(font, px);
            canvas.LetterSpacing = ls;
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
            sb.Append(" soft_shrink=").Append(report.SoftShrink.ToString(CultureInfo.InvariantCulture));
            sb.Append(" soft_ellipsis=").Append(report.SoftEllipsis.ToString(CultureInfo.InvariantCulture));
            sb.Append(" soft_wrap=").Append(report.SoftWrap.ToString(CultureInfo.InvariantCulture));
            sb.Append('\n');
            // TODO: fragment elements are still translated one run at a time.
            sb.Append("fragments are per-run translations; joining them is a later task\n");
            for (int i = 0; i < report.Findings.Count; i++)
            {
                Finding f = report.Findings[i];
                sb.Append(f.Kind).Append('\t').Append(f.File).Append('\t').Append(f.ElementId).Append('\t').Append(f.Detail).Append('\n');
            }
            return sb.ToString();
        }

        static List<TextFit.Icon> IconsOf(List<Dictionary<string, object>> els, Dictionary<string, object> parent, Dictionary<int, int> byI)
        {
            int id = (int)Num(parent, "i", -1);
            List<TextFit.Icon> list = null;
            if (!parent.TryGetValue("runs", out var rv) || !(rv is List<object> runs) || runs.Count == 0) return null;
            if (!(runs[0] is Dictionary<string, object> run)) return null;
            double[] rr = Box(run);
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
                if (!emoji && !symbol) continue;
                double iconRight = box[0] + box[2];
                double runRight = rr[0] + rr[2];
                bool lead = iconRight <= rr[0] + 2 && rr[0] - iconRight <= TextFit.IconGapSlop;
                bool trail = box[0] >= runRight - 2 && box[0] - runRight <= TextFit.IconGapSlop;
                if (!lead && !trail) continue;
                if (list == null) list = new List<TextFit.Icon>();
                list.Add(new TextFit.Icon { Index = i, X = box[0], Y = box[1], W = box[2], H = box[3] });
            }
            return list;
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
