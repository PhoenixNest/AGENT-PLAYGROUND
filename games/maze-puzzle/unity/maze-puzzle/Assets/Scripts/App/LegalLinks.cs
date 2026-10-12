namespace OrbitalSalvage.App
{
    /// <summary>
    /// The only App-assembly place that names the external legal pages.
    /// Both values are placeholders until the real policy URLs exist.
    /// TODO: replace https://www.bing.com with the shipped Privacy Policy and User Agreement.
    /// TODO: Kids Category — an external link in a kids app may need a parental gate before
    /// it can ship in the App Store Kids Category. This class only names the URLs. It does not
    /// open them and it does not build that gate.
    /// </summary>
    public static class LegalLinks
    {
        public const string PrivacyPolicy = "https://www.bing.com";
        public const string UserAgreement = "https://www.bing.com";
    }
}
