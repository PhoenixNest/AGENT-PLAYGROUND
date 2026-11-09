using UnityEngine;

namespace Home.Game.Runtime.EntryPoint.Mazes
{
	public class MazeRotator : MonoBehaviour
	{
		public string id;

		public Transform transformCached;

		private void Reset()
		{
			transformCached = transform;
		}
	}
}
