using System;
using UnityEngine.Events;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/Input Field Submit")]
	[RequireComponent(typeof(InputField))]
	public class InputFieldEnterSubmit : MonoBehaviour
	{
		[Serializable]
		public class EnterSubmitEvent : UnityEvent<string>
		{
		}

		public EnterSubmitEvent EnterSubmit;

		public bool defocusInput;

		private InputField _input;

		private void Awake()
		{
		}

		public void OnEndEdit(string txt)
		{
		}
	}
}
