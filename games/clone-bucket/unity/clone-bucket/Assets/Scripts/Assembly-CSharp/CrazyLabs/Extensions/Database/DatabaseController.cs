using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CrazyLabs.Extensions.Database
{
	public class DatabaseController : MonoBehaviour
	{
		[CompilerGenerated]
		private static Action m_OnInitialized;

		[SerializeField]
		private DatabaseConfig _config;

		private static DatabaseController _instance;

		private static string _mainDataSetId;

		private Dictionary<string, List<string>> _dataSetKeys;

		private Dictionary<string, DataSetParsedData<int>> _dataSetIntegerData;

		private Dictionary<string, DataSetParsedData<float>> _dataSetFloatData;

		private Dictionary<string, DataSetParsedData<string>> _dataSetStringData;

		private Dictionary<string, DataSetParsedData<bool>> _dataSetBoolData;

		public static bool IsInitialized { get; private set; }

		public static event Action OnInitialized
		{
			add
			{
				m_OnInitialized = (Action)Delegate.Combine(m_OnInitialized, value);
			}
			remove
			{
				m_OnInitialized = (Action)Delegate.Remove(m_OnInitialized, value);
			}
		}

		private void Awake()
		{
			_instance = this;
			IsInitialized = false;
			_dataSetKeys = new Dictionary<string, List<string>>();
			_dataSetIntegerData = new Dictionary<string, DataSetParsedData<int>>();
			_dataSetFloatData = new Dictionary<string, DataSetParsedData<float>>();
			_dataSetStringData = new Dictionary<string, DataSetParsedData<string>>();
			_dataSetBoolData = new Dictionary<string, DataSetParsedData<bool>>();
			InitializeDatabase();
		}

		private void InitializeDatabase()
		{
			_mainDataSetId = _config != null ? _config.mainDataSetId : null;
			if (_config == null)
			{
				Debug.LogError("[MM3:DB] DatabaseController has no DatabaseConfig; no data sets loaded");
			}

			TryOverrideDataSetsWithRemoteFiles();
			InitializeAllDataSets();

			IsInitialized = true;

			if (m_OnInitialized != null)
			{
				m_OnInitialized();
			}
		}

		private void TryOverrideDataSetsWithRemoteFiles()
		{
		}

		private void InitializeAllDataSets()
		{
			if (_config == null || _config.allDataSets == null)
			{
				return;
			}

			foreach (DataSetConfig dataSet in _config.allDataSets)
			{
				if (dataSet == null)
				{
					continue;
				}

				ParseDataSet(dataSet);
			}
		}

		private void ParseDataSet(DataSetConfig dataSet)
		{
			string dataSetId = dataSet.id;
			try
			{
				dataSet.InitializeDataSet();

				var integers = new DataSetParsedData<int>(dataSet);
				integers.parsedData = new Dictionary<string, Dictionary<string, int>>();
				var floats = new DataSetParsedData<float>(dataSet);
				floats.parsedData = new Dictionary<string, Dictionary<string, float>>();
				var strings = new DataSetParsedData<string>(dataSet);
				strings.parsedData = new Dictionary<string, Dictionary<string, string>>();
				var bools = new DataSetParsedData<bool>(dataSet);
				bools.parsedData = new Dictionary<string, Dictionary<string, bool>>();

				foreach (KeyValuePair<string, Dictionary<string, string>> row in dataSet.Data)
				{
					var rowIntegers = new Dictionary<string, int>();
					var rowFloats = new Dictionary<string, float>();
					var rowStrings = new Dictionary<string, string>();
					var rowBools = new Dictionary<string, bool>();
					foreach (KeyValuePair<string, string> property in row.Value)
					{
						string text = property.Value ?? string.Empty;
						rowStrings[property.Key] = text;
						if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
						{
							rowIntegers[property.Key] = intValue;
						}

						if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue))
						{
							rowFloats[property.Key] = floatValue;
						}

						if (bool.TryParse(text, out bool boolValue))
						{
							rowBools[property.Key] = boolValue;
						}
						else if (text == "1" || text == "0")
						{
							rowBools[property.Key] = text == "1";
						}
					}

					integers.parsedData[row.Key] = rowIntegers;
					floats.parsedData[row.Key] = rowFloats;
					strings.parsedData[row.Key] = rowStrings;
					bools.parsedData[row.Key] = rowBools;
				}

				_dataSetKeys[dataSetId] = new List<string>(dataSet.Entries);
				_dataSetIntegerData[dataSetId] = integers;
				_dataSetFloatData[dataSetId] = floats;
				_dataSetStringData[dataSetId] = strings;
				_dataSetBoolData[dataSetId] = bools;
			}
			catch (Exception exception)
			{
				Debug.LogError("[MM3:DB] Failed to parse data set '" + dataSetId + "': " + exception.Message);
			}
		}

		private void CheckIndividualParametersRemoteConfiguration()
		{
		}

		public static int GetInt(string dataSetId, string itemId, string propertyId)
		{
			return TryGetValue(_instance?._dataSetIntegerData, dataSetId, itemId, propertyId, out int value) ? value : 0;
		}

		public static int GetInt(string itemId, string propertyId)
		{
			return GetInt(_mainDataSetId, itemId, propertyId);
		}

		public static float GetFloat(string dataSetId, string itemId, string propertyId)
		{
			return TryGetValue(_instance?._dataSetFloatData, dataSetId, itemId, propertyId, out float value) ? value : 0f;
		}

		public static float GetFloat(string itemId, string propertyId)
		{
			return GetFloat(_mainDataSetId, itemId, propertyId);
		}

		public static string GetString(string dataSetId, string itemId, string propertyId)
		{
			return TryGetValue(_instance?._dataSetStringData, dataSetId, itemId, propertyId, out string value) ? value : null;
		}

		public static string GetString(string itemId, string propertyId)
		{
			return GetString(_mainDataSetId, itemId, propertyId);
		}

		public static bool GetBool(string dataSetId, string itemId, string propertyId)
		{
			return TryGetValue(_instance?._dataSetBoolData, dataSetId, itemId, propertyId, out bool value) && value;
		}

		public static bool GetBool(string itemId, string propertyId)
		{
			return GetBool(_mainDataSetId, itemId, propertyId);
		}

		public static List<string> GetAllKeys(string dataSetId)
		{
			if (_instance?._dataSetKeys == null || dataSetId == null)
			{
				return new List<string>();
			}

			return _instance._dataSetKeys.TryGetValue(dataSetId, out List<string> keys) ? new List<string>(keys) : new List<string>();
		}

		private static bool TryGetValue<T>(Dictionary<string, DataSetParsedData<T>> store, string dataSetId, string itemId, string propertyId, out T value)
		{
			value = default;
			if (store == null || dataSetId == null || itemId == null || propertyId == null)
			{
				return false;
			}

			if (!store.TryGetValue(dataSetId, out DataSetParsedData<T> parsed) || parsed.parsedData == null)
			{
				return false;
			}

			if (!parsed.parsedData.TryGetValue(itemId, out Dictionary<string, T> properties))
			{
				return false;
			}

			if (!properties.TryGetValue(propertyId, out value))
			{
				return false;
			}

			return true;
		}
	}
}
