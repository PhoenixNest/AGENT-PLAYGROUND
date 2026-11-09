namespace UnityEngine.UI.Extensions.ColorPicker
{
	[RequireComponent(typeof(Text))]
	public class ColorLabel : MonoBehaviour
	{
		public ColorPickerControl picker;

		public ColorValues type;

		public string prefix;

		public float minValue;

		public float maxValue;

		public int precision;

		private Text label;

		private void Awake()
		{
		}

		private void OnEnable()
		{
		}

		private void OnDestroy()
		{
		}

		private void ColorChanged(Color color)
		{
		}

		private void HSVChanged(float hue, float sateration, float value)
		{
		}

		private void UpdateValue()
		{
		}

		private string ConvertToDisplayString(float value)
		{
			return null;
		}
	}
}
