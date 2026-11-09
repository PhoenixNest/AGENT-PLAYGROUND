using System;

namespace UnityEngine.UI.Extensions
{
	public interface IFancyGridViewContext : IFancyScrollRectContext, IFancyCellGroupContext
	{
		Func<float> GetStartAxisSpacing { get; set; }

		Func<float> GetCellSize { get; set; }
	}
}
