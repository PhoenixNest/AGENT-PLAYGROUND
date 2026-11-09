using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Tabtale.TTPlugins
{
	public class TTPUtils
	{
		public const string AssetBundlesOutputPath = "AssetBundles";

		private static string _bundleIdentifier;

		public static string BundleIdentifier => null;

		public static void Copy(string sourceDirectory, string targetDirectory)
		{
		}

		public static string ReadStreamingAssetsFile(string fileRelativePath)
		{
			return null;
		}

		public static string CombinePaths(List<string> paths)
		{
			return null;
		}

		public static void CopyDir(string sourceDirectory, string targetDirectory)
		{
		}

		private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
		{
		}

		public static byte[] ReadDataFromStreamingAssets(string fileRelativePath)
		{
			return null;
		}
	}
}
