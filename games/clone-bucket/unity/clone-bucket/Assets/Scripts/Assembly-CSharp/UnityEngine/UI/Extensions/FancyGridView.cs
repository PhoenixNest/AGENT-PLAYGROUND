using System;
using System.Collections.Generic;
using UnityEngine.UI.Extensions.EasingCore;

namespace UnityEngine.UI.Extensions
{
	public abstract class FancyGridView<TItemData, TContext> : FancyScrollRect<TItemData[], TContext> where TContext : class, IFancyGridViewContext, new()
	{
		protected abstract class DefaultCellGroup : FancyCellGroup<TItemData, TContext>
		{
		}

		[SerializeField]
		protected float startAxisSpacing;

		[SerializeField]
		protected int startAxisCellCount;

		[SerializeField]
		protected Vector2 cellSize;

		private GameObject cellGroupTemplate;

		protected sealed override GameObject CellPrefab => null;

		protected override float CellSize => 0f;

		public int DataCount { get; private set; }

		protected override void Initialize()
		{
		}

		protected abstract void SetupCellTemplate();

		protected virtual void Setup<TGroup>(FancyCell<TItemData, TContext> cellTemplate) where TGroup : FancyCell<TItemData[], TContext>
		{
		}

		public virtual void UpdateContents(IList<TItemData> items)
		{
		}

		protected override void JumpTo(int itemIndex, float alignment = 0.5f)
		{
		}

		protected override void ScrollTo(int itemIndex, float duration, float alignment = 0.5f, Action onComplete = null)
		{
		}

		protected override void ScrollTo(int itemIndex, float duration, Ease easing, float alignment = 0.5f, Action onComplete = null)
		{
		}
	}
	public abstract class FancyGridView<TItemData> : FancyGridView<TItemData, FancyGridViewContext>
	{
	}
}
