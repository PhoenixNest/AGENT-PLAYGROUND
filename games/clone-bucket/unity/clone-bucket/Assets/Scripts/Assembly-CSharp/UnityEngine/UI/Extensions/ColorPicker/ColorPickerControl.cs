using System.Collections.Generic;

namespace UnityEngine.UI.Extensions.ColorPicker
{
	[ExecuteInEditMode]
	public class ColorPickerControl : MonoBehaviour
	{
		private float _hue;

		private float _saturation;

		private float _brightness;

		private float _red;

		private float _green;

		private float _blue;

		private float _alpha;

		public ColorChangedEvent onValueChanged;

		public HSVChangedEvent onHSVChanged;

		[SerializeField]
		private bool hsvSlidersOn;

		[SerializeField]
		private List<GameObject> hsvSliders;

		[SerializeField]
		private bool rgbSlidersOn;

		[SerializeField]
		private List<GameObject> rgbSliders;

		[SerializeField]
		private GameObject alphaSlider;

		public Color CurrentColor
		{
			get
			{
				return default;
			}
			set
			{
			}
		}

		public float H
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public float S
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public float V
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public float R
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public float G
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public float B
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		private float A
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public void SetHSVSlidersOn(bool value)
		{
		}

		public void SetRGBSlidersOn(bool value)
		{
		}

		private void Update()
		{
		}

		private void Start()
		{
		}

		private void RGBChanged()
		{
		}

		private void HSVChanged()
		{
		}

		private void SendChangedEvent()
		{
		}

		public void AssignColor(ColorValues type, float value)
		{
		}

		public float GetValue(ColorValues type)
		{
			return 0f;
		}
	}
}
