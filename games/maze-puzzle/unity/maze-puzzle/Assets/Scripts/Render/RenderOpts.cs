using UnityEngine;

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
        public static bool RoundShadow;
        public static bool PlayHalf;
        public static bool DirtyClip;
        public static bool BackdropHold;
        public static bool RoundPlus;
        public static bool FastShape;
        public static bool NoPrewarm;
        public static bool FastArc;
        public static bool PlayBatch;
        public static int AmbientHz;

        public static void Clear()
        {
            ShadowScope = false;
            ShadowLow = false;
            BackdropScope = false;
            BackdropLow = false;
            Dirty = false;
            GameOne = false;
            RoundShadow = false;
            PlayHalf = false;
            DirtyClip = false;
            BackdropHold = false;
            RoundPlus = false;
            FastShape = false;
            NoPrewarm = false;
            FastArc = false;
            PlayBatch = false;
            AmbientHz = 0;
        }

        /// <summary>One boot line: format support and whether the opt shaders loaded from Resources.</summary>
        public static string BootLine()
        {
            int r8 = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8) ? 1 : 0;
            int rh = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RHalf) ? 1 : 0;
            int rf = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RFloat) ? 1 : 0;
            int a32 = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32) ? 1 : 0;
            int ah = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf) ? 1 : 0;
            string blit = ShaderState("Canvas2D/ShadowBlit", "Hidden/Canvas2D/ShadowBlit");
            string over = ShaderState("Canvas2D/PremulOver", "Hidden/Canvas2D/PremulOver");
            string round = ShaderState("Canvas2D/RoundShadow", "Hidden/Canvas2D/RoundShadow");
            string fast = ShaderState("Canvas2D/FastShape", "Hidden/Canvas2D/FastShape");
            string fill = Shader.Find("Hidden/Canvas2D/Fill") != null ? "find" : "missing";
            return "[Perf] boot r8=" + r8 + " rhalf=" + rh + " rfloat=" + rf
                + " argb32=" + a32 + " argbhalf=" + ah
                + " shadowblit=" + blit + " premul=" + over + " roundshadow=" + round + " fastshape=" + fast + " fill=" + fill;
        }

        static string ShaderState(string resource, string name)
        {
            if (Resources.Load<Shader>(resource) != null) return "resources";
            if (Shader.Find(name) != null) return "find";
            return "missing";
        }
    }
}
