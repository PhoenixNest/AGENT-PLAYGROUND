using System;
using System.Collections.Generic;
using UnityEngine.UI.Extensions.EasingCore;

namespace UnityEngine.UI.Extensions
{
	[RequireComponent(typeof(Scroller))]
	public abstract class FancyScrollRect<TItemData, TContext> : FancyScrollView<TItemData, TContext> where TContext : class, IFancyScrollRectContext, new()
	{
		[SerializeField]
		protected float reuseCellMarginCount;

		[SerializeField]
		protected float paddingHead;

		[SerializeField]
		protected float paddingTail;

		[SerializeField]
		protected float spacing;

		private Scroller cachedScroller;

		protected abstract float CellSize { get; }

		protected virtual bool Scrollable => false;

		protected Scroller Scroller => null;

		private float ScrollLength => 0f;

		private float ViewportLength => 0f;

		private float PaddingHeadLength => 0f;

		private float MaxScrollPosition => 0f;

		protected override void Initialize()
		{
		}

		private void OnScrollerValueChanged(float p)
		{
		}

		private void ShrinkScrollbar(float offset)
		{
		}

		protected override void Refresh()
		{
		}

		protected override void Relayout()
		{
		}

		protected void RefreshScroller()
		{
		}

		protected override void UpdateContents(IList<TItemData> items)
		{
		}

		protected new void UpdatePosition(float position)
		{
		}

		protected virtual void JumpTo(int itemIndex, float alignment = 0.5f)
		{
		}

		protected virtual void ScrollTo(int index, float duration, float alignment = 0.5f, Action onComplete = null)
		{
		}

		protected virtual void ScrollTo(int index, float duration, Ease easing, float alignment = 0.5f, Action onComplete = null)
		{
		}

		protected void UpdateScrollbarSize(float viewportLength)
		{
		}

		protected float ToFancyScrollViewPosition(float position)
		{
			return 0f;
		}

		protected float ToScrollerPosition(float position)
		{
			return 0f;
		}

		protected float ToScrollerPosition(float position, float alignment = 0.5f)
		{
			return 0f;
		}

		protected void AdjustCellIntervalAndScrollOffset()
		{
		}

		protected virtual void OnValidate()
		{
		}
	}
	public abstract class FancyScrollRect<TItemData> : FancyScrollRect<TItemData, FancyScrollRectContext>
	{
	}
}
