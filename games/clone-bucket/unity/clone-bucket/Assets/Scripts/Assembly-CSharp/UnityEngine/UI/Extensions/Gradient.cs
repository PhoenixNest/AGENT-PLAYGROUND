namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Effects/Extensions/Gradient")]
	public class Gradient : BaseMeshEffect
	{
		[SerializeField]
		private GradientMode _gradientMode;

		[SerializeField]
		private GradientDir _gradientDir;

		[SerializeField]
		private bool _overwriteAllColor;

		[SerializeField]
		private Color _vertex1;

		[SerializeField]
		private Color _vertex2;

		private Graphic targetGraphic;

		public GradientMode GradientMode
		{
			get
			{
				return GradientMode.Global;
			}
			set
			{
			}
		}

		public GradientDir GradientDir
		{
			get
			{
				return GradientDir.Vertical;
			}
			set
			{
			}
		}

		public bool OverwriteAllColor
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public Color Vertex1
		{
			get
			{
				return default;
			}
			set
			{
			}
		}

		public Color Vertex2
		{
			get
			{
				return default;
			}
			set
			{
			}
		}

		protected override void Awake()
		{
		}

		public override void ModifyMesh(VertexHelper vh)
		{
		}

		private bool CompareCarefully(Color col1, Color col2)
		{
			return false;
		}
	}
}
