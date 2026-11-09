namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Effects/Extensions/SoftMaskScript")]
	[ExecuteInEditMode]
	public class SoftMaskScript : MonoBehaviour
	{
		private Material mat;

		private Canvas cachedCanvas;

		private Transform cachedCanvasTransform;

		private readonly Vector3[] m_WorldCorners;

		private readonly Vector3[] m_CanvasCorners;

		[Tooltip("The area that is to be used as the container.")]
		public RectTransform MaskArea;

		[Tooltip("Texture to be used to do the soft alpha")]
		public Texture AlphaMask;

		[Range(0f, 1f)]
		[Tooltip("At what point to apply the alpha min range 0-1")]
		public float CutOff;

		[Tooltip("Implement a hard blend based on the Cutoff")]
		public bool HardBlend;

		[Tooltip("Flip the masks alpha value")]
		public bool FlipAlphaMask;

		[Tooltip("If a different Mask Scaling Rect is given, and this value is true, the area around the mask will not be clipped")]
		public bool DontClipMaskScalingRect;

		private Vector2 maskOffset;

		private Vector2 maskScale;

		private void Start()
		{
		}

		private void Update()
		{
		}

		private void SetMask()
		{
		}

		public Rect GetCanvasRect()
		{
			return default;
		}
	}
}
