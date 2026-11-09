using UnityEngine;

namespace Home.Game.Runtime.EntryPoint.Mazes
{
	public class MazeDynamicWall : MonoBehaviour
	{
		public string id;

		public Transform transformCached;

		private void Reset()
		{
			transformCached = transform;
		}
	}
}
