using System;
using System.Collections.Generic;

namespace UnityEngine.UI.Extensions
{
	[Serializable]
	public class CableCurve
	{
		[SerializeField]
		private Vector2 m_start;

		[SerializeField]
		private Vector2 m_end;

		[SerializeField]
		private float m_slack;

		[SerializeField]
		private int m_steps;

		[SerializeField]
		private bool m_regen;

		private static Vector2[] emptyCurve;

		[SerializeField]
		private Vector2[] points;

		public bool regenPoints
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public Vector2 start
		{
			get
			{
				return default;
			}
			set
			{
			}
		}

		public Vector2 end
		{
			get
			{
				return default;
			}
			set
			{
			}
		}

		public float slack
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public int steps
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public Vector2 midPoint => default;

		public CableCurve()
		{
		}

		public CableCurve(Vector2[] inputPoints)
		{
		}

		public CableCurve(List<Vector2> inputPoints)
		{
		}

		public CableCurve(CableCurve v)
		{
		}

		public Vector2[] Points()
		{
			return null;
		}
	}
}
