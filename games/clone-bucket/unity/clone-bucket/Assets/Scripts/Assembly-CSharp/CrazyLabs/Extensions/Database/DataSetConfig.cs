using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CrazyLabs.Extensions.Database
{
	[CreateAssetMenu(menuName = "Configs/DataSetConfig", fileName = "DataSetConfig")]
	public class DataSetConfig : ScriptableObject
	{
		[Header("Core")]
		public string id;

		[SerializeField]
		private TextAsset _dataAsset;

		[SerializeField]
		private EDataSetSeparatorType _separatorType;

		[SerializeField]
		private List<string> _serializedKeys;

		[Header("Remote File Override")]
		public string remoteFileName;

		public string remoteFeatureName;

		private string[] _customData;

		public List<string> Entries { get; private set; } = new List<string>();

		public List<string> Keys => _serializedKeys;

		public Dictionary<string, Dictionary<string, string>> Data { get; private set; } = new Dictionary<string, Dictionary<string, string>>();

		public void InitializeDataSet()
		{
			List<string> lines = GetAllDataLines();
			if (lines.Count == 0)
			{
				Debug.LogError("[MM3:DB] Data set '" + id + "' has no lines (asset missing?)");
				Entries = new List<string>();
				Data = new Dictionary<string, Dictionary<string, string>>();
				return;
			}

			Dictionary<string, int> keysToColumnsIds = DetectSerializedColumnsIds(lines);
			CreateDatabase(lines, keysToColumnsIds);
		}

		public void OverrideData(string[] data)
		{
			_customData = data;
			InitializeDataSet();
		}

		private void CreateDatabase(IReadOnlyList<string> textLines, IReadOnlyDictionary<string, int> keysToColumnsIds)
		{
			char separator = GetCurrentSplitSymbol();
			var ids = new List<string>();
			var rows = new Dictionary<string, Dictionary<string, string>>();
			BuildRows(textLines, separator, ids, rows);
			Entries = ids;
			Data = rows;
		}

		private List<string> GetAllDataLines()
		{
			string text = _customData != null && _customData.Length > 0
				? string.Join("\n", _customData)
				: (_dataAsset != null ? _dataAsset.text : string.Empty);
			return SplitLines(text);
		}

		private Dictionary<string, int> DetectSerializedColumnsIds(List<string> textLines)
		{
			return HeaderColumns(SplitFields(textLines[0], GetCurrentSplitSymbol()));
		}

		private char GetCurrentSplitSymbol()
		{
			switch (_separatorType)
			{
				case EDataSetSeparatorType.Tsv:
					return '\t';
				case EDataSetSeparatorType.Semicolon:
					return ';';
				default:
					return ',';
			}
		}

		public static (List<string> header, List<string> ids, Dictionary<string, Dictionary<string, string>> rows) ParseTable(string text, char separator)
		{
			List<string> lines = SplitLines(text);
			var header = lines.Count > 0 ? SplitFields(lines[0], separator) : new List<string>();
			var ids = new List<string>();
			var rows = new Dictionary<string, Dictionary<string, string>>();
			if (lines.Count > 0)
			{
				BuildRows(lines, separator, ids, rows);
			}

			return (header, ids, rows);
		}

		public static List<string> SplitFields(string line, char separator)
		{
			var fields = new List<string>();
			var current = new StringBuilder();
			bool inQuotes = false;
			for (int i = 0; i < line.Length; i++)
			{
				char c = line[i];
				if (inQuotes)
				{
					if (c == '"')
					{
						if (i + 1 < line.Length && line[i + 1] == '"')
						{
							current.Append('"');
							i++;
						}
						else
						{
							inQuotes = false;
						}
					}
					else
					{
						current.Append(c);
					}
				}
				else if (c == '"')
				{
					inQuotes = true;
				}
				else if (c == separator)
				{
					fields.Add(current.ToString().Trim());
					current.Length = 0;
				}
				else
				{
					current.Append(c);
				}
			}

			fields.Add(current.ToString().Trim());
			return fields;
		}

		private static List<string> SplitLines(string text)
		{
			var result = new List<string>();
			if (string.IsNullOrEmpty(text))
			{
				return result;
			}

			foreach (string raw in text.Split('\n'))
			{
				string line = raw.TrimEnd('\r');
				if (line.Trim().Length > 0)
				{
					result.Add(line);
				}
			}

			return result;
		}

		private static Dictionary<string, int> HeaderColumns(List<string> header)
		{
			var columns = new Dictionary<string, int>();
			for (int i = 0; i < header.Count; i++)
			{
				if (!columns.ContainsKey(header[i]))
				{
					columns.Add(header[i], i);
				}
			}

			return columns;
		}

		private static void BuildRows(IReadOnlyList<string> lines, char separator, List<string> ids, Dictionary<string, Dictionary<string, string>> rows)
		{
			if (lines.Count == 0)
			{
				return;
			}

			List<string> header = SplitFields(lines[0], separator);
			for (int line = 1; line < lines.Count; line++)
			{
				List<string> fields = SplitFields(lines[line], separator);
				string itemId = fields[0];
				if (string.IsNullOrEmpty(itemId))
				{
					Debug.LogWarning("[MM3:DB] Row " + line + " has no item id, skipped");
					continue;
				}

				if (rows.ContainsKey(itemId))
				{
					Debug.LogWarning("[MM3:DB] Duplicate item id '" + itemId + "', later row skipped");
					continue;
				}

				var properties = new Dictionary<string, string>();
				for (int column = 1; column < header.Count; column++)
				{
					string name = header[column];
					if (properties.ContainsKey(name))
					{
						continue;
					}

					properties.Add(name, column < fields.Count ? fields[column] : string.Empty);
				}

				rows.Add(itemId, properties);
				ids.Add(itemId);
			}
		}
	}
}
