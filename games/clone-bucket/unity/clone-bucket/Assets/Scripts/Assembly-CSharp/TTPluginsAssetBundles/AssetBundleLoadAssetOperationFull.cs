using System;
using UnityEngine;

namespace TTPluginsAssetBundles
{
	public class AssetBundleLoadAssetOperationFull : AssetBundleLoadAssetOperation
	{
		protected string m_AssetBundleName;

		protected string m_AssetName;

		protected string m_DownloadingError;

		protected Type m_Type;

		protected AssetBundleRequest m_Request;

		public AssetBundleLoadAssetOperationFull(string bundleName, string assetName, Type type)
		{
		}

		public override T GetAsset<T>()
		{
			return null;
		}

		public override bool Update()
		{
			return false;
		}

		public override bool IsDone()
		{
			return false;
		}
	}
}
