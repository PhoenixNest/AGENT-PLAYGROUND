namespace UnityEngine.UI.Extensions.ColorPicker
{
	[RequireComponent(typeof(InputField))]
	public class HexColorField : MonoBehaviour
	{
		public ColorPickerControl ColorPicker;

		public bool displayAlpha;

		private InputField hexInputField;

		private const string hexRegex = "^#?(?:[0-9a-fA-F]{3,4}){1,2}$";

		private void Awake()
		{
		}

		private void OnDestroy()
		{
		}

		private void UpdateHex(Color newColor)
		{
		}

		private void UpdateColor(string newHex)
		{
		}

		private string ColorToHex(Color32 color)
		{
			return null;
		}

		public static bool HexToColor(string hex, out Color32 color)
		{
			color = default;
			return false;
		}
	}
}
