using UnityEngine;

namespace TTPluginsAssetBundles
{
	public abstract class AssetBundleLoadAssetOperation : AssetBundleLoadOperation
	{
		public abstract T GetAsset<T>() where T : Object;
	}
}
