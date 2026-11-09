using System.Collections.Generic;

namespace UnityEngine.UI.Extensions
{
	public class BezierPath
	{
		public int SegmentsPerCurve;

		public float MINIMUM_SQR_DISTANCE;

		public float DIVISION_THRESHOLD;

		private List<Vector2> controlPoints;

		private int curveCount;

		public void SetControlPoints(List<Vector2> newControlPoints)
		{
		}

		public void SetControlPoints(Vector2[] newControlPoints)
		{
		}

		public List<Vector2> GetControlPoints()
		{
			return null;
		}

		public void Interpolate(List<Vector2> segmentPoints, float scale)
		{
		}

		public void SamplePoints(List<Vector2> sourcePoints, float minSqrDistance, float maxSqrDistance, float scale)
		{
		}

		public Vector2 CalculateBezierPoint(int curveIndex, float t)
		{
			return default;
		}

		public List<Vector2> GetDrawingPoints0()
		{
			return null;
		}

		public List<Vector2> GetDrawingPoints1()
		{
			return null;
		}

		public List<Vector2> GetDrawingPoints2()
		{
			return null;
		}

		private List<Vector2> FindDrawingPoints(int curveIndex)
		{
			return null;
		}

		private int FindDrawingPoints(int curveIndex, float t0, float t1, List<Vector2> pointList, int insertionIndex)
		{
			return 0;
		}

		private Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
		{
			return default;
		}
	}
}
