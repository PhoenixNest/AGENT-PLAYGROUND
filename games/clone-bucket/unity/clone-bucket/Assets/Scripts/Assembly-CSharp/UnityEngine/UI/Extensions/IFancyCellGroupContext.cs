using System;

namespace UnityEngine.UI.Extensions
{
	public interface IFancyCellGroupContext
	{
		GameObject CellTemplate { get; set; }

		Func<int> GetGroupCount { get; set; }
	}
}
