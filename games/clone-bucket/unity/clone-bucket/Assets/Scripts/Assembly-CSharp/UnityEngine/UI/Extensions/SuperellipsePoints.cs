using System.Collections.Generic;

namespace UnityEngine.UI.Extensions
{
	[ExecuteInEditMode]
	public class SuperellipsePoints : MonoBehaviour
	{
		public float xLimits;

		public float yLimits;

		[Range(1f, 96f)]
		public float superness;

		private float lastXLim;

		private float lastYLim;

		private float lastSuper;

		[Space]
		[Range(1f, 32f)]
		public int levelOfDetail;

		private int lastLoD;

		[Space]
		public Material material;

		private List<Vector2> pointList;

		private void Start()
		{
		}

		private void Update()
		{
		}

		private void RecalculateSuperellipse()
		{
		}

		private float Superellipse(float a, float b, float x, float n)
		{
			return 0f;
		}
	}
}
