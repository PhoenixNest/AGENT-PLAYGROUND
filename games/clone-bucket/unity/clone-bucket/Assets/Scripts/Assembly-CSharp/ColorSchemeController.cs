using System;
using System.Collections.Generic;
using System.Globalization;
using CrazyLabs.Extensions.Database;
using Levels;
using UnityEngine;
using Zenject;

public class ColorSchemeController : IColorSchemeController
{
	internal const string DefaultSchemeId = "Grey";

	private const string LevelsDataSetId = "LevelsData";

	private const string SchemePropertyFormat = "ColorScheme{0}";

	private ColorSchemesConfig _config;

	private readonly Dictionary<ColorSchemeItemType, string> _currentSchemeIds = new Dictionary<ColorSchemeItemType, string>();

	[Inject]
	private void Construct(ColorSchemesConfig config)
	{
		_config = config;
		LevelsController.OnLevelSpawned += ApplyCurrentLevelScheme;
	}

	public void ApplyScheme(string schemeId)
	{
		ColorSchemeConfig scheme = FindScheme(schemeId);
		if (scheme == null)
		{
			return;
		}

		ColorSchemeItemView[] views = FindViews();
		for (int i = 0; i < views.Length; i++)
		{
			ColorSchemeItemType type = views[i].ItemType;
			_currentSchemeIds[type] = schemeId;
			views[i].Show(RandomMaterial(scheme.GetMaterials(type)));
		}
	}

	public void ApplySchemeForType(string schemeId, ColorSchemeItemType itemType)
	{
		ColorSchemeConfig scheme = FindScheme(schemeId);
		if (scheme == null)
		{
			return;
		}

		RenderSettings.ambientMode = scheme.useSkyboxForAmbientLight ? UnityEngine.Rendering.AmbientMode.Skybox : UnityEngine.Rendering.AmbientMode.Flat;

		List<Material> materials = scheme.GetMaterials(itemType);
		_currentSchemeIds[itemType] = schemeId;

		ColorSchemeItemView[] views = FindViews();
		for (int i = 0; i < views.Length; i++)
		{
			if (views[i].ItemType == itemType)
			{
				views[i].Show(RandomMaterial(materials));
			}
		}
	}

	public Material GetItemMaterial(ColorSchemeItemType itemType)
	{
		string schemeId;
		if (!_currentSchemeIds.TryGetValue(itemType, out schemeId))
		{
			return null;
		}

		ColorSchemeConfig scheme = _config != null ? _config.GetScheme(schemeId) : null;
		return scheme != null ? RandomMaterial(scheme.GetMaterials(itemType)) : null;
	}

	private void ApplyCurrentLevelScheme()
	{
		_currentSchemeIds.Clear();

		int total = LevelsController.TotalLevelCount;
		int last = LevelsController.LastLevelIndex;
		int row = total > 0 ? ((last - 1) % total) + 1 : last;
		string rowId = row.ToString(CultureInfo.InvariantCulture);

		foreach (ColorSchemeItemType type in Enum.GetValues(typeof(ColorSchemeItemType)))
		{
			string property = string.Format(SchemePropertyFormat, type);
			string schemeId = DatabaseController.GetString(LevelsDataSetId, rowId, property);
			ApplySchemeForType(schemeId, type);
		}
	}

	private ColorSchemeConfig FindScheme(string schemeId)
	{
		ColorSchemeConfig scheme = _config != null ? _config.GetScheme(schemeId) : null;
		if (scheme == null)
		{
			Debug.LogWarning($"[MM3:Colour] Unknown colour scheme '{schemeId}'.");
		}

		return scheme;
	}

	private static Material RandomMaterial(List<Material> materials)
	{
		if (materials == null || materials.Count == 0)
		{
			return null;
		}

		return materials[UnityEngine.Random.Range(0, materials.Count)];
	}

	private static ColorSchemeItemView[] FindViews()
	{
		return UnityEngine.Object.FindObjectsByType<ColorSchemeItemView>(FindObjectsInactive.Include);
	}
}
