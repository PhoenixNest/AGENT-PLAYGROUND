using System;

namespace OrbitalSalvage.Loc
{
    /// <summary>
    /// <c>text-transform: uppercase</c> for a non-English run. English keeps <c>ToUpperInvariant</c>
    /// on its own cached path and does not call this.
    /// </summary>
    public static class LocCase
    {
        public static bool IsCaseless(string languageCode)
        {
            switch (languageCode)
            {
                case "zh-Hans":
                case "zh-Hant":
                case "ja":
                case "ko":
                case "km":
                case "hi":
                case "ar":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Latin-script languages uppercase with invariant culture. German ß becomes SS (CSS).
        /// Caseless scripts return the same string so a Latin token such as "kg" is left alone.
        /// Combining marks stay on their base. Uzbek ʻ (U+02BB) and ʼ (U+02BC) have no case.
        /// </summary>
        public static string Upper(string text, string languageCode)
        {
            if (string.IsNullOrEmpty(text)) return text ?? "";
            if (IsCaseless(languageCode)) return text;
            if (text.IndexOf('ß') >= 0) text = text.Replace("ß", "SS");
            return text.ToUpperInvariant();
        }
    }
}
