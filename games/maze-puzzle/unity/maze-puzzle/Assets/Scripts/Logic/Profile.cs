using System.Collections.Generic;
using OrbitalSalvage.JsCompat;

namespace OrbitalSalvage.Logic
{
    public sealed class ProfileSettings
    {
        public bool SoundEnabled = true;
        public double SfxVolume = 80;
        public double AmbientVolume = 70;
        public bool HapticsEnabled = true;
        /// <summary>
        /// Registry language code, or "" when the player has not chosen one.
        /// Empty is omitted from JSON so an unset profile stays byte-identical.
        /// Unsupported codes are kept as stored; callers resolve them through the registry.
        /// </summary>
        public string Language = "";
    }

    public sealed class Profile
    {
        public double Version = 2;
        public double Credits = 250;
        public double CareerDebris;
        public double CareerContracts;
        public double InfiniteRound;
        public double InfiniteBest;
        public OrderedMap<string> Equipped = new OrderedMap<string>();
        public OrderedMap<List<string>> Unlocked = new OrderedMap<List<string>>();
        public ProfileSettings Settings = new ProfileSettings();
        public List<string> Achievements = new List<string>();
        public List<string> CompletedSectors = new List<string>();
        public bool DailyShiftCompletedToday;
        public string LastDailyShiftDate = "";
        public List<string> CompletedDailyTiers = new List<string>();
        public string DailyMasterClaimedDate = "";
        public string LastDailyGiftDate = "";
        public List<KeyValuePair<string, JsonValue>> ExtraKeys = new List<KeyValuePair<string, JsonValue>>();

        public static Profile CreateDefault()
        {
            var p = new Profile();
            p.Equipped.Set("ring", "ring_donut");
            p.Equipped.Set("singularity", "singularity_nibbles");
            p.Equipped.Set("skybox", "skybox_cotton_candy");
            p.Unlocked.Set("rings", new List<string> { "ring_donut" });
            p.Unlocked.Set("singularities", new List<string> { "singularity_nibbles" });
            p.Unlocked.Set("skyboxes", new List<string> { "skybox_cotton_candy" });
            return p;
        }

        private static JsonValue StringList(List<string> list)
        {
            JsonValue arr = JsonValue.NewArray();
            foreach (string s in list) arr.Add(JsonValue.FromString(s));
            return arr;
        }

        public JsonValue ToJson()
        {
            JsonValue o = JsonValue.NewObject();
            o.Set("version", Version);
            o.Set("credits", Credits);
            o.Set("careerDebris", CareerDebris);
            o.Set("careerContracts", CareerContracts);
            if (InfiniteRound != 0) o.Set("infiniteRound", InfiniteRound);
            if (InfiniteBest != 0) o.Set("infiniteBest", InfiniteBest);

            JsonValue eq = JsonValue.NewObject();
            foreach (string k in Equipped.Keys) eq.Set(k, Equipped.Get(k));
            o.Set("equipped", eq);

            JsonValue un = JsonValue.NewObject();
            foreach (string k in Unlocked.Keys) un.Set(k, StringList(Unlocked.Get(k)));
            o.Set("unlocked", un);

            JsonValue st = JsonValue.NewObject();
            st.Set("soundEnabled", Settings.SoundEnabled);
            st.Set("sfxVolume", Settings.SfxVolume);
            st.Set("ambientVolume", Settings.AmbientVolume);
            st.Set("hapticsEnabled", Settings.HapticsEnabled);
            if (!string.IsNullOrEmpty(Settings.Language)) st.Set("language", Settings.Language);
            o.Set("settings", st);

            o.Set("achievements", StringList(Achievements));
            o.Set("completedSectors", StringList(CompletedSectors));
            o.Set("dailyShiftCompletedToday", DailyShiftCompletedToday);
            o.Set("lastDailyShiftDate", LastDailyShiftDate);
            o.Set("completedDailyTiers", StringList(CompletedDailyTiers));
            o.Set("dailyMasterClaimedDate", DailyMasterClaimedDate);
            o.Set("lastDailyGiftDate", LastDailyGiftDate);
            foreach (KeyValuePair<string, JsonValue> kv in ExtraKeys) o.Set(kv.Key, kv.Value);
            return o;
        }

        public string ToText()
        {
            return JsonWriter.Stringify(ToJson());
        }

        private static bool Truthy(JsonValue v)
        {
            if (v == null) return false;
            switch (v.Kind)
            {
                case JsonKind.Null: return false;
                case JsonKind.Bool: return v.Bool;
                case JsonKind.Number: return v.Number != 0 && !double.IsNaN(v.Number);
                case JsonKind.String: return v.String.Length > 0;
                default: return true;
            }
        }

        private static List<string> ReadStrings(JsonValue v)
        {
            var list = new List<string>();
            if (v == null || v.Kind != JsonKind.Array) return list;
            for (int i = 0; i < v.Count; i++)
            {
                if (v[i].Kind == JsonKind.String) list.Add(v[i].String);
            }
            return list;
        }

        public static Profile FromParsed(JsonValue parsed, string todayKey)
        {
            Profile p = CreateDefault();
            var known = new HashSet<string>
            {
                "version", "credits", "careerDebris", "careerContracts", "equipped", "unlocked", "settings", "achievements",
                "completedSectors", "dailyShiftCompletedToday", "lastDailyShiftDate", "completedDailyTiers",
                "dailyMasterClaimedDate", "lastDailyGiftDate", "infiniteRound", "infiniteBest",
            };

            bool hasSettings = Truthy(parsed.Get("settings"));
            bool completedSectorsTruthy = Truthy(parsed.Get("completedSectors"));
            bool tiersTruthy = Truthy(parsed.Get("completedDailyTiers"));
            bool masterTruthy = Truthy(parsed.Get("dailyMasterClaimedDate"));

            JsonValue v;
            if ((v = parsed.Get("version")) != null && v.Kind == JsonKind.Number) p.Version = v.Number;
            if ((v = parsed.Get("credits")) != null && v.Kind == JsonKind.Number) p.Credits = v.Number;
            if ((v = parsed.Get("careerDebris")) != null && v.Kind == JsonKind.Number) p.CareerDebris = v.Number;
            if ((v = parsed.Get("careerContracts")) != null && v.Kind == JsonKind.Number) p.CareerContracts = v.Number;
            if ((v = parsed.Get("infiniteRound")) != null && v.Kind == JsonKind.Number) p.InfiniteRound = v.Number;
            if ((v = parsed.Get("infiniteBest")) != null && v.Kind == JsonKind.Number) p.InfiniteBest = v.Number;

            if ((v = parsed.Get("equipped")) != null && v.Kind == JsonKind.Object)
            {
                p.Equipped = new OrderedMap<string>();
                foreach (KeyValuePair<string, JsonValue> kv in v.Members)
                {
                    if (kv.Value.Kind == JsonKind.String) p.Equipped.Set(kv.Key, kv.Value.String);
                }
            }
            if ((v = parsed.Get("unlocked")) != null && v.Kind == JsonKind.Object)
            {
                p.Unlocked = new OrderedMap<List<string>>();
                foreach (KeyValuePair<string, JsonValue> kv in v.Members)
                {
                    p.Unlocked.Set(kv.Key, ReadStrings(kv.Value));
                }
            }
            if (hasSettings)
            {
                JsonValue s = parsed.Get("settings");
                if (s.Kind == JsonKind.Object)
                {
                    JsonValue x;
                    if ((x = s.Get("soundEnabled")) != null && x.Kind == JsonKind.Bool) p.Settings.SoundEnabled = x.Bool;
                    if ((x = s.Get("sfxVolume")) != null && x.Kind == JsonKind.Number) p.Settings.SfxVolume = x.Number;
                    if ((x = s.Get("ambientVolume")) != null && x.Kind == JsonKind.Number) p.Settings.AmbientVolume = x.Number;
                    if ((x = s.Get("hapticsEnabled")) != null && x.Kind == JsonKind.Bool) p.Settings.HapticsEnabled = x.Bool;
                    if ((x = s.Get("language")) != null && x.Kind == JsonKind.String) p.Settings.Language = x.String;
                }
            }
            p.Achievements = ReadStrings(parsed.Get("achievements"));
            p.CompletedSectors = completedSectorsTruthy ? ReadStrings(parsed.Get("completedSectors")) : new List<string>();
            if ((v = parsed.Get("dailyShiftCompletedToday")) != null && v.Kind == JsonKind.Bool) p.DailyShiftCompletedToday = v.Bool;
            if ((v = parsed.Get("lastDailyShiftDate")) != null && v.Kind == JsonKind.String) p.LastDailyShiftDate = v.String;
            p.CompletedDailyTiers = tiersTruthy ? ReadStrings(parsed.Get("completedDailyTiers")) : new List<string>();
            p.DailyMasterClaimedDate = masterTruthy && parsed.Get("dailyMasterClaimedDate").Kind == JsonKind.String
                ? parsed.Get("dailyMasterClaimedDate").String
                : "";
            if ((v = parsed.Get("lastDailyGiftDate")) != null && v.Kind == JsonKind.String) p.LastDailyGiftDate = v.String;

            if (p.LastDailyShiftDate != todayKey)
            {
                p.CompletedDailyTiers = new List<string>();
                p.DailyShiftCompletedToday = false;
            }

            if (parsed.Kind == JsonKind.Object)
            {
                foreach (KeyValuePair<string, JsonValue> kv in parsed.Members)
                {
                    if (!known.Contains(kv.Key)) p.ExtraKeys.Add(kv);
                }
            }
            return p;
        }
    }
}
