using System.Collections.Generic;
using UnityEngine;

namespace Home.Game.Runtime.EntryPoint.Mazes
{
	public class MazeCell : MonoBehaviour
	{
		public int layerIndex;

		public List<MazeCellConnection> connections;
	}
}
