using System;
using System.Collections.Generic;

namespace UnityEngine.UI.Extensions
{
	[RequireComponent(typeof(CanvasRenderer))]
	public class UIPrimitiveBase : MaskableGraphic, ILayoutElement, ICanvasRaycastFilter
	{
		protected static Material s_ETC1DefaultUI;

		private List<Vector2> outputList;

		[SerializeField]
		private Sprite m_Sprite;

		[NonSerialized]
		private Sprite m_OverrideSprite;

		internal float m_EventAlphaThreshold;

		[SerializeField]
		private ResolutionMode m_improveResolution;

		[SerializeField]
		protected float m_Resolution;

		[SerializeField]
		private bool m_useNativeSize;

		public Sprite sprite
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public Sprite overrideSprite
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		protected Sprite activeSprite => null;

		public float eventAlphaThreshold
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public ResolutionMode ImproveResolution
		{
			get
			{
				return ResolutionMode.None;
			}
			set
			{
			}
		}

		public float Resolution
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public bool UseNativeSize
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public static Material defaultETC1GraphicMaterial => null;

		public override Texture mainTexture => null;

		public bool hasBorder => false;

		public float pixelsPerUnit => 0f;

		public override Material material
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public virtual float minWidth => 0f;

		public virtual float preferredWidth => 0f;

		public virtual float flexibleWidth => 0f;

		public virtual float minHeight => 0f;

		public virtual float preferredHeight => 0f;

		public virtual float flexibleHeight => 0f;

		public virtual int layoutPriority => 0;

		protected UIPrimitiveBase()
		{
		}

		protected UIVertex[] SetVbo(Vector2[] vertices, Vector2[] uvs)
		{
			return null;
		}

		protected Vector2[] IncreaseResolution(Vector2[] input)
		{
			return null;
		}

		protected List<Vector2> IncreaseResolution(List<Vector2> input)
		{
			return null;
		}

		protected virtual void GeneratedUVs()
		{
		}

		protected virtual void ResolutionToNativeSize(float distance)
		{
		}

		public virtual void CalculateLayoutInputHorizontal()
		{
		}

		public virtual void CalculateLayoutInputVertical()
		{
		}

		public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
		{
			return false;
		}

		private Vector2 MapCoordinate(Vector2 local, Rect rect)
		{
			return default;
		}

		private Vector4 GetAdjustedBorders(Vector4 border, Rect rect)
		{
			return default;
		}

		protected override void OnEnable()
		{
		}
	}
}
