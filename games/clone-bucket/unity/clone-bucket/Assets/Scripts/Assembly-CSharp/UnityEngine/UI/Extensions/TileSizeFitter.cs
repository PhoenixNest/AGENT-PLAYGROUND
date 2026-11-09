using System;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
	[RequireComponent(typeof(RectTransform))]
	[AddComponentMenu("Layout/Extensions/Tile Size Fitter")]
	[ExecuteInEditMode]
	public class TileSizeFitter : UIBehaviour, ILayoutSelfController, ILayoutController
	{
		[SerializeField]
		private Vector2 m_Border;

		[SerializeField]
		private Vector2 m_TileSize;

		[NonSerialized]
		private RectTransform m_Rect;

		private DrivenRectTransformTracker m_Tracker;

		public Vector2 Border
		{
			get
			{
				return default;
			}
			set
			{
			}
		}

		public Vector2 TileSize
		{
			get
			{
				return default;
			}
			set
			{
			}
		}

		private RectTransform rectTransform => null;

		protected override void OnEnable()
		{
		}

		protected override void OnDisable()
		{
		}

		protected override void OnRectTransformDimensionsChange()
		{
		}

		private void UpdateRect()
		{
		}

		private Vector2 GetParentSize()
		{
			return default;
		}

		public virtual void SetLayoutHorizontal()
		{
		}

		public virtual void SetLayoutVertical()
		{
		}

		protected void SetDirty()
		{
		}
	}
}
