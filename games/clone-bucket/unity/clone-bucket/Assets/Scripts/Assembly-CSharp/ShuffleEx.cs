using System;
using System.Collections.Generic;

public static class ShuffleEx
{
	private static Random rng = new Random();

	public static void Shuffle<T>(this IList<T> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException(nameof(list));
		}
		for (int i = list.Count - 1; i > 0; i--)
		{
			int j = rng.Next(i + 1);
			T tmp = list[i];
			list[i] = list[j];
			list[j] = tmp;
		}
	}
}
