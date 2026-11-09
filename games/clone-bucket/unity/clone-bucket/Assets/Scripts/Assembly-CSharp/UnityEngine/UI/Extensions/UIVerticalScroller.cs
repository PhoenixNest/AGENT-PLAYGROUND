using System;
using UnityEngine.Events;

namespace UnityEngine.UI.Extensions
{
	[RequireComponent(typeof(ScrollRect))]
	[AddComponentMenu("Layout/Extensions/Vertical Scroller")]
	public class UIVerticalScroller : MonoBehaviour
	{
		[Serializable]
		public class IntEvent : UnityEvent<int>
		{
		}

		[Tooltip("desired ScrollRect")]
		public ScrollRect scrollRect;

		[Tooltip("Center display area (position of zoomed content)")]
		public RectTransform center;

		[Tooltip("Size / spacing of elements")]
		public RectTransform elementSize;

		[Tooltip("Scale = 1/ (1+distance from center * shrinkage)")]
		public Vector2 elementShrinkage;

		[Tooltip("Minimum element scale (furthest from center)")]
		public Vector2 minScale;

		[Tooltip("Select the item to be in center on start.")]
		public int startingIndex;

		[Tooltip("Stop scrolling past last element from inertia.")]
		public bool stopMomentumOnEnd;

		[Tooltip("Set Items out of center to not interactible.")]
		public bool disableUnfocused;

		[Tooltip("Button to go to the next page. (optional)")]
		public GameObject scrollUpButton;

		[Tooltip("Button to go to the previous page. (optional)")]
		public GameObject scrollDownButton;

		[Tooltip("Event fired when a specific item is clicked, exposes index number of item. (optional)")]
		public IntEvent OnButtonClicked;

		[Tooltip("Event fired when the focused item is Changed. (optional)")]
		public IntEvent OnFocusChanged;

		[HideInInspector]
		public GameObject[] _arrayOfElements;

		private float[] distReposition;

		private float[] distance;

		public int focusedElementIndex { get; private set; }

		public string result { get; private set; }

		[HideInInspector]
		public RectTransform scrollingPanel => null;

		public UIVerticalScroller()
		{
		}

		public UIVerticalScroller(RectTransform center, RectTransform elementSize, ScrollRect scrollRect, GameObject[] arrayOfElements)
		{
		}

		public void Awake()
		{
		}

		public void updateChildren(int startingIndex = -1, GameObject[] arrayOfElements = null)
		{
		}

		public void Start()
		{
		}

		public void Update()
		{
		}

		private void ScrollingElements()
		{
		}

		public void SnapToElement(int element)
		{
		}

		public void ScrollUp()
		{
		}

		public void ScrollDown()
		{
		}
	}
}
