using System;
using System.Globalization;

public class UIUtils
{
	private static readonly string[] Suffixes = { "K", "M", "B", "T" };

	public static string ToSI(double d)
	{
		if (Math.Abs(d) < 1000.0 && Math.Abs(Math.Round(d)) < 1000.0)
		{
			return ToFormat(d);
		}

		double scaled = d;
		int index = -1;
		while (Math.Abs(scaled) >= 1000.0 && index < Suffixes.Length - 1)
		{
			scaled /= 1000.0;
			index++;
		}

		string digits = Math.Abs(scaled) < 10.0
			? ToFormat(scaled)
			: Math.Round(scaled).ToString("0", CultureInfo.InvariantCulture);

		if (Math.Abs(Math.Round(scaled)) >= 1000.0 && index < Suffixes.Length - 1)
		{
			scaled /= 1000.0;
			index++;
			digits = ToFormat(scaled);
		}

		return digits + Suffixes[index];
	}

	private static string ToFormat(double value)
	{
		return value.ToString("0.#", CultureInfo.InvariantCulture);
	}
}
