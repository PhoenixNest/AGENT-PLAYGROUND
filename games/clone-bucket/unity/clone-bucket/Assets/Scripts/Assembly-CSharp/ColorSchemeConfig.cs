using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Configs/ColorSchemeConfig", fileName = "ColorSchemeConfig_")]
public class ColorSchemeConfig : ScriptableObject
{
	[Header("Identity")]
	public string id;

	[Header("Materials")]
	public List<Material> ballsMaterials;

	public List<Material> mazeWallsMaterials;

	public List<Material> mazeBackgroundMaterials;

	public List<Material> stageBackgroundMaterials;

	public List<Material> bucketMaterials;

	public List<Material> bucketGuidesMaterials;

	public List<Material> bucketStandMaterials;

	[Header("Lightning")]
	public bool useSkyboxForAmbientLight;

	public List<Material> GetMaterials(ColorSchemeItemType itemType)
	{
		switch (itemType)
		{
			case ColorSchemeItemType.Balls:
				return ballsMaterials;
			case ColorSchemeItemType.MazeWalls:
				return mazeWallsMaterials;
			case ColorSchemeItemType.MazeBackground:
				return mazeBackgroundMaterials;
			case ColorSchemeItemType.StageBackground:
				return stageBackgroundMaterials;
			case ColorSchemeItemType.Bucket:
				return bucketMaterials;
			case ColorSchemeItemType.BucketGuides:
				return bucketGuidesMaterials;
			case ColorSchemeItemType.BucketStand:
				return bucketStandMaterials;
			default:
				return null;
		}
	}
}
