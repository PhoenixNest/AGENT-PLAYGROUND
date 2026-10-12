namespace OrbitalSalvage.Loc
{
    /// <summary>
    /// One C# literal. <see cref="English"/> is the source text (<c>{0}</c> placeholders).
    /// <see cref="Context"/> is the translator note. <see cref="MaxChars"/> is a chip or button
    /// budget in Unicode code points, or 0 when there is no tight budget.
    /// <see cref="Dynamic"/> keys are built at runtime and need not appear as a string literal.
    /// </summary>
    public sealed class LocStringDef
    {
        public readonly string Key;
        public readonly string English;
        public readonly string Context;
        public readonly int MaxChars;
        public readonly bool Dynamic;

        public LocStringDef(string key, string english, string context, int maxChars, bool dynamic)
        {
            Key = key;
            English = english ?? "";
            Context = context ?? "";
            MaxChars = maxChars;
            Dynamic = dynamic;
        }
    }
}
