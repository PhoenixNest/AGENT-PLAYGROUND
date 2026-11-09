using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class MaterialManager : SingletonBehaviour<MaterialManager>
{
	public List<MultiplicatorMaterial> MultipricatorMaterials;

	public List<MultiplicatorMaterial> MultipricatorMaterialsCrazyLabs;

	public Material BallDivideMaterial;

	public Material MazeCapSharedMaterial;

	public Material MazePartFrontSharedMaterial;

	public Material MazePartSideSharedMaterial;

	public MeshRenderer LeftFogMeshRenderer;

	public MeshRenderer RightFogMeshRenderer;

	public MeshRenderer BottomFogMeshRenderer;

	public MeshRenderer SkyBoxMeshRenderer;

	public List<GameTheme> CrazyLabsThemes;

	public GameTheme OriginalTheme;

	public List<Material> OriginalBallMaterials;

	public List<Material> CrazyLabsBallMaterials;

	public Transform MainDirLight;

	public Transform BallsDirLight;

	public Material WhiteBallMaterial;

	[HideInInspector]
	public GameTheme CurrentGameTheme;

	private bool isCrazyLabsThemes;

	private static readonly int BaseColorId = Shader.PropertyToID("_Color");

	private static readonly int HighlightColorId = Shader.PropertyToID("_HColor");

	private static readonly int ShadowColorId = Shader.PropertyToID("_SColor");

	protected override void Awake()
	{
		base.Awake();
		SetInstance();

#if UNITY_EDITOR
		EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
		UpdateTheme();
	}

#if UNITY_EDITOR
	private static readonly Dictionary<Material, (Color color, Color highlight, Color shadow)> EditorSavedColors =
		new Dictionary<Material, (Color color, Color highlight, Color shadow)>();

	private void OnPlayModeStateChanged(PlayModeStateChange state)
	{
		if (state == PlayModeStateChange.ExitingPlayMode)
		{
			RestoreEditorMaterialColors();
		}
	}

	private static void SaveEditorMaterialColors(Material material)
	{
		if (material == null || EditorSavedColors.ContainsKey(material))
		{
			return;
		}

		EditorSavedColors[material] = (
			material.HasProperty(BaseColorId) ? material.GetColor(BaseColorId) : Color.clear,
			material.HasProperty(HighlightColorId) ? material.GetColor(HighlightColorId) : Color.clear,
			material.HasProperty(ShadowColorId) ? material.GetColor(ShadowColorId) : Color.clear);
	}

	private static void RestoreEditorMaterialColors()
	{
		foreach (KeyValuePair<Material, (Color color, Color highlight, Color shadow)> pair in EditorSavedColors)
		{
			Material material = pair.Key;
			if (material == null)
			{
				continue;
			}

			if (material.HasProperty(BaseColorId))
			{
				material.SetColor(BaseColorId, pair.Value.color);
			}

			if (material.HasProperty(HighlightColorId))
			{
				material.SetColor(HighlightColorId, pair.Value.highlight);
			}

			if (material.HasProperty(ShadowColorId))
			{
				material.SetColor(ShadowColorId, pair.Value.shadow);
			}
		}

		EditorSavedColors.Clear();
	}
#endif

	private void OnDestroy()
	{
#if UNITY_EDITOR
		EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		RestoreEditorMaterialColors();
#endif
	}

	public void SetInstance()
	{
		instance = this;
	}

	public Material GetMultiplicatorMaterial(int value)
	{
		List<MultiplicatorMaterial> list = isCrazyLabsThemes ? MultipricatorMaterialsCrazyLabs : MultipricatorMaterials;
		if (list == null)
		{
			return null;
		}

		for (int i = 0; i < list.Count; i++)
		{
			MultiplicatorMaterial entry = list[i];
			if (entry != null && entry.Value == value)
			{
				return entry.material;
			}
		}

		return null;
	}

	public Material GetBallMaterial()
	{
		SkinBallItem skin = GameManager.instance != null ? GameManager.instance.CurrentBallSkin : null;
		if (skin == null && SkinManager.instance != null)
		{
			skin = SkinManager.instance.GetDefaultBall();
		}

		if (skin != null && skin.materials != null && skin.materials.Count > 0)
		{
			return skin.materials[UnityEngine.Random.Range(0, skin.materials.Count)];
		}

		return OriginalBallMaterials != null && OriginalBallMaterials.Count > 0 ? OriginalBallMaterials[0] : null;
	}

	public void UpdateTheme()
	{
		isCrazyLabsThemes = ABTestVariables.instance != null && ABTestVariables.instance.IsCrazyLabsThemes();

		GameTheme theme = OriginalTheme;
		if (isCrazyLabsThemes && CrazyLabsThemes != null && CrazyLabsThemes.Count > 0)
		{
			int level = GameManager.instance != null ? GameManager.instance.CurrentLevel : 0;
			theme = CrazyLabsThemes[(level / 10) % CrazyLabsThemes.Count];
		}

		if (theme == CurrentGameTheme)
		{
			return;
		}

		CurrentGameTheme = theme;
		if (theme == null)
		{
			return;
		}

		SetMaterialColors(MazeCapSharedMaterial, theme.MazeCapColors);
		if (SkyBoxMeshRenderer != null)
		{
			SkyBoxMeshRenderer.sharedMaterial = theme.SkyBoxMaterial;
		}

		if (LeftFogMeshRenderer != null)
		{
			LeftFogMeshRenderer.sharedMaterial = theme.SideFogMaterial;
		}

		if (RightFogMeshRenderer != null)
		{
			RightFogMeshRenderer.sharedMaterial = theme.SideFogMaterial;
		}

		if (BottomFogMeshRenderer != null)
		{
			BottomFogMeshRenderer.sharedMaterial = theme.BottomFogMaterial;
		}

		if (theme.SkyBoxMaterial != null)
		{
			RenderSettings.skybox = theme.SkyBoxMaterial;
		}

		if (MainDirLight != null)
		{
			MainDirLight.rotation = theme.MainDirLightRotation;
		}

		if (BallsDirLight != null)
		{
			BallsDirLight.rotation = theme.BallsDirLightRotation;
		}

		SetMaterialColors(MazePartFrontSharedMaterial, theme.MazePartFrontColors);
		SetMaterialColors(MazePartSideSharedMaterial, theme.MazePartSideColors);
	}

	private void SetMaterialColors(Material mazeMaterial, MazeMaterialColors mazeMaterialColors)
	{
		if (mazeMaterial == null || mazeMaterialColors == null)
		{
			return;
		}

#if UNITY_EDITOR
		SaveEditorMaterialColors(mazeMaterial);
#endif
		SetColorIfChanged(mazeMaterial, BaseColorId, mazeMaterialColors.Color);
		SetColorIfChanged(mazeMaterial, HighlightColorId, mazeMaterialColors.HighlightColor);
		SetColorIfChanged(mazeMaterial, ShadowColorId, mazeMaterialColors.ShadowColor);
	}

	private static void SetColorIfChanged(Material material, int propertyId, Color value)
	{
		if (!material.HasProperty(propertyId) || material.GetColor(propertyId) == value)
		{
			return;
		}

		material.SetColor(propertyId, value);
	}
}
