using System;

namespace OrbitalSalvage.Render
{
    /// <summary>
    /// Places a translated or overridden string inside the captured English element box.
    /// Pure: the caller measures. English paint does not call this.
    /// </summary>
    /// <remarks>
    /// Side padding is 16px, the horizontal padding of <c>.btn-cartoon-primary</c>
    /// (the 288px pause buttons). A shrink-wrapped span whose captured run is already
    /// wider than that inset relaxes the pad back to the element edges so a same-length
    /// string still fits. Sibling icons that are children of the element reserve their box.
    /// A button whose icon+text pair is centered shifts that icon with the text when the
    /// pair still fits. A shrink-wrapped label span (the home play label) does not own
    /// the sibling icon, so the text stays inside its own box and the icon is not moved.
    /// </remarks>
    public static class TextFit
    {
        public const double SidePadding = 16;
        public const double FloorRatio = 0.72;
        public const double ShrinkStep = 0.02;
        public const double SoftShrink = 0.80;
        public const double CenterSlop = 2;
        public const double RightEdgeSlop = 1.5;
        public const double RightMinLeft = 8;
        /// <summary>A box this close to the run width is the text itself, not a centering frame.</summary>
        public const double ShrinkWrapSlack = 2;
        /// <summary>A button run this close to the element center stays centered. The coin on Claim +100 sits 6px off.</summary>
        public const double ButtonCenterSlop = 8;
        public const double FlushLeftSlack = 1;
        public const double OutsideSlop = 0.5;
        public const double TwoLineFactor = 1.9;
        /// <summary>A captured run at most this tall is one line: <c>1.5 * fontPx + 2</c>.</summary>
        public const double SingleLineFactor = 1.5;
        public const double SingleLineSlack = 2;
        /// <summary>An element shorter than this cannot grow a second line.</summary>
        public const double ControlHeightFactor = 2.2;
        public const double IconGapSlop = 8;
        /// <summary>iPhone home-indicator band. Text whose bottom passes this is a hard audit finding.</summary>
        public const double HomeIndicatorY = 898.25;
        public const string Ellipsis = "\u2026";

        public enum Anchor
        {
            Left,
            Center,
            Right
        }

        public enum Action
        {
            Single,
            Wrap,
            Shrink,
            Ellipsis
        }

        public struct Icon
        {
            public int Index;
            public double X, Y, W, H;
        }

        public struct Input
        {
            public double RunX, RunY, RunW, RunH;
            public double ElX, ElY, ElW, ElH;
            public bool CenteredBlock;
            public bool IsButton;
            /// <summary>Id or class looks like a button, even when the tag is not <c>button</c>.</summary>
            public bool ButtonLike;
            public double FontPx;
            public double LetterSpacing;
            public bool OriginalMultiLine;
            public double SidePad;
            public Icon[] Icons;
            /// <summary>When &gt; 0, wrapped lines may not extend below this y. 0 disables the band.</summary>
            public double BandY;
            /// <summary>Bottom y this run may use. 0 means the element bottom. Following runs reserve their captured height below this.</summary>
            public double CeilingY;
            /// <summary>Parent content box. ParentW &lt;= 0 means unknown. Used only when this element is shrink-wrapped to the run.</summary>
            public double ParentX, ParentW;
        }

        public struct Layout
        {
            public Anchor Anchor;
            public Action Action;
            public double FontPx;
            public double LetterSpacing;
            public double Scale;
            public double Available;
            public double Left;
            public double Right;
            public int MaxLines;
            public double LinePitch;
            public double X;
            public double IconShift;
            public int IconIndex;
            public bool SoftShrink;
            public bool KeepX;
        }

        public delegate double MeasureAt(double fontPx, double letterSpacing);

        public static Layout Plan(Input input, MeasureAt measure)
        {
            if (input.SidePad <= 0) input.SidePad = SidePadding;
            var plan = new Layout
            {
                Anchor = ChooseAnchor(input),
                FontPx = input.FontPx,
                LetterSpacing = input.LetterSpacing,
                Scale = 1,
                IconIndex = -1,
                LinePitch = input.RunH > 1 ? input.RunH : Math.Max(1, input.FontPx)
            };
            double width = measure != null ? measure(input.FontPx, input.LetterSpacing) : input.RunW;
            // An icon that already sits on the English run is left alone unless the translation is wider.
            if (HasRunOverlap(input) && width <= input.RunW + 0.5)
            {
                plan.Action = Action.Single;
                plan.KeepX = true;
                plan.X = input.RunX;
                plan.Left = input.RunX;
                plan.Right = input.RunX + Math.Max(1, input.RunW);
                plan.Available = Math.Max(1, input.RunW);
                return plan;
            }
            Limits(input, out double left, out double right, out bool leading, out int leadIndex, out double leadX, out double leadW);
            plan.Left = left;
            plan.Right = right;
            plan.Available = Math.Max(1, right - left);
            if (width <= plan.Available + 0.5)
            {
                plan.Action = Action.Single;
                PlaceSingle(input, ref plan, width, leading, leadIndex, leadX, leadW);
                return plan;
            }
            if (AllowsWrap(input))
            {
                int room = MaxLines(input, plan.LinePitch);
                if (room >= 2)
                {
                    plan.Action = Action.Wrap;
                    plan.MaxLines = room;
                    return plan;
                }
            }
            int steps = (int)Math.Round((1 - FloorRatio) / ShrinkStep);
            for (int step = 1; step <= steps; step++)
            {
                double scale = 1 - step * ShrinkStep;
                double px = input.FontPx * scale;
                double ls = input.LetterSpacing * scale;
                double w = measure != null ? measure(px, ls) : width * scale;
                if (w <= plan.Available + 0.5)
                {
                    plan.Action = Action.Shrink;
                    plan.Scale = scale;
                    plan.FontPx = px;
                    plan.LetterSpacing = ls;
                    plan.SoftShrink = scale < SoftShrink - 1e-9;
                    PlaceSingle(input, ref plan, w, leading, leadIndex, leadX, leadW);
                    return plan;
                }
            }
            if (AllowsWrap(input))
            {
                int room = MaxLines(input, plan.LinePitch);
                if (room >= 2)
                {
                    plan.Action = Action.Wrap;
                    plan.MaxLines = room;
                    plan.Scale = FloorRatio;
                    plan.FontPx = input.FontPx * FloorRatio;
                    plan.LetterSpacing = input.LetterSpacing * FloorRatio;
                    plan.SoftShrink = true;
                    return plan;
                }
            }
            plan.Action = Action.Ellipsis;
            plan.Scale = FloorRatio;
            plan.FontPx = input.FontPx * FloorRatio;
            plan.LetterSpacing = input.LetterSpacing * FloorRatio;
            plan.SoftShrink = true;
            double ellipsisWidth = measure != null ? measure(plan.FontPx, plan.LetterSpacing) : width * FloorRatio;
            PlaceSingle(input, ref plan, Math.Min(ellipsisWidth, plan.Available), leading, leadIndex, leadX, leadW);
            return plan;
        }

        public static Anchor ChooseAnchor(Input input)
        {
            double runCenter = input.RunX + input.RunW * 0.5;
            double elCenter = input.ElX + input.ElW * 0.5;
            double leftGap = input.RunX - input.ElX;
            double rightGap = (input.ElX + input.ElW) - (input.RunX + input.RunW);
            if (input.IsButton)
            {
                Icon lead;
                if (TryLeading(input, out lead))
                {
                    double pairLeft = lead.X;
                    double pairRight = input.RunX + input.RunW;
                    double pairCenter = (pairLeft + pairRight) * 0.5;
                    if (Math.Abs(pairCenter - elCenter) <= CenterSlop) return Anchor.Center;
                }
            }
            // A shrink-wrapped box has the same centre as its run because the box is the English text.
            // That is not centering. Left is the captured edge, unless the box itself is centered or
            // flush-right in a wider parent.
            if (input.ElW - input.RunW <= ShrinkWrapSlack && leftGap <= FlushLeftSlack && rightGap <= ShrinkWrapSlack)
                return ShrinkAnchor(input);
            if (input.IsButton && leftGap >= RightMinLeft && rightGap >= RightMinLeft && Math.Abs(runCenter - elCenter) <= ButtonCenterSlop)
                return Anchor.Center;
            if (Math.Abs(runCenter - elCenter) <= CenterSlop || input.CenteredBlock) return Anchor.Center;
            if (rightGap <= RightEdgeSlop && leftGap >= RightMinLeft) return Anchor.Right;
            return Anchor.Left;
        }

        static Anchor ShrinkAnchor(Input input)
        {
            if (input.ParentW > 1)
            {
                double elCenter = input.ElX + input.ElW * 0.5;
                double parentCenter = input.ParentX + input.ParentW * 0.5;
                bool parentWider = input.ParentW > input.ElW + ShrinkWrapSlack;
                if (parentWider && Math.Abs(elCenter - parentCenter) <= CenterSlop) return Anchor.Center;
                double parentRight = input.ParentX + input.ParentW;
                double elRight = input.ElX + input.ElW;
                if (parentWider && parentRight - elRight <= RightEdgeSlop && input.ElX - input.ParentX >= RightMinLeft)
                    return Anchor.Right;
            }
            return Anchor.Left;
        }

        public static double PlaceX(Anchor anchor, double elX, double elW, double textW, double left, double right)
        {
            double x;
            if (anchor == Anchor.Right) x = right - textW;
            else if (anchor == Anchor.Center) x = (left + right - textW) * 0.5;
            else x = left;
            double minX = elX - OutsideSlop;
            double maxX = elX + elW - textW + OutsideSlop;
            if (textW <= right - left + 0.5)
            {
                if (x < left) x = left;
                if (x + textW > right) x = right - textW;
            }
            if (x < minX) x = minX;
            if (textW <= elW + 1 && x > maxX) x = maxX;
            return x;
        }

        public static string Ellipsize(string text, double width, Func<string, double> measure)
        {
            if (text == null) text = "";
            if (measure(text) <= width + 0.5) return text;
            if (measure(Ellipsis) > width + 0.5) return Ellipsis;
            int lo = 0;
            int hi = text.Length;
            while (lo < hi)
            {
                int mid = (lo + hi + 1) / 2;
                if (mid > 0 && mid < text.Length && char.IsHighSurrogate(text[mid - 1])) mid--;
                if (mid <= lo) break;
                string trial = text.Substring(0, mid) + Ellipsis;
                if (measure(trial) <= width + 0.5) lo = mid;
                else hi = mid - 1;
            }
            if (lo > 0 && lo < text.Length && char.IsHighSurrogate(text[lo - 1])) lo--;
            return text.Substring(0, lo) + Ellipsis;
        }

        static void PlaceSingle(Input input, ref Layout plan, double textW, bool leading, int leadIndex, double leadX, double leadW)
        {
            if (plan.Anchor == Anchor.Center && input.IsButton && leading)
            {
                double gap = input.RunX - (leadX + leadW);
                if (gap < 0) gap = 0;
                if (gap <= IconGapSlop)
                {
                    double pairW = leadW + gap + textW;
                    double pairLeft = input.ElX + (input.ElW - pairW) * 0.5;
                    bool fits = pairLeft >= input.ElX - OutsideSlop
                        && pairLeft + pairW <= input.ElX + input.ElW + OutsideSlop;
                    if (fits)
                    {
                        plan.IconShift = pairLeft - leadX;
                        plan.IconIndex = leadIndex;
                        plan.X = pairLeft + leadW + gap;
                        return;
                    }
                }
            }
            plan.X = PlaceX(plan.Anchor, input.ElX, input.ElW, textW, plan.Left, plan.Right);
        }

        /// <summary>Tag, id, or class marks a control that cannot grow a second line.</summary>
        public static bool LooksLikeControl(string tag, string id, string classes)
        {
            if (tag == "button") return true;
            string blob = (id ?? "") + " " + (classes ?? "");
            return blob.IndexOf("btn", StringComparison.OrdinalIgnoreCase) >= 0
                || blob.IndexOf("button", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool RunIsSingleLine(Input input)
        {
            if (input.FontPx <= 0) return true;
            return input.RunH <= SingleLineFactor * input.FontPx + SingleLineSlack;
        }

        /// <summary>Single-line buttons and short controls shrink and ellipsize. They do not wrap.</summary>
        public static bool LocksToOneLine(Input input)
        {
            if (!RunIsSingleLine(input)) return false;
            if (input.IsButton || input.ButtonLike) return true;
            if (input.FontPx <= 0) return false;
            return input.ElH < ControlHeightFactor * input.FontPx;
        }

        /// <summary>Wrap only a run that was already multi-line, or a text block taller than two lines.</summary>
        public static bool AllowsWrap(Input input)
        {
            if (LocksToOneLine(input)) return false;
            if (input.OriginalMultiLine) return true;
            if (input.FontPx <= 0) return false;
            if (!RunIsSingleLine(input)) return true;
            return !input.IsButton && !input.ButtonLike && input.ElH > 2.0 * input.FontPx;
        }

        public static bool OverlapsRun(Icon icon, Input input)
        {
            double x0 = Math.Max(icon.X, input.RunX);
            double x1 = Math.Min(icon.X + icon.W, input.RunX + input.RunW);
            double y0 = Math.Max(icon.Y, input.RunY);
            double y1 = Math.Min(icon.Y + icon.H, input.RunY + input.RunH);
            return x1 - x0 > 0.5 && y1 - y0 > 0.5;
        }

        static bool HasRunOverlap(Input input)
        {
            if (input.Icons == null) return false;
            for (int i = 0; i < input.Icons.Length; i++)
                if (OverlapsRun(input.Icons[i], input)) return true;
            return false;
        }

        static int MaxLines(Input input, double pitch)
        {
            if (LocksToOneLine(input)) return 1;
            if (pitch < 1) pitch = 1;
            double bottom = input.ElY + input.ElH;
            if (input.CeilingY > 0) bottom = Math.Min(bottom, input.CeilingY);
            int max = (int)Math.Floor((bottom - input.RunY + 0.5) / pitch);
            if (max < 1) max = 1;
            if (input.BandY > 0)
            {
                int fit = 0;
                for (int i = 0; i < max; i++)
                {
                    double lineBottom = input.RunY + (i + 1) * pitch;
                    if (lineBottom > input.BandY + 0.01 && i > 0) break;
                    fit++;
                }
                if (fit < 1) fit = 1;
                max = fit;
            }
            return max;
        }

        static void Limits(Input input, out double left, out double right, out bool leading, out int leadIndex, out double leadX, out double leadW)
        {
            double pad = input.SidePad;
            left = input.ElX + pad;
            right = input.ElX + input.ElW - pad;
            // A run already sitting on the element's left edge is a block label. The 16px button pad
            // would indent it away from the sibling line that shares that edge.
            if (input.RunX - input.ElX <= FlushLeftSlack)
                left = input.RunX;
            leading = false;
            leadIndex = -1;
            leadX = 0;
            leadW = 0;
            Icon[] icons = input.Icons;
            if (icons != null)
            {
                for (int i = 0; i < icons.Length; i++)
                {
                    Icon icon = icons[i];
                    if (icon.W <= 0 || icon.H <= 0) continue;
                    if (icon.Y + icon.H < input.RunY - 2 || icon.Y > input.RunY + input.RunH + 2) continue;
                    double iconRight = icon.X + icon.W;
                    double runRight = input.RunX + input.RunW;
                    bool lead = iconRight <= input.RunX + 2 && input.RunX - iconRight <= IconGapSlop;
                    bool trail = icon.X >= runRight - 2 && icon.X - runRight <= IconGapSlop;
                    if (lead)
                    {
                        double edge = iconRight + Math.Max(0, input.RunX - iconRight);
                        if (edge > left) left = edge;
                        if (!leading || icon.X < leadX)
                        {
                            leading = true;
                            leadIndex = icon.Index;
                            leadX = icon.X;
                            leadW = icon.W;
                        }
                    }
                    else if (trail)
                    {
                        double edge = icon.X - Math.Max(0, icon.X - runRight);
                        if (edge < right) right = edge;
                    }
                    else if (OverlapsRun(icon, input))
                    {
                        double iconMid = icon.X + icon.W * 0.5;
                        double runMid = input.RunX + input.RunW * 0.5;
                        if (iconMid <= runMid)
                        {
                            if (iconRight > left) left = iconRight;
                        }
                        else if (icon.X < right) right = icon.X;
                    }
                }
            }
            if (right - left < input.RunW && input.RunW <= input.ElW + 0.5)
            {
                double relaxedLeft = input.ElX;
                double relaxedRight = input.ElX + input.ElW;
                if (leading || left > input.ElX + pad + 0.01) relaxedLeft = Math.Max(relaxedLeft, left);
                if (right < input.ElX + input.ElW - pad - 0.01) relaxedRight = Math.Min(relaxedRight, right);
                if (icons != null)
                {
                    for (int i = 0; i < icons.Length; i++)
                    {
                        Icon icon = icons[i];
                        double runRight = input.RunX + input.RunW;
                        if (icon.X >= runRight - 2 && icon.X - runRight <= IconGapSlop)
                            relaxedRight = Math.Min(relaxedRight, icon.X);
                    }
                }
                if (relaxedRight - relaxedLeft > right - left)
                {
                    left = relaxedLeft;
                    right = relaxedRight;
                }
            }
            if (right < left) right = left + 1;
        }

        static bool TryLeading(Input input, out Icon lead)
        {
            lead = default;
            if (input.Icons == null) return false;
            bool found = false;
            for (int i = 0; i < input.Icons.Length; i++)
            {
                Icon icon = input.Icons[i];
                double iconRight = icon.X + icon.W;
                if (iconRight <= input.RunX + 2 && input.RunX - iconRight <= IconGapSlop)
                {
                    if (!found || icon.X < lead.X) lead = icon;
                    found = true;
                }
            }
            return found;
        }
    }
}
