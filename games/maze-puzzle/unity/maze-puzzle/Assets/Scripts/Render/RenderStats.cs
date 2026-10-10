namespace OrbitalSalvage.Render
{
    /// <summary>
    /// Process-wide paint counters. Call sites only increment static ints. Nothing here allocates,
    /// and nothing here is read on the product path unless a perf or bench consumer asks.
    /// </summary>
    public static class RenderStats
    {
        public static int Submits;
        public static int Commands;
        public static int Draws;
        public static int BackdropExact;
        public static int BackdropGpu;
        public static int Shadows;
        public static int Blits;
        public static int CacheBuilds;
        public static int Edges;
        public static int ShadowScopeHit;
        public static int ShadowLowHit;
        public static int ShadowCacheHit;
        public static int ShadowMissClip;
        public static int ShadowMissFmt;
        public static int ShadowMissSmall;
        public static int BackdropCacheHit;
        public static int BackdropCacheMiss;
        public static int RoundShadow;
        public static int DirtyFrames;
        public static int DirtyFull;
        public static int DirtyFullCold;
        public static int DirtyFullPage;
        public static int DirtyFullArea;
        public static int DirtyAreaSum;
        public static int ShapeRect, ShapeCirc, ShapeEll, ShapeRr, ShapeStroke, ShapeOther;
        public static int EdgeRect, EdgeCirc, EdgeEll, EdgeRr, EdgeStroke, EdgeOther;
        public static int GameRect, GameCirc, GameEll, GameRr, GameStroke, GameOther;
        public static int GameEdgeRect, GameEdgeCirc, GameEdgeEll, GameEdgeRr, GameEdgeStroke, GameEdgeOther;
        public static int FastRect, FastCirc, FastEll, FastRr, FastLine, FastArc;
        public static int FastBatch, FastBatchInst;
        public static int GpuAlloc, ReadPx;
        /// <summary>Playfield section draws. sky, hole, ring, flappers, bumpers, gates, debris, vfx.</summary>
        public static readonly int[] SecDraw = new int[8];
        public static readonly int[] SecEdge = new int[8];
        public static int RoundMissXform, RoundMissInset, RoundMissText, RoundMissShader;
        public static int PrewarmBake;
        /// <summary>0 chrome, 1 playfield. Shape counters split on this.</summary>
        public static int Caller;
        /// <summary>When true, <see cref="RenderShape"/> adds its edge count to the pending kind.</summary>
        public static bool CountShape;
        public static int PendingKind;
        /// <summary>Last mover whose bounds exceeded 85% of the page. Not cleared per frame.</summary>
        public static string DirtyWide = "";
        public static int DirtyWidePct;
        public static string DirtyWideLogged = "";

        /// <summary>kind: 0 other, 1 rect, 2 circle, 3 ellipse, 4 round-rect, 5 stroke.</summary>
        public static void NoteCmd(int kind)
        {
            BumpCmd(kind, false);
            if (Caller == 1) BumpCmd(kind, true);
        }

        public static void NoteSec(int slot, int draws, int edges)
        {
            if ((uint)slot >= 8) return;
            SecDraw[slot] += draws;
            SecEdge[slot] += edges;
        }

        public static void NoteEdge(int n)
        {
            if (!CountShape || n <= 0) return;
            BumpEdge(PendingKind, n, false);
            if (Caller == 1) BumpEdge(PendingKind, n, true);
        }

        static void BumpCmd(int kind, bool game)
        {
            if (game)
            {
                if (kind == 1) GameRect++;
                else if (kind == 2) GameCirc++;
                else if (kind == 3) GameEll++;
                else if (kind == 4) GameRr++;
                else if (kind == 5) GameStroke++;
                else GameOther++;
            }
            else if (kind == 1) ShapeRect++;
            else if (kind == 2) ShapeCirc++;
            else if (kind == 3) ShapeEll++;
            else if (kind == 4) ShapeRr++;
            else if (kind == 5) ShapeStroke++;
            else ShapeOther++;
        }

        static void BumpEdge(int kind, int n, bool game)
        {
            if (game)
            {
                if (kind == 1) GameEdgeRect += n;
                else if (kind == 2) GameEdgeCirc += n;
                else if (kind == 3) GameEdgeEll += n;
                else if (kind == 4) GameEdgeRr += n;
                else if (kind == 5) GameEdgeStroke += n;
                else GameEdgeOther += n;
            }
            else if (kind == 1) EdgeRect += n;
            else if (kind == 2) EdgeCirc += n;
            else if (kind == 3) EdgeEll += n;
            else if (kind == 4) EdgeRr += n;
            else if (kind == 5) EdgeStroke += n;
            else EdgeOther += n;
        }

        public struct Snap
        {
            public int Submits;
            public int Commands;
            public int Draws;
            public int BackdropExact;
            public int BackdropGpu;
            public int Shadows;
            public int Blits;
            public int CacheBuilds;
            public int Edges;
            public int ShadowScopeHit;
            public int ShadowLowHit;
            public int ShadowCacheHit;
            public int ShadowMissClip;
            public int ShadowMissFmt;
            public int ShadowMissSmall;
            public int BackdropCacheHit;
            public int BackdropCacheMiss;
            public int RoundShadow;
            public int DirtyFrames;
            public int DirtyFull;
            public int DirtyFullCold;
            public int DirtyFullPage;
            public int DirtyFullArea;
            public int DirtyAreaSum;
            public int ShapeRect, ShapeCirc, ShapeEll, ShapeRr, ShapeStroke, ShapeOther;
            public int EdgeRect, EdgeCirc, EdgeEll, EdgeRr, EdgeStroke, EdgeOther;
            public int GameRect, GameCirc, GameEll, GameRr, GameStroke, GameOther;
            public int GameEdgeRect, GameEdgeCirc, GameEdgeEll, GameEdgeRr, GameEdgeStroke, GameEdgeOther;
            public int FastRect, FastCirc, FastEll, FastRr, FastLine, FastArc;
            public int FastBatch, FastBatchInst;
            public int GpuAlloc, ReadPx;
            public int[] SecDraw, SecEdge;
            public int RoundMissXform, RoundMissInset, RoundMissText, RoundMissShader;
            public int PrewarmBake;
        }

        static int[] Copy8(int[] src)
        {
            var d = new int[8];
            if (src != null)
            {
                int n = src.Length < 8 ? src.Length : 8;
                for (int i = 0; i < n; i++) d[i] = src[i];
            }
            return d;
        }

        static int[] Sub8(int[] now, int[] then)
        {
            var d = new int[8];
            for (int i = 0; i < 8; i++)
            {
                int a = now != null && i < now.Length ? now[i] : 0;
                int b = then != null && i < then.Length ? then[i] : 0;
                d[i] = a - b;
            }
            return d;
        }

        public static Snap Read()
        {
            return new Snap
            {
                Submits = Submits,
                Commands = Commands,
                Draws = Draws,
                BackdropExact = BackdropExact,
                BackdropGpu = BackdropGpu,
                Shadows = Shadows,
                Blits = Blits,
                CacheBuilds = CacheBuilds,
                Edges = Edges,
                ShadowScopeHit = ShadowScopeHit,
                ShadowLowHit = ShadowLowHit,
                ShadowCacheHit = ShadowCacheHit,
                ShadowMissClip = ShadowMissClip,
                ShadowMissFmt = ShadowMissFmt,
                ShadowMissSmall = ShadowMissSmall,
                BackdropCacheHit = BackdropCacheHit,
                BackdropCacheMiss = BackdropCacheMiss,
                RoundShadow = RoundShadow,
                DirtyFrames = DirtyFrames,
                DirtyFull = DirtyFull,
                DirtyFullCold = DirtyFullCold,
                DirtyFullPage = DirtyFullPage,
                DirtyFullArea = DirtyFullArea,
                DirtyAreaSum = DirtyAreaSum,
                ShapeRect = ShapeRect, ShapeCirc = ShapeCirc, ShapeEll = ShapeEll, ShapeRr = ShapeRr, ShapeStroke = ShapeStroke, ShapeOther = ShapeOther,
                EdgeRect = EdgeRect, EdgeCirc = EdgeCirc, EdgeEll = EdgeEll, EdgeRr = EdgeRr, EdgeStroke = EdgeStroke, EdgeOther = EdgeOther,
                GameRect = GameRect, GameCirc = GameCirc, GameEll = GameEll, GameRr = GameRr, GameStroke = GameStroke, GameOther = GameOther,
                GameEdgeRect = GameEdgeRect, GameEdgeCirc = GameEdgeCirc, GameEdgeEll = GameEdgeEll, GameEdgeRr = GameEdgeRr, GameEdgeStroke = GameEdgeStroke, GameEdgeOther = GameEdgeOther,
                FastRect = FastRect, FastCirc = FastCirc, FastEll = FastEll, FastRr = FastRr, FastLine = FastLine, FastArc = FastArc,
                FastBatch = FastBatch, FastBatchInst = FastBatchInst,
                GpuAlloc = GpuAlloc, ReadPx = ReadPx,
                SecDraw = Copy8(SecDraw), SecEdge = Copy8(SecEdge),
                RoundMissXform = RoundMissXform, RoundMissInset = RoundMissInset, RoundMissText = RoundMissText, RoundMissShader = RoundMissShader,
                PrewarmBake = PrewarmBake
            };
        }

        public static Snap Delta(Snap now, Snap then)
        {
            return new Snap
            {
                Submits = now.Submits - then.Submits,
                Commands = now.Commands - then.Commands,
                Draws = now.Draws - then.Draws,
                BackdropExact = now.BackdropExact - then.BackdropExact,
                BackdropGpu = now.BackdropGpu - then.BackdropGpu,
                Shadows = now.Shadows - then.Shadows,
                Blits = now.Blits - then.Blits,
                CacheBuilds = now.CacheBuilds - then.CacheBuilds,
                Edges = now.Edges - then.Edges,
                ShadowScopeHit = now.ShadowScopeHit - then.ShadowScopeHit,
                ShadowLowHit = now.ShadowLowHit - then.ShadowLowHit,
                ShadowCacheHit = now.ShadowCacheHit - then.ShadowCacheHit,
                ShadowMissClip = now.ShadowMissClip - then.ShadowMissClip,
                ShadowMissFmt = now.ShadowMissFmt - then.ShadowMissFmt,
                ShadowMissSmall = now.ShadowMissSmall - then.ShadowMissSmall,
                BackdropCacheHit = now.BackdropCacheHit - then.BackdropCacheHit,
                BackdropCacheMiss = now.BackdropCacheMiss - then.BackdropCacheMiss,
                RoundShadow = now.RoundShadow - then.RoundShadow,
                DirtyFrames = now.DirtyFrames - then.DirtyFrames,
                DirtyFull = now.DirtyFull - then.DirtyFull,
                DirtyFullCold = now.DirtyFullCold - then.DirtyFullCold,
                DirtyFullPage = now.DirtyFullPage - then.DirtyFullPage,
                DirtyFullArea = now.DirtyFullArea - then.DirtyFullArea,
                DirtyAreaSum = now.DirtyAreaSum - then.DirtyAreaSum,
                ShapeRect = now.ShapeRect - then.ShapeRect, ShapeCirc = now.ShapeCirc - then.ShapeCirc, ShapeEll = now.ShapeEll - then.ShapeEll,
                ShapeRr = now.ShapeRr - then.ShapeRr, ShapeStroke = now.ShapeStroke - then.ShapeStroke, ShapeOther = now.ShapeOther - then.ShapeOther,
                EdgeRect = now.EdgeRect - then.EdgeRect, EdgeCirc = now.EdgeCirc - then.EdgeCirc, EdgeEll = now.EdgeEll - then.EdgeEll,
                EdgeRr = now.EdgeRr - then.EdgeRr, EdgeStroke = now.EdgeStroke - then.EdgeStroke, EdgeOther = now.EdgeOther - then.EdgeOther,
                GameRect = now.GameRect - then.GameRect, GameCirc = now.GameCirc - then.GameCirc, GameEll = now.GameEll - then.GameEll,
                GameRr = now.GameRr - then.GameRr, GameStroke = now.GameStroke - then.GameStroke, GameOther = now.GameOther - then.GameOther,
                GameEdgeRect = now.GameEdgeRect - then.GameEdgeRect, GameEdgeCirc = now.GameEdgeCirc - then.GameEdgeCirc, GameEdgeEll = now.GameEdgeEll - then.GameEdgeEll,
                GameEdgeRr = now.GameEdgeRr - then.GameEdgeRr, GameEdgeStroke = now.GameEdgeStroke - then.GameEdgeStroke, GameEdgeOther = now.GameEdgeOther - then.GameEdgeOther,
                FastRect = now.FastRect - then.FastRect, FastCirc = now.FastCirc - then.FastCirc, FastEll = now.FastEll - then.FastEll,
                FastRr = now.FastRr - then.FastRr, FastLine = now.FastLine - then.FastLine, FastArc = now.FastArc - then.FastArc,
                FastBatch = now.FastBatch - then.FastBatch, FastBatchInst = now.FastBatchInst - then.FastBatchInst,
                GpuAlloc = now.GpuAlloc - then.GpuAlloc, ReadPx = now.ReadPx - then.ReadPx,
                SecDraw = Sub8(now.SecDraw, then.SecDraw), SecEdge = Sub8(now.SecEdge, then.SecEdge),
                RoundMissXform = now.RoundMissXform - then.RoundMissXform, RoundMissInset = now.RoundMissInset - then.RoundMissInset,
                RoundMissText = now.RoundMissText - then.RoundMissText, RoundMissShader = now.RoundMissShader - then.RoundMissShader,
                PrewarmBake = now.PrewarmBake - then.PrewarmBake
            };
        }
    }
}
