namespace UnityEngine.UI.Extensions
{
	[ExecuteInEditMode]
	public class CardExpanding3D : MonoBehaviour
	{
		[SerializeField]
		private float lerpSpeed;

		[SerializeField]
		private float cornerSize;

		[Header("Parts")]
		public RectTransform[] cardCorners;

		public RectTransform[] cardEdges;

		public RectTransform cardCenter;

		[Header("Card Info")]
		[Tooltip("Positions and sizes card to its current transform.")]
		public bool cardAutoSize;

		public Vector2 cardSize;

		public Vector2 cardPosition;

		[Range(1f, 96f)]
		public int cardSuperness;

		[Tooltip("Positions and sizes the page to the top third of the screen.")]
		[Header("Page Info")]
		public bool pageAutoSize;

		public Vector2 pageSize;

		public Vector2 pagePosition;

		[Range(1f, 96f)]
		public int pageSuperness;

		private int animationActive;

		private Vector2[] nextCornerPos;

		private Vector2[] nextEdgePos;

		private Vector2[] nextEdgeScale;

		private Vector2 nextCenterScale;

		private Vector2 nextPos;

		private int nextSuperness;

		private RectTransform rect;

		private Vector2 nextMin;

		private Vector2 nextMax;

		private void Start()
		{
		}

		private void Update()
		{
		}

		public void ToggleCard()
		{
		}
	}
}
