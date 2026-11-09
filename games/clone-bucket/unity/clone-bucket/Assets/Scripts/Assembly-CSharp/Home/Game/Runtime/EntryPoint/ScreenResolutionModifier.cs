using UnityEngine;

namespace Home.Game.Runtime.EntryPoint
{
	public class ScreenResolutionModifier : MonoBehaviour
	{
		public enum AspectRatioRelations
		{
			WidthControlHeight = 0,
			HeightControlWidth = 1
		}

		[SerializeField]
		[Header("Screen resolution modifier")]
		[Range(0.1f, 2f)]
		private float _screenResolutionModifier;

		[Header("Minimal resolution settings")]
		[SerializeField]
		private AspectRatioRelations _aspectRationRelations;

		[SerializeField]
		private int _minWidth;

		[SerializeField]
		private int _minHeight;

		private void Start()
		{
			SetResolution();
		}

		private void SetResolution()
		{
			float displayWidth = Screen.width;
			float displayHeight = Screen.height;
			if (displayWidth <= 0f || displayHeight <= 0f)
			{
				return;
			}

			float width = _screenResolutionModifier * displayWidth;
			float height = _screenResolutionModifier * displayHeight;

			if (width < _minWidth || height < _minHeight)
			{
				if (_aspectRationRelations == AspectRatioRelations.HeightControlWidth)
				{
					if (height < _minHeight)
					{
						height = _minHeight;
						width = _minHeight / (displayHeight / displayWidth);
					}
				}
				else if (width < _minWidth)
				{
					width = _minWidth;
					height = _minWidth / (displayWidth / displayHeight);
				}
			}

			int intWidth = float.IsFinite(width) ? (int)width : int.MinValue;
			int intHeight = float.IsFinite(height) ? (int)height : int.MinValue;

			Screen.SetResolution(intWidth, intHeight, FullScreenMode.FullScreenWindow);
		}
	}
}
