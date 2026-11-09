using System.Collections.Generic;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("Layout/Extensions/Flow Layout Group")]
	public class FlowLayoutGroup : LayoutGroup
	{
		public enum Axis
		{
			Horizontal = 0,
			Vertical = 1
		}

		public float SpacingX;

		public float SpacingY;

		public bool ExpandHorizontalSpacing;

		public bool ChildForceExpandWidth;

		public bool ChildForceExpandHeight;

		public bool invertOrder;

		private float _layoutHeight;

		private float _layoutWidth;

		[SerializeField]
		protected Axis m_StartAxis;

		private readonly IList<RectTransform> _itemList;

		public Axis startAxis
		{
			get
			{
				return Axis.Horizontal;
			}
			set
			{
			}
		}

		protected bool IsCenterAlign => false;

		protected bool IsRightAlign => false;

		protected bool IsMiddleAlign => false;

		protected bool IsLowerAlign => false;

		public override void CalculateLayoutInputHorizontal()
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

		public float SetLayout(int axis, bool layoutInput)
		{
			return 0f;
		}

		private float CalculateRowVerticalOffset(float groupHeight, float yOffset, float currentRowHeight)
		{
			return 0f;
		}

		private float CalculateColHorizontalOffset(float groupWidth, float xOffset, float currentColWidth)
		{
			return 0f;
		}

		protected void LayoutRow(IList<RectTransform> contents, float rowWidth, float rowHeight, float maxWidth, float xOffset, float yOffset, int axis)
		{
		}

		protected void LayoutCol(IList<RectTransform> contents, float colWidth, float colHeight, float maxHeight, float xOffset, float yOffset, int axis)
		{
		}

		public float GetGreatestMinimumChildWidth()
		{
			return 0f;
		}

		public float GetGreatestMinimumChildHeigth()
		{
			return 0f;
		}
	}
}
