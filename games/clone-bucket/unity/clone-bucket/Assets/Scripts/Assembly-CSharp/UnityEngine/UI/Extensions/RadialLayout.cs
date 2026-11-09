namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("Layout/Extensions/Radial Layout")]
	public class RadialLayout : LayoutGroup
	{
		public float fDistance;

		[Range(0f, 360f)]
		public float MinAngle;

		[Range(0f, 360f)]
		public float MaxAngle;

		[Range(0f, 360f)]
		public float StartAngle;

		public bool OnlyLayoutVisible;

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
