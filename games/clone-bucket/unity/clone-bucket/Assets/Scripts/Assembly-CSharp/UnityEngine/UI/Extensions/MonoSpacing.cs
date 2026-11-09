namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Effects/Extensions/Mono Spacing")]
	[RequireComponent(typeof(Text))]
	[RequireComponent(typeof(RectTransform))]
	public class MonoSpacing : BaseMeshEffect
	{
		[SerializeField]
		private float m_spacing;

		public float HalfCharWidth;

		public bool UseHalfCharWidth;

		private RectTransform rectTransform;

		private Text text;

		public float Spacing
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		protected MonoSpacing()
		{
		}

		protected override void Awake()
		{
		}

		public override void ModifyMesh(VertexHelper vh)
		{
		}
	}
}
