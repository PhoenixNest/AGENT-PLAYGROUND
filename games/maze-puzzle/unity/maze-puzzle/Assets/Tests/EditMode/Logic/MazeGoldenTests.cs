using System.Collections.Generic;
using NUnit.Framework;
using OrbitalSalvage.JsCompat;

namespace OrbitalSalvage.Logic.Tests
{
    public class MazeGoldenTests
    {
        private static JsonValue _doc;

        private static JsonValue Doc
        {
            get { return _doc ?? (_doc = Fixtures.Load("maze.json")); }
        }

        [Test]
        public void AllFiftyLevels_ExactJsonText()
        {
            JsonValue levels = Doc["levels"];
            Assert.AreEqual(50, levels.Count);
            for (int i = 0; i < levels.Count; i++)
            {
                int n = (int)levels[i]["n"].Number;
                string expected = levels[i]["text"].String;
                string actual = MazeJson.ToText(MazeLevelBuilder.BuildLevel(n));
                Assert.AreEqual(expected, actual, "level " + n);
            }
        }

        [Test]
        public void GetSectors_ExactJsonText()
        {
            JsonValue sectors = Doc["sectors"];
            List<SectorConfig> built = MazeLevelBuilder.GetSectors();
            Assert.AreEqual(sectors.Count, built.Count);
            for (int i = 0; i < built.Count; i++)
            {
                Assert.AreEqual(sectors[i].String, MazeJson.ToText(built[i]), "sector " + (i + 1));
            }
        }

        [Test]
        public void SectorWorlds_ExactJsonText()
        {
            JsonValue arr = JsonValue.NewArray();
            foreach (WorldInfo w in MazeLevelBuilder.GetSectorWorlds())
            {
                JsonValue o = JsonValue.NewObject();
                o.Set("id", w.Id);
                o.Set("worldIndex", w.WorldIndex);
                o.Set("title", w.Title);
                o.Set("tagline", w.Tagline);
                o.Set("icon", w.Icon);
                o.Set("color", w.Color);
                JsonValue range = JsonValue.NewArray();
                range.Add(JsonValue.FromNumber(w.LevelRangeStart));
                range.Add(JsonValue.FromNumber(w.LevelRangeEnd));
                o.Set("levelRange", range);
                arr.Add(o);
            }
            Assert.AreEqual(Doc["worlds"].String, JsonWriter.Stringify(arr));
        }

        private static DailyShiftInfo ReadInfo(JsonValue i)
        {
            var info = new DailyShiftInfo();
            if (i["id"].Kind == JsonKind.String) info.Id = i["id"].String;
            if (i["tier"].Kind == JsonKind.String) info.Tier = i["tier"].String;
            if (i["title"].Kind == JsonKind.String) info.Title = i["title"].String;
            if (i["quota"].Kind == JsonKind.Number) info.Quota = i["quota"].Number;
            if (i["rewardCredits"].Kind == JsonKind.Number) info.RewardCredits = i["rewardCredits"].Number;
            if (i["hazardModifier"].Kind == JsonKind.String) info.HazardModifier = i["hazardModifier"].String;
            if (i["seed"].Kind == JsonKind.String) info.Seed = Fixtures.D(i["seed"].String);
            return info;
        }

        [Test]
        public void DailyShifts_ExactJsonText()
        {
            JsonValue list = Doc["daily"];
            Assert.Greater(list.Count, 40);
            for (int i = 0; i < list.Count; i++)
            {
                JsonValue entry = list[i];
                DailyShiftInfo info = ReadInfo(entry["info"]);
                Assert.AreEqual(entry["text"].String, MazeJson.ToText(MazeLevelBuilder.BuildDailyShift(info)), "daily case " + i + " " + JsonWriter.Stringify(entry["info"]));
            }
            JsonValue noTitle = Doc["dailyNoTitle"];
            for (int i = 0; i < noTitle.Count; i++)
            {
                DailyShiftInfo info = ReadInfo(noTitle[i]["info"]);
                Assert.AreEqual(noTitle[i]["text"].String, MazeJson.ToText(MazeLevelBuilder.BuildDailyShift(info)));
            }
        }
    }
}
