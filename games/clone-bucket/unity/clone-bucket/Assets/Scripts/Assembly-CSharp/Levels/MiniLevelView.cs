using Home.Game.Runtime.EntryPoint.Mazes;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Levels
{
	public class MiniLevelView : MonoBehaviour
	{
		public GameObject gameObjectCached;

		public Transform ballsParentInsideMaze;

		public Transform ballsParentOutsideMaze;

		public RotateController rotateController;

		public Bucket bucket;

		public TextMeshProUGUI bucketBallCounter;

		public MeshRenderer bucketStand;

		public string mazeShape;

		[FormerlySerializedAs("_allGates")]
		public GateBase[] allGates;

		public MazeJsonGenerator mazeJsonGenerator;

		public Maze maze;

		private void Reset()
		{
			gameObjectCached = gameObject;
			if (rotateController == null)
			{
				rotateController = GetComponentInChildren<RotateController>(true);
			}

			if (bucket == null)
			{
				bucket = GetComponentInChildren<Bucket>(true);
			}
		}
	}
}
