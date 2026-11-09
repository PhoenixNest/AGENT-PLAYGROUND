namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/Primitives/Cut Corners")]
	public class UICornerCut : UIPrimitiveBase
	{
		public Vector2 cornerSize;

		[Header("Corners to cut")]
		[SerializeField]
		private bool m_cutUL;

		[SerializeField]
		private bool m_cutUR;

		[SerializeField]
		private bool m_cutLL;

		[SerializeField]
		private bool m_cutLR;

		[SerializeField]
		[Tooltip("Up-Down colors become Left-Right colors")]
		private bool m_makeColumns;

		[Header("Color the cut bars differently")]
		[SerializeField]
		private bool m_useColorUp;

		[SerializeField]
		private Color32 m_colorUp;

		[SerializeField]
		private bool m_useColorDown;

		[SerializeField]
		private Color32 m_colorDown;

		public bool CutUL
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public bool CutUR
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public bool CutLL
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public bool CutLR
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public bool MakeColumns
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public bool UseColorUp
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public Color32 ColorUp
		{
			get
			{
				return default;
			}
			set
			{
			}
		}

		public bool UseColorDown
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public Color32 ColorDown
		{
			get
			{
				return default;
			}
			set
			{
			}
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
		}

		private static void AddSquare(Rect rect, Rect rectUV, Color32 color32, VertexHelper vh)
		{
		}

		private static void AddSquare(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Rect rectUV, Color32 color32, VertexHelper vh)
		{
		}

		private static int AddVert(float x, float y, Rect area, Color32 color32, VertexHelper vh)
		{
			return 0;
		}
	}
}
