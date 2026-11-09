namespace UnityEngine.UI.Extensions.ColorPicker
{
	[RequireComponent(typeof(Slider))]
	public class ColorSlider : MonoBehaviour
	{
		public ColorPickerControl ColorPicker;

		public ColorValues type;

		private Slider slider;

		private bool listen;

		private void Awake()
		{
		}

		private void OnDestroy()
		{
		}

		private void ColorChanged(Color newColor)
		{
		}

		private void HSVChanged(float hue, float saturation, float value)
		{
		}

		private void SliderChanged(float newValue)
		{
		}
	}
}
