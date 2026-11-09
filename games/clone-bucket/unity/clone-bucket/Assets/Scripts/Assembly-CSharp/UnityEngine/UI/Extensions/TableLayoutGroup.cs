namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("Layout/Extensions/Table Layout Group")]
	public class TableLayoutGroup : LayoutGroup
	{
		public enum Corner
		{
			UpperLeft = 0,
			UpperRight = 1,
			LowerLeft = 2,
			LowerRight = 3
		}

		[SerializeField]
		protected Corner startCorner;

		[SerializeField]
		protected float[] columnWidths;

		[SerializeField]
		protected float minimumRowHeight;

		[SerializeField]
		protected bool flexibleRowHeight;

		[SerializeField]
		protected float columnSpacing;

		[SerializeField]
		protected float rowSpacing;

		private float[] preferredRowHeights;

		public Corner StartCorner
		{
			get
			{
				return Corner.UpperLeft;
			}
			set
			{
			}
		}

		public float[] ColumnWidths
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public float MinimumRowHeight
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public bool FlexibleRowHeight
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public float ColumnSpacing
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public float RowSpacing
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public override void CalculateLayoutInputHorizontal()
		{
		}

		public override void CalculateLayoutInputVertical()
		{
		}

		public override void SetLayoutHorizontal()
		{
		}

		public override void SetLayoutVertical()
		{
		}
	}
}
