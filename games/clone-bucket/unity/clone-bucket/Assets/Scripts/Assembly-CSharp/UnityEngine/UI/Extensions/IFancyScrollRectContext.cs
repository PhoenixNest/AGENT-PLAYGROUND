using System;

namespace UnityEngine.UI.Extensions
{
	public interface IFancyScrollRectContext
	{
		ScrollDirection ScrollDirection { get; set; }

		Func<(float ScrollSize, float ReuseMargin)> CalculateScrollSize { get; set; }
	}
}
