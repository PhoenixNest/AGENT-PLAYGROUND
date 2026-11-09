using UnityEngine;

namespace Levels
{
	[CreateAssetMenu(fileName = "LevelsConfig", menuName = "Configs/LevelsConfig")]
	public class LevelsConfig : ScriptableObject
	{
		public MiniLevelView jsonBaseLevelPrefab;

		public LevelView prefab;

		public Vector3 cameraLocalPosition;

		public Vector3 cameraLocalRotation;

		public Vector3[] ballSpawnPositions;

		public Vector3 distanceBetweenMiniLevels;

		public float ballGoalPercentTarget;

		public float ballGoalPercentPerfect;
	}
}
