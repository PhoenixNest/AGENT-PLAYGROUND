using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class MaterialAudit
{
	public const string CsvPath = @"<local-temp>\p2\materials.csv";

	private static readonly Regex ShaderRef = new Regex(
		@"m_Shader:\s*\{fileID:\s*(-?\d+),\s*guid:\s*([0-9a-f]{32})", RegexOptions.Compiled);
	private static readonly Regex PropertyName = new Regex(
		@"^\s+- (_[A-Za-z0-9_]+):", RegexOptions.Compiled | RegexOptions.Multiline);

	public static void Audit()
	{
		var rows = new List<string>();
		rows.Add("materialPath,shaderGuid,shaderRef,shaderName,isSupported,isErrorShader,isPlaceholderShader,propertyNames");

		var perShader = new Dictionary<string, int>();
		var perRef = new Dictionary<string, int>();

		foreach (string materialPath in AllMaterialPaths())
		{
			string yaml = File.ReadAllText(materialPath);
			Match m = ShaderRef.Match(yaml);
			string guid = m.Success ? m.Groups[2].Value : "";
			string projectPath = guid.Length == 32 ? AssetDatabase.GUIDToAssetPath(guid) : "";
			string shaderRef;
			if (guid.Length == 0 || guid == "00000000000000000000000000000000")
			{
				shaderRef = "none";
			}
			else if (!string.IsNullOrEmpty(projectPath))
			{
				shaderRef = projectPath.StartsWith("Assets/") ? "project" : "package";
			}
			else if (IsBuiltinGuid(guid))
			{
				shaderRef = "builtin";
			}
			else
			{
				shaderRef = "missing";
			}

			Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
			Shader shader = material != null ? material.shader : null;
			string shaderName = shader != null ? shader.name : "(null)";
			bool supported = shader != null && shader.isSupported;
			bool errorShader = shaderName == "Hidden/InternalErrorShader" || shader == null;
			bool placeholder = false;
			if (!string.IsNullOrEmpty(projectPath) && projectPath.EndsWith(".shader"))
			{
				placeholder = File.ReadAllText(projectPath).Contains("//DummyShaderTextExporter");
			}

			string props = string.Join(";", PropertyNames(yaml).Take(12));
			rows.Add(string.Join(",", Csv(materialPath), guid, shaderRef, Csv(shaderName),
				supported, errorShader, placeholder, Csv(props)));

			Increment(perShader, shaderName + (placeholder ? " [placeholder]" : ""));
			Increment(perRef, shaderRef);
		}

		Directory.CreateDirectory(Path.GetDirectoryName(CsvPath));
		File.WriteAllLines(CsvPath, rows, new UTF8Encoding(false));

		Debug.Log("MaterialAudit wrote " + (rows.Count - 1) + " materials to " + CsvPath);
		foreach (var kv in perRef.OrderByDescending(kv => kv.Value))
		{
			Debug.Log("MaterialAudit shaderRef " + kv.Key + ": " + kv.Value);
		}
		foreach (var kv in perShader.OrderByDescending(kv => kv.Value))
		{
			Debug.Log("MaterialAudit shader " + kv.Key + ": " + kv.Value);
		}
	}

	private static bool IsBuiltinGuid(string guid)
	{
		return guid.StartsWith("0000000000000000f") || guid.StartsWith("0000000000000000e");
	}

	private static IEnumerable<string> PropertyNames(string yaml)
	{
		foreach (Match m in PropertyName.Matches(yaml))
		{
			yield return m.Groups[1].Value;
		}
	}

	private static IEnumerable<string> AllMaterialPaths()
	{
		string dataPath = Application.dataPath.Replace('\\', '/');
		foreach (string full in Directory.GetFiles(Application.dataPath, "*.mat", SearchOption.AllDirectories))
		{
			yield return "Assets" + full.Replace('\\', '/').Substring(dataPath.Length);
		}
	}

	private static string Csv(string value)
	{
		return "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
	}

	private static void Increment(Dictionary<string, int> map, string key)
	{
		int count;
		map.TryGetValue(key, out count);
		map[key] = count + 1;
	}
}
