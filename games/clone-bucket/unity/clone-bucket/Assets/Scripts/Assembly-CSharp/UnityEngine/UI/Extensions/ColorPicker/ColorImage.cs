namespace UnityEngine.UI.Extensions.ColorPicker
{
	[RequireComponent(typeof(Image))]
	public class ColorImage : MonoBehaviour
	{
		public ColorPickerControl picker;

		private Image image;

		private void Awake()
		{
		}

		private void OnDestroy()
		{
		}

		private void ColorChanged(Color newColor)
		{
		}
	}
}
