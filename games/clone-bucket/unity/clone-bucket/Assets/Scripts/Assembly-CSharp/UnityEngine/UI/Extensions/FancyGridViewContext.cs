using System;
using System.Runtime.CompilerServices;

namespace UnityEngine.UI.Extensions
{
	public class FancyGridViewContext : IFancyGridViewContext, IFancyScrollRectContext, IFancyCellGroupContext
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

		GameObject IFancyCellGroupContext.CellTemplate
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

		Func<int> IFancyCellGroupContext.GetGroupCount
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

		Func<float> IFancyGridViewContext.GetStartAxisSpacing
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

		Func<float> IFancyGridViewContext.GetCellSize
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
