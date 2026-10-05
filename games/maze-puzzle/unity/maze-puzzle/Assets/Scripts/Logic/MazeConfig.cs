using System.Collections.Generic;

namespace OrbitalSalvage.Logic
{
    public sealed class DividerConfig
    {
        public double? RMin;
        public double? RMax;
        public double Angle;
        public bool? IsInner;
    }

    public sealed class GateConfig
    {
        public string Type;
        public double Value;
        public double Angle;
        public double? RMin;
        public double? RMax;
        public double? R;
        public double? Length;
        public bool? IsInner;
        public string Color;
    }

    public sealed class OneWayFlapConfig
    {
        public double? RMin;
        public double? RMax;
        public double Angle;
        public double MaxSwing;
        public double? NormalSign;
        public bool? IsInner;
    }

    public sealed class DeflectorPinConfig
    {
        public double R;
        public double Angle;
        public double Radius;
        public double Restitution;
        public bool? IsInner;
        public string Color;
        public string Accent;
    }

    public sealed class TrackWallConfig
    {
        public double R;
        public double GapAngle;
        public double GapSize;
    }

    public sealed class SectorConfig
    {
        public string Id;
        public double? LevelNumber;
        public double? WorldIndex;
        public string Name;
        public string Code;
        public string Description;
        public double Quota;
        public double InitialDebrisCount;
        public List<string> InitialDebrisTypes;
        public double RingRadius;
        public double CorridorWidth;
        public double? InnerRadius;
        public double ApertureAngle;
        public bool? ConcentricRings;
        public double? GearRatio;
        public List<TrackWallConfig> TrackWalls;
        public List<DividerConfig> Dividers;
        public List<GateConfig> Gates;
        public List<DeflectorPinConfig> DeflectorPins;
        public List<OneWayFlapConfig> OneWayFlaps;
        public bool? IsFinale;
        public bool? IsDaily;
        public string DailyTier;
        public double? RewardCredits;
    }

    public sealed class WorldInfo
    {
        public string Id;
        public int WorldIndex;
        public string Title;
        public string Tagline;
        public string Icon;
        public string Color;
        public int LevelRangeStart;
        public int LevelRangeEnd;
    }

    public sealed class DailyShiftInfo
    {
        public string Id;
        public string Tier;
        public string TierBadge;
        public string TierName;
        public string Title;
        public double? Quota;
        public double? RewardCredits;
        public string HazardModifier;
        public string ThemeColor;
        public double? Seed;
        public bool IsCompleted;

        public OrbitalSalvage.JsCompat.JsonValue ToJson()
        {
            var o = OrbitalSalvage.JsCompat.JsonValue.NewObject();
            if (Id != null) o.Set("id", Id);
            if (Tier != null) o.Set("tier", Tier);
            if (TierBadge != null) o.Set("tierBadge", TierBadge);
            if (TierName != null) o.Set("tierName", TierName);
            if (Title != null) o.Set("title", Title);
            if (Quota.HasValue) o.Set("quota", Quota.Value);
            if (RewardCredits.HasValue) o.Set("rewardCredits", RewardCredits.Value);
            if (HazardModifier != null) o.Set("hazardModifier", HazardModifier);
            if (ThemeColor != null) o.Set("themeColor", ThemeColor);
            if (Seed.HasValue) o.Set("seed", Seed.Value);
            o.Set("isCompleted", IsCompleted);
            return o;
        }
    }
}
