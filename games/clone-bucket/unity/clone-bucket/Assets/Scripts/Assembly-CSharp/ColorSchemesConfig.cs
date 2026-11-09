using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Configs/ColorSchemesConfig", fileName = "ColorSchemesConfig")]
public class ColorSchemesConfig : ScriptableObject
{
	[Header("Schemes")]
	public List<ColorSchemeConfig> schemes;

	public ColorSchemeConfig GetScheme(string schemeId)
	{
		if (schemes == null || schemeId == null)
		{
			return null;
		}

		for (int i = 0; i < schemes.Count; i++)
		{
			ColorSchemeConfig scheme = schemes[i];
			if (scheme != null && string.Equals(scheme.id, schemeId, StringComparison.Ordinal))
			{
				return scheme;
			}
		}

		return null;
	}
}
