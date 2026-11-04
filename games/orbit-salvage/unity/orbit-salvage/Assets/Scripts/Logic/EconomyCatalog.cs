using System.Collections.Generic;
using OrbitalSalvage.JsCompat;

namespace OrbitalSalvage.Logic
{
    public sealed class CosmeticItem
    {
        public string Id;
        public string Name;
        public string Desc;
        public double Cost;
        public string Color;
        public string Accent;
        public string CoreColor;
        public string HaloColor;
        public string[] BgGradient;
        public bool Unlocked;

        public JsonValue ToJson()
        {
            JsonValue o = JsonValue.NewObject();
            o.Set("id", Id);
            o.Set("name", Name);
            o.Set("desc", Desc);
            o.Set("cost", Cost);
            if (Color != null) o.Set("color", Color);
            if (Accent != null) o.Set("accent", Accent);
            if (CoreColor != null) o.Set("coreColor", CoreColor);
            if (HaloColor != null) o.Set("haloColor", HaloColor);
            if (BgGradient != null)
            {
                JsonValue arr = JsonValue.NewArray();
                foreach (string c in BgGradient) arr.Add(JsonValue.FromString(c));
                o.Set("bgGradient", arr);
            }
            o.Set("unlocked", Unlocked);
            return o;
        }
    }

    public sealed class Achievement
    {
        public string Id;
        public string Title;
        public string Desc;
        public string Icon;
        public double Reward;

        public JsonValue ToJson()
        {
            JsonValue o = JsonValue.NewObject();
            o.Set("id", Id);
            o.Set("title", Title);
            o.Set("desc", Desc);
            o.Set("icon", Icon);
            o.Set("reward", Reward);
            return o;
        }
    }

    public static class EconomyCatalog
    {
        public static readonly CosmeticItem[] Rings =
        {
            new CosmeticItem { Id = "ring_donut", Name = "Frosted Donut Rig", Desc = "Sweet strawberry icing with sprinkles and bouncy cartoon dough!", Cost = 0, Color = "#f472b6", Accent = "#fde047", Unlocked = true },
            new CosmeticItem { Id = "ring_duckie", Name = "Rubber Duckie Floatie", Desc = "Squeaky bright yellow bath duck with cute orange beak and water bubbles!", Cost = 500, Color = "#facc15", Accent = "#fb923c", Unlocked = false },
            new CosmeticItem { Id = "ring_candyland", Name = "Peppermint Candy Ring", Desc = "Crisp red and white candy cane stripes with minty freshness!", Cost = 1200, Color = "#ef4444", Accent = "#ffffff", Unlocked = false },
            new CosmeticItem { Id = "ring_spacecat", Name = "Cosmic Kitty Saucer", Desc = "Lilac flying saucer with adorable cat ears and neon whiskers!", Cost = 2500, Color = "#c084fc", Accent = "#38bdf8", Unlocked = false },
        };

        public static readonly CosmeticItem[] Singularities =
        {
            new CosmeticItem { Id = "singularity_nibbles", Name = "Nibbles the Void", Desc = "The hungriest, cutest cartoon black hole with big anime eyes! Nom nom!", Cost = 0, CoreColor = "#1e1b4b", HaloColor = "#818cf8", Unlocked = true },
            new CosmeticItem { Id = "singularity_strawberry", Name = "Berry Swirl Vortex", Desc = "Sweet strawberry cyclone that eats trash like fruit smoothie!", Cost = 600, CoreColor = "#831843", HaloColor = "#f472b6", Unlocked = false },
            new CosmeticItem { Id = "singularity_sleepy", Name = "Sleepy Star Cloud", Desc = "A sleepy pastel blue cloud that yawns and gulps down space junk!", Cost = 1500, CoreColor = "#0c4a6e", HaloColor = "#38bdf8", Unlocked = false },
            new CosmeticItem { Id = "singularity_confetti", Name = "Party Popper Hole", Desc = "Fiesta vortex exploding with rainbow sparkles and party confetti!", Cost = 3000, CoreColor = "#3b0764", HaloColor = "#facc15", Unlocked = false },
        };

        public static readonly CosmeticItem[] Skyboxes =
        {
            new CosmeticItem { Id = "skybox_cotton_candy", Name = "Cotton Candy Nebula", Desc = "Fluffy pastel indigo and magenta clouds with sparkling star smileys.", Cost = 0, BgGradient = new[] { "#1e1b4b", "#312e81", "#4338ca" }, Unlocked = true },
            new CosmeticItem { Id = "skybox_cheese_moon", Name = "Cheese Moon Orbit", Desc = "A happy cartoon moon made of Swiss cheese floating in twilight.", Cost = 800, BgGradient = new[] { "#0f172a", "#1e293b", "#334155" }, Unlocked = false },
            new CosmeticItem { Id = "skybox_mint_soda", Name = "Mint Soda Lagoon", Desc = "Sparkling emerald and turquoise cosmos with fizzy space bubbles.", Cost = 2000, BgGradient = new[] { "#042f2e", "#064e3b", "#065f46" }, Unlocked = false },
        };

        public static readonly Achievement[] Achievements =
        {
            new Achievement { Id = "ach_first_drop", Title = "First Nom Nom!", Desc = "Feed Nibbles your very first piece of space junk!", Icon = "\U0001F60B", Reward = 100 },
            new Achievement { Id = "ach_cleaner_50", Title = "Tidy Space Cadet", Desc = "Vacuum up 50 pieces of space trash into Nibbles' tummy.", Icon = "\U0001F9F9", Reward = 250 },
            new Achievement { Id = "ach_cleaner_250", Title = "Kessler Hero", Desc = "Clean up 250 space items to save the orbit from clutter!", Icon = "\u2B50", Reward = 750 },
            new Achievement { Id = "ach_cleaner_1000", Title = "Galaxy Janitor King", Desc = "1,000 items recycled! You are the ultimate cosmic cleaner!", Icon = "\U0001F451", Reward = 2000 },
            new Achievement { Id = "ach_daily_shift", Title = "Daily Duty Star", Desc = "Complete today's wacky Daily Shift cleanup job!", Icon = "\U0001F389", Reward = 500 },
        };

        public static CosmeticItem[] Category(string plural)
        {
            switch (plural)
            {
                case "rings": return Rings;
                case "singularities": return Singularities;
                case "skyboxes": return Skyboxes;
                default: return null;
            }
        }

        public static JsonValue CatalogToJson()
        {
            JsonValue o = JsonValue.NewObject();
            o.Set("rings", ItemsToJson(Rings));
            o.Set("singularities", ItemsToJson(Singularities));
            o.Set("skyboxes", ItemsToJson(Skyboxes));
            return o;
        }

        public static JsonValue AchievementsToJson()
        {
            JsonValue arr = JsonValue.NewArray();
            foreach (Achievement a in Achievements) arr.Add(a.ToJson());
            return arr;
        }

        private static JsonValue ItemsToJson(CosmeticItem[] items)
        {
            JsonValue arr = JsonValue.NewArray();
            foreach (CosmeticItem i in items) arr.Add(i.ToJson());
            return arr;
        }
    }
}
