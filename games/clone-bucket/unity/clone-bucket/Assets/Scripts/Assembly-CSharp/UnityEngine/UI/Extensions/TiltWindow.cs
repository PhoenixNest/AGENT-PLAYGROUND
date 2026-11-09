using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
	public class TiltWindow : MonoBehaviour, IDragHandler, IEventSystemHandler
	{
		public Vector2 range;

		private Transform mTrans;

		private Quaternion mStart;

		private Vector2 mRot;

		private Vector2 m_screenPos;

		private void Start()
		{
		}

		private void Update()
		{
		}

		public void OnDrag(PointerEventData eventData)
		{
		}
	}
}
