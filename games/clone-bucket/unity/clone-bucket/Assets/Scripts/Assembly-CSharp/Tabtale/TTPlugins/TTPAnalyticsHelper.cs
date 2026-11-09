using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Tabtale.TTPlugins
{
	public class TTPAnalyticsHelper
	{
		public static IDictionary<string, object> GenerateProductsReceived(IDictionary<string, object>[] items, IDictionary<string, object> realCurrency, IDictionary<string, object>[] virtualCurrencies)
		{
			return null;
		}

		[Preserve]
		public static IDictionary<string, object> GenerateProductsSpent(IDictionary<string, object>[] items, IDictionary<string, object> realCurrency, IDictionary<string, object>[] virtualCurrencies)
		{
			return null;
		}

		[Preserve]
		public static IDictionary<string, object> GenerateItem(int itemAmount, string itemName, string itemType)
		{
			return null;
		}

		public static IDictionary<string, object> GenerateVirtualCurrency(int virtualCurrencyAmount, string virtualCurrencyName, string virtualCurrencyType)
		{
			return null;
		}

		[Preserve]
		public static IDictionary<string, object> GenerateRealCurrency(string realCurrencyAmount, string realCurrencyType)
		{
			return null;
		}

		private static int ConvertCurrency(string code, decimal value)
		{
			return 0;
		}

		private static IDictionary<string, int> GetISOValue()
		{
			return null;
		}
	}
}
