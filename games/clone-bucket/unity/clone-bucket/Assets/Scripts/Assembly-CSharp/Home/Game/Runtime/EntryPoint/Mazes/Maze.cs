using System.Collections.Generic;
using UnityEngine;

namespace Home.Game.Runtime.EntryPoint.Mazes
{
	public class Maze : MonoBehaviour
	{
		public string mazeShape;

		public Transform ballSpawnPoint;

		public Transform bucketTransform;

		public List<MazeCell> cells;

		public Transform mazeRotatorsParent;

		public List<GameObject> wallsRingsParents;

		public List<GameObject> wallsRadialsParents;

		public Transform wallsDynamicParent;

		public GameObject gatesParent;

		public List<GateBase> gates;

		public Transform levelEndTransform;

		private void Reset()
		{
		}

		[ContextMenu("Fix Walls")]
		private void FixWalls()
		{
		}

		[ContextMenu("Sort Walls")]
		private void SortWalls()
		{
		}

		[ContextMenu("Make Maze Walls")]
		private void MakeMazeWalls()
		{
		}
	}
}
