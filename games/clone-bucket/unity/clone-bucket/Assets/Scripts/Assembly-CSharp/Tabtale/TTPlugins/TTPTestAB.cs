using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tabtale.TTPlugins
{
	public class TTPTestAB
	{
		private const string TAG = "TTPTestAB::";

		private static TTPTestAB _testAb;

		private static TTPTestABViewController _controller;

		private static GameObject _testAbConsole;

		private static string _loadedExperiment;

		private static string _loadedVariant;

		private const string STORED_FILENAME = "testAb.json";

		private static string _storedFilePath;

		private static Dictionary<string, object> _config;

		private static Dictionary<string, object> _configDictionary;

		public static Action OnHideConsole;

		public static string LoadedExperiment
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public static string LoadedVariant
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public static Dictionary<string, object> Config => null;

		public static void Create()
		{
		}

		private TTPTestAB()
		{
		}

		public static void Show()
		{
		}

		public static void ApplyExperimentWithVariant(string experimentName, string variantName)
		{
		}

		public static string[] GetExperiments()
		{
			return null;
		}

		public static string[] GetVariantNamesForExperiment(string name)
		{
			return null;
		}

		public static string GetVariantForExperiment(string experimentName, string variantName)
		{
			return null;
		}

		public static bool IsSavedConfigForABTestExist()
		{
			return false;
		}

		private static void LoadConfigFromFileAndApply()
		{
		}

		private static void SetupConsole()
		{
		}

		private static void SaveConfigToFile(string experimentName, string variantName)
		{
		}

		private static void SendOnRemoteConfigUpdate(string jsonData)
		{
		}
	}
}
