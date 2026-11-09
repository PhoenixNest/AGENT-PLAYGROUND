using System.Collections.Generic;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/Primitives/Squircle")]
	public class UISquircle : UIPrimitiveBase
	{
		public enum Type
		{
			Classic = 0,
			Scaled = 1
		}

		private const float C = 1f;

		[Space]
		public Type squircleType;

		[Range(1f, 40f)]
		public float n;

		[Min(0.1f)]
		public float delta;

		public float quality;

		[Min(0f)]
		public float radius;

		private float a;

		private float b;

		private List<Vector2> vert;

		private float SquircleFunc(float t, bool xByY)
		{
			return 0f;
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
		}
	}
}
