using System.Collections.Generic;

namespace UnityEngine.UI.Extensions.ColorPicker
{
	public class ColorPickerPresets : MonoBehaviour
	{
		public enum SaveType
		{
			None = 0,
			PlayerPrefs = 1,
			JsonFile = 2
		}

		protected class JsonColor
		{
			public Color32[] colors;

			public void SetColors(Color[] colorsIn)
			{
			}

			public Color[] GetColors()
			{
				return null;
			}
		}

		public ColorPickerControl picker;

		[SerializeField]
		protected GameObject presetPrefab;

		[SerializeField]
		protected int maxPresets;

		[SerializeField]
		protected Color[] predefinedPresets;

		protected List<Color> presets;

		public Image createPresetImage;

		public Transform createButton;

		[SerializeField]
		public SaveType saveMode;

		[SerializeField]
		protected string playerPrefsKey;

		public virtual string JsonFilePath => null;

		protected virtual void Reset()
		{
		}

		protected virtual void Awake()
		{
		}

		public virtual void CreatePresetButton()
		{
		}

		public virtual void LoadPresets(SaveType saveType)
		{
		}

		public virtual void SavePresets(SaveType saveType)
		{
		}

		public virtual void CreatePreset(Color color, bool loading)
		{
		}

		public virtual void CreatePreset(Color color)
		{
		}

		public virtual void PresetSelect(Image sender)
		{
		}

		protected virtual void HSVChanged(float h, float s, float v)
		{
		}

		protected virtual void ColorChanged(Color color)
		{
		}
	}
}
