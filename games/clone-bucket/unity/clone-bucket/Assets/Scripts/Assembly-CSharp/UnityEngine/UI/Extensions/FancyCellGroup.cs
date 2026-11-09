namespace UnityEngine.UI.Extensions
{
	public abstract class FancyCellGroup<TItemData, TContext> : FancyCell<TItemData[], TContext> where TContext : class, IFancyCellGroupContext, new()
	{
		protected virtual FancyCell<TItemData, TContext>[] Cells { get; private set; }

		protected virtual FancyCell<TItemData, TContext>[] InstantiateCells()
		{
			return null;
		}

		public override void Initialize()
		{
		}

		public override void UpdateContent(TItemData[] contents)
		{
		}

		public override void UpdatePosition(float position)
		{
		}
	}
}
