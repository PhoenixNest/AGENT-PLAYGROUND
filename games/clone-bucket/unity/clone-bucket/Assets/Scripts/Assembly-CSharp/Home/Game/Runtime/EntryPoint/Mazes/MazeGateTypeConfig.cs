using System;
using UnityEngine;

namespace Home.Game.Runtime.EntryPoint.Mazes
{
	[Serializable]
	public class MazeGateTypeConfig
	{
		public GameObject prefab;

		public float spawnChance;

		public int minimumCount;

		public int maximumCount;

		public int valueMin;

		public int valueMax;

		public int valueStep;
	}
}
