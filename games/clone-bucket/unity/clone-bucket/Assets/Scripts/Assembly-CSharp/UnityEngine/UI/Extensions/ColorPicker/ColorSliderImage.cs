namespace UnityEngine.UI.Extensions.ColorPicker
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(RawImage))]
	public class ColorSliderImage : MonoBehaviour
	{
		public ColorPickerControl picker;

		public ColorValues type;

		public Slider.Direction direction;

		private RawImage image;

		private RectTransform RectTransform => null;

		private void Awake()
		{
		}

		private void OnEnable()
		{
		}

		private void OnDisable()
		{
		}

		private void OnDestroy()
		{
		}

		private void ColorChanged(Color newColor)
		{
		}

		private void ColorChanged(float hue, float saturation, float value)
		{
		}

		private void RegenerateTexture()
		{
		}
	}
}
