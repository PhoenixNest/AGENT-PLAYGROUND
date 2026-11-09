using System.Collections.Generic;

namespace UnityEngine.UI.Extensions
{
	public abstract class FancyScrollView<TItemData, TContext> : MonoBehaviour where TContext : class, new()
	{
		[Range(0.01f, 1f)]
		[SerializeField]
		protected float cellInterval;

		[SerializeField]
		[Range(0f, 1f)]
		protected float scrollOffset;

		[SerializeField]
		protected bool loop;

		[SerializeField]
		protected Transform cellContainer;

		private readonly IList<FancyCell<TItemData, TContext>> pool;

		protected bool initialized;

		protected float currentPosition;

		protected abstract GameObject CellPrefab { get; }

		protected IList<TItemData> ItemsSource { get; set; }

		protected TContext Context { get; }

		protected virtual void Initialize()
		{
		}

		protected virtual void UpdateContents(IList<TItemData> itemsSource)
		{
		}

		protected virtual void Relayout()
		{
		}

		protected virtual void Refresh()
		{
		}

		protected virtual void UpdatePosition(float position)
		{
		}

		private void UpdatePosition(float position, bool forceRefresh)
		{
		}

		private void ResizePool(float firstPosition)
		{
		}

		private void UpdateCells(float firstPosition, int firstIndex, bool forceRefresh)
		{
		}

		private int CircularIndex(int i, int size)
		{
			return 0;
		}
	}
	public abstract class FancyScrollView<TItemData> : FancyScrollView<TItemData, NullContext>
	{
	}
}
