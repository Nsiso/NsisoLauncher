using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.MicrosoftLogin
{
    public class OAuthFlow
    {
        private HttpClient client;

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

        public OAuthFlow(HttpClient arg_client)
        {
            this.client = arg_client;
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

        private void RedirectUrlToAuthCode(Uri uri)
        {
            string query = uri.Query;
            query.TrimStart('?');
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
                    this.AuthCode = value;
                }
                else if (key == "state")
                {
                    this.State = value;
                }
            }
        }

        public async Task<string> MicrosoftCodeToAccessToken(string code, CancellationToken cancellation)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("client_id", ClientId);
            args.Add("code", AuthCode);
            args.Add("grant_type", "authorization_code");
            args.Add("redirect_uri", RedirectUri.AbsoluteUri);
            args.Add("scope", Scope);
            HttpContent content = new FormUrlEncodedContent(args);
            var result = await client.PostAsync(OAuthTokenUri, content, cancellation);
            result.EnsureSuccessStatusCode();

            return null;
        }
    }
}
