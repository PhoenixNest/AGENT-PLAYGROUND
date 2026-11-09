using UnityEngine;

namespace Balls
{
	[CreateAssetMenu(fileName = "BallsConfig", menuName = "Configs/BallsConfig")]
	public class BallsConfig : ScriptableObject
	{
		public BallView prefab;

		public float inMazeMaxSpeed;

		public float outMazeMaxSpeed;

		public Vector3 smallScale;

		public Vector3 mediumScale;

		public Vector3 bigScale;

		public int initialPoolAmount;

		public int ignoreMultiplicatorCapacity;

		public bool useBallLayers;

		public int[] ballLayers;

		public int[] minimumBallsForLayers;

		[Header("Ball Value Tiers (Multiplicator overflow)")]
		public bool useBallValueTiers;

		public int[] valuePerTier;

		public int[] totalBallsForNextTier;

		public float[] chancesToOverrideTier;
	}
}
