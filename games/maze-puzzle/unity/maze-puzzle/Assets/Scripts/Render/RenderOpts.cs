namespace OrbitalSalvage.Render
{
    /// <summary>
    /// Product switches pushed by <c>GpuOpt</c> from PlayerPrefs <c>os.parity.opt</c>.
    /// All default false. Render code reads these; it does not parse the pref.
    /// </summary>
    public static class RenderOpts
    {
        public static bool ShadowScope;
        public static bool ShadowLow;
        public static bool BackdropScope;
        public static bool BackdropLow;
        public static bool Dirty;
        public static bool GameOne;
        public static int AmbientHz;

        public static void Clear()
        {
            ShadowScope = false;
            ShadowLow = false;
            BackdropScope = false;
            BackdropLow = false;
            Dirty = false;
            GameOne = false;
            AmbientHz = 0;
        }
    }
}
