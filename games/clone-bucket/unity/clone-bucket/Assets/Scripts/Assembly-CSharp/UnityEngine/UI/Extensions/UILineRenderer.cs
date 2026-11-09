using System;
using System.Collections.Generic;

namespace UnityEngine.UI.Extensions
{
	[RequireComponent(typeof(RectTransform))]
	[AddComponentMenu("UI/Extensions/Primitives/UILineRenderer")]
	public class UILineRenderer : UIPrimitiveBase
	{
		private enum SegmentType
		{
			Start = 0,
			Middle = 1,
			End = 2,
			Full = 3
		}

		public enum JoinType
		{
			Bevel = 0,
			Miter = 1
		}

		public enum BezierType
		{
			None = 0,
			Quick = 1,
			Basic = 2,
			Improved = 3,
			Catenary = 4
		}

		private const float MIN_MITER_JOIN = (float)Math.PI / 12f;

		private const float MIN_BEVEL_NICE_JOIN = (float)Math.PI / 6f;

		private static Vector2 UV_TOP_LEFT;

		private static Vector2 UV_BOTTOM_LEFT;

		private static Vector2 UV_TOP_CENTER_LEFT;

		private static Vector2 UV_TOP_CENTER_RIGHT;

		private static Vector2 UV_BOTTOM_CENTER_LEFT;

		private static Vector2 UV_BOTTOM_CENTER_RIGHT;

		private static Vector2 UV_TOP_RIGHT;

		private static Vector2 UV_BOTTOM_RIGHT;

		private static Vector2[] startUvs;

		private static Vector2[] middleUvs;

		private static Vector2[] endUvs;

		private static Vector2[] fullUvs;

		[SerializeField]
		[Tooltip("Points to draw lines between\n Can be improved using the Resolution Option")]
		internal Vector2[] m_points;

		[SerializeField]
		[Tooltip("Segments to be drawn\n This is a list of arrays of points")]
		internal List<Vector2[]> m_segments;

		[SerializeField]
		[Tooltip("Thickness of the line")]
		internal float lineThickness;

		[SerializeField]
		[Tooltip("Use the relative bounds of the Rect Transform (0,0 -> 0,1) or screen space coordinates")]
		internal bool relativeSize;

		[SerializeField]
		[Tooltip("Do the points identify a single line or split pairs of lines")]
		internal bool lineList;

		[SerializeField]
		[Tooltip("Add end caps to each line\nMultiple caps when used with Line List")]
		internal bool lineCaps;

		[SerializeField]
		[Tooltip("Resolution of the Bezier curve, different to line Resolution")]
		internal int bezierSegmentsPerCurve;

		[Tooltip("The type of Join used between lines, Square/Mitre or Curved/Bevel")]
		public JoinType LineJoins;

		[Tooltip("Bezier method to apply to line, see docs for options\nCan't be used in conjunction with Resolution as Bezier already changes the resolution")]
		public BezierType BezierMode;

		[HideInInspector]
		public bool drivenExternally;

		public float LineThickness
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public bool RelativeSize
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public bool LineList
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public bool LineCaps
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public int BezierSegmentsPerCurve
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public Vector2[] Points
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public List<Vector2[]> Segments
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		private void PopulateMesh(VertexHelper vh, Vector2[] pointsToDraw)
		{
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
		}

		private UIVertex[] CreateLineCap(Vector2 start, Vector2 end, SegmentType type)
		{
			return null;
		}

		private UIVertex[] CreateLineSegment(Vector2 start, Vector2 end, SegmentType type, UIVertex[] previousVert = null)
		{
			return null;
		}

		protected override void GeneratedUVs()
		{
		}

		protected override void ResolutionToNativeSize(float distance)
		{
		}

		private int GetSegmentPointCount()
		{
			return 0;
		}

		public Vector2 GetPosition(int index, int segmentIndex = 0)
		{
			return default;
		}

		public Vector2 GetPositionBySegment(int index, int segment)
		{
			return default;
		}

		public Vector2 GetClosestPoint(Vector2 p1, Vector2 p2, Vector2 p3)
		{
			return default;
		}
	}
}
