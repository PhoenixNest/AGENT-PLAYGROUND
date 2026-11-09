using System.Collections.Generic;

namespace UnityEngine.UI.Extensions
{
	[ExecuteInEditMode]
	public class Triangulator
	{
		private List<Vector2> m_points;

		public Triangulator(Vector2[] points)
		{
		}

		public int[] Triangulate()
		{
			return null;
		}

		private float Area()
		{
			return 0f;
		}

		private bool Snip(int u, int v, int w, int n, int[] V)
		{
			return false;
		}

		private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
		{
			return false;
		}
	}
}
