using System;
using System.Collections.Generic;
using System.Globalization;
using OrbitalSalvage.JsCompat;

namespace OrbitalSalvage.Logic
{
    public sealed class CategoryNames
    {
        public string Singular;
        public string Plural;
    }

    public sealed class EconomyManager
    {
        public const string StorageKey = "ORBITAL_SALVAGE_CARTOON_PROFILE_V1";

        private readonly IKeyValueStore _store;
        private readonly IWallClock _clock;

        public Profile Profile;

        public EconomyManager(IKeyValueStore store, IWallClock clock)
        {
            _store = store;
            _clock = clock;
            Profile = LoadProfile();
        }

        public static string ToIsoDay(double epochMs)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Math.Floor(epochMs));
            return dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        public string Today()
        {
            return ToIsoDay(_clock.NowEpochMs);
        }

        public Profile GetDefaultProfile()
        {
            return Profile.CreateDefault();
        }

        public bool CanClaimDailyGift()
        {
            return Profile.LastDailyGiftDate != Today();
        }

        public bool ClaimDailyGift(double amount = 100)
        {
            if (!CanClaimDailyGift()) return false;
            string today = Today();
            Profile.LastDailyGiftDate = today;
            Profile.Credits += amount;
            SaveProfile();
            return true;
        }

        public CategoryNames NormalizeCategory(string cat)
        {
            if (cat == "ring" || cat == "rings") return new CategoryNames { Singular = "ring", Plural = "rings" };
            if (cat == "singularity" || cat == "singularities") return new CategoryNames { Singular = "singularity", Plural = "singularities" };
            if (cat == "skybox" || cat == "skyboxes") return new CategoryNames { Singular = "skybox", Plural = "skyboxes" };
            return new CategoryNames { Singular = cat, Plural = cat };
        }

        public Profile LoadProfile()
        {
            try
            {
                string raw = _store.GetItem(StorageKey);
                if (!string.IsNullOrEmpty(raw))
                {
                    JsonValue parsed = JsonParser.Parse(raw);
                    return Profile.FromParsed(parsed, Today());
                }
            }
            catch (Exception)
            {
            }
            return GetDefaultProfile();
        }

        public Profile ResetProfile()
        {
            Profile = GetDefaultProfile();
            SaveProfile();
            return Profile;
        }

        public void RecordSectorCompleted(string sectorId)
        {
            if (Profile.CompletedSectors == null)
            {
                Profile.CompletedSectors = new List<string>();
            }
            if (!Profile.CompletedSectors.Contains(sectorId))
            {
                Profile.CompletedSectors.Add(sectorId);
                SaveProfile();
            }
        }

        public void SaveProfile()
        {
            try
            {
                _store.SetItem(StorageKey, Profile.ToText());
            }
            catch (Exception)
            {
            }
        }

        public double AddCredits(double amount)
        {
            Profile.Credits += JsMath.Max(0, JsMath.Floor(amount));
            SaveProfile();
            return Profile.Credits;
        }

        public bool SpendCredits(double amount)
        {
            if (Profile.Credits >= amount)
            {
                Profile.Credits -= amount;
                SaveProfile();
                return true;
            }
            return false;
        }

        public bool IsUnlocked(string category, string itemId)
        {
            string plural = NormalizeCategory(category).Plural;
            List<string> list = Profile.Unlocked.Get(plural);
            return list != null && list.Contains(itemId);
        }

        public bool UnlockItem(string category, string itemId, double cost)
        {
            string plural = NormalizeCategory(category).Plural;
            if (IsUnlocked(plural, itemId)) return true;
            if (SpendCredits(cost))
            {
                if (Profile.Unlocked.Get(plural) == null)
                {
                    Profile.Unlocked.Set(plural, new List<string>());
                }
                Profile.Unlocked.Get(plural).Add(itemId);
                SaveProfile();
                return true;
            }
            return false;
        }

        public bool EquipItem(string category, string itemId)
        {
            CategoryNames n = NormalizeCategory(category);
            if (IsUnlocked(n.Plural, itemId))
            {
                Profile.Equipped.Set(n.Singular, itemId);
                SaveProfile();
                return true;
            }
            return false;
        }

        public List<Achievement> RecordDebrisSwallowed(double count = 1)
        {
            Profile.CareerDebris += count;
            SaveProfile();
            return CheckAchievements();
        }

        public List<Achievement> RecordContractComplete(bool isDaily = false, string tier = "silver")
        {
            Profile.CareerContracts++;
            if (isDaily)
            {
                string today = Today();
                if (Profile.LastDailyShiftDate != today)
                {
                    Profile.LastDailyShiftDate = today;
                    Profile.CompletedDailyTiers = new List<string>();
                    Profile.DailyMasterClaimedDate = "";
                }
                if (Profile.CompletedDailyTiers == null)
                {
                    Profile.CompletedDailyTiers = new List<string>();
                }
                if (!string.IsNullOrEmpty(tier) && !Profile.CompletedDailyTiers.Contains(tier))
                {
                    Profile.CompletedDailyTiers.Add(tier);
                }
                Profile.DailyShiftCompletedToday = true;
            }
            SaveProfile();
            return CheckAchievements();
        }

        public bool CanClaimDailyMasterBonus()
        {
            string today = Today();
            List<DailyShiftInfo> shifts = GetDailyShifts();
            bool allCompleted = shifts.Count > 0 && shifts.TrueForAll(s => s.IsCompleted);
            return allCompleted && Profile.DailyMasterClaimedDate != today;
        }

        public bool IsDailyMasterClaimed()
        {
            return Profile.DailyMasterClaimedDate == Today();
        }

        public bool ClaimDailyMasterBonus(double amount = 300)
        {
            if (!CanClaimDailyMasterBonus()) return false;
            string today = Today();
            Profile.DailyMasterClaimedDate = today;
            Profile.Credits += amount;
            SaveProfile();
            return true;
        }

        public List<Achievement> CheckAchievements()
        {
            var newlyUnlocked = new List<Achievement>();
            Profile p = Profile;

            foreach (Achievement ach in EconomyCatalog.Achievements)
            {
                if (p.Achievements.Contains(ach.Id)) continue;

                bool qualified = false;
                if (ach.Id == "ach_first_drop" && p.CareerDebris >= 1) qualified = true;
                if (ach.Id == "ach_cleaner_50" && p.CareerDebris >= 50) qualified = true;
                if (ach.Id == "ach_cleaner_250" && p.CareerDebris >= 250) qualified = true;
                if (ach.Id == "ach_cleaner_1000" && p.CareerDebris >= 1000) qualified = true;
                if (ach.Id == "ach_daily_shift" && p.DailyShiftCompletedToday) qualified = true;

                if (qualified)
                {
                    p.Achievements.Add(ach.Id);
                    p.Credits += ach.Reward;
                    newlyUnlocked.Add(ach);
                }
            }

            if (newlyUnlocked.Count > 0)
            {
                SaveProfile();
            }
            return newlyUnlocked;
        }

        public List<DailyShiftInfo> GetDailyShifts()
        {
            string today = Today();
            List<string> completedTiers = Profile.LastDailyShiftDate == today
                ? (Profile.CompletedDailyTiers ?? new List<string>())
                : new List<string>();

            double seed = 0;
            for (int i = 0; i < today.Length; i++)
            {
                seed = JsMath.ToInt32(seed * 31 + today[i]) & 0xffffff;
            }

            string[] funModifiers =
            {
                "Fizzy Soda Storm: Bubble bursts push trash!",
                "Super Bouncy Day: Extra springy corridors!",
                "Double Sprinkles: Bonus coins for sweet recycling!",
                "Snack Attack: Nibbles is extra hungry today!",
            };

            string modBronze = "Calm Orbit Sweep: Gentle centrifugal speed, wide gates & easy flow.";
            string modSilver = funModifiers[(int)(seed % funModifiers.Length)];
            string modGold = "Cosmic Vortex Overdrive: High-density debris cloud & dual shredder gates!";

            double bronzeSeed = JsMath.ToInt32(seed * 7 + 101) & 0xffffff;
            double silverSeed = JsMath.ToInt32(seed * 13 + 307) & 0xffffff;
            double goldSeed = JsMath.ToInt32(seed * 23 + 709) & 0xffffff;

            return new List<DailyShiftInfo>
            {
                new DailyShiftInfo
                {
                    Id = "daily_bronze",
                    Tier = "bronze",
                    TierBadge = "\U0001F949 BRONZE",
                    TierName = "Morning Warmup",
                    Title = "Bronze Shift \u2022 Cadet Sweep",
                    Quota = 25 + (bronzeSeed % 10),
                    RewardCredits = 200,
                    HazardModifier = modBronze,
                    ThemeColor = "#f59e0b",
                    Seed = bronzeSeed,
                    IsCompleted = completedTiers.Contains("bronze"),
                },
                new DailyShiftInfo
                {
                    Id = "daily_silver",
                    Tier = "silver",
                    TierBadge = "\U0001F948 SILVER",
                    TierName = "Hazard Shift",
                    Title = "Silver Shift \u2022 Sector Hazard",
                    Quota = 50 + (silverSeed % 15),
                    RewardCredits = 350,
                    HazardModifier = modSilver,
                    ThemeColor = "#38bdf8",
                    Seed = silverSeed,
                    IsCompleted = completedTiers.Contains("silver"),
                },
                new DailyShiftInfo
                {
                    Id = "daily_gold",
                    Tier = "gold",
                    TierBadge = "\U0001F947 GOLD",
                    TierName = "Apex Overdrive",
                    Title = "Gold Shift \u2022 Vortex Apex",
                    Quota = 80 + (goldSeed % 20),
                    RewardCredits = 500,
                    HazardModifier = modGold,
                    ThemeColor = "#facc15",
                    Seed = goldSeed,
                    IsCompleted = completedTiers.Contains("gold"),
                },
            };
        }

        public DailyShiftInfo GetDailyShiftInfo()
        {
            List<DailyShiftInfo> shifts = GetDailyShifts();
            return shifts.Count > 1 ? shifts[1] : shifts[0];
        }

        public static double CampaignPayout(double rewardCredits, double collectedMass, double quota)
        {
            double baseReward = Truthy(rewardCredits) ? rewardCredits : 250;
            double surplus = JsMath.Max(0, collectedMass - quota);
            return baseReward + surplus * 4;
        }

        private static bool Truthy(double v)
        {
            return v != 0 && !double.IsNaN(v);
        }
    }
}
