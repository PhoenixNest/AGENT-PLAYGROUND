using System;
using System.Collections.Generic;

namespace OrbitalSalvage.Loc
{
    public interface ILocTableLoader
    {
        bool TryLoad(string code, out string json);
    }

    /// <summary>
    /// String tables. <see cref="CurrentLanguage"/> is <c>en</c> unless that other code's table
    /// parsed successfully. English spec lookup always returns false so painted English runs,
    /// including their layout whitespace, stay untouched. A missing non-English key falls back
    /// to English (spec: return false; str: the English value, or the key) and is recorded in
    /// <see cref="MissingKeys"/> up to <see cref="MissingKeyLimit"/>.
    /// </summary>
    public static class Loc
    {
        public const int MissingKeyLimit = 256;

        static readonly Dictionary<string, LocTable> Tables = new Dictionary<string, LocTable>(StringComparer.Ordinal);
        static readonly HashSet<string> MissingSet = new HashSet<string>(StringComparer.Ordinal);
        static readonly List<string> MissingList = new List<string>();

        static ILocTableLoader _loader;
        static string _current = LanguageRegistry.DefaultCode;
        static bool _english = true;
        static int _version;

        public static string CurrentLanguage
        {
            get { return _current; }
        }

        /// <summary>
        /// True when the active language is English. Paint reads this once per run.
        /// It is a field, not a string compare.
        /// </summary>
        public static bool EnglishActive
        {
            get { return _english; }
        }

        public static int Version
        {
            get { return _version; }
        }

        public static IReadOnlyList<string> MissingKeys
        {
            get { return MissingList; }
        }

        public static event Action LanguageChanged;

        /// <summary>
        /// Replaces the table loader and drops cached tables. Does not change the active language
        /// and does not raise <see cref="LanguageChanged"/>.
        /// </summary>
        public static void SetLoader(ILocTableLoader loader)
        {
            _loader = loader;
            Tables.Clear();
        }

        /// <summary>
        /// Test and harness reset. Does not raise <see cref="LanguageChanged"/>.
        /// </summary>
        public static void Reset(ILocTableLoader loader)
        {
            _loader = loader;
            Tables.Clear();
            MissingSet.Clear();
            MissingList.Clear();
            _current = LanguageRegistry.DefaultCode;
            _english = true;
            _version = 0;
        }

        /// <summary>
        /// Activates a registry code. Returns false when <paramref name="code"/> is not in the
        /// registry. A supported code whose table did not load leaves the active language at
        /// <c>en</c> (or moves it back there). <see cref="Version"/> increments only when the
        /// active code changes.
        /// </summary>
        public static bool SetLanguage(string code)
        {
            if (!LanguageRegistry.IsSupported(code)) return false;
            string active = code;
            if (code != LanguageRegistry.DefaultCode)
            {
                LocTable table;
                if (!TryGetTable(code, out table)) active = LanguageRegistry.DefaultCode;
            }
            if (active == _current) return true;
            _current = active;
            _english = active == LanguageRegistry.DefaultCode;
            MissingSet.Clear();
            MissingList.Clear();
            _version++;
            RaiseChanged();
            return true;
        }

        /// <summary>
        /// Forces a language without writing the profile. <paramref name="code"/> may be a registry
        /// code or the hidden pseudo-locale <see cref="PseudoLocale.Code"/>. <c>qps</c> builds its
        /// table from the loaded English table and is not in <see cref="LanguageRegistry.All"/>.
        /// </summary>
        public static bool SetDiagnostic(string code)
        {
            if (code == PseudoLocale.Code) return ActivatePseudo();
            return SetLanguage(code);
        }

        static bool ActivatePseudo()
        {
            LocTable english;
            if (!TryGetTable(LanguageRegistry.DefaultCode, out english)) return false;
            Tables[PseudoLocale.Code] = PseudoLocale.BuildTable(english);
            if (_current == PseudoLocale.Code) return true;
            _current = PseudoLocale.Code;
            _english = false;
            MissingSet.Clear();
            MissingList.Clear();
            _version++;
            RaiseChanged();
            return true;
        }

        static void RaiseChanged()
        {
            Action handler = LanguageChanged;
            if (handler != null) handler();
        }

        public static string Tr(string key)
        {
            return TranslateStr(key);
        }

        public static string Tr(string key, params object[] args)
        {
            return LocText.Format(TranslateStr(key), args);
        }

        /// <summary>
        /// Looks up normalised spec text. English always returns false. A hit returns the
        /// translation trimmed, without the source run's leading or trailing whitespace.
        /// A miss returns false and records <c>spec:</c> plus the normalised key.
        /// </summary>
        public static bool TryTranslateSpecText(string englishRunText, out string translated)
        {
            translated = null;
            if (_current == LanguageRegistry.DefaultCode) return false;
            string key = LocText.Normalize(englishRunText);
            if (key.Length == 0) return false;
            LocTable table;
            if (!TryGetTable(_current, out table)) return false;
            string value;
            if (!table.Spec.TryGetValue(key, out value))
            {
                RecordMiss("spec:" + key);
                return false;
            }
            translated = value == null ? "" : value.Trim();
            return true;
        }

        /// <summary>
        /// Chooses the active language from a stored profile code and a device provider.
        /// <paramref name="storedLanguage"/> is the raw <c>settings.language</c> value.
        /// This method does not write the profile. <see cref="LocInitResult.PersistLanguage"/>
        /// is the canonical code to store, or null when the host must leave the file alone.
        /// English detection is never persisted. A non-English detection is persisted only when
        /// that table loaded. A stored supported code whose table is missing stays stored and
        /// the active language is English, so a later drop-in of the table can pick it up.
        /// The stored value is then authoritative on the next boot.
        /// </summary>
        public static LocInitResult InitializeFromProfile(string storedLanguage, IDeviceLocaleProvider device)
        {
            string stored = storedLanguage ?? "";
            string canonical = Canonical(stored);
            if (canonical != null && TableAvailable(canonical))
            {
                SetLanguage(canonical);
                return new LocInitResult
                {
                    ActiveLanguage = canonical,
                    PersistLanguage = stored == canonical ? null : canonical,
                };
            }
            if (canonical != null)
            {
                SetLanguage(LanguageRegistry.DefaultCode);
                return new LocInitResult
                {
                    ActiveLanguage = LanguageRegistry.DefaultCode,
                    PersistLanguage = null,
                };
            }

            string detected = null;
            if (device != null)
            {
                detected = LocaleResolver.ResolveSystemLanguage(device.SystemLanguageName, device.CultureName);
            }
            if (detected != null && detected != LanguageRegistry.DefaultCode && TableAvailable(detected))
            {
                SetLanguage(detected);
                return new LocInitResult
                {
                    ActiveLanguage = detected,
                    PersistLanguage = detected,
                };
            }
            SetLanguage(LanguageRegistry.DefaultCode);
            return new LocInitResult
            {
                ActiveLanguage = LanguageRegistry.DefaultCode,
                PersistLanguage = null,
            };
        }

        static string Canonical(string stored)
        {
            if (stored.Length == 0) return null;
            if (LanguageRegistry.IsSupported(stored)) return stored;
            return LocaleResolver.ResolveTag(stored);
        }

        static bool TableAvailable(string code)
        {
            if (code == LanguageRegistry.DefaultCode) return true;
            LocTable table;
            return TryGetTable(code, out table);
        }

        static string TranslateStr(string key)
        {
            if (key == null) key = "";
            if (_current != LanguageRegistry.DefaultCode)
            {
                LocTable current;
                if (TryGetTable(_current, out current))
                {
                    string value;
                    if (current.Str.TryGetValue(key, out value)) return value ?? "";
                }
                RecordMiss(key);
            }
            LocTable english;
            if (TryGetTable(LanguageRegistry.DefaultCode, out english))
            {
                string value;
                if (english.Str.TryGetValue(key, out value)) return value ?? "";
            }
            return key;
        }

        static void RecordMiss(string key)
        {
            if (MissingSet.Contains(key)) return;
            if (MissingList.Count >= MissingKeyLimit) return;
            MissingSet.Add(key);
            MissingList.Add(key);
        }

        static bool TryGetTable(string code, out LocTable table)
        {
            if (Tables.TryGetValue(code, out table)) return table != null;
            table = null;
            if (_loader == null)
            {
                Tables[code] = null;
                return false;
            }
            string json;
            if (!_loader.TryLoad(code, out json) || string.IsNullOrEmpty(json) || !LocTable.TryParse(json, out table))
            {
                Tables[code] = null;
                table = null;
                return false;
            }
            Tables[code] = table;
            return true;
        }
    }
}
