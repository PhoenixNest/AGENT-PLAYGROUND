using System;
using System.Runtime.CompilerServices;

namespace UnityEngine.UI.Extensions
{
	public class FancyScrollRectContext : IFancyScrollRectContext
	{
		ScrollDirection IFancyScrollRectContext.ScrollDirection
		{
			[CompilerGenerated]
			get
			{
				return ScrollDirection.Vertical;
			}
			[CompilerGenerated]
			set
			{
			}
		}

		Func<(float ScrollSize, float ReuseMargin)> IFancyScrollRectContext.CalculateScrollSize
		{
			[CompilerGenerated]
			get
			{
				return null;
			}
			[CompilerGenerated]
			set
			{
			}
		}
	}
}
