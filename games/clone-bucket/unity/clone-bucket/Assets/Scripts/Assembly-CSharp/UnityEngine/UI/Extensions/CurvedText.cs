namespace UnityEngine.UI.Extensions
{
	[RequireComponent(typeof(Text), typeof(RectTransform))]
	[AddComponentMenu("UI/Effects/Extensions/Curved Text")]
	public class CurvedText : BaseMeshEffect
	{
		[SerializeField]
		private AnimationCurve _curveForText;

		[SerializeField]
		private float _curveMultiplier;

		private RectTransform rectTrans;

		public AnimationCurve CurveForText
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public float CurveMultiplier
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		protected override void Awake()
		{
		}

		protected override void OnEnable()
		{
		}

		public override void ModifyMesh(VertexHelper vh)
		{
		}

		protected override void OnRectTransformDimensionsChange()
		{
		}
	}
}
