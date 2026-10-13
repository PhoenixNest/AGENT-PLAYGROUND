namespace OrbitalSalvage.Loc
{
    /// <summary>
    /// Tracking and line pitch for a translated run. English paint does not call this.
    /// </summary>
    public static class LocTextStyle
    {
        public static bool IsCjk(string languageCode)
        {
            switch (languageCode)
            {
                case "zh-Hans":
                case "zh-Hant":
                case "ja":
                case "ko":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Captured CSS letter-spacing is Latin tracking, about 0.2–1px on a 0.5em advance.
        /// A CJK advance is already about 1em, so that tracking is dropped to 0.
        /// Any other language, including English and a null code, keeps the captured value.
        /// </summary>
        public static double LetterSpacing(string languageCode, double captured)
        {
            if (IsCjk(languageCode)) return 0;
            return captured;
        }

        /// <summary>
        /// For a CJK run whose captured English text already occupied more than one line,
        /// the line pitch is that box divided by the English line count. That is the CSS
        /// line box the spec measured. A single-line run returns 0 so the caller keeps
        /// the captured run height. Latin languages always return 0.
        /// </summary>
        public static double LinePitchOverride(string languageCode, string englishRaw, double runH)
        {
            if (!IsCjk(languageCode) || runH <= 0 || string.IsNullOrEmpty(englishRaw)) return 0;
            int lines = 1;
            for (int i = 0; i < englishRaw.Length; i++)
                if (englishRaw[i] == '\n') lines++;
            if (lines < 2) return 0;
            return runH / lines;
        }
    }
}
