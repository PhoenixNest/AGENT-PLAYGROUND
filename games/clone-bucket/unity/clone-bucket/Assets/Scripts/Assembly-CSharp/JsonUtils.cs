using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public static class JsonUtils
{
	public static string ToJsonString(Dictionary<string, object> parameters)
	{
		if (parameters == null)
		{
			return "{}";
		}
		var sb = new StringBuilder("{");
		bool first = true;
		foreach (KeyValuePair<string, object> pair in parameters)
		{
			if (!first)
			{
				sb.Append(',');
			}
			first = false;
			sb.Append(Quote(pair.Key)).Append(':').Append(ValueToJson(pair.Value));
		}
		return sb.Append('}').ToString();
	}

	private static string ToJsonString(object[] array)
	{
		if (array == null)
		{
			return "[]";
		}
		var sb = new StringBuilder("[");
		for (int i = 0; i < array.Length; i++)
		{
			if (i > 0)
			{
				sb.Append(',');
			}
			sb.Append(ValueToJson(array[i]));
		}
		return sb.Append(']').ToString();
	}

	private static string ValueToJson(object value)
	{
		switch (value)
		{
			case null:
				return "null";
			case string s:
				return Quote(s);
			case bool b:
				return b ? "true" : "false";
			case int i:
				return i.ToString(CultureInfo.InvariantCulture);
			case long l:
				return l.ToString(CultureInfo.InvariantCulture);
			case float f:
				return float.IsNaN(f) || float.IsInfinity(f) ? "null" : f.ToString("R", CultureInfo.InvariantCulture);
			case double d:
				return double.IsNaN(d) || double.IsInfinity(d) ? "null" : d.ToString("R", CultureInfo.InvariantCulture);
			case Dictionary<string, object> dict:
				return ToJsonString(dict);
			case object[] arr:
				return ToJsonString(arr);
			default:
				return Quote(Convert.ToString(value, CultureInfo.InvariantCulture));
		}
	}

	private static string Quote(string text)
	{
		var sb = new StringBuilder("\"");
		foreach (char c in text ?? string.Empty)
		{
			switch (c)
			{
				case '"':
					sb.Append("\\\"");
					break;
				case '\\':
					sb.Append("\\\\");
					break;
				case '\n':
					sb.Append("\\n");
					break;
				case '\r':
					sb.Append("\\r");
					break;
				case '\t':
					sb.Append("\\t");
					break;
				default:
					if (c < 0x20)
					{
						sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
					}
					else
					{
						sb.Append(c);
					}
					break;
			}
		}
		return sb.Append('"').ToString();
	}
}
