using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/Dropdown List")]
	[RequireComponent(typeof(RectTransform))]
	public class DropDownList : MonoBehaviour
	{
		[Serializable]
		public class SelectionChangedEvent : UnityEvent<int>
		{
		}

		public Color disabledTextColor;

		public List<DropDownListItem> Items;

		public bool OverrideHighlighted;

		private bool _isPanelActive;

		private bool _hasDrawnOnce;

		private DropDownListButton _mainButton;

		private RectTransform _rectTransform;

		private RectTransform _overlayRT;

		private RectTransform _scrollPanelRT;

		private RectTransform _scrollBarRT;

		private RectTransform _slidingAreaRT;

		private RectTransform _itemsPanelRT;

		private Canvas _canvas;

		private RectTransform _canvasRT;

		private ScrollRect _scrollRect;

		private List<DropDownListButton> _panelItems;

		private GameObject _itemTemplate;

		[SerializeField]
		private float _scrollBarWidth;

		private int _selectedIndex;

		[SerializeField]
		private int _itemsToDisplay;

		public bool SelectFirstItemOnStart;

		public SelectionChangedEvent OnSelectionChanged;

		public DropDownListItem SelectedItem { get; private set; }

		public float ScrollBarWidth
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public int ItemsToDisplay
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public void Start()
		{
		}

		private bool Initialize()
		{
			return false;
		}

		public void RefreshItems(params object[] list)
		{
		}

		public void AddItem(DropDownListItem item)
		{
		}

		public void AddItem(string item)
		{
		}

		public void AddItem(Sprite item)
		{
		}

		public void RemoveItem(DropDownListItem item)
		{
		}

		public void RemoveItem(string item)
		{
		}

		public void RemoveItem(Sprite item)
		{
		}

		public void ResetItems()
		{
		}

		private void RebuildPanel()
		{
		}

		private void OnItemClicked(int indx)
		{
		}

		private void UpdateSelected()
		{
		}

		private void RedrawPanel()
		{
		}

		public void ToggleDropdownPanel(bool directClick)
		{
		}
	}
}
