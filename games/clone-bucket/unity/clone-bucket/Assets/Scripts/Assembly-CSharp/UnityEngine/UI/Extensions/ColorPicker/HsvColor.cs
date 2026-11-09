namespace UnityEngine.UI.Extensions.ColorPicker
{
	public struct HsvColor
	{
		public double H;

		public double S;

		public double V;

		public float NormalizedH
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public float NormalizedS
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public float NormalizedV
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public HsvColor(double h, double s, double v)
		{
			H = 0.0;
			S = 0.0;
			V = 0.0;
		}

		public override string ToString()
		{
			return null;
		}
	}
}
