namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/Primitives/Diamond Graph")]
	public class DiamondGraph : UIPrimitiveBase
	{
		[SerializeField]
		private float m_a;

		[SerializeField]
		private float m_b;

		[SerializeField]
		private float m_c;

		[SerializeField]
		private float m_d;

		public float A
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public float B
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public float C
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public float D
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
		}
	}
}
