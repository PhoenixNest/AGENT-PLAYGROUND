using UnityEngine;

namespace Tabtale.TTPlugins
{
	public class TTPSafeArea : MonoBehaviour
	{
		public enum SimDevice
		{
			None = 0,
			iPhoneX = 1
		}

		private RectTransform Panel;

		private Rect LastSafeArea;

		public static SimDevice Sim;

		private Rect[] NSA_iPhoneX;

		private void Awake()
		{
		}

		private void Update()
		{
		}

		private void Refresh()
		{
		}

		private Rect GetSafeArea()
		{
			return default;
		}

		private void ApplySafeArea(Rect safeArea)
		{
		}
	}
}
