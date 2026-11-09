using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace TTPluginsAssetBundles
{
	public class LoadedAssetBundle
	{
		public AssetBundle m_AssetBundle;

		public int m_ReferencedCount;

		[CompilerGenerated]
		private Action m_unload;

		internal event Action unload
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		internal void OnUnload()
		{
		}

		public LoadedAssetBundle(AssetBundle assetBundle)
		{
		}
	}
}
