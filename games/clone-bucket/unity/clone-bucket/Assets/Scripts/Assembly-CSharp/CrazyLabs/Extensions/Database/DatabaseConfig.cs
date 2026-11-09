using System.Collections.Generic;
using UnityEngine;

namespace CrazyLabs.Extensions.Database
{
	[CreateAssetMenu(menuName = "Configs/DatabaseConfig", fileName = "DatabaseConfig")]
	public class DatabaseConfig : ScriptableObject
	{
		public string mainDataSetId;

		public List<DataSetConfig> allDataSets;
	}
}
