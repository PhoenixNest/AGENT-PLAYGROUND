namespace UnityEngine.UI.Extensions
{
	public static class UIExtensionsInputManager
	{
		public static Vector3 MousePosition => default;

		public static Vector3 MouseScrollDelta => default;

		public static bool GetMouseButton(int button)
		{
			return false;
		}

		public static bool GetMouseButtonDown(int button)
		{
			return false;
		}

		public static bool GetMouseButtonUp(int button)
		{
			return false;
		}

		public static bool GetButton(string input)
		{
			return false;
		}

		public static bool GetButtonDown(string input)
		{
			return false;
		}

		public static bool GetButtonUp(string input)
		{
			return false;
		}

		public static bool GetKey(KeyCode key)
		{
			return false;
		}

		public static bool GetKeyDown(KeyCode key)
		{
			return false;
		}

		public static bool GetKeyUp(KeyCode key)
		{
			return false;
		}

		public static float GetAxisRaw(string axis)
		{
			return 0f;
		}
	}
}
