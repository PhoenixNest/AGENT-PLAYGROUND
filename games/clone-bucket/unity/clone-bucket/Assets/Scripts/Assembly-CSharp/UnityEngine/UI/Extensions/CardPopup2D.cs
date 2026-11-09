namespace UnityEngine.UI.Extensions
{
	[RequireComponent(typeof(Rigidbody))]
	public class CardPopup2D : MonoBehaviour
	{
		[SerializeField]
		private float rotationSpeed;

		[SerializeField]
		private float centeringSpeed;

		[SerializeField]
		private bool singleScene;

		private Rigidbody rbody;

		private bool isFalling;

		private Vector3 cardFallRotation;

		private bool fallToZero;

		private float startZPos;

		private void Start()
		{
		}

		private void Update()
		{
		}

		public void CardEnter()
		{
		}

		public void CardFallAway(float fallRotation)
		{
		}
	}
}
