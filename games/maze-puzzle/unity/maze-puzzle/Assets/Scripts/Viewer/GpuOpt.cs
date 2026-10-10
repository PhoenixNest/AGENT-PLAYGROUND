using System;
using OrbitalSalvage.Render;
using UnityEngine;

namespace OrbitalSalvage.Viewer
{
    /// <summary>
    /// Device A/B switches. PlayerPrefs <see cref="PrefKey"/> is a comma-separated list.
    /// When the pref is missing or empty, <see cref="Defaults"/> is used (empty = every option off).
    /// Flip <see cref="Defaults"/> to turn a confirmed set on without another code change.
    /// The product reads the pref once, on the first <see cref="Apply()"/> call.
    /// </summary>
    public static class GpuOpt
    {
        public const string PrefKey = "os.parity.opt";

        /// <summary>One-line default. Empty keeps the product path. Example: "shadowscope,shadowlow,backdroplow,dirty".</summary>
        public static string Defaults = "";

        public static bool ShadowScope { get; private set; }
        public static bool ShadowLow { get; private set; }
        public static bool BackdropScope { get; private set; }
        public static bool BackdropLow { get; private set; }
        public static bool Dirty { get; private set; }
        public static bool GameOne { get; private set; }
        public static bool RoundShadow { get; private set; }
        public static bool PlayHalf { get; private set; }
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
            Apply(string.IsNullOrEmpty(pref) ? Defaults : pref);
        }

        /// <summary>Parse <paramref name="csv"/> and push the flags. Does not latch, so tests can override.</summary>
        public static void Apply(string csv)
        {
            ShadowScope = ShadowLow = BackdropScope = BackdropLow = Dirty = GameOne = RoundShadow = PlayHalf = false;
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
            RenderOpts.AmbientHz = AmbientHz;
            if (!bootLogged && Application.isPlaying)
            {
                bootLogged = true;
                Debug.Log(RenderOpts.BootLine() + " active=" + Active);
            }
        }

        /// <summary>Allow a later <see cref="Apply()"/> to read the pref again. Tests use this.</summary>
        public static void ResetLatch() { latched = false; }

        static void Token(string tok)
        {
            if (tok.Length == 0 || tok == "none") return;
            if (tok == "shadowscope") ShadowScope = true;
            else if (tok == "shadowlow") ShadowLow = true;
            else if (tok == "backdropscope") BackdropScope = true;
            else if (tok == "backdroplow") BackdropLow = true;
            else if (tok == "dirty") Dirty = true;
            else if (tok == "gameone") GameOne = true;
            else if (tok == "rrshadow") RoundShadow = true;
            else if (tok == "playhalf") PlayHalf = true;
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
            if (AmbientHz > 0)
            {
                if (s.Length > 0) s += ",";
                s += "ambhz=" + AmbientHz.ToString();
            }
            return s.Length == 0 ? "none" : s;
        }
    }
}
