using System;
using System.Collections.Generic;

namespace Home.Game.Runtime.EntryPoint.Mazes
{
	[Serializable]
	public class MazeCellConnection
	{
		public MazeCell target;

		public List<MazeWall> mazeWalls;
	}
}
