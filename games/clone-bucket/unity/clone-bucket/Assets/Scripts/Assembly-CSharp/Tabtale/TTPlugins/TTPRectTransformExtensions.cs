using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tabtale.TTPlugins
{
	internal static class TTPRectTransformExtensions
	{
		private static Dictionary<RectTransform.Edge, Func<RectTransform, RectTransform, float>> _getInsetFromParentEdgeMappedActions;

		private static Dictionary<RectTransform.Edge, Action<RectTransform, RectTransform, float, float>> _setInsetAndSizeFromParentEdgeWithCurrentAnchorsMappedActions;

		public static float GetInsetFromParentTopEdge(this RectTransform child, RectTransform parentHint)
		{
			return 0f;
		}

		public static float GetInsetFromParentBottomEdge(this RectTransform child, RectTransform parentHint)
		{
			return 0f;
		}

		public static float GetInsetFromParentLeftEdge(this RectTransform child, RectTransform parentHint)
		{
			return 0f;
		}

		public static float GetInsetFromParentRightEdge(this RectTransform child, RectTransform parentHint)
		{
			return 0f;
		}

		public static float GetInsetFromParentEdge(this RectTransform child, RectTransform parentHint, RectTransform.Edge parentEdge)
		{
			return 0f;
		}

		public static void SetSizeFromParentEdgeWithCurrentAnchors(this RectTransform child, RectTransform.Edge fixedEdge, float newSize)
		{
		}

		public static void SetSizeFromParentEdgeWithCurrentAnchors(this RectTransform child, RectTransform parentHint, RectTransform.Edge fixedEdge, float newSize)
		{
		}

		public static void SetInsetAndSizeFromParentEdgeWithCurrentAnchors(this RectTransform child, RectTransform.Edge fixedEdge, float newInset, float newSize)
		{
		}

		public static void SetInsetAndSizeFromParentEdgeWithCurrentAnchors(this RectTransform child, RectTransform parentHint, RectTransform.Edge fixedEdge, float newInset, float newSize)
		{
		}
	}
}
