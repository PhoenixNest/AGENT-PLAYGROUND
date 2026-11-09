using System.Collections.Generic;

namespace Home.Game.Runtime.EntryPoint.Mazes
{
	public struct MazeGateDefinition
	{
		public float positionX;

		public float positionY;

		public float rotationX;

		public float rotationY;

		public float rotationZ;

		public string type;

		public int value;

		public bool isMovable;

		public float moveTime;

		public float size;

		public string l_type_1;

		public int l_value_1;

		public string l_type_2;

		public int l_value_2;

		public string l_type_3;

		public int l_value_3;

		public string l_type_4;

		public int l_value_4;

		public List<(string, int)> GetLoopPool()
		{
			return null;
		}
	}
}
