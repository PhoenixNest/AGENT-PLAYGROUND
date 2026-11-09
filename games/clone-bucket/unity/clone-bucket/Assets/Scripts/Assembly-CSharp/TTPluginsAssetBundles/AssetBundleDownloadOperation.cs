namespace TTPluginsAssetBundles
{
	public abstract class AssetBundleDownloadOperation : AssetBundleLoadOperation
	{
		public bool done { get; protected set; }

		public string assetBundleName { get; private set; }

		public LoadedAssetBundle assetBundle { get; protected set; }

		public string error { get; protected set; }

		protected abstract bool downloadIsDone { get; }

		protected abstract void FinishDownload();

		public override bool Update()
		{
			return false;
		}

		public override bool IsDone()
		{
			return false;
		}

		public abstract string GetSourceURL();

		public AssetBundleDownloadOperation(string assetBundleName)
		{
		}
	}
}
