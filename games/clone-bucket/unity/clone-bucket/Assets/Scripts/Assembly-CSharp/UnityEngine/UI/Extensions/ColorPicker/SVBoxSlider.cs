namespace UnityEngine.UI.Extensions.ColorPicker
{
	[RequireComponent(typeof(BoxSlider), typeof(RawImage))]
	[ExecuteInEditMode]
	public class SVBoxSlider : MonoBehaviour
	{
		public ColorPickerControl picker;

		private BoxSlider slider;

		private RawImage image;

		private float lastH;

		private bool listen;

		public RectTransform RectTransform => null;

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

		private void SliderChanged(float saturation, float value)
		{
		}

		private void HSVChanged(float h, float s, float v)
		{
		}

		private void RegenerateSVTexture()
		{
		}
	}
}
