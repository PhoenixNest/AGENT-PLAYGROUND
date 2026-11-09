namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/Primitives/UILineTextureRenderer")]
	public class UILineTextureRenderer : UIPrimitiveBase
	{
		[SerializeField]
		private Rect m_UVRect;

		[SerializeField]
		private Vector2[] m_points;

		public float LineThickness;

		public bool UseMargins;

		public Vector2 Margin;

		public bool relativeSize;

		public Rect uvRect
		{
			get
			{
				return default;
			}
			set
			{
			}
		}

		public Vector2[] Points
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
		}

		public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
		{
			return default;
		}
	}
}
