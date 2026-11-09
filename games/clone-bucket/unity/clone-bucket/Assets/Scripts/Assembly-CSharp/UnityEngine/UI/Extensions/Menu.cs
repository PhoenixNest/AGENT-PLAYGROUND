namespace UnityEngine.UI.Extensions
{
	public abstract class Menu<T> : Menu where T : Menu<T>
	{
		public static T Instance { get; private set; }

		protected virtual void Awake()
		{
		}

		protected virtual void OnDestroy()
		{
		}

		protected static void Open()
		{
		}

		protected static void Close()
		{
		}

		public override void OnBackPressed()
		{
		}
	}
	public abstract class Menu : MonoBehaviour
	{
		[Tooltip("Destroy the Game Object when menu is closed (reduces memory usage)")]
		public bool DestroyWhenClosed;

		[Tooltip("Disable menus that are under this one in the stack")]
		public bool DisableMenusUnderneath;

		public abstract void OnBackPressed();
	}
}
