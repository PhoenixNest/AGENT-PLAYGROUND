namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("Layout/Extensions/Curved Layout")]
	public class CurvedLayout : LayoutGroup
	{
		public Vector3 CurveOffset;

		[Tooltip("axis along which to place the items, Normalized before use")]
		public Vector3 itemAxis;

		[Tooltip("size of each item along the Normalized axis")]
		public float itemSize;

		public float centerpoint;

		protected override void OnEnable()
		{
		}

		public override void SetLayoutHorizontal()
		{
		}

		public override void SetLayoutVertical()
		{
		}

		public override void CalculateLayoutInputVertical()
		{
		}

		public override void CalculateLayoutInputHorizontal()
		{
		}

		private void CalculateRadial()
		{
		}
	}
}
