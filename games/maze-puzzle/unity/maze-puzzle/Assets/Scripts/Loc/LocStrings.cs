using System;
using System.Collections.Generic;

namespace OrbitalSalvage.Loc
{
    /// <summary>
    /// English source for <see cref="Loc.Tr"/>. The active language does not need
    /// <c>en.json</c>: a missing table entry, and English itself, use this registry.
    /// Placeholder order is <c>{0}</c> style so a translation can reorder.
    /// Units (<c>kg</c>) and the coin emoji sit inside the string.
    /// </summary>
    public static class LocStrings
    {
        public static readonly LocStringDef[] All = new LocStringDef[]
        {
            Def("loading.tip.0", "Calibrating cosmic centrifuges...", "Loading screen tip under the progress bar."),
            Def("loading.tip.1", "Polishing space soda cans...", "Loading screen tip under the progress bar."),
            Def("loading.tip.2", "Waking up Nibbles for breakfast...", "Loading screen tip under the progress bar."),
            Def("loading.tip.3", "Sprinkling candy on orbital rings...", "Loading screen tip under the progress bar."),
            Def("loading.tip.4", "Ready for cleanup launch! \U0001F680", "Loading screen tip. Keep the rocket emoji."),

            Def("toast.progress.reset", "Progress Reset to Day 1! \U0001F9F9", "Toast after Tools \u2192 Reset Saved Profile. Keep the broom emoji."),
            Def("toast.shop.unlocked", "Item Unlocked & Equipped! \U0001F389", "Toast after buying a shop item. Keep the party emoji."),
            Def("toast.shop.need_coins", "Need more gold coins! \U0001FA99", "Toast when a purchase costs more than the balance. Keep the coin emoji."),
            Def("toast.shop.equipped", "Item Equipped! \u2728", "Toast after equipping an owned item. Keep the sparkles."),
            Def("toast.trophy", "\U0001F3C6 Trophy: {0} (+{1} \U0001FA99)", "Toast when an achievement unlocks. {0} is the achievement title passed through Loc.TrName, {1} is the coin reward. Keep the trophy and coin emoji."),
            Def("toast.daily.all_clear", "\U0001F389 All 3 Daily Shifts Cleared! Claim Master Bonus in Daily Tab! \U0001F3C6", "Toast when all three daily shifts are done and the master bonus is unclaimed. Keep both emoji."),
            Def("toast.sector.cleanup", "Clean up {0}! \U0001F680", "Toast when a level loads. {0} is the level or daily-shift name passed through Loc.TrName (may be long). Keep the rocket."),
            Def("toast.infinite.round", "Round {0}, best {1}! \U0001F680", "Toast when an infinite-mode round starts, after all 50 levels are done. {0} is the round number, {1} is the best round reached. Keep the rocket."),
            Def("toast.backup.deployed", "\U0001F680 Auxiliary Scrap Pod Deployed! (+8 Scrap)", "Toast when the backup pod fires. The 8 is fixed. Keep the rocket."),
            Def("toast.gift.already", "\U0001F381 Claimed today. Come back tomorrow!", "Toast when the daily gift button is pressed again. Keep the gift emoji."),
            Def("toast.gift.claimed", "\U0001F381 Claimed +100 \U0001FA99 Janitor Stipend!", "Toast after the daily gift pays 100 coins. Keep the gift and coin emoji."),
            Def("toast.daily.master_claimed", "\U0001F3C6 Claimed +300 \U0001FA99 Daily Master Bonus!", "Toast after the daily master bonus pays 300 coins. Keep the trophy and coin emoji."),
            Def("toast.sound.off", "Sound Off \U0001F507", "Toast when the HUD mute control turns sound off. Keep the muted speaker."),
            Def("toast.sound.on", "Sound On \U0001F50A", "Toast when the HUD mute control turns sound on. Keep the speaker."),

            Def("home.level.title", "{0} \u2022 {1}", "Home mission name and pause level name. {0} is the level code and {1} is the level name, both passed through Loc.TrName."),
            Def("home.mission.goal", "Goal: {0} kg", "Home mission card goal. {0} is the quota. Keep kg in the string. About 18 characters at small quotas."),
            Def("home.infinite.world", "Infinite Mode", "Home mission card small badge once all 50 levels are done. Names the endless mode. Keep it short, about the length of a world name."),
            Def("home.infinite.title", "Round {0}", "Home mission card title in infinite mode. {0} is the round number. Same word for 'round' as toast.infinite.round."),
            Def("home.infinite.best", "Best: {0}", "Home mission card gold pill in infinite mode. {0} is the best round reached. Keep it very short, it sits where 'Goal: {0} kg' sits."),
            Def("home.mission.world_fallback", "World {0}", "Home mission world line only when the world table has no match. {0} is the world number."),
            Def("home.daily.bounty", "+{0} \U0001FA99 Bounty", "Home card bounty for the silver daily shift. {0} is the coin amount. Keep the coin emoji."),

            Def("home.ring.duckie", "\U0001F986 DUCKIE FLOAT", "Home diorama ring label. Keep the duck emoji. About 14 characters."),
            Def("home.ring.candy", "\U0001F36C CANDY RING", "Home diorama ring label. Keep the candy emoji. About 12 characters."),
            Def("home.ring.kitty", "\U0001F431 KITTY SAUCER", "Home diorama ring label. Keep the cat emoji. About 14 characters."),
            Def("home.ring.donut", "\U0001F369 DONUT FLOAT", "Home diorama ring label, default ring. Keep the donut emoji. About 13 characters."),

            Def("home.face.strawberry", "\U0001F353", "Home diorama black-hole face for the strawberry skin. Keep the strawberry."),
            Def("home.face.sleepy", "\U0001F4A4", "Home diorama black-hole face for the sleepy skin. Keep the zzz."),
            Def("home.face.confetti", "\U0001F389", "Home diorama black-hole face for the party skin. Keep the party popper."),
            Def("home.face.nibbles", "\U0001F60B", "Home diorama black-hole face, default Nibbles. Keep the yum face."),
            Def("home.face.poke", "\U0001F970", "Home diorama face held for a moment after a poke. Keep the heart-eyes face."),

            Def("hud.sector_code", "\u2B50 {0}", "In-game HUD sector chip. {0} is the level code passed through Loc.TrName. Keep the star."),
            Def("hud.sound.muted", "\U0001F507", "Muted speaker on the HUD and the pause sound button. Keep this emoji."),
            Def("hud.sound.on", "\U0001F50A", "Speaker on the HUD and the pause sound button. Keep this emoji."),

            Def("unit.kg", "{0} kg", "Debrief collected mass and defeat spilled mass. {0} is the mass. Keep kg."),
            Def("unit.quota", "{0} / {1} kg", "HUD quota and defeat collected line. {0} is collected mass, {1} is the quota. Keep kg and the slash."),
            Def("unit.plus_coins", "+{0} \U0001FA99", "Debrief coin payout. {0} is the payout. Keep the plus and the coin emoji."),

            Def("dock.shop.free", "FREE", "Shop dock badge while the daily gift is unclaimed. Chip, about 6 characters.", 6),
            Def("dock.daily.gift", "GIFT", "Daily dock badge when every shift is done and the master bonus is ready. Chip, about 6 characters.", 6),
            Def("dock.daily.check", "\u2713", "Daily dock badge when the master bonus is already claimed. Keep the check mark."),
            Def("dock.daily.new", "{0} NEW", "Daily dock badge while shifts remain. {0} is how many are left (1 to 3). Chip, about 6 characters.", 8),

            Def("gift.title.ready", "Janitor Care Package!", "Shop gift banner title while the daily gift is unclaimed."),
            Def("gift.title.claimed", "Allowance Claimed! \u2728", "Shop gift banner title after the gift is claimed. Keep the sparkles."),
            Def("gift.button.claim", "Claim +100 \U0001FA99", "Daily gift button while unclaimed. The 100 is fixed. Button, about 14 characters. Keep the coin.", 16),
            Def("gift.button.claimed", "Claimed Today \u2728", "Daily gift button after claiming. Button, about 16 characters. Keep the sparkles.", 18),
            Def("gift.desc.ready", "Claim your free daily +100 \U0001FA99 allowance.", "Shop gift banner sentence while unclaimed. The 100 is fixed. Keep the coin."),
            Def("gift.desc.claimed", "Come back tomorrow for another +100 \U0001FA99 care package!", "Shop gift banner sentence after claiming. The 100 is fixed. Keep the coin."),
            Def("gift.icon.ready", "\U0001F381", "Shop gift banner icon while unclaimed. Keep the gift."),
            Def("gift.icon.claimed", "\U0001F389", "Shop gift banner icon after claiming. Keep the party popper."),

            Def("pause.quota", "Goal: Feed Nibbles {0} kg ({1}/{0} kg eaten)", "Pause modal quota line. {0} is the quota, {1} is mass eaten. Keep kg. The quota appears twice so a translation can repeat it."),

            Def("shop.footer.equipped", "EQUIPPED \u2728", "Shop card badge on the equipped item. Chip, about 12 characters. Keep the sparkles.", 14),
            Def("shop.footer.wear", "Wear", "Shop card button on an owned item that is not equipped. Button, about 8 characters.", 10),
            Def("shop.footer.free", "Free Starter", "Shop card button on an unowned item that costs nothing. Button, about 14 characters.", 16),
            Def("shop.footer.unlock", "Unlock ({0} \U0001FA99)", "Shop card button on a locked item. {0} is the price. Button, about 16 characters. Keep the coin.", 22),

            Def("daily.countdown", "Resets in {0}h {1}m", "Daily reset countdown in America/Los_Angeles. {0} is hours, {1} is minutes. Keep the h and m units in the string."),

            Def("fx.hub.orbital", "\u2728 ORBITAL \u2728", "Ring hub title when the equipped ring has no special label. Keep the sparkles. About 14 characters.", 16),
            Def("fx.hub.sort", "SORT RIG", "Ring hub subtitle for the default label. About 8 characters.", 12),
            Def("fx.hub.duckie", "\U0001F986 DUCKIE \U0001F986", "Ring hub title for the duckie ring. Keep both ducks. About 12 characters.", 16),
            Def("fx.hub.float", "FLOAT RIG", "Ring hub subtitle for the duckie ring. About 9 characters.", 12),
            Def("fx.hub.candy", "\U0001F36C CANDY \U0001F36C", "Ring hub title for the candy ring. Keep both candies. About 11 characters.", 16),
            Def("fx.hub.mint", "MINT RIG", "Ring hub subtitle for the candy ring. About 8 characters.", 12),
            Def("fx.hub.kitty", "\U0001F431 KITTY \U0001F431", "Ring hub title for the kitty ring. Keep both cats. About 11 characters.", 16),
            Def("fx.hub.cosmic", "COSMIC RIG", "Ring hub subtitle for the kitty ring. About 10 characters.", 12),
            Def("fx.hub.donut", "\U0001F369 DONUT \U0001F369", "Ring hub title for the donut ring. Keep both donuts. About 11 characters.", 16),
            Def("fx.hub.sweet", "SWEET RIG", "Ring hub subtitle for the donut ring. About 9 characters.", 12),
            Def("fx.hub.gear", "\u2699\uFE0F GEAR RIG \u2699\uFE0F", "Ring hub title while concentric planetary rings are on. Keep both gears. About 14 characters.", 18),
            Def("fx.hub.planetary", "PLANETARY", "Ring hub subtitle for concentric rings. About 9 characters.", 12),

            Def("fx.gate.zap", "\u26A1 \u00D7{0}", "Multiplier gate badge, drawn every frame. {0} is the multiplier. Keep the bolt and the multiply sign. About 8 characters.", 12),
            Def("fx.gate.shred", "\U0001F4A5 \u00D7{0}!", "Super-shredder gate badge, drawn every frame. {0} is the multiplier. Keep the collision emoji and the multiply sign.", 12),
            Def("fx.gate.magnet", "\U0001F9F2 +{0}", "Compactor gate badge, drawn every frame. {0} is the added mass. Keep the magnet.", 12),
            Def("fx.gate.boost", "\u23E9 BOOST", "Booster gate badge, drawn every frame. Keep the fast-forward glyph. About 8 characters.", 12),

            Def("fx.swallow.nom", "NOM! \U0001F60B", "Floating word when Nibbles swallows trash. One of four, chosen at random. Keep the face."),
            Def("fx.swallow.yum", "YUM! \u2728", "Floating word when Nibbles swallows trash. Keep the sparkles."),
            Def("fx.swallow.gulp", "GULP! \U0001F369", "Floating word when Nibbles swallows trash. Keep the donut."),
            Def("fx.swallow.tasty", "TASTY! \u2B50", "Floating word when Nibbles swallows trash. Keep the star."),
            Def("fx.shred", "\u00D7{0}! SHRED \u26A1", "Floating word when trash crosses a multiply gate. {0} is the gate value. Keep the multiply sign and the bolt."),
            Def("fx.compact", "+{0}! COMPACT \u2728", "Floating word when trash crosses an add gate. {0} is the gate value. Keep the sparkles."),
            Def("fx.boost", "BOOST! \u23E9", "Floating word when trash crosses a boost gate. Keep the fast-forward glyph."),
            Def("fx.poof", "POOF! \U0001F4A8", "Floating word when trash explodes. Keep the dash symbol."),
            Def("fx.turbo.cw", "TURBO! \u23E9", "Floating word for a clockwise boost. Keep the fast-forward glyph."),
            Def("fx.turbo.ccw", "\u23EA TURBO!", "Floating word for a counter-clockwise boost. Keep the rewind glyph."),

            Def("intro.step.rotate", "Swipe anywhere to spin the orbital ring!", "First-time player onboarding: step 1 swipe instructions."),
            Def("intro.step.multiply", "Pass scrap through the \u00D72 Shredder to multiply mass!", "First-time player onboarding: step 2 multiply gate instructions."),
            Def("intro.step.feed", "Dump scrap into my mouth to reach the {0} kg quota!", "First-time player onboarding: step 3 feed Nibbles instructions."),
            Def("intro.safety.backup", "\U0001F680 Auxiliary Pod Deployed! Launching extra scrap!", "Level 1 safety net toast when new player runs out of scrap."),
        };

        static readonly Dictionary<string, LocStringDef> ByKey = Build();
        static readonly Dictionary<string, string> DataByEnglish = BuildData();

        public static int Count
        {
            get { return All.Length; }
        }

        public static bool TryGet(string key, out string english)
        {
            english = null;
            if (key == null) return false;
            LocStringDef row;
            if (!ByKey.TryGetValue(key, out row) || row == null) return false;
            english = row.English ?? "";
            return true;
        }

        public static bool TryGetDef(string key, out LocStringDef def)
        {
            def = null;
            if (key == null) return false;
            return ByKey.TryGetValue(key, out def) && def != null;
        }

        /// <summary>
        /// Registry row whose key starts with <c>data.</c> and whose English equals
        /// <paramref name="english"/>. Used by <see cref="Loc.TrName"/> before spec lookup.
        /// </summary>
        public static bool TryGetDataKey(string english, out string key)
        {
            key = null;
            if (string.IsNullOrEmpty(english)) return false;
            return DataByEnglish.TryGetValue(english, out key);
        }

        /// <summary>Key order is ordinal, for the extractor.</summary>
        public static List<KeyValuePair<string, string>> SortedPairs()
        {
            var keys = new List<string>(All.Length);
            for (int i = 0; i < All.Length; i++) keys.Add(All[i].Key);
            keys.Sort(StringComparer.Ordinal);
            var pairs = new List<KeyValuePair<string, string>>(keys.Count);
            for (int i = 0; i < keys.Count; i++)
            {
                string english;
                TryGet(keys[i], out english);
                pairs.Add(new KeyValuePair<string, string>(keys[i], english));
            }
            return pairs;
        }

        public static LocStringDef[] SortedDefs()
        {
            var copy = new LocStringDef[All.Length];
            for (int i = 0; i < All.Length; i++) copy[i] = All[i];
            Array.Sort(copy, delegate (LocStringDef a, LocStringDef b)
            {
                return string.CompareOrdinal(a.Key, b.Key);
            });
            return copy;
        }

        static Dictionary<string, LocStringDef> Build()
        {
            var map = new Dictionary<string, LocStringDef>(All.Length, StringComparer.Ordinal);
            for (int i = 0; i < All.Length; i++)
            {
                LocStringDef row = All[i];
                if (row == null || string.IsNullOrEmpty(row.Key))
                    throw new InvalidOperationException("loc registry row missing a key");
                if (map.ContainsKey(row.Key))
                    throw new InvalidOperationException("duplicate loc key " + row.Key);
                map.Add(row.Key, row);
            }
            return map;
        }

        static Dictionary<string, string> BuildData()
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int i = 0; i < All.Length; i++)
            {
                LocStringDef row = All[i];
                if (row == null || row.Key == null || !row.Key.StartsWith("data.", StringComparison.Ordinal)) continue;
                if (string.IsNullOrEmpty(row.English))
                    throw new InvalidOperationException("data loc key missing english " + row.Key);
                if (map.ContainsKey(row.English))
                    throw new InvalidOperationException("duplicate data english " + row.English);
                map.Add(row.English, row.Key);
            }
            return map;
        }

        static LocStringDef Def(string key, string english, string context)
        {
            return new LocStringDef(key, english, context, 0, false);
        }

        static LocStringDef Def(string key, string english, string context, int maxChars)
        {
            return new LocStringDef(key, english, context, maxChars, false);
        }
    }
}
