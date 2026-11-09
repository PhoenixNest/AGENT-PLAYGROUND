namespace UnityEngine.UI.Extensions
{
	public static class ExtentionMethods
	{
		public static T GetOrAddComponent<T>(this GameObject child) where T : Component
		{
			return null;
		}

		public static bool IsPrefab(this GameObject gameObject)
		{
			return false;
		}
	}
}
