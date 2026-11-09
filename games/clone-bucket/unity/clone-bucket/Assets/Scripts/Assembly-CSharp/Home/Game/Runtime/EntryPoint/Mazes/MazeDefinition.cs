using System;
using System.Collections.Generic;

namespace Home.Game.Runtime.EntryPoint.Mazes
{
	[Serializable]
	public struct MazeDefinition
	{
		public string shapeId;

		public int ballsStarting;

		public int ballsGoal;

		public float endPosition;

		public bool[,] wallsRings;

		public bool[,] wallsRadials;

		public List<MazeDynamicWallDefinition> dynamicWalls;

		public List<MazeRotatorDefinition> rotators;

		public List<MazeGateDefinition> gates;
	}
}
