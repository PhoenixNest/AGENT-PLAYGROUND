using System.Collections.Generic;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/UI Infinite Scroll")]
	public class UI_InfiniteScroll : MonoBehaviour
	{
		[Tooltip("If false, will Init automatically, otherwise you need to call Init() method")]
		public bool InitByUser;

		protected ScrollRect _scrollRect;

		private ContentSizeFitter _contentSizeFitter;

		private VerticalLayoutGroup _verticalLayoutGroup;

		private HorizontalLayoutGroup _horizontalLayoutGroup;

		private GridLayoutGroup _gridLayoutGroup;

		protected bool _isVertical;

		protected bool _isHorizontal;

		private float _disableMarginX;

		private float _disableMarginY;

		private bool _hasDisabledGridComponents;

		protected List<RectTransform> items;

		private Vector2 _newAnchoredPosition;

		private float _threshold;

		private int _itemCount;

		private float _recordOffsetX;

		private float _recordOffsetY;

		protected virtual void Awake()
		{
		}

		public virtual void SetNewItems(ref List<Transform> newItems)
		{
		}

		private void SetItems()
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
