using System.Collections.Generic;

namespace CrazyLabs.Extensions.Database
{
	public struct DataSetParsedData<T>
	{
		public Dictionary<string, Dictionary<string, T>> parsedData;

		public DataSetParsedData(DataSetConfig dataSet)
		{
			parsedData = null;
		}

		public void OverrideField(string entryId, string parameterId, string newValue)
		{
		}
	}
}
