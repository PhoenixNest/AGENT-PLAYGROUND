using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
	[RequireComponent(typeof(InputField))]
	[AddComponentMenu("UI/Extensions/Return Key Trigger")]
	public class ReturnKeyTriggersButton : MonoBehaviour, ISubmitHandler, IEventSystemHandler
	{
		private EventSystem _system;

		public Button button;

		private bool highlight;

		public float highlightDuration;

		private void Start()
		{
		}

		private void RemoveHighlight()
		{
		}

		public void OnSubmit(BaseEventData eventData)
		{
		}
	}
}
