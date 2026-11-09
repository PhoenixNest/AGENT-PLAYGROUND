using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Tabtale.TTPlugins
{
	public class TTPOnDemandsResources
	{
		public class DownloadResult
		{
			public bool success;

			public string bundleName;

			public int errorCode;

			public AssetBundle assetBundle;
		}

		private class DownloadItem
		{
			public string bundleName;

			public Action<DownloadResult> action;

			public bool shouldLoad;

			public DownloadItem(string bundleName, Action<DownloadResult> action, bool shouldLoad)
			{
			}
		}

		private interface ITTPOnDemandResources
		{
			void DownloadBundle(string bundleName);

			bool IsBundleInstalled(string bundleName);

			AssetBundle GetAssetBundle(string bundleName);
		}

		private class EditorImpl : ITTPOnDemandResources
		{
			public void DownloadBundle(string bundleName)
			{
			}

			public AssetBundle GetAssetBundle(string bundleName)
			{
				return null;
			}

			public bool IsBundleInstalled(string bundleName)
			{
				return false;
			}
		}

		[Preserve]
		private class OnDemandResourcesDelegate : MonoBehaviour
		{
			[Serializable]
			private class OnODRInstalledMessage
			{
				public string bundleName;
			}

			[Serializable]
			private class OnODRFailedMessage
			{
				public string bundleName;

				public int errorCode;
			}

			public void OnODRInstalled(string message)
			{
			}

			public void OnODRFailed(string message)
			{
			}

			private void callActions(DownloadResult downloadResult)
			{
			}
		}

		private static OnDemandResourcesDelegate _odrDelegate;

		private static List<DownloadItem> _actionsList;

		private static ITTPOnDemandResources _impl;

		private static OnDemandResourcesDelegate Delegate => null;

		private static ITTPOnDemandResources Impl => null;

		public static void DownloadBundle(string bundleName, Action<DownloadResult> downloadFinished, bool shouldLoad = false)
		{
		}

		public static bool IsBundleInstalled(string bundleName)
		{
			return false;
		}

		public static AssetBundle GetAssetBundle(string bundleName)
		{
			return null;
		}
	}
}
