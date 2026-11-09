using System.Collections.Generic;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/Primitives/UI Circle")]
	public class UICircle : UIPrimitiveBase
	{
		[Tooltip("The Arc Invert property will invert the construction of the Arc.")]
		public bool ArcInvert;

		[Tooltip("The Arc property is a percentage of the entire circumference of the circle.")]
		[Range(0f, 1f)]
		public float Arc;

		[Tooltip("The Arc Steps property defines the number of segments that the Arc will be divided into.")]
		[Range(0f, 1000f)]
		public int ArcSteps;

		[Range(0f, 360f)]
		[Tooltip("The Arc Rotation property permits adjusting the geometry orientation around the Z axis.")]
		public int ArcRotation;

		[Tooltip("The Progress property allows the primitive to be used as a progression indicator.")]
		[Range(0f, 1f)]
		public float Progress;

		private float _progress;

		public Color ProgressColor;

		public bool Fill;

		public float Thickness;

		public int Padding;

		private List<int> indices;

		private List<UIVertex> vertices;

		private Vector2 uvCenter;

		protected override void OnPopulateMesh(VertexHelper vh)
		{
		}

		public void SetProgress(float progress)
		{
		}

		public void SetArcSteps(int steps)
		{
		}

		public void SetInvertArc(bool invert)
		{
		}

		public void SetArcRotation(int rotation)
		{
		}

		public void SetFill(bool fill)
		{
		}

		public void SetBaseColor(Color color)
		{
		}

		public void UpdateBaseAlpha(float value)
		{
		}

		public void SetProgressColor(Color color)
		{
		}

		public void UpdateProgressAlpha(float value)
		{
		}

		public void SetPadding(int padding)
		{
		}

		public void SetThickness(int thickness)
		{
		}
	}
}
