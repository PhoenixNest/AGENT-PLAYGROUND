using UnityEngine;

namespace OrbitalSalvage.Loc
{
    /// <summary>
    /// Reads <c>os.parity.lang</c> once at boot, and only when the key is set.
    /// <c>qps</c> or a registry code forces <see cref="Loc"/> for this run and is not written to the profile.
    /// </summary>
    public static class LocBootPref
    {
        public const string PrefKey = "os.parity.lang";

        public static void Apply()
        {
            if (!PlayerPrefs.HasKey(PrefKey)) return;
            string code = PlayerPrefs.GetString(PrefKey, "");
            if (string.IsNullOrEmpty(code)) return;
            Loc.SetDiagnostic(code);
        }
    }
}
