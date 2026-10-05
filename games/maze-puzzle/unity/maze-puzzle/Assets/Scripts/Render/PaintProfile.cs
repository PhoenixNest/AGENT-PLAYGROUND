using System;
using System.Diagnostics;
using System.Text;

namespace OrbitalSalvage.Render
{
    /// <summary>
    /// Exclusive stage times for one <see cref="HomeChrome"/> draw. Default off; the viewer self-test turns it on.
    /// Nested <see cref="Begin"/>/<see cref="End"/> pairs subtract child time so the rows sum to the draw.
    /// </summary>
    public static class PaintProfile
    {
        public static bool Enabled;

        public const int Cache = 0, PaintBox = 1, CssBox = 2, PaintRuns = 3, ParseShadows = 4, ToCss = 5,
            Layout = 6, GlyphEmit = 7, GlyphFlat = 8, Stroker = 9, FillPolys = 10, RenderShape = 11,
            Shadow = 12, Submit = 13, Blit = 14, Backdrop = 15, Readback = 16, CpuBlur = 17;

        static readonly string[] Names =
        {
            "cache", "paintBox", "cssBox", "paintRuns", "parseShadows", "toCss",
            "textLayout", "glyphEmit", "glyphFlat", "stroker", "fillPolys", "renderShape",
            "shadow", "submit", "blit", "backdrop", "readback", "cpuBlur"
        };

        const int N = 18;
        const int StackMax = 32;

        static readonly long[] ticks = new long[N];
        static readonly int[] calls = new int[N];
        static readonly int[] stack = new int[StackMax];
        static readonly long[] started = new long[StackMax];
        static readonly long[] child = new long[StackMax];
        static int sp;
        static long wall;

        public static int Elements, Runs, Glyphs, FlatMisses, ShadowPasses, Flushes, DrawCalls, Verts, ReadPixels, NewTextures;
        public static int CropW, CropH, FrameW, FrameH;
        public static long ReadPixelsTicks;

        public static void Reset()
        {
            Array.Clear(ticks, 0, N);
            Array.Clear(calls, 0, N);
            sp = 0;
            wall = 0;
            Elements = Runs = Glyphs = FlatMisses = ShadowPasses = Flushes = DrawCalls = Verts = ReadPixels = NewTextures = 0;
            ReadPixelsTicks = 0;
        }

        public static long Stamp() => Enabled ? Stopwatch.GetTimestamp() : 0;

        public static long WallTicks => wall;

        public static long StageTicks(int id) => ticks[id];

        public static void AddWall(long dt) { if (Enabled) wall += dt; }

        public static void Begin(int id)
        {
            if (!Enabled) return;
            stack[sp] = id;
            started[sp] = Stopwatch.GetTimestamp();
            child[sp] = 0;
            sp++;
        }

        public static void End(int id)
        {
            if (!Enabled) return;
            sp--;
            long dt = Stopwatch.GetTimestamp() - started[sp];
            ticks[id] += dt - child[sp];
            calls[id]++;
            if (sp > 0) child[sp - 1] += dt;
        }

        public static void CountFlush() { if (Enabled) Flushes++; }
        public static void CountDraw(int verts) { if (Enabled) { DrawCalls++; Verts += verts; } }
        public static void CountReadPixels() { if (Enabled) ReadPixels++; }
        public static void CountNewTexture() { if (Enabled) NewTextures++; }
        public static void CountShadow() { if (Enabled) ShadowPasses++; }
        public static void CountGlyph() { if (Enabled) Glyphs++; }
        public static void CountFlatMiss() { if (Enabled) FlatMisses++; }

        public static string Report(int draws)
        {
            double freq = Stopwatch.Frequency / 1000.0;
            double total = draws > 0 ? wall / freq / draws : 0;
            var sb = new StringBuilder();
            sb.Append("stage | ms/draw | % | calls/draw\n");
            long accounted = 0;
            for (int i = 0; i < N; i++) accounted += ticks[i];
            for (int i = 0; i < N; i++)
            {
                if (calls[i] == 0 && ticks[i] == 0) continue;
                double ms = draws > 0 ? ticks[i] / freq / draws : 0;
                double pct = wall > 0 ? 100.0 * ticks[i] / wall : 0;
                double c = draws > 0 ? (double)calls[i] / draws : 0;
                sb.Append(Names[i]).Append(" | ").Append(ms.ToString("0.000")).Append(" | ").Append(pct.ToString("0.0")).Append(" | ").Append(c.ToString("0.0")).Append('\n');
            }
            double other = draws > 0 ? (wall - accounted) / freq / draws : 0;
            double otherPct = wall > 0 ? 100.0 * (wall - accounted) / wall : 0;
            sb.Append("other | ").Append(other.ToString("0.000")).Append(" | ").Append(otherPct.ToString("0.0")).Append(" | \n");
            sb.Append("TOTAL | ").Append(total.ToString("0.000")).Append(" | 100 | \n");
            sb.Append("counts/draw");
            sb.Append(" elements ").Append(Per(Elements, draws));
            sb.Append(" runs ").Append(Per(Runs, draws));
            sb.Append(" glyphs ").Append(Per(Glyphs, draws));
            sb.Append(" flatMiss ").Append(Per(FlatMisses, draws));
            sb.Append(" shadowPasses ").Append(Per(ShadowPasses, draws));
            sb.Append(" flushes ").Append(Per(Flushes, draws));
            sb.Append(" drawCalls ").Append(Per(DrawCalls, draws));
            sb.Append(" verts ").Append(Per(Verts, draws));
            sb.Append(" readPixels ").Append(Per(ReadPixels, draws));
            sb.Append(" newTex ").Append(Per(NewTextures, draws));
            sb.Append(" crop ").Append(CropW).Append("x").Append(CropH);
            sb.Append(" frame ").Append(FrameW).Append("x").Append(FrameH);
            sb.Append(" readPixelsMs ").Append(draws > 0 ? (ReadPixelsTicks / freq / draws).ToString("0.000") : "0");
            sb.Append('\n');
            return sb.ToString();
        }

        static string Per(int n, int draws) => draws > 0 ? (n / (double)draws).ToString("0.0") : "0";
    }
}
