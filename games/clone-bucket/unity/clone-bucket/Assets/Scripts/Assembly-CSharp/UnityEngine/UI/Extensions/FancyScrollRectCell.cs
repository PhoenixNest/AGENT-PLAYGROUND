namespace UnityEngine.UI.Extensions
{
	public abstract class FancyScrollRectCell<TItemData, TContext> : FancyCell<TItemData, TContext> where TContext : class, IFancyScrollRectContext, new()
	{
		public override void UpdatePosition(float position)
		{
		}

		protected virtual void UpdatePosition(float normalizedPosition, float localPosition)
		{
		}
	}
	public abstract class FancyScrollRectCell<TItemData> : FancyScrollRectCell<TItemData, FancyScrollRectContext>
	{
		public sealed override void SetContext(FancyScrollRectContext context)
		{
		}
	}
}
