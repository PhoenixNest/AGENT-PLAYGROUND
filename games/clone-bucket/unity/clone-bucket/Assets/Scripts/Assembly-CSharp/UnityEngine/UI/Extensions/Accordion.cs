namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/Accordion/Accordion Group")]
	[RequireComponent(typeof(HorizontalOrVerticalLayoutGroup), typeof(ContentSizeFitter), typeof(ToggleGroup))]
	public class Accordion : MonoBehaviour
	{
		public enum Transition
		{
			Instant = 0,
			Tween = 1
		}

		private bool m_expandVertical;

		[SerializeField]
		private Transition m_Transition;

		[SerializeField]
		private float m_TransitionDuration;

		[HideInInspector]
		public bool ExpandVerticval => false;

		public Transition transition
		{
			get
			{
				return Transition.Instant;
			}
			set
			{
			}
		}

		public float transitionDuration
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		private void Awake()
		{
		}
	}
}
