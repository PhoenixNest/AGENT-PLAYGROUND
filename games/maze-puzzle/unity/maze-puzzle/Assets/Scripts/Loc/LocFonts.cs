using System;
using System.Collections.Generic;
using OrbitalSalvage.JsCompat;

namespace OrbitalSalvage.Loc
{
    public interface ILocFontManifestLoader
    {
        bool TryLoad(out string json);
    }

    /// <summary>
    /// Face names for one language, CSS family, and CSS weight. The manifest is
    /// <c>Resources/Localization/fonts.json</c>. A missing manifest falls back to the Latin faces
    /// already shipped with the outline renderer. This type does not open font files.
    /// </summary>
    public static class LocFonts
    {
        public const string Fredoka = "Fredoka";
        public const string Nunito = "Nunito";

        static readonly int[] FredokaWeights = { 400, 600, 700 };
        static readonly string[] FredokaFiles = { "Fredoka-400", "Fredoka-600", "Fredoka-700" };
        static readonly int[] NunitoWeights = { 700, 800, 900 };
        static readonly string[] NunitoFiles = { "Nunito-700", "Nunito-800", "Nunito-900" };
        static readonly string[] SymbolFiles = { "NotoSansSymbols2-Regular", "NotoSansSymbols-Regular" };

        static ILocFontManifestLoader loader;
        static bool parsed;
        static Dictionary<string, FaceList> roles;
        static Dictionary<string, GroupDef> groups;
        static string[] symbols = SymbolFiles;

        sealed class FaceList
        {
            public int[] Weights = Array.Empty<int>();
            public string[] Files = Array.Empty<string>();

            public string FileFor(int cssWeight)
            {
                int face = MatchWeight(cssWeight, Weights);
                if (face < 0) return null;
                for (int i = 0; i < Weights.Length; i++)
                    if (Weights[i] == face) return Files[i];
                return Files.Length > 0 ? Files[0] : null;
            }
        }

        sealed class GroupDef
        {
            public string DisplayRole;
            public string BodyRole;
            public FaceList Display;
            public FaceList Body;
            public string[] Fallback = Array.Empty<string>();
        }

        public static bool HasLoader
        {
            get { return loader != null; }
        }

        public static void SetLoader(ILocFontManifestLoader manifestLoader)
        {
            loader = manifestLoader;
            parsed = false;
            roles = null;
            groups = null;
        }

        public static void Reset()
        {
            SetLoader(null);
        }

        /// <summary>
        /// Ordered face names. <paramref name="languageCode"/> <c>en</c> is Fredoka or Nunito, then the
        /// two symbol fonts. Other Latin-script codes insert Nunito after Fredoka. Script groups put
        /// the manifest face first. A single listed weight is used for every CSS weight (no synthetic bold).
        /// </summary>
        public static string[] GetChain(string languageCode, string family, int cssWeight)
        {
            EnsureParsed();
            string code = string.IsNullOrEmpty(languageCode) ? LanguageRegistry.DefaultCode : languageCode;
            int weight = cssWeight < 1 ? 1 : (cssWeight > 1000 ? 1000 : cssWeight);
            if (!IsFredoka(family) && !IsNunito(family))
            {
                var forced = new List<string>(3);
                Add(forced, "Nunito-700");
                AddSymbols(forced);
                return forced.ToArray();
            }

            string groupId = GroupId(code);
            GroupDef group = null;
            bool haveGroup = groups != null && groups.TryGetValue(groupId, out group);

            var names = new List<string>(6);
            bool display = IsFredoka(family);
            string primary = haveGroup ? Face(group, display, weight) : RoleFile(display ? "fredoka" : "nunito", weight);
            Add(names, primary);
            if (code != LanguageRegistry.DefaultCode && display)
                Add(names, RoleFile("nunito", weight));
            if (haveGroup) AppendFallback(names, group.Fallback, weight);
            else AddSymbols(names);
            return names.ToArray();
        }

        /// <summary>True when the first requested face did not load and the language is not Latin-script.</summary>
        public static bool NeedsLatinFallback(string languageCode, string[] wanted, string[] loaded)
        {
            if (IsLatinScript(languageCode)) return false;
            if (wanted == null || wanted.Length == 0) return true;
            if (loaded == null || loaded.Length == 0) return true;
            return loaded[0] != wanted[0];
        }

        public static bool IsLatinScript(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode) || languageCode == LanguageRegistry.DefaultCode) return true;
            if (languageCode == PseudoLocale.Code) return true;
            LanguageInfo info;
            if (!LanguageRegistry.TryGet(languageCode, out info)) return true;
            return info.FontGroup == FontGroup.Latin || info.FontGroup == FontGroup.LatinVi;
        }

        public static void FillPickerOverrides(IDictionary<string, string> dest)
        {
            if (dest == null) return;
            IReadOnlyList<LanguageInfo> all = LanguageRegistry.All;
            for (int i = 0; i < all.Count; i++)
                dest[LanguageMarker.ButtonId(all[i].Code)] = all[i].Code;
        }

        public static string CodeForEndonym(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            string trimmed = text.Trim();
            if (trimmed.StartsWith(LanguageMarker.Prefix, StringComparison.Ordinal))
                trimmed = trimmed.Substring(LanguageMarker.Prefix.Length).Trim();
            IReadOnlyList<LanguageInfo> all = LanguageRegistry.All;
            for (int i = 0; i < all.Count; i++)
                if (all[i].Endonym == trimmed) return all[i].Code;
            return null;
        }

        static void EnsureParsed()
        {
            if (parsed) return;
            parsed = true;
            roles = DefaultRoles();
            groups = null;
            symbols = SymbolFiles;
            if (loader == null) return;
            string json;
            if (!loader.TryLoad(out json) || string.IsNullOrEmpty(json)) return;
            JsonValue root;
            if (!JsonParser.TryParse(json, out root) || root == null || root.Kind != JsonKind.Object) return;
            Dictionary<string, FaceList> nextRoles = DefaultRoles();
            if (!ReadRoles(root.Get("roles"), nextRoles)) return;
            string[] nextSymbols = ReadStringArray(root.Get("symbols"));
            if (nextSymbols == null || nextSymbols.Length == 0) nextSymbols = SymbolFiles;
            Dictionary<string, GroupDef> nextGroups;
            if (!ReadGroups(root.Get("groups"), out nextGroups)) return;
            roles = nextRoles;
            groups = nextGroups;
            symbols = nextSymbols;
        }

        static Dictionary<string, FaceList> DefaultRoles()
        {
            var map = new Dictionary<string, FaceList>(StringComparer.Ordinal);
            map["fredoka"] = new FaceList { Weights = FredokaWeights, Files = FredokaFiles };
            map["nunito"] = new FaceList { Weights = NunitoWeights, Files = NunitoFiles };
            return map;
        }

        static bool ReadRoles(JsonValue node, Dictionary<string, FaceList> dest)
        {
            if (node == null) return true;
            if (node.Kind != JsonKind.Object) return false;
            for (int i = 0; i < node.Members.Count; i++)
            {
                FaceList list;
                if (!ReadFaceList(node.Members[i].Value, out list)) return false;
                dest[node.Members[i].Key] = list;
            }
            return true;
        }

        static bool ReadGroups(JsonValue node, out Dictionary<string, GroupDef> dest)
        {
            dest = new Dictionary<string, GroupDef>(StringComparer.Ordinal);
            if (node == null) return true;
            if (node.Kind != JsonKind.Object) return false;
            for (int i = 0; i < node.Members.Count; i++)
            {
                GroupDef group;
                if (!ReadGroup(node.Members[i].Value, out group)) return false;
                dest[node.Members[i].Key] = group;
            }
            return true;
        }

        static bool ReadGroup(JsonValue node, out GroupDef group)
        {
            group = null;
            if (node == null || node.Kind != JsonKind.Object) return false;
            var parsedGroup = new GroupDef();
            if (!ReadSlot(node.Get("display"), out parsedGroup.DisplayRole, out parsedGroup.Display)) return false;
            if (!ReadSlot(node.Get("body"), out parsedGroup.BodyRole, out parsedGroup.Body)) return false;
            string[] fallback = ReadStringArray(node.Get("fallback"));
            parsedGroup.Fallback = fallback ?? Array.Empty<string>();
            group = parsedGroup;
            return true;
        }

        static bool ReadSlot(JsonValue node, out string role, out FaceList list)
        {
            role = null;
            list = null;
            if (node == null) return true;
            if (node.Kind == JsonKind.String)
            {
                role = node.String;
                return true;
            }
            return ReadFaceList(node, out list);
        }

        static bool ReadFaceList(JsonValue node, out FaceList list)
        {
            list = null;
            if (node == null || node.Kind != JsonKind.Array || node.Items.Count == 0) return false;
            var weights = new int[node.Items.Count];
            var files = new string[node.Items.Count];
            for (int i = 0; i < node.Items.Count; i++)
            {
                JsonValue item = node.Items[i];
                if (item == null || item.Kind != JsonKind.Object) return false;
                JsonValue weight = item.Get("weight");
                JsonValue file = item.Get("file");
                if (weight == null || weight.Kind != JsonKind.Number || file == null || file.Kind != JsonKind.String) return false;
                if (string.IsNullOrEmpty(file.String)) return false;
                weights[i] = (int)weight.Number;
                files[i] = file.String;
            }
            list = new FaceList { Weights = weights, Files = files };
            return true;
        }

        static string[] ReadStringArray(JsonValue node)
        {
            if (node == null || node.Kind != JsonKind.Array) return null;
            var values = new string[node.Items.Count];
            for (int i = 0; i < node.Items.Count; i++)
            {
                if (node.Items[i] == null || node.Items[i].Kind != JsonKind.String) return null;
                values[i] = node.Items[i].String;
            }
            return values;
        }

        static string GroupId(string code)
        {
            if (code == PseudoLocale.Code) return "latin";
            LanguageInfo info;
            if (LanguageRegistry.TryGet(code, out info)) return info.FontGroupId();
            return "latin";
        }

        static string Face(GroupDef group, bool display, int cssWeight)
        {
            string role = display ? group.DisplayRole : group.BodyRole;
            if (!string.IsNullOrEmpty(role)) return RoleFile(role, cssWeight);
            FaceList list = display ? group.Display : group.Body;
            return list == null ? null : list.FileFor(cssWeight);
        }

        static string RoleFile(string role, int cssWeight)
        {
            FaceList list;
            if (roles != null && roles.TryGetValue(role, out list)) return list.FileFor(cssWeight);
            if (role == "fredoka") return new FaceList { Weights = FredokaWeights, Files = FredokaFiles }.FileFor(cssWeight);
            if (role == "nunito") return new FaceList { Weights = NunitoWeights, Files = NunitoFiles }.FileFor(cssWeight);
            return null;
        }

        static void AppendFallback(List<string> names, string[] fallback, int cssWeight)
        {
            if (fallback == null) return;
            for (int i = 0; i < fallback.Length; i++)
            {
                string entry = fallback[i];
                if (entry == "@nunito") Add(names, RoleFile("nunito", cssWeight));
                else if (entry == "@symbols") AddSymbols(names);
                else Add(names, entry);
            }
        }

        static void AddSymbols(List<string> names)
        {
            string[] list = symbols ?? SymbolFiles;
            for (int i = 0; i < list.Length; i++) Add(names, list[i]);
        }

        static void Add(List<string> names, string file)
        {
            if (string.IsNullOrEmpty(file)) return;
            for (int i = 0; i < names.Count; i++)
                if (names[i] == file) return;
            names.Add(file);
        }

        static bool IsFredoka(string family)
        {
            return Same(family, Fredoka);
        }

        static bool IsNunito(string family)
        {
            return Same(family, Nunito);
        }

        static bool Same(string family, string name)
        {
            if (string.IsNullOrEmpty(family)) return false;
            string trimmed = family.Trim().Trim('"').Trim('\'');
            return string.Equals(trimmed, name, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>CSS Fonts 4 weight matching. Same steps as <c>ParityFonts.MatchWeight</c>.</summary>
        public static int MatchWeight(int desired, int[] available)
        {
            if (available == null || available.Length == 0) return -1;
            var asc = (int[])available.Clone();
            Array.Sort(asc);
            if (desired >= 400 && desired <= 500)
            {
                for (int i = 0; i < asc.Length; i++) if (asc[i] >= desired && asc[i] <= 500) return asc[i];
                for (int i = asc.Length - 1; i >= 0; i--) if (asc[i] < desired) return asc[i];
                for (int i = 0; i < asc.Length; i++) if (asc[i] > 500) return asc[i];
            }
            else if (desired < 400)
            {
                for (int i = asc.Length - 1; i >= 0; i--) if (asc[i] <= desired) return asc[i];
                for (int i = 0; i < asc.Length; i++) if (asc[i] > desired) return asc[i];
            }
            else
            {
                for (int i = 0; i < asc.Length; i++) if (asc[i] >= desired) return asc[i];
                for (int i = asc.Length - 1; i >= 0; i--) if (asc[i] < desired) return asc[i];
            }
            return asc[0];
        }
    }
}
