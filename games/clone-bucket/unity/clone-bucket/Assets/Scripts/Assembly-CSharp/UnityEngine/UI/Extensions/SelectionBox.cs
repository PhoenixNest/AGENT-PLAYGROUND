using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/Selection Box")]
	[RequireComponent(typeof(Canvas))]
	public class SelectionBox : MonoBehaviour
	{
		public class SelectionEvent : UnityEvent<IBoxSelectable[]>
		{
		}

		public Color color;

		public Sprite art;

		private Vector2 origin;

		public RectTransform selectionMask;

		private RectTransform boxRect;

		private IBoxSelectable[] selectables;

		private MonoBehaviour[] selectableGroup;

		private IBoxSelectable clickedBeforeDrag;

		private IBoxSelectable clickedAfterDrag;

		public SelectionEvent onSelectionChange;

		private void ValidateCanvas()
		{
		}

		private void SetSelectableGroup(IEnumerable<MonoBehaviour> behaviourCollection)
		{
		}

		private void CreateBoxRect()
		{
		}

		private void ResetBoxRect()
		{
		}

		private void BeginSelection()
		{
		}

		private bool PointIsValidAgainstSelectionMask(Vector2 screenPoint)
		{
			return false;
		}

		private IBoxSelectable GetSelectableAtMousePosition()
		{
			return null;
		}

		private void DragSelection()
		{
		}

		private void ApplySingleClickDeselection()
		{
		}

		private void ApplyPreSelections()
		{
		}

		private Vector2 GetScreenPointOfSelectable(IBoxSelectable selectable)
		{
			return default;
		}

		private Camera GetScreenPointCamera(RectTransform rectTransform)
		{
			return null;
		}

		public IBoxSelectable[] GetAllSelected()
		{
			return null;
		}

		private void EndSelection()
		{
		}

		private void Start()
		{
		}

		private void Update()
		{
		}
	}
}
