using System.Collections.Generic;
using System.IO;
using System.Linq;
using CrazyLabs.Extensions.Database;
using Game.Levels;
using Levels;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class WP1_LevelsTests
{
	private const string TablePath = "TextAsset/LevelsData.txt";

	private static readonly int[] ExpectedOnScreenRows =
	{
		1, 2, 3, 4, 6, 7, 9, 10, 11, 13, 14, 15, 16, 17, 18, 20, 22, 23, 24, 25,
		26, 27, 28, 29, 30, 31, 32, 36, 37, 38, 40, 41, 42, 44, 47, 49, 50, 51, 52, 53,
		54, 58, 59, 60, 61, 62, 63, 64, 65, 66
	};

	private static string ReadTable()
	{
		return File.ReadAllText(Path.Combine(Application.dataPath, TablePath));
	}

	private static (List<string> header, List<string> ids, Dictionary<string, Dictionary<string, string>> rows) Table()
	{
		return DataSetConfig.ParseTable(ReadTable(), ',');
	}

	private static List<string> LiveIds((List<string> header, List<string> ids, Dictionary<string, Dictionary<string, string>> rows) table)
	{
		return LevelsController.FilterLiveItemIds(table.ids, id => table.rows[id]["Level Name"]);
	}

	[Test]
	public void Table_Has_107_Rows_And_20_Columns()
	{
		var table = Table();
		Assert.AreEqual(20, table.header.Count, "column count");
		Assert.AreEqual(107, table.ids.Count, "row count");
		Assert.AreEqual(107, table.rows.Count, "distinct row ids");
		foreach (string id in table.ids)
		{
			Assert.AreEqual(19, table.rows[id].Count, "properties of row " + id);
		}
	}

	[Test]
	public void Split_Fields_Respects_Quotes()
	{
		var fields = DataSetConfig.SplitFields("a,\"b,c\",d,\"say \"\"hi\"\"\"", ',');
		CollectionAssert.AreEqual(new[] { "a", "b,c", "d", "say \"hi\"" }, fields);
	}

	[Test]
	public void Live_List_Has_71_Rows()
	{
		Assert.AreEqual(71, LiveIds(Table()).Count);
	}

	[Test]
	public void Live_List_Excludes_2_8_Rows_And_LevelSquareRein1()
	{
		var table = Table();
		List<string> live = LiveIds(table);
		foreach (string id in live)
		{
			string name = table.rows[id]["Level Name"];
			Assert.IsFalse(name.Contains("2.8"), "2.8 row is live: " + name);
			Assert.AreNotEqual("LevelSquareRein1", name);
		}

		Assert.IsFalse(LevelsController.IsLiveLevelName("StartLevel4_2.8"));
		Assert.IsFalse(LevelsController.IsLiveLevelName("LevelSquareRein1"));
		Assert.IsTrue(LevelsController.IsLiveLevelName("Level1"));
	}

	[Test]
	public void On_Screen_N_Is_Live_Row_N_For_1_To_50()
	{
		var table = Table();
		List<string> live = LiveIds(table);
		for (int n = 1; n <= ExpectedOnScreenRows.Length; n++)
		{
			MiniLevelData mini = LevelsController.ParseMiniLevelData(table.rows[live[n - 1]]);
			Assert.AreEqual(ExpectedOnScreenRows[n - 1], mini.missionIndex, "on-screen " + n + " (" + mini.levelName + ")");
		}
	}

	[Test]
	public void Boss_Rows_Are_14_27_42_60_74_99()
	{
		var table = Table();
		List<string> bossRows = table.ids
			.Where(id => table.rows[id]["Is Boss Level"] == "TRUE")
			.ToList();
		CollectionAssert.AreEquivalent(new[] { "14", "27", "42", "60", "74", "99" }, bossRows);
	}

	[Test]
	public void Boss_On_Screen_Levels_Are_11_22_33_44_55_66()
	{
		var table = Table();
		List<string> live = LiveIds(table);
		var bossOnScreen = new List<int>();
		for (int n = 1; n <= live.Count; n++)
		{
			if (LevelsController.ParseMiniLevelData(table.rows[live[n - 1]]).isBossLevel)
			{
				bossOnScreen.Add(n);
			}
		}

		CollectionAssert.AreEqual(new[] { 11, 22, 33, 44, 55, 66 }, bossOnScreen);
	}

	[Test]
	public void Every_Live_Level_Has_A_Prefab()
	{
		var table = Table();
		List<string> missing = new List<string>();
		foreach (string id in LiveIds(table))
		{
			string name = table.rows[id]["Level Name"];
			string path = Path.Combine(Application.dataPath, "Resources/prefabs/minilevels/" + name + ".prefab");
			if (!File.Exists(path))
			{
				missing.Add(name);
			}
		}

		CollectionAssert.IsEmpty(missing);
	}

	[Test]
	public void Gate_Types_And_Values_Have_Equal_Length_For_Every_Row()
	{
		var table = Table();
		foreach (string id in table.ids)
		{
			MiniLevelData mini = LevelsController.ParseMiniLevelData(table.rows[id]);
			Assert.AreEqual(mini.gateTypes.Count, mini.gateValues.Count, "row " + id);
			Assert.Greater(mini.gateTypes.Count, 0, "row " + id + " has no gates");
		}
	}

	[Test]
	public void First_Row_Parses_To_Recorded_Values()
	{
		var table = Table();
		MiniLevelData mini = LevelsController.ParseMiniLevelData(table.rows["1"]);
		Assert.AreEqual("StartLevel1", mini.levelName);
		Assert.AreEqual(10, mini.startBallAmount);
		Assert.AreEqual(60, mini.targetBallAmount);
		Assert.AreEqual(110, mini.perfectBallAmount);
		Assert.AreEqual(LevelType.Prefab, mini.levelType);
		CollectionAssert.AreEqual(new[] { GateType.Multiply, GateType.Multiply }, mini.gateTypes);
		CollectionAssert.AreEqual(new[] { 11, 6 }, mini.gateValues);
	}

	[Test]
	public void Looping_Flag_Is_Read_Per_Row()
	{
		var table = Table();
		Assert.IsFalse(LevelsController.ParseLevelData(table.rows["1"]).isLooping);
		Assert.IsTrue(LevelsController.ParseLevelData(table.rows["7"]).isLooping);
	}

	[Test]
	public void Live_Rows_Have_Perfect_Above_Target()
	{
		var table = Table();
		foreach (string id in LiveIds(table))
		{
			MiniLevelData mini = LevelsController.ParseMiniLevelData(table.rows[id]);
			Assert.Greater(mini.targetBallAmount, 0, mini.levelName);
			Assert.Greater(mini.perfectBallAmount, mini.targetBallAmount, mini.levelName);
		}
	}

	[Test]
	public void Stars_Use_Recovered_Thresholds_And_Write_Nothing_Below_Target()
	{
		const int target = 60;
		const int perfect = 110;
		Assert.AreEqual(-1, LevelsController.CalculateStars(0, target, perfect));
		Assert.AreEqual(-1, LevelsController.CalculateStars(59, target, perfect));
		Assert.AreEqual(1, LevelsController.CalculateStars(60, target, perfect));
		Assert.AreEqual(1, LevelsController.CalculateStars(84, target, perfect));
		Assert.AreEqual(2, LevelsController.CalculateStars(85, target, perfect));
		Assert.AreEqual(2, LevelsController.CalculateStars(109, target, perfect));
		Assert.AreEqual(3, LevelsController.CalculateStars(110, target, perfect));
	}

	[Test]
	public void Stars_Round_Half_Even_On_Odd_Gap()
	{
		Assert.AreEqual(1, LevelsController.CalculateStars(61, 60, 65));
		Assert.AreEqual(2, LevelsController.CalculateStars(62, 60, 65));
		Assert.AreEqual(2, LevelsController.CalculateStars(64, 60, 65));
		Assert.AreEqual(3, LevelsController.CalculateStars(65, 60, 65));
	}

	[Test]
	public void Single_Level_Stars_Keep_Rebuilt_Rule()
	{
		Assert.AreEqual(0, LevelsController.CalculateSingleLevelStars(59, 60, 110));
		Assert.AreEqual(1, LevelsController.CalculateSingleLevelStars(60, 60, 110));
		Assert.AreEqual(2, LevelsController.CalculateSingleLevelStars(110, 60, 110));
	}

	[Test]
	public void GateValues_Pair_By_Index_And_Stop_At_The_Shorter_List()
	{
		var plan = LevelsController.PlanGateValues(4, new System.Collections.Generic.List<int> { 6, 11 });
		Assert.AreEqual(2, plan.Count);
		Assert.AreEqual((0, 6), plan[0]);
		Assert.AreEqual((1, 11), plan[1]);

		var longer = LevelsController.PlanGateValues(1, new System.Collections.Generic.List<int> { 5, 10, 15 });
		Assert.AreEqual(1, longer.Count);
		Assert.AreEqual((0, 5), longer[0]);

		Assert.AreEqual(0, LevelsController.PlanGateValues(3, null).Count);
	}
}
