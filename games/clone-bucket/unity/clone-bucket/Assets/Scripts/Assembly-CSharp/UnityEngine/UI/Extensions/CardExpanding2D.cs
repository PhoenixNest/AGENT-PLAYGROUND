namespace UnityEngine.UI.Extensions
{
	public class CardExpanding2D : MonoBehaviour
	{
		[SerializeField]
		private float lerpSpeed;

		[SerializeField]
		private RectTransform buttonRect;

		private Vector2 closeButtonMin;

		private Vector2 closeButtonMax;

		[SerializeField]
		private Vector2 cardSize;

		[SerializeField]
		private Vector2 pageSize;

		private Vector2 cardCenter;

		private Vector2 pageCenter;

		private Vector2 cardMin;

		private Vector2 cardMax;

		private Vector2 pageMin;

		private Vector2 pageMax;

		private RectTransform rectTrans;

		private int animationActive;

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
