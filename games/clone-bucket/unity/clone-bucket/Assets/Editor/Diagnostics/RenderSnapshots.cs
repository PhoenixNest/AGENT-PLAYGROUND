using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RenderSnapshots
{
	private const string OutputEnvVar = "MM3_DIAG_OUT";
	private const string ScenePath = "Assets/Scenes/Main.unity";
	private const string SchemesConfigPath = "Assets/MonoBehaviour/ColorSchemesConfig.asset";
	private const string PrefabFolder = "prefabs/minilevels/";
	private const int Width = 1080;
	private const int Height = 1920;

	private static readonly Vector3 LevelCameraPosition = new Vector3(0f, -0.4f, -16.93f);
	private static readonly Vector3 LevelCameraEuler = new Vector3(3f, 0f, 0f);
	private const float LevelCameraFov = 50f;

	public static void RenderAll()
	{
		try
		{
			RenderAllCore();
		}
		finally
		{
			RestoreSharedColors();
		}
	}

	private static void RenderAllCore()
	{
		string dir = OutputDir();
		Directory.CreateDirectory(dir);

		OpenMainScene();
		Camera mainCam = FindGameCamera();
		if (mainCam == null)
		{
			Debug.LogError("[MM3:RenderSnapshots] No camera found in " + ScenePath);
		}
		else
		{
			mainCam.aspect = (float)Width / Height;
			ApplyGreyScheme();
			RenderToPng(mainCam, Path.Combine(dir, "main_grey.png"));
		}

		CaptureLevel("StartLevel1", Path.Combine(dir, "level01_grey.png"));
		CaptureLevel("BossLevel1", Path.Combine(dir, "level11_grey.png"));
		CaptureLevel("Level7", Path.Combine(dir, "level24_grey.png"));

		Debug.Log("[MM3:RenderSnapshots] Done. Output: " + dir);
	}

	private static void CaptureLevel(string prefabName, string outPath)
	{
		OpenMainScene();

		GameObject prefab = Resources.Load<GameObject>(PrefabFolder + prefabName);
		if (prefab == null)
		{
			Debug.LogError("[MM3:RenderSnapshots] Missing prefab Resources/" + PrefabFolder + prefabName);
			return;
		}

		GameObject root = new GameObject("[diag] level root");
		root.transform.position = Vector3.zero;
		GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
		instance.transform.SetParent(root.transform, false);
		instance.transform.localPosition = Vector3.zero;

		Camera cam = FindGameCamera();
		if (cam == null)
		{
			Debug.LogError("[MM3:RenderSnapshots] No camera found for " + prefabName);
			return;
		}
		cam.transform.SetParent(null, true);
		cam.transform.position = LevelCameraPosition;
		cam.transform.rotation = Quaternion.Euler(LevelCameraEuler);
		cam.fieldOfView = LevelCameraFov;
		cam.aspect = (float)Width / Height;
		ApplyGreyScheme();

		RenderToPng(cam, outPath);
		Debug.Log("[MM3:RenderSnapshots] " + prefabName + " -> " + outPath);
	}

	private static void ApplyGameTheme()
	{
		MaterialManager manager = null;
		foreach (MaterialManager m in Object.FindObjectsByType<MaterialManager>(FindObjectsInactive.Include))
		{
			manager = m;
			break;
		}
		if (manager == null)
		{
			Debug.LogWarning("[MM3:RenderSnapshots] No MaterialManager in " + ScenePath + "; theme not applied.");
			return;
		}

		manager.SetInstance();
		SwapForCopy(manager, 0, manager.MazeCapSharedMaterial);
		SwapForCopy(manager, 1, manager.MazePartFrontSharedMaterial);
		SwapForCopy(manager, 2, manager.MazePartSideSharedMaterial);
		Quaternion mainRot = manager.MainDirLight != null ? manager.MainDirLight.localRotation : Quaternion.identity;
		Quaternion ballsRot = manager.BallsDirLight != null ? manager.BallsDirLight.localRotation : Quaternion.identity;
		Color before = SwapsColor(Swaps.Count > 0 ? Swaps[0].Original : null);
		manager.UpdateTheme();
		Color after = SwapsColor(Swaps.Count > 0 ? Swaps[0].Original : null);
		Debug.Log("[MM3:RenderSnapshots] asset " + (Swaps.Count > 0 ? Swaps[0].Original.name : "none")
			+ " _Color before=" + before + " after=" + after
			+ " field=" + (manager.MazeCapSharedMaterial != null ? manager.MazeCapSharedMaterial.name : "null"));
		if (System.Environment.GetEnvironmentVariable("MM3_LIGHT_AUTHORED") == "1")
		{
			if (manager.MainDirLight != null) manager.MainDirLight.localRotation = mainRot;
			if (manager.BallsDirLight != null) manager.BallsDirLight.localRotation = ballsRot;
		}

		foreach (Multiplicator gate in Object.FindObjectsByType<Multiplicator>(FindObjectsInactive.Include))
		{
			int value = gate.Value;
			if (value <= 0 || gate.visualMeshRenderer == null)
			{
				continue;
			}
			Material material = manager.GetMultiplicatorMaterial(value - 1);
			if (material != null)
			{
				gate.visualMeshRenderer.sharedMaterial = material;
			}
		}

		foreach (DivideGate gate in Object.FindObjectsByType<DivideGate>(FindObjectsInactive.Include))
		{
			if (gate.VisualMeshRenderer == null)
			{
				continue;
			}
			Material material = manager.GetMultiplicatorMaterial(-1);
			if (material != null)
			{
				gate.VisualMeshRenderer.sharedMaterial = material;
			}
		}
	}

	private struct SwapRecord
	{
		public MaterialManager Manager;
		public int Slot;
		public Material Original;
		public Material Copy;
	}

	private static readonly List<SwapRecord> Swaps = new List<SwapRecord>();

	private static void SwapForCopy(MaterialManager manager, int slot, Material original)
	{
		if (original == null)
		{
			return;
		}
		Material copy = new Material(original);
		copy.name = original.name + " (render copy)";
		Swaps.Add(new SwapRecord { Manager = manager, Slot = slot, Original = original, Copy = copy });

		foreach (MeshRenderer r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include))
		{
			Material[] shared = r.sharedMaterials;
			bool changed = false;
			for (int i = 0; i < shared.Length; i++)
			{
				if (shared[i] == original)
				{
					shared[i] = copy;
					changed = true;
				}
			}
			if (changed)
			{
				r.sharedMaterials = shared;
			}
		}

		switch (slot)
		{
			case 0: manager.MazeCapSharedMaterial = copy; break;
			case 1: manager.MazePartFrontSharedMaterial = copy; break;
			case 2: manager.MazePartSideSharedMaterial = copy; break;
		}
	}

	private static Color SwapsColor(Material m)
	{
		return m != null && m.HasProperty("_Color") ? m.GetColor("_Color") : Color.clear;
	}

	private static void RestoreSharedColors()
	{
		foreach (SwapRecord s in Swaps)
		{
			switch (s.Slot)
			{
				case 0: s.Manager.MazeCapSharedMaterial = s.Original; break;
				case 1: s.Manager.MazePartFrontSharedMaterial = s.Original; break;
				case 2: s.Manager.MazePartSideSharedMaterial = s.Original; break;
			}
			if (s.Copy != null)
			{
				Object.DestroyImmediate(s.Copy);
			}
		}
		Swaps.Clear();
	}

	private static void ApplyGreyScheme()
	{
		ApplyGameTheme();
		var config = AssetDatabase.LoadAssetAtPath<ColorSchemesConfig>(SchemesConfigPath);
		if (config == null)
		{
			Debug.LogError("[MM3:RenderSnapshots] Missing " + SchemesConfigPath);
			return;
		}

		var controller = new ColorSchemeController();
		var construct = typeof(ColorSchemeController).GetMethod("Construct",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		if (construct == null)
		{
			Debug.LogError("[MM3:RenderSnapshots] ColorSchemeController.Construct not found");
			return;
		}
		construct.Invoke(controller, new object[] { config });
		controller.ApplyScheme(ColorSchemeController.DefaultSchemeId);

		if (System.Environment.GetEnvironmentVariable("MM3_AMBIENT") != "skybox")
		{
			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
			var probe = new UnityEngine.Rendering.SphericalHarmonicsL2();
			probe.AddAmbientLight(RenderSettings.ambientSkyColor);
			RenderSettings.ambientProbe = probe;
		}
		DynamicGI.UpdateEnvironment();
		Debug.Log("[MM3:RenderSnapshots] ambientMode=" + RenderSettings.ambientMode
			+ " skybox=" + (RenderSettings.skybox != null ? RenderSettings.skybox.name : "none")
			+ " ambientSky=" + RenderSettings.ambientSkyColor);
	}

	private static void OpenMainScene()
	{
		EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
	}

	private static Camera FindGameCamera()
	{
		Camera tagged = null;
		foreach (Camera c in Object.FindObjectsByType<Camera>())
		{
			if (c.CompareTag("MainCamera")) { tagged = c; break; }
			if (tagged == null && c.name == "Main Camera") tagged = c;
		}
		return tagged;
	}

	private static void RenderToPng(Camera cam, string path)
	{
		RenderTexture previousTarget = cam.targetTexture;
		RenderTexture previousActive = RenderTexture.active;
		var rt = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
		var tex = new Texture2D(Width, Height, TextureFormat.RGB24, false);
		try
		{
			cam.targetTexture = rt;
			cam.Render();
			RenderTexture.active = rt;
			tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
			tex.Apply();
			File.WriteAllBytes(path, tex.EncodeToPNG());
		}
		finally
		{
			cam.targetTexture = previousTarget;
			RenderTexture.active = previousActive;
			Object.DestroyImmediate(rt);
			Object.DestroyImmediate(tex);
		}
	}

	private static string OutputDir()
	{
		string env = System.Environment.GetEnvironmentVariable(OutputEnvVar);
		return string.IsNullOrEmpty(env) ? Path.Combine(Path.GetTempPath(), "mm3-diag") : env;
	}
}
