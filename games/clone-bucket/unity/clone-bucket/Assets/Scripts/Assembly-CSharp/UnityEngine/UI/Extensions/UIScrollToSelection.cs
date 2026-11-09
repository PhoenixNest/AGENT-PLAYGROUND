using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/UIScrollToSelection")]
	[RequireComponent(typeof(ScrollRect))]
	public class UIScrollToSelection : MonoBehaviour
	{
		public enum ScrollType
		{
			VERTICAL = 0,
			HORIZONTAL = 1,
			BOTH = 2
		}

		[SerializeField]
		[Header("[ Settings ]")]
		private ScrollType scrollDirection;

		[SerializeField]
		private float scrollSpeed;

		[SerializeField]
		[Header("[ Input ]")]
		private bool cancelScrollOnInput;

		[SerializeField]
		private List<KeyCode> cancelScrollKeycodes;

		protected RectTransform LayoutListGroup => null;

		protected ScrollType ScrollDirection => ScrollType.VERTICAL;

		protected float ScrollSpeed => 0f;

		protected bool CancelScrollOnInput => false;

		protected List<KeyCode> CancelScrollKeycodes => null;

		protected RectTransform ScrollWindow { get; set; }

		protected ScrollRect TargetScrollRect { get; set; }

		protected EventSystem CurrentEventSystem => null;

		protected GameObject LastCheckedGameObject { get; set; }

		protected GameObject CurrentSelectedGameObject => null;

		protected RectTransform CurrentTargetRectTransform { get; set; }

		protected bool IsManualScrollingAvailable { get; set; }

		protected virtual void Awake()
		{
		}

		protected virtual void Start()
		{
		}

		protected virtual void Update()
		{
		}

		private void UpdateReferences()
		{
		}

		private void CheckIfScrollingShouldBeLocked()
		{
		}

		private void ScrollRectToLevelSelection()
		{
		}

		private void UpdateVerticalScrollPosition(RectTransform selection)
		{
		}

		private void UpdateHorizontalScrollPosition(RectTransform selection)
		{
		}

		private float GetScrollOffset(float position, float listAnchorPosition, float targetLength, float maskLength)
		{
			return 0f;
		}
	}
}
