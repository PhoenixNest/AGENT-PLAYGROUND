namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/InputFocus")]
	[RequireComponent(typeof(InputField))]
	public class InputFocus : MonoBehaviour
	{
		protected InputField _inputField;

		public bool _ignoreNextActivation;

		private void Start()
		{
		}

		private void Update()
		{
		}

		public void buttonPressed()
		{
		}

		public void OnEndEdit(string textString)
		{
		}
	}
}
