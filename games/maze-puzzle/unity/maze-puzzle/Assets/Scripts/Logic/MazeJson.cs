using System.Collections.Generic;
using OrbitalSalvage.JsCompat;

namespace OrbitalSalvage.Logic
{
    public static class MazeJson
    {
        private static void SetNum(JsonValue o, string key, double? v)
        {
            if (v.HasValue) o.Set(key, v.Value);
        }

        private static void SetBool(JsonValue o, string key, bool? v)
        {
            if (v.HasValue) o.Set(key, v.Value);
        }

        private static void SetStr(JsonValue o, string key, string v)
        {
            if (v != null) o.Set(key, v);
        }

        public static JsonValue ToJson(SectorConfig c)
        {
            JsonValue o = JsonValue.NewObject();
            SetStr(o, "id", c.Id);
            SetNum(o, "levelNumber", c.LevelNumber);
            SetNum(o, "worldIndex", c.WorldIndex);
            SetStr(o, "name", c.Name);
            SetStr(o, "code", c.Code);
            SetStr(o, "description", c.Description);
            o.Set("quota", c.Quota);
            o.Set("initialDebrisCount", c.InitialDebrisCount);
            if (c.InitialDebrisTypes != null)
            {
                JsonValue arr = JsonValue.NewArray();
                foreach (string t in c.InitialDebrisTypes) arr.Add(JsonValue.FromString(t));
                o.Set("initialDebrisTypes", arr);
            }
            o.Set("ringRadius", c.RingRadius);
            o.Set("corridorWidth", c.CorridorWidth);
            SetNum(o, "innerRadius", c.InnerRadius);
            o.Set("apertureAngle", c.ApertureAngle);
            SetBool(o, "concentricRings", c.ConcentricRings);
            SetNum(o, "gearRatio", c.GearRatio);
            if (c.TrackWalls != null)
            {
                JsonValue arr = JsonValue.NewArray();
                foreach (TrackWallConfig t in c.TrackWalls)
                {
                    JsonValue e = JsonValue.NewObject();
                    e.Set("r", t.R);
                    e.Set("gapAngle", t.GapAngle);
                    e.Set("gapSize", t.GapSize);
                    arr.Add(e);
                }
                o.Set("trackWalls", arr);
            }
            if (c.Dividers != null)
            {
                JsonValue arr = JsonValue.NewArray();
                foreach (DividerConfig d in c.Dividers)
                {
                    JsonValue e = JsonValue.NewObject();
                    SetNum(e, "rMin", d.RMin);
                    SetNum(e, "rMax", d.RMax);
                    e.Set("angle", d.Angle);
                    SetBool(e, "isInner", d.IsInner);
                    arr.Add(e);
                }
                o.Set("dividers", arr);
            }
            if (c.Gates != null)
            {
                JsonValue arr = JsonValue.NewArray();
                foreach (GateConfig g in c.Gates)
                {
                    JsonValue e = JsonValue.NewObject();
                    SetStr(e, "type", g.Type);
                    e.Set("value", g.Value);
                    e.Set("angle", g.Angle);
                    SetNum(e, "rMin", g.RMin);
                    SetNum(e, "rMax", g.RMax);
                    SetBool(e, "isInner", g.IsInner);
                    SetStr(e, "color", g.Color);
                    arr.Add(e);
                }
                o.Set("gates", arr);
            }
            if (c.DeflectorPins != null)
            {
                JsonValue arr = JsonValue.NewArray();
                foreach (DeflectorPinConfig p in c.DeflectorPins)
                {
                    JsonValue e = JsonValue.NewObject();
                    e.Set("r", p.R);
                    e.Set("angle", p.Angle);
                    e.Set("radius", p.Radius);
                    e.Set("restitution", p.Restitution);
                    SetBool(e, "isInner", p.IsInner);
                    SetStr(e, "color", p.Color);
                    SetStr(e, "accent", p.Accent);
                    arr.Add(e);
                }
                o.Set("deflectorPins", arr);
            }
            if (c.OneWayFlaps != null)
            {
                JsonValue arr = JsonValue.NewArray();
                foreach (OneWayFlapConfig f in c.OneWayFlaps)
                {
                    JsonValue e = JsonValue.NewObject();
                    SetNum(e, "rMin", f.RMin);
                    SetNum(e, "rMax", f.RMax);
                    e.Set("angle", f.Angle);
                    e.Set("maxSwing", f.MaxSwing);
                    SetNum(e, "normalSign", f.NormalSign);
                    SetBool(e, "isInner", f.IsInner);
                    arr.Add(e);
                }
                o.Set("oneWayFlaps", arr);
            }
            SetBool(o, "isFinale", c.IsFinale);
            SetBool(o, "isDaily", c.IsDaily);
            SetStr(o, "dailyTier", c.DailyTier);
            SetNum(o, "rewardCredits", c.RewardCredits);
            return o;
        }

        public static string ToText(SectorConfig c)
        {
            return JsonWriter.Stringify(ToJson(c));
        }
    }
}
