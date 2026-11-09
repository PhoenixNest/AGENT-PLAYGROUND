using System.Collections.Generic;

namespace Home.Game.Runtime.EntryPoint.Mazes
{
	public class MazePathHead
	{
		public MazeCell CurrentCell { get; private set; }

		public int StepCount { get; private set; }

		public int SameLayerStreak { get; private set; }

		public int UpwardStepCount { get; private set; }

		public HashSet<MazeCell> VisitedCells { get; }

		public MazePathHead(MazeCell startCell)
		{
		}

		public void StepTo(MazeCell targetCell)
		{
		}
	}
}
