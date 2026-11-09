using System;
using UnityEngine.Events;

namespace UnityEngine.UI.Extensions
{
	[RequireComponent(typeof(RectTransform))]
	[DisallowMultipleComponent]
	[AddComponentMenu("UI/Extensions/Re-orderable list")]
	public class ReorderableList : MonoBehaviour
	{
		[Serializable]
		public struct ReorderableListEventStruct
		{
			public GameObject DroppedObject;

			public int FromIndex;

			public ReorderableList FromList;

			public bool IsAClone;

			public GameObject SourceObject;

			public int ToIndex;

			public ReorderableList ToList;

			public void Cancel()
			{
			}
		}

		[Serializable]
		public class ReorderableListHandler : UnityEvent<ReorderableListEventStruct>
		{
		}

		[Tooltip("Child container with re-orderable items in a layout group")]
		public LayoutGroup ContentLayout;

		[Tooltip("Parent area to draw the dragged element on top of containers. Defaults to the root Canvas")]
		public RectTransform DraggableArea;

		[Tooltip("Can items be dragged from the container?")]
		public bool IsDraggable;

		[Tooltip("Should the draggable components be removed or cloned?")]
		public bool CloneDraggedObject;

		[Tooltip("Can new draggable items be dropped in to the container?")]
		public bool IsDropable;

		[Tooltip("Should dropped items displace a current item if the list is full?\n Depending on the dropped items origin list, the displaced item may be added, dropped in space or deleted.")]
		public bool IsDisplacable;

		[Tooltip("Should items being dragged over this list have their sizes equalized?")]
		public bool EqualizeSizesOnDrag;

		public int maxItems;

		[Header("UI Re-orderable Events")]
		public ReorderableListHandler OnElementDropped;

		public ReorderableListHandler OnElementGrabbed;

		public ReorderableListHandler OnElementRemoved;

		public ReorderableListHandler OnElementAdded;

		public ReorderableListHandler OnElementDisplacedFrom;

		public ReorderableListHandler OnElementDisplacedTo;

		public ReorderableListHandler OnElementDisplacedFromReturned;

		public ReorderableListHandler OnElementDisplacedToReturned;

		public ReorderableListHandler OnElementDroppedWithMaxItems;

		private RectTransform _content;

		private ReorderableListContent _listContent;

		public RectTransform Content => null;

		private Canvas GetCanvas()
		{
			return null;
		}

		public void Refresh()
		{
		}

		private void Start()
		{
		}

		public void TestReOrderableListTarget(ReorderableListEventStruct item)
		{
		}
	}
}
