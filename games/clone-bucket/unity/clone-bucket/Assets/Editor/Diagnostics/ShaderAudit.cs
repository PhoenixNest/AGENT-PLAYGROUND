using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class ShaderAudit
{
	private const string OutputEnvVar = "MM3_DIAG_OUT";
	private const string CsvName = "shader-audit.csv";

	public static void Run()
	{
		var shaders = new Dictionary<string, string>();
		foreach (string guid in AssetDatabase.FindAssets("t:Shader", new[] { "Assets" }))
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			shaders[path] = "shader-asset";
		}
		foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets" }))
		{
			string matPath = AssetDatabase.GUIDToAssetPath(guid);
			Material material = AssetDatabase.LoadAssetAtPath<Material>(matPath);
			if (material == null || material.shader == null)
			{
				Debug.LogWarning("[MM3:ShaderAudit] Material with no shader: " + matPath);
				continue;
			}
			string shaderPath = AssetDatabase.GetAssetPath(material.shader);
			if (string.IsNullOrEmpty(shaderPath) || !shaderPath.StartsWith("Assets/"))
			{
				shaderPath = "builtin:" + material.shader.name;
			}
			if (!shaders.ContainsKey(shaderPath))
			{
				shaders[shaderPath] = "used-by-material";
			}
		}

		foreach (string path in shaders.Keys.ToList())
		{
			if (path.StartsWith("Assets/"))
			{
				AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
			}
		}
		Shader.WarmupAllShaders();

		var rows = new List<string>();
		rows.Add("shaderName,assetPath,source,hasError,isSupported,errorCount,warningCount,messages");

		int withErrors = 0;
		int withWarnings = 0;
		foreach (var kv in shaders.OrderBy(kv => kv.Key))
		{
			Shader shader = ResolveShader(kv.Key);
			if (shader == null)
			{
				rows.Add(string.Join(",", Csv("(not loaded)"), Csv(kv.Key), kv.Value,
					true, false, 1, 0, Csv("could not load shader")));
				withErrors++;
				Debug.LogError("[MM3:ShaderAudit] Could not load shader " + kv.Key);
				continue;
			}

			ForceCompile(shader);

			bool hasError = ShaderUtil.ShaderHasError(shader);
			bool supported = shader.isSupported;
			ShaderMessage[] messages = ShaderUtil.GetShaderMessages(shader) ?? new ShaderMessage[0];

			int errors = 0;
			int warnings = 0;
			var texts = new List<string>();
			foreach (ShaderMessage msg in messages)
			{
				string severity = msg.severity.ToString();
				if (severity.IndexOf("Error", System.StringComparison.OrdinalIgnoreCase) >= 0) errors++;
				else warnings++;
				string where = msg.file + ":" + msg.line;
				texts.Add(severity + " " + where + " (" + msg.platform + ") " + msg.message);
			}

			if (hasError || errors > 0) withErrors++;
			if (warnings > 0) withWarnings++;

			rows.Add(string.Join(",", Csv(shader.name), Csv(kv.Key), kv.Value,
				hasError, supported, errors, warnings, Csv(string.Join(" | ", texts))));

			Debug.Log("[MM3:ShaderAudit] " + shader.name + " hasError=" + hasError + " supported=" + supported
				+ " errors=" + errors + " warnings=" + warnings + " (" + kv.Key + ")");
			foreach (string t in texts)
			{
				Debug.Log("[MM3:ShaderAudit]   " + shader.name + ": " + t);
			}
		}

		string dir = OutputDir();
		Directory.CreateDirectory(dir);
		string csvPath = Path.Combine(dir, CsvName);
		File.WriteAllLines(csvPath, rows, new UTF8Encoding(false));
		Debug.Log("[MM3:ShaderAudit] Audited " + shaders.Count + " shaders. With errors: " + withErrors
			+ ", with warnings: " + withWarnings + ". Wrote " + csvPath);
	}

	private static Shader ResolveShader(string key)
	{
		if (key.StartsWith("builtin:"))
		{
			return Shader.Find(key.Substring("builtin:".Length));
		}
		return AssetDatabase.LoadAssetAtPath<Shader>(key);
	}

	private static void ForceCompile(Shader shader)
	{
		var temp = new Material(shader);
		Object.DestroyImmediate(temp);
	}

	private static string OutputDir()
	{
		string env = System.Environment.GetEnvironmentVariable(OutputEnvVar);
		return string.IsNullOrEmpty(env) ? Path.Combine(Path.GetTempPath(), "mm3-diag") : env;
	}

	private static string Csv(string value)
	{
		return "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
	}
}
