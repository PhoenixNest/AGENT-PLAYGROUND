using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class KeyAssetCheck
{
	private const string ScenePath = "Assets/Scenes/Main.unity";
	private const string PrefabFolder = "Assets/Resources/prefabs/minilevels/";
	private static readonly string[] PrefabNames = { "Level1", "Level24", "BossLevel1" };

	public static void Run()
	{
		var report = new StringBuilder();

		Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
		report.AppendLine("== Scene " + ScenePath + " (valid=" + scene.IsValid() + ", loaded=" + scene.isLoaded + ")");
		InspectRoots(scene.GetRootGameObjects(), report);

		foreach (string name in PrefabNames)
		{
			string path = PrefabFolder + name + ".prefab";
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			report.AppendLine();
			if (prefab == null)
			{
				report.AppendLine("== Prefab " + path + " NOT FOUND");
				continue;
			}
			report.AppendLine("== Prefab " + path);
			InspectRoots(new[] { prefab }, report);
		}

		Debug.Log(report.ToString());
	}

	private static void InspectRoots(GameObject[] roots, StringBuilder report)
	{
		var counts = new Dictionary<string, int>();
		var missing = new List<string>();
		var rotate = new List<string>();
		var gameManager = new List<string>();
		int objectCount = 0;

		foreach (GameObject root in roots)
		{
			report.AppendLine("  root: " + root.name + " (active=" + root.activeSelf + ")");
			Walk(root.transform, counts, missing, rotate, gameManager, ref objectCount);
		}

		report.AppendLine("  objects (recursive): " + objectCount);
		report.AppendLine("  component counts:");
		foreach (var kv in counts.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key))
		{
			report.AppendLine("    " + kv.Key + ": " + kv.Value);
		}
		report.AppendLine("  missing scripts: " + missing.Count);
		foreach (string line in missing.Take(50))
		{
			report.AppendLine("    " + line);
		}
		report.AppendLine("  RotateController: " + rotate.Count);
		foreach (string line in rotate)
		{
			report.AppendLine("    " + line);
		}
		report.AppendLine("  GameManager: " + gameManager.Count);
		foreach (string line in gameManager)
		{
			report.AppendLine("    " + line);
		}
	}

	private static void Walk(Transform node, Dictionary<string, int> counts, List<string> missing,
		List<string> rotate, List<string> gameManager, ref int objectCount)
	{
		objectCount++;
		int missingHere = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject);
		if (missingHere > 0)
		{
			missing.Add(ObjectPath(node) + " (" + missingHere + ")");
		}

		foreach (Component component in node.GetComponents<Component>())
		{
			if (component == null)
			{
				continue;
			}
			string type = component.GetType().Name;
			Increment(counts, type);

			var controller = component as RotateController;
			if (controller != null)
			{
				rotate.Add(ObjectPath(node) + " Multiplier=" + controller.Multiplier + " Damping=" + controller.Damping);
			}

			var manager = component as GameManager;
			if (manager != null)
			{
				gameManager.Add(ObjectPath(node) + " ControllerMultiplier=" + manager.ControllerMultiplier
					+ " ControllerDamping=" + manager.ControllerDamping);
			}
		}

		for (int i = 0; i < node.childCount; i++)
		{
			Walk(node.GetChild(i), counts, missing, rotate, gameManager, ref objectCount);
		}
	}

	private static string ObjectPath(Transform node)
	{
		string path = node.name;
		while (node.parent != null)
		{
			node = node.parent;
			path = node.name + "/" + path;
		}
		return path;
	}

	private static void Increment(Dictionary<string, int> map, string key)
	{
		int count;
		map.TryGetValue(key, out count);
		map[key] = count + 1;
	}
}
