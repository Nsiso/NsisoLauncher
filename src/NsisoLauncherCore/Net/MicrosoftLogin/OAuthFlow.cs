using Newtonsoft.Json;
using NsisoLauncherCore.Net.MicrosoftLogin.Modules;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.MicrosoftLogin
{
    public class OAuthFlow
    {
        private NetRequester requester;

        public Uri OAuthAuthorizeUri { get; set; } = new Uri("https://login.live.com/oauth20_authorize.srf");

        public Uri OAuthTokenUri { get; set; } = new Uri("https://login.live.com/oauth20_token.srf");

        /// <summary>
        /// your Azure client ID
        /// </summary>
        public string ClientId { get; set; } = "00000000402b5328";

        /// <summary>
        /// Response type
        /// </summary>
        public string ResponseType { get; set; } = "code";

        /// <summary>
        /// oauth scope
        /// </summary>
        public string Scope { get; set; } = "service::user.auth.xboxlive.com::MBI_SSL";

        /// <summary>
        /// your redirect uri
        /// </summary>
        public Uri RedirectUri { get; set; } = new Uri("https://login.live.com/oauth20_desktop.srf");

        /// <summary>
        /// optional; used to prevent CSRF & restoring previous application states
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// The auth code from 1st step oauth
        /// </summary>
        public string AuthCode { get; set; }

        public OAuthFlow(NetRequester arg_requester)
        {
            this.requester = arg_requester;
        }

        public Uri GetAuthorizeUri()
        {
            UriBuilder uriBuilder = new UriBuilder(OAuthAuthorizeUri);
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.AppendFormat("client_id={0}", Uri.EscapeDataString(ClientId));
            queryBuilder.AppendFormat("&response_type={0}", Uri.EscapeDataString(ResponseType));
            queryBuilder.AppendFormat("&redirect_uri={0}", Uri.EscapeDataString(RedirectUri.OriginalString));
            queryBuilder.AppendFormat("&scope={0}", Uri.EscapeDataString(Scope));
            if (!string.IsNullOrWhiteSpace(State))
            {
                queryBuilder.AppendFormat("&state={0}", Uri.EscapeDataString(State));
            }
            uriBuilder.Query = queryBuilder.ToString();
            return uriBuilder.Uri;
        }

        public string RedirectUrlToAuthCode(Uri uri)
        {
            string query = uri.Query.TrimStart('?');
            string[] queries = query.Split('&');
            foreach (var item in queries)
            {
                string[] arg = item.Split('=');
                if (arg.Length != 2)
                {
                    throw new Exception(string.Format("The query arg : {0} size have no key and value slited by = char", item));
                }
                string key = arg[0];
                string value = arg[1];
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }
                if (key == "code")
                {
                    return value;
                }
            }
            return null;
        }

        public async Task<MicrosoftToken> MicrosoftCodeToAccessToken(string code, CancellationToken cancellation = default)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("client_id", ClientId);
            args.Add("code", code);
            args.Add("grant_type", "authorization_code");
            args.Add("redirect_uri", RedirectUri.AbsoluteUri);
            args.Add("scope", Scope);
            FormUrlEncodedContent content = new FormUrlEncodedContent(args);
            var result = await requester.Client.PostAsync(OAuthTokenUri, content, cancellation);
            result.EnsureSuccessStatusCode();

            string jsonStr = await result.Content.ReadAsStringAsync();
            MicrosoftToken token = JsonConvert.DeserializeObject<MicrosoftToken>(jsonStr);

            return token;
        }

        public async Task<MicrosoftToken> RefreshMicrosoftAccessToken(MicrosoftToken token, CancellationToken cancellation = default)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("client_id", ClientId);
            args.Add("refresh_token", token.Refresh_token);
            args.Add("grant_type", "refresh_token");
            args.Add("redirect_uri", RedirectUri.AbsoluteUri);
            FormUrlEncodedContent content = new FormUrlEncodedContent(args);
            var result = await requester.Client.PostAsync(OAuthTokenUri, content, cancellation);
            result.EnsureSuccessStatusCode();

            string jsonStr = await result.Content.ReadAsStringAsync();
            MicrosoftToken re_token = JsonConvert.DeserializeObject<MicrosoftToken>(jsonStr);

            return re_token;
        }
    }
}
