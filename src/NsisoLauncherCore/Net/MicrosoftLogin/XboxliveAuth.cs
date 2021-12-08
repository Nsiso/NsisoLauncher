using Newtonsoft.Json;
using NsisoLauncherCore.Net.MicrosoftLogin.Modules;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.MicrosoftLogin
{
    public class XboxliveAuth
    {
        /// <summary>
        /// The uri of xbl
        /// </summary>
        public Uri XblAuthenticateUri { get; set; } = new Uri("https://user.auth.xboxlive.com/user/authenticate");
        public Uri XstsAuthenticateUri { get; set; } = new Uri("https://xsts.auth.xboxlive.com/xsts/authorize");

        public async Task<XboxLiveAuthResult> XblAuthenticate(XblAuthProperties properties, CancellationToken cancellation = default)
        {
            XblAuthRequest request = new XblAuthRequest()
            {
                Properties = properties,
                RelyingParty = "http://auth.xboxlive.com",
                TokenType = "JWT"
            };
            return await Post(XblAuthenticateUri, request, cancellation);
        }

        public async Task<XboxLiveAuthResult> XstsAuthenticate(XstsAuthProperties properties, CancellationToken cancellation = default)
        {
            XstsAuthRequest request = new XstsAuthRequest()
            {
                Properties = properties,
                RelyingParty = "rp://api.minecraftservices.com/",
                TokenType = "JWT"
            };
            return await Post(XstsAuthenticateUri, request, cancellation);
        }

        public async Task<XboxLiveToken> Authenticate(string ms_token, CancellationToken cancellation = default)
        {
            var xbl_result = await XblAuthenticate(new XblAuthProperties(ms_token), cancellation);
            string xbl_token = xbl_result.Token;
            var xsts_result = await XstsAuthenticate(new XstsAuthProperties(xbl_token), cancellation);
            return new XboxLiveToken()
            {
                XblAuthResult = xbl_result,
                XstsAuthResult = xsts_result
            };
        }

        private async Task<XboxLiveAuthResult> Post(Uri uri, XboxAuthRequest properties, CancellationToken cancellation = default)
        {
            string json_str = JsonConvert.SerializeObject(properties);
            HttpContent content = new StringContent(json_str);
            content.Headers.ContentType.MediaType = "application/json";

            var result = await NetRequester.HttpPostAsync(uri, content, cancellation);
            result.EnsureSuccessStatusCode();

            var respond_str = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<XboxLiveAuthResult>(respond_str);
        }
    }
}
