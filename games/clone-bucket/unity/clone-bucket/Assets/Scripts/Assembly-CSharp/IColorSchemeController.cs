using UnityEngine;

public interface IColorSchemeController
{
	void ApplyScheme(string schemeId);

	void ApplySchemeForType(string schemeId, ColorSchemeItemType itemType);

	Material GetItemMaterial(ColorSchemeItemType itemType);
}
