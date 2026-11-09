using System;

namespace UnityEngine.UI.Extensions
{
	public class CUIBezierCurve : MonoBehaviour
	{
		public static readonly int CubicBezierCurvePtNum;

		[SerializeField]
		protected Vector3[] controlPoints;

		public Action OnRefresh;

		public Vector3[] ControlPoints => null;

		public void Refresh()
		{
		}

		public Vector3 GetPoint(float _time)
		{
			return default;
		}

		public Vector3 GetTangent(float _time)
		{
			return default;
		}

		public void ReportSet()
		{
		}
	}
}
