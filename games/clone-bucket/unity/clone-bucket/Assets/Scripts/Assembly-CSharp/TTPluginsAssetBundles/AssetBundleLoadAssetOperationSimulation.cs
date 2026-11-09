using UnityEngine;

namespace TTPluginsAssetBundles
{
	public class AssetBundleLoadAssetOperationSimulation : AssetBundleLoadAssetOperation
	{
		private Object m_SimulatedObject;

		public AssetBundleLoadAssetOperationSimulation(Object simulatedObject)
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
