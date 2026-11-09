using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/ComboBox")]
	[RequireComponent(typeof(RectTransform))]
	public class ComboBox : MonoBehaviour
	{
		[Serializable]
		public class SelectionChangedEvent : UnityEvent<string>
		{
		}

		public Color disabledTextColor;

		public List<string> AvailableOptions;

		[SerializeField]
		private float _scrollBarWidth;

		[SerializeField]
		private int _itemsToDisplay;

		[SerializeField]
		private bool _sortItems;

		public SelectionChangedEvent OnSelectionChanged;

		private bool _isPanelActive;

		private bool _hasDrawnOnce;

		private InputField _mainInput;

		private RectTransform _inputRT;

		private RectTransform _rectTransform;

		private RectTransform _overlayRT;

		private RectTransform _scrollPanelRT;

		private RectTransform _scrollBarRT;

		private RectTransform _slidingAreaRT;

		private RectTransform _itemsPanelRT;

		private Canvas _canvas;

		private RectTransform _canvasRT;

		private ScrollRect _scrollRect;

		private List<string> _panelItems;

		private Dictionary<string, GameObject> panelObjects;

		private GameObject itemTemplate;

		public DropDownListItem SelectedItem { get; private set; }

		public string Text { get; private set; }

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

		public void Awake()
		{
		}

		private bool Initialize()
		{
			return false;
		}

		public void AddItem(string item)
		{
		}

		public void RemoveItem(string item)
		{
		}

		public void SetAvailableOptions(List<string> newOptions)
		{
		}

		public void SetAvailableOptions(string[] newOptions)
		{
		}

		public void ResetItems()
		{
		}

		private void RebuildPanel()
		{
		}

		private void OnItemClicked(string item)
		{
		}

		private void RedrawPanel()
		{
		}

		public void OnValueChanged(string currText)
		{
		}

		public void ToggleDropdownPanel(bool directClick)
		{
		}
	}
}
