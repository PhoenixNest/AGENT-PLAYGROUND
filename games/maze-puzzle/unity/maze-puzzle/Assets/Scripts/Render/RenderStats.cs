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
                CacheBuilds = CacheBuilds
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
                CacheBuilds = now.CacheBuilds - then.CacheBuilds
            };
        }
    }
}
