using System;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/ScrollRectEx")]
	public class ScrollRectEx : ScrollRect
	{
		private bool routeToParent;

		private void DoForParents<T>(Action<T> action) where T : IEventSystemHandler
		{
		}

		public override void OnInitializePotentialDrag(PointerEventData eventData)
		{
		}

		public override void OnDrag(PointerEventData eventData)
		{
		}

		public override void OnBeginDrag(PointerEventData eventData)
		{
		}

		public override void OnEndDrag(PointerEventData eventData)
		{
		}

		public override void OnScroll(PointerEventData eventData)
		{
		}
	}
}
