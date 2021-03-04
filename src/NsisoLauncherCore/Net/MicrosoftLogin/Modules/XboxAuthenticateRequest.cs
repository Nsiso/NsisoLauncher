using System.Collections.Generic;
using Newtonsoft.Json;

namespace NsisoLauncherCore.Net.MicrosoftLogin.Modules
{
    public class XblAuthProperties
    {
        [JsonProperty("AuthMethod")]
        public string AuthMethod { get; set; } = "RPS";

        [JsonProperty("SiteName")]
        public string SiteName { get; set; } = "user.auth.xboxlive.com";

        [JsonProperty("RpsTicket")]
        public string RpsTicket { get; set; }

        public XblAuthProperties(string access_token)
        {
            this.RpsTicket = access_token;
        }
    }

    public class XblAuthRequest : XboxAuthRequest
    {
        [JsonProperty("Properties")]
        public XblAuthProperties Properties { get; set; }
    }

    public class XstsAuthProperties
    {
        [JsonProperty("SandboxId")]
        public string SandBoxId { get; set; } = "RETAIL";

        [JsonProperty("UserTokens")]
        public List<string> UserTokens { get; set; }

        public XstsAuthProperties(string xbl_token)
        {
            UserTokens = new List<string>(1);
            UserTokens.Add(xbl_token);
        }
    }

    public class XstsAuthRequest : XboxAuthRequest
    {
        [JsonProperty("Properties")]
        public XstsAuthProperties Properties { get; set; }
    }

    public class XboxLiveAuthResult
    {
        [JsonProperty("IssueInstant")]
        public string IssueInstant { get; set; }

        [JsonProperty("NotAfter")]
        public string NotAfter { get; set; }

        [JsonProperty("Token")]
        public string Token { get; set; }

        [JsonProperty("DisplayClaims")]
        public Claims DisplayClaims { get; set; }
    }

    public class Claims
    {
        /// <summary>
        /// ?
        /// </summary>
        [JsonProperty("xui")]
        public List<UhsContent> Xui { get; set; }
    }

    public class UhsContent
    {
        [JsonProperty("uhs")]
        public string Uhs { get; set; }
    }

    public abstract class XboxAuthRequest
    {
        [JsonProperty("RelyingParty")]
        public string RelyingParty { get; set; }

        [JsonProperty("TokenType")]
        public string TokenType { get; set; }
    }
}
