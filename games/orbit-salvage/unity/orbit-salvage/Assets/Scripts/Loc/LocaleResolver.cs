using System;
using System.Collections.Generic;

namespace OrbitalSalvage.Loc
{
    /// <summary>
    /// What the host can read about the device. The default Unity implementation lives in
    /// <c>OrbitalSalvage.Loc.Unity</c> and is not used by EditMode tests; tests inject a provider.
    /// Native NSLocale / Android Locale lookup is a later task.
    /// </summary>
    public interface IDeviceLocaleProvider
    {
        /// <summary>Unity <c>SystemLanguage</c> name, for example <c>German</c>. Empty when unknown.</summary>
        string SystemLanguageName { get; }

        /// <summary>Culture name such as <c>de-DE</c> or <c>fil-PH</c>. Empty when unknown.</summary>
        string CultureName { get; }
    }

    public sealed class FixedDeviceLocale : IDeviceLocaleProvider
    {
        public string SystemLanguageName { get; }
        public string CultureName { get; }

        public FixedDeviceLocale(string systemLanguageName, string cultureName)
        {
            SystemLanguageName = systemLanguageName ?? "";
            CultureName = cultureName ?? "";
        }
    }

    /// <summary>
    /// Maps a BCP-47 tag or a Unity <c>SystemLanguage</c> name onto a registry code.
    /// Returns null when nothing in the registry matches. Case-insensitive. Underscores are hyphens.
    /// </summary>
    public static class LocaleResolver
    {
        static readonly Dictionary<string, string> SystemLanguages = BuildSystemLanguages();

        public static string ResolveTag(string tagOrLocale)
        {
            if (string.IsNullOrWhiteSpace(tagOrLocale)) return null;
            string[] parts = Split(tagOrLocale);
            if (parts.Length == 0) return null;
            string primary = parts[0];

            if (primary == "uz") return ResolveUz(parts);
            if (primary == "zh") return ResolveZh(parts);
            if (primary == "pt") return "pt-PT";
            if (primary == "fil" || primary == "tl") return "fil";
            if (primary == "id" || primary == "in") return "id";
            if (primary == "nb" || primary == "nn" || primary == "no" || primary == "iw") return null;
            if (LanguageRegistry.IsSupported(primary)) return primary;
            return null;
        }

        /// <summary>
        /// <paramref name="systemLanguage"/> is <c>SystemLanguage.ToString()</c> (<c>German</c>,
        /// <c>ChineseSimplified</c>). When <paramref name="cultureName"/> resolves to a supported
        /// code it wins, so Filipino, Khmer, Hindi, and Uzbek can be seen even though the enum
        /// lacks them. An unrecognised culture falls through to the enum map. Both empty yields null.
        /// </summary>
        public static string ResolveSystemLanguage(string systemLanguage, string cultureName)
        {
            if (!string.IsNullOrWhiteSpace(cultureName))
            {
                string fromCulture = ResolveTag(cultureName);
                if (fromCulture != null) return fromCulture;
            }
            if (string.IsNullOrWhiteSpace(systemLanguage)) return null;
            string mapped;
            if (SystemLanguages.TryGetValue(systemLanguage.Trim(), out mapped)) return mapped;
            return null;
        }

        static string ResolveUz(string[] parts)
        {
            for (int i = 1; i < parts.Length; i++)
            {
                if (parts[i] == "cyrl") return null;
            }
            return "uz";
        }

        static string ResolveZh(string[] parts)
        {
            bool hans = false;
            bool hant = false;
            string region = null;
            for (int i = 1; i < parts.Length; i++)
            {
                string part = parts[i];
                if (part == "hans") hans = true;
                else if (part == "hant") hant = true;
                else if (part.Length == 2 && IsLetters(part)) region = part;
            }
            if (hant && !hans) return "zh-Hant";
            if (hans && !hant) return "zh-Hans";
            if (region == "tw" || region == "hk" || region == "mo") return "zh-Hant";
            return "zh-Hans";
        }

        static bool IsLetters(string part)
        {
            for (int i = 0; i < part.Length; i++)
            {
                char c = part[i];
                if (c < 'a' || c > 'z') return false;
            }
            return true;
        }

        static string[] Split(string tag)
        {
            string trimmed = tag.Trim().Replace('_', '-');
            string[] raw = trimmed.Split('-');
            var parts = new List<string>(raw.Length);
            for (int i = 0; i < raw.Length; i++)
            {
                if (raw[i].Length == 0) continue;
                parts.Add(raw[i].ToLowerInvariant());
            }
            return parts.ToArray();
        }

        static Dictionary<string, string> BuildSystemLanguages()
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            map["English"] = "en";
            map["German"] = "de";
            map["French"] = "fr";
            map["Spanish"] = "es";
            map["Italian"] = "it";
            map["Portuguese"] = "pt-PT";
            map["Vietnamese"] = "vi";
            map["Indonesian"] = "id";
            map["Japanese"] = "ja";
            map["Korean"] = "ko";
            map["Chinese"] = "zh-Hans";
            map["ChineseSimplified"] = "zh-Hans";
            map["ChineseTraditional"] = "zh-Hant";
            map["Arabic"] = "ar";
            map["Hindi"] = "hi";
            return map;
        }
    }
}
