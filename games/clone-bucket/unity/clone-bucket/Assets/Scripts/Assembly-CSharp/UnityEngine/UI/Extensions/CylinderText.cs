namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Effects/Extensions/Cylinder Text")]
	[RequireComponent(typeof(Text), typeof(RectTransform))]
	public class CylinderText : BaseMeshEffect
	{
		public float radius;

		protected override void Awake()
		{
		}

		protected override void OnEnable()
		{
		}

		public override void ModifyMesh(VertexHelper vh)
		{
		}
	}
}
