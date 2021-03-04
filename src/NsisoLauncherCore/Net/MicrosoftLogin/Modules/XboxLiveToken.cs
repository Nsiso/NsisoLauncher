using System.Linq;

namespace NsisoLauncherCore.Net.MicrosoftLogin.Modules
{
    public class XboxLiveToken
    {
        public XboxLiveAuthResult XblAuthResult { get; set; }

        public XboxLiveAuthResult XstsAuthResult { get; set; }

        public string XblToken { get => XblAuthResult.Token; }

        public string XstsToken { get => XstsAuthResult.Token; }

        public string Uhs { get => XstsAuthResult.DisplayClaims?.Xui?.FirstOrDefault()?.Uhs; }
    }
}
