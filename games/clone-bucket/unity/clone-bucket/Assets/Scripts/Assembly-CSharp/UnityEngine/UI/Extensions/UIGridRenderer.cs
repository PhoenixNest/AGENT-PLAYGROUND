namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/Primitives/UIGridRenderer")]
	public class UIGridRenderer : UILineRenderer
	{
		[SerializeField]
		private int m_GridColumns;

		[SerializeField]
		private int m_GridRows;

		public int GridColumns
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public int GridRows
		{
			get
			{
				return 0;
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
