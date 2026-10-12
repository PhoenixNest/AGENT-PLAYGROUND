using UnityEngine;

namespace OrbitalSalvage.Loc
{
    public sealed class ResourcesFontManifestLoader : ILocFontManifestLoader
    {
        public bool TryLoad(out string json)
        {
            json = null;
            TextAsset asset = Resources.Load<TextAsset>("Localization/fonts");
            if (asset == null || string.IsNullOrEmpty(asset.text)) return false;
            json = asset.text;
            return true;
        }
    }
}
