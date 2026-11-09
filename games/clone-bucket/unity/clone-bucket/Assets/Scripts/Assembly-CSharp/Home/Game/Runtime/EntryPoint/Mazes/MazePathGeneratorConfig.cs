using System.Collections.Generic;
using UnityEngine;

namespace Home.Game.Runtime.EntryPoint.Mazes
{
	[CreateAssetMenu(menuName = "Mazes/Path Generator Config", fileName = "MazePathGeneratorConfig")]
	public class MazePathGeneratorConfig : ScriptableObject
	{
		[Header("Sub Gate")]
		public MazeGateTypeConfig subGate;

		[Header("Divide Gate")]
		public MazeGateTypeConfig divideGate;

		[Header("Multiplicator")]
		public MazeGateTypeConfig multiplicator;

		[Header("Dynamic Walls")]
		public List<MazeDynamicWall> dynamicWallsPrefabs;

		[Header("Rotators")]
		public List<MazeRotator> rotatorPrefabs;
	}
}
