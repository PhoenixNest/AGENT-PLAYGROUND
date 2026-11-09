using System;
using UnityEngine;

[Serializable]
public class GameTheme
{
	public MazeMaterialColors MazeCapColors;

	public MazeMaterialColors MazePartFrontColors;

	public MazeMaterialColors MazePartSideColors;

	public Material SideFogMaterial;

	public Material BottomFogMaterial;

	public Material SkyBoxMaterial;

	public Material[] StandMaterials;

	public Quaternion MainDirLightRotation;

	public Quaternion BallsDirLightRotation;
}
