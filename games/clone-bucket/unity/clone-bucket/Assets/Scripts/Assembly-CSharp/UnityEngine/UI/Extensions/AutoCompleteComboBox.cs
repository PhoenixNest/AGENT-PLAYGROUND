using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityEngine.UI.Extensions
{
	[RequireComponent(typeof(RectTransform))]
	[AddComponentMenu("UI/Extensions/AutoComplete ComboBox")]
	public class AutoCompleteComboBox : MonoBehaviour
	{
		[Serializable]
		public class SelectionChangedEvent : UnityEvent<string, bool>
		{
		}

		[Serializable]
		public class SelectionTextChangedEvent : UnityEvent<string>
		{
		}

		[Serializable]
		public class SelectionValidityChangedEvent : UnityEvent<bool>
		{
		}

		public Color disabledTextColor;

		public List<string> AvailableOptions;

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

		private List<string> _prunedPanelItems;

		private Dictionary<string, GameObject> panelObjects;

		private GameObject itemTemplate;

		[SerializeField]
		private float _scrollBarWidth;

		[SerializeField]
		private int _itemsToDisplay;

		public bool SelectFirstItemOnStart;

		[Tooltip("Change input text color based on matching items")]
		[SerializeField]
		private bool _ChangeInputTextColorBasedOnMatchingItems;

		public float DropdownOffset;

		public Color ValidSelectionTextColor;

		public Color MatchingItemsRemainingTextColor;

		public Color NoItemsRemainingTextColor;

		public AutoCompleteSearchType autocompleteSearchType;

		private bool _selectionIsValid;

		public SelectionTextChangedEvent OnSelectionTextChanged;

		public SelectionValidityChangedEvent OnSelectionValidityChanged;

		public SelectionChangedEvent OnSelectionChanged;

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

		public bool InputColorMatching
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public void Awake()
		{
		}

		public void Start()
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

		private void SetInputTextColor()
		{
		}

		public void ToggleDropdownPanel(bool directClick = false)
		{
		}

		private void PruneItems(string currText)
		{
		}

		private void PruneItemsLinq(string currText)
		{
		}

		private void PruneItemsArray(string currText)
		{
		}
	}
}
