namespace UnityEngine.UI.Extensions
{
	public abstract class SimpleMenu<T> : Menu<T> where T : SimpleMenu<T>
	{
		public static void Show()
		{
		}

		public static void Hide()
		{
		}
	}
}
