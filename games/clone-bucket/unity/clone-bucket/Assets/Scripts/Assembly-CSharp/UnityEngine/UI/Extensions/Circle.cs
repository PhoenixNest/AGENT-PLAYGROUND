namespace UnityEngine.UI.Extensions
{
	public class Circle
	{
		[SerializeField]
		private float xAxis;

		[SerializeField]
		private float yAxis;

		[SerializeField]
		private int steps;

		public float X
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public float Y
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public int Steps
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public Circle(float radius)
		{
		}

		public Circle(float radius, int steps)
		{
		}

		public Circle(float xAxis, float yAxis)
		{
		}

		public Circle(float xAxis, float yAxis, int steps)
		{
		}

		public Vector2 Evaluate(float t)
		{
			return default;
		}

		public void Evaluate(float t, out Vector2 eval)
		{
			eval = default;
		}
	}
}
