using System.Globalization;
using UnityEngine;

namespace OrbitalSalvage.Loc
{
    /// <summary>
    /// Unity <c>SystemLanguage</c> overload. The mapping table itself is
    /// <see cref="LocaleResolver.ResolveSystemLanguage(string, string)"/> so tests can pass enum
    /// names without this assembly.
    /// </summary>
    public static class LocaleResolverUnity
    {
        public static string ResolveSystemLanguage(SystemLanguage language, string cultureName)
        {
            return LocaleResolver.ResolveSystemLanguage(language.ToString(), cultureName);
        }
    }

    /// <summary>
    /// Default device lookup: <c>Application.systemLanguage</c> and
    /// <c>CultureInfo.CurrentUICulture.Name</c>. Native preferred-language APIs are a later task.
    /// </summary>
    public sealed class UnityDeviceLocaleProvider : IDeviceLocaleProvider
    {
        public string SystemLanguageName
        {
            get { return Application.systemLanguage.ToString(); }
        }

        public string CultureName
        {
            get
            {
                CultureInfo culture = CultureInfo.CurrentUICulture;
                return culture == null ? "" : culture.Name;
            }
        }
    }

    public sealed class ResourcesLocTableLoader : ILocTableLoader
    {
        public bool TryLoad(string code, out string json)
        {
            json = null;
            if (string.IsNullOrEmpty(code)) return false;
            TextAsset asset = Resources.Load<TextAsset>("Localization/" + code);
            if (asset == null || string.IsNullOrEmpty(asset.text)) return false;
            json = asset.text;
            return true;
        }
    }
}
