using System;
using OrbitalSalvage.Render;
using UnityEngine;

namespace OrbitalSalvage.Viewer
{
    /// <summary>
    /// Device A/B switches. PlayerPrefs <see cref="PrefKey"/> is a comma-separated list.
    /// An empty pref uses <see cref="MobileDefaults"/> on an iOS or Android player and
    /// <see cref="Defaults"/> everywhere else (empty, so editor stills stay pinned).
    /// A saved pref replaces that set entirely. <c>none</c> and <c>default</c> leave every option off.
    /// The product reads the pref once, on the first <see cref="Apply()"/> call.
    /// </summary>
    public static class GpuOpt
    {
        public const string PrefKey = "os.parity.opt";

        /// <summary>Desktop and editor default. Empty keeps the pinned stills and reference hashes.</summary>
        public static string Defaults = "";

        /// <summary>iOS and Android player default. A non-empty <see cref="PrefKey"/> replaces this entirely.</summary>
        public const string MobileDefaults = "rrshadow,rrplus,backdroplow,playhalf,fastshape,fastarc,playbatch";

        /// <summary>True on an iOS or Android player. Tests replace this; the editor stays false.</summary>
        public static Func<bool> IsMobilePlayer = DetectMobilePlayer;

        /// <summary>Platform default chosen by the last <see cref="Apply()"/>, before a pref override.</summary>
        public static string ChosenDefaults { get; private set; } = "";

        public static bool ShadowScope { get; private set; }
        public static bool ShadowLow { get; private set; }
        public static bool BackdropScope { get; private set; }
        public static bool BackdropLow { get; private set; }
        public static bool Dirty { get; private set; }
        public static bool GameOne { get; private set; }
        public static bool RoundShadow { get; private set; }
        public static bool PlayHalf { get; private set; }
        public static bool DirtyClip { get; private set; }
        public static bool BackdropHold { get; private set; }
        public static bool RoundPlus { get; private set; }
        public static bool FastShape { get; private set; }
        public static bool NoPrewarm { get; private set; }
        public static bool FastArc { get; private set; }
        public static bool PlayBatch { get; private set; }
        public static int AmbientHz { get; private set; }
        public static string Active { get; private set; } = "";

        static bool latched;
        static bool bootLogged;

        /// <summary>First call reads the pref (or <see cref="Defaults"/>). Later calls are free.</summary>
        public static void Apply()
        {
            if (latched) return;
            latched = true;
            string pref = PlayerPrefs.GetString(PrefKey, "");
            ChosenDefaults = PlatformDefaults(IsMobilePlayer());
            Apply(Resolve(pref, IsMobilePlayer()));
        }

        public static bool DetectMobilePlayer()
        {
            RuntimePlatform platform = Application.platform;
            return platform == RuntimePlatform.IPhonePlayer || platform == RuntimePlatform.Android;
        }

        public static string PlatformDefaults(bool mobile)
        {
            return mobile ? MobileDefaults : Defaults;
        }

        /// <summary>A saved pref wins outright, including <c>none</c> and <c>default</c>. Empty uses the platform default.</summary>
        public static string Resolve(string pref, bool mobile)
        {
            if (!string.IsNullOrEmpty(pref)) return pref;
            return PlatformDefaults(mobile);
        }

        /// <summary>Parse <paramref name="csv"/> and push the flags. Does not latch, so tests can override.</summary>
        public static void Apply(string csv)
        {
            ShadowScope = ShadowLow = BackdropScope = BackdropLow = Dirty = GameOne = RoundShadow = PlayHalf = false;
            DirtyClip = BackdropHold = RoundPlus = FastShape = NoPrewarm = FastArc = PlayBatch = false;
            AmbientHz = 0;
            if (!string.IsNullOrEmpty(csv))
            {
                int i = 0;
                while (i <= csv.Length)
                {
                    int comma = csv.IndexOf(',', i);
                    if (comma < 0) comma = csv.Length;
                    int start = i;
                    int end = comma;
                    while (start < end && char.IsWhiteSpace(csv[start])) start++;
                    while (end > start && char.IsWhiteSpace(csv[end - 1])) end--;
                    if (end > start) Token(csv.Substring(start, end - start));
                    if (comma >= csv.Length) break;
                    i = comma + 1;
                }
            }
            Active = Describe();
            RenderOpts.ShadowScope = ShadowScope;
            RenderOpts.ShadowLow = ShadowLow;
            RenderOpts.BackdropScope = BackdropScope;
            RenderOpts.BackdropLow = BackdropLow;
            RenderOpts.Dirty = Dirty;
            RenderOpts.GameOne = GameOne;
            RenderOpts.RoundShadow = RoundShadow;
            RenderOpts.PlayHalf = PlayHalf;
            RenderOpts.DirtyClip = DirtyClip;
            RenderOpts.BackdropHold = BackdropHold;
            RenderOpts.RoundPlus = RoundPlus;
            RenderOpts.FastShape = FastShape;
            RenderOpts.NoPrewarm = NoPrewarm;
            RenderOpts.FastArc = FastArc;
            RenderOpts.PlayBatch = PlayBatch;
            RenderOpts.AmbientHz = AmbientHz;
            if (!bootLogged && Application.isPlaying)
            {
                bootLogged = true;
                string defaults = string.IsNullOrEmpty(ChosenDefaults) ? "none" : ChosenDefaults;
                Debug.Log(RenderOpts.BootLine() + " active=" + Active + " defaults=" + defaults);
            }
        }

        /// <summary>Allow a later <see cref="Apply()"/> to read the pref again. Tests use this.</summary>
        public static void ResetLatch() { latched = false; }

        static void Token(string tok)
        {
            if (tok.Length == 0 || tok == "none" || tok == "default") return;
            if (tok == "shadowscope") ShadowScope = true;
            else if (tok == "shadowlow") ShadowLow = true;
            else if (tok == "backdropscope") BackdropScope = true;
            else if (tok == "backdroplow") BackdropLow = true;
            else if (tok == "dirty") Dirty = true;
            else if (tok == "gameone") GameOne = true;
            else if (tok == "rrshadow") RoundShadow = true;
            else if (tok == "playhalf") PlayHalf = true;
            else if (tok == "dirtyclip") DirtyClip = true;
            else if (tok == "bdhold") BackdropHold = true;
            else if (tok == "rrplus") RoundPlus = true;
            else if (tok == "fastshape") FastShape = true;
            else if (tok == "noprewarm") NoPrewarm = true;
            else if (tok == "fastarc") FastArc = true;
            else if (tok == "playbatch") PlayBatch = true;
            else if (tok.StartsWith("ambhz=", StringComparison.Ordinal))
            {
                int n;
                if (int.TryParse(tok.Substring(6), out n) && n > 0 && n <= 60) AmbientHz = n;
            }
        }

        static string Describe()
        {
            var s = "";
            void Add(bool on, string name)
            {
                if (!on) return;
                if (s.Length > 0) s += ",";
                s += name;
            }
            Add(ShadowScope, "shadowscope");
            Add(ShadowLow, "shadowlow");
            Add(BackdropScope, "backdropscope");
            Add(BackdropLow, "backdroplow");
            Add(Dirty, "dirty");
            Add(GameOne, "gameone");
            Add(RoundShadow, "rrshadow");
            Add(PlayHalf, "playhalf");
            Add(DirtyClip, "dirtyclip");
            Add(BackdropHold, "bdhold");
            Add(RoundPlus, "rrplus");
            Add(FastShape, "fastshape");
            Add(NoPrewarm, "noprewarm");
            Add(FastArc, "fastarc");
            Add(PlayBatch, "playbatch");
            if (AmbientHz > 0)
            {
                if (s.Length > 0) s += ",";
                s += "ambhz=" + AmbientHz.ToString();
            }
            return s.Length == 0 ? "none" : s;
        }
    }
}
