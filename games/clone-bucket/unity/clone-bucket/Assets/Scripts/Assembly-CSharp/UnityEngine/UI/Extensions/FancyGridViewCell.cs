namespace UnityEngine.UI.Extensions
{
	public abstract class FancyGridViewCell<TItemData, TContext> : FancyScrollRectCell<TItemData, TContext> where TContext : class, IFancyGridViewContext, new()
	{
		protected override void UpdatePosition(float normalizedPosition, float localPosition)
		{
		}
	}
	public abstract class FancyGridViewCell<TItemData> : FancyGridViewCell<TItemData, FancyGridViewContext>
	{
		public sealed override void SetContext(FancyGridViewContext context)
		{
		}
	}
}
