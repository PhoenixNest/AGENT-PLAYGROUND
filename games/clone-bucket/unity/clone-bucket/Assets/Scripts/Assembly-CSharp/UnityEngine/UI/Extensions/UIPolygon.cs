namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/Primitives/UI Polygon")]
	public class UIPolygon : UIPrimitiveBase
	{
		public bool fill;

		public float thickness;

		[Range(3f, 360f)]
		public int sides;

		[Range(0f, 360f)]
		public float rotation;

		[Range(0f, 1f)]
		public float[] VerticesDistances;

		private float size;

		public void DrawPolygon(int _sides)
		{
		}

		public void DrawPolygon(int _sides, float[] _VerticesDistances)
		{
		}

		public void DrawPolygon(int _sides, float[] _VerticesDistances, float _rotation)
		{
		}

		private void Update()
		{
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
		}
	}
}
