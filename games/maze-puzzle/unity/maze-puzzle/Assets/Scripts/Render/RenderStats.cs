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
                DirtyAreaSum = DirtyAreaSum
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
                DirtyAreaSum = now.DirtyAreaSum - then.DirtyAreaSum
            };
        }
    }
}
