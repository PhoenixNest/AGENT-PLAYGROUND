namespace OrbitalSalvage.Loc
{
    /// <summary>
    /// The current language in the picker is a leading check mark on that button's endonym,
    /// not a filled style. One English capture stays valid for every selection: Unity replaces
    /// the run text of the previous button and the new one.
    /// U+2713 is not Extended_Pictographic, so the capture harness does not treat it as an emoji.
    /// Fredoka and Nunito do not contain it. Noto Sans Symbols 2 does, and the outline chain
    /// falls back to that face per glyph.
    /// </summary>
    public static class LanguageMarker
    {
        public const string Prefix = "\u2713 ";
        public const string ButtonPrefix = "btn-lang-";

        public static string Mark(string endonym)
        {
            return Prefix + (endonym ?? "");
        }

        public static string ButtonId(string code)
        {
            return ButtonPrefix + (code ?? "");
        }
    }
}
