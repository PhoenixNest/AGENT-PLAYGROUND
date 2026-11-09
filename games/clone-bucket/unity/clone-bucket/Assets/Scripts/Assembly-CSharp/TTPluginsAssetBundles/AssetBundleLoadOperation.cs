using System.Collections;

namespace TTPluginsAssetBundles
{
	public abstract class AssetBundleLoadOperation : IEnumerator
	{
		public object Current => null;

		public bool MoveNext()
		{
			return false;
		}

		public void Reset()
		{
		}

		public abstract bool Update();

		public abstract bool IsDone();
	}
}
