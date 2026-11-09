namespace UnityEngine.UI.Extensions
{
	[RequireComponent(typeof(RectTransform), typeof(Graphic))]
	[DisallowMultipleComponent]
	[AddComponentMenu("UI/Effects/Extensions/Flippable")]
	public class UIFlippable : BaseMeshEffect
	{
		[SerializeField]
		private bool m_Horizontal;

		[SerializeField]
		private bool m_Veritical;

		public bool horizontal
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public bool vertical
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public override void ModifyMesh(VertexHelper verts)
		{
		}
	}
}
