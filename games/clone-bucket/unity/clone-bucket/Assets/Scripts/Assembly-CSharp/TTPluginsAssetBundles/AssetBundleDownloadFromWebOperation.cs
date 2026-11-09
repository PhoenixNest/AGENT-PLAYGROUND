using UnityEngine.Networking;

namespace TTPluginsAssetBundles
{
	public class AssetBundleDownloadFromWebOperation : AssetBundleDownloadOperation
	{
		private UnityWebRequest _www;

		private readonly string _url;

		protected override bool downloadIsDone => false;

		public AssetBundleDownloadFromWebOperation(string assetBundleName, UnityWebRequest www)
			: base(null)
		{
		}

		protected override void FinishDownload()
		{
		}

		public override string GetSourceURL()
		{
			return null;
		}
	}
}
