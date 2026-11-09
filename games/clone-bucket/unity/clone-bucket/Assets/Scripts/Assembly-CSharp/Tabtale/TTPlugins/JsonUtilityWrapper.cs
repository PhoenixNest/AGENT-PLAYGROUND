using System;

namespace Tabtale.TTPlugins
{
	public class JsonUtilityWrapper
	{
		public static string ToJson(object obj)
		{
			return null;
		}

		public static string ToJson(object obj, bool prettyPrint)
		{
			return null;
		}

		public static T FromJson<T>(string json)
		{
			return default;
		}

		public static object FromJson(string json, Type type)
		{
			return null;
		}

		public static void FromJsonOverwrite(string json, object objectToOverwrite)
		{
		}
	}
}
