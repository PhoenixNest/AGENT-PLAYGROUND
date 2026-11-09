using System.Collections.Generic;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/UI Scrollrect Occlusion")]
	public class UI_ScrollRectOcclusion : MonoBehaviour
	{
		public bool InitByUser;

		private ScrollRect _scrollRect;

		private ContentSizeFitter _contentSizeFitter;

		private VerticalLayoutGroup _verticalLayoutGroup;

		private HorizontalLayoutGroup _horizontalLayoutGroup;

		private GridLayoutGroup _gridLayoutGroup;

		private bool _isVertical;

		private bool _isHorizontal;

		private float _disableMarginX;

		private float _disableMarginY;

		private bool hasDisabledGridComponents;

		private List<RectTransform> items;

		private void Awake()
		{
		}

		public void Init()
		{
		}

		private void DisableGridComponents()
		{
		}

		public void OnScroll(Vector2 pos)
		{
		}
	}
}
