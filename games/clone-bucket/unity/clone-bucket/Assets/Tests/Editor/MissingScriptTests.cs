using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissingScriptTests
{
	private const int MaxListed = 50;

	[Test]
	public void NoMissingScriptsInScenesPrefabsOrAssets()
	{
		var offenders = new List<string>();
		int sceneCount = 0;
		int prefabCount = 0;
		int assetCount = 0;

		EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

		foreach (string path in AssetPathsWithExtension(".unity"))
		{
			sceneCount++;
			Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
			foreach (GameObject root in scene.GetRootGameObjects())
			{
				CollectMissing(root.transform, path, offenders);
			}
			EditorSceneManager.CloseScene(scene, true);
		}

		foreach (string path in AssetPathsWithExtension(".prefab"))
		{
			prefabCount++;
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			if (prefab == null)
			{
				offenders.Add(path + " | could not be loaded");
				continue;
			}
			CollectMissing(prefab.transform, path, offenders);
		}

		foreach (string path in AssetPathsWithExtension(".asset"))
		{
			assetCount++;
			Object main = AssetDatabase.LoadMainAssetAtPath(path);
			if (main == null)
			{
				string classId = FirstClassId(path);
				if (classId == "114")
				{
					offenders.Add(path + " | MonoBehaviour could not be loaded (class 114)");
				}
				else
				{
					Debug.LogWarning("MissingScriptTests: could not load main asset " + path
						+ " (class " + (classId ?? "unknown") + "); not counted as a missing script.");
				}
				continue;
			}
			if (!(main is MonoBehaviour || main is ScriptableObject))
			{
				continue;
			}
			using (var serialized = new SerializedObject(main))
			{
				SerializedProperty script = serialized.FindProperty("m_Script");
				if (script != null && script.objectReferenceValue == null)
				{
					offenders.Add(path + " | " + main.name + " | script missing");
				}
			}
		}

		Debug.Log(string.Format("MissingScriptTests checked {0} scenes, {1} prefabs, {2} assets. Offenders: {3}",
			sceneCount, prefabCount, assetCount, offenders.Count));

		if (offenders.Count > 0)
		{
			var message = new StringBuilder();
			message.AppendFormat("{0} missing-script offender(s). Listing up to {1}:", offenders.Count, MaxListed);
			for (int i = 0; i < offenders.Count && i < MaxListed; i++)
			{
				message.AppendLine();
				message.Append("  ").Append(offenders[i]);
			}
			Assert.Fail(message.ToString());
		}
	}

	private static void CollectMissing(Transform node, string assetPath, List<string> offenders)
	{
		int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject);
		if (missing > 0)
		{
			offenders.Add(assetPath + " | " + ObjectPath(node) + " | " + missing + " missing");
		}
		for (int i = 0; i < node.childCount; i++)
		{
			CollectMissing(node.GetChild(i), assetPath, offenders);
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

	private static string FirstClassId(string assetPath)
	{
		string fullPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), assetPath);
		foreach (string line in File.ReadLines(fullPath))
		{
			if (line.StartsWith("--- !u!"))
			{
				int end = line.IndexOf(' ', 7);
				return end > 7 ? line.Substring(7, end - 7) : null;
			}
		}
		return null;
	}

	private static IEnumerable<string> AssetPathsWithExtension(string extension)
	{
		string dataPath = Application.dataPath.Replace('\\', '/');
		foreach (string full in Directory.GetFiles(Application.dataPath, "*" + extension, SearchOption.AllDirectories))
		{
			string normalized = full.Replace('\\', '/');
			yield return "Assets" + normalized.Substring(dataPath.Length);
		}
	}
}
