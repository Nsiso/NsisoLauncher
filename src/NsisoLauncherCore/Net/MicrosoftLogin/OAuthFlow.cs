using Newtonsoft.Json;
using NsisoLauncherCore.Net.MicrosoftLogin.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace NsisoLauncherCore.Net.MicrosoftLogin
{
    public class OAuthFlow
    {
        private IPublicClientApplication publicClientApp;

        private readonly string tenant = "consumers";

        private readonly string[] scopes = { "XboxLive.signin", "offline_access" };

        /// <summary>
        /// your Azure client ID
        /// </summary>
        public string ClientId { get; set; } = "aca71205-b0f3-4c94-b8a8-9b58c2a8f555";


        public OAuthFlow()
        {
            publicClientApp = PublicClientApplicationBuilder.Create(ClientId)
                .WithDefaultRedirectUri()
                .WithAuthority(AzureCloudInstance.AzurePublic, tenant)
                .Build();
        }

        public async Task<AuthenticationResult> Login(CancellationToken cancellation = default)
        {
            try
            {
                var authResult = await publicClientApp.AcquireTokenInteractive(scopes)
                    .ExecuteAsync(cancellation);
                return authResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<AuthenticationResult> Login(IAccount account, CancellationToken cancellation = default)
        {
            try
            {
                var authResult = await publicClientApp.AcquireTokenSilent(scopes, account)
                    .ExecuteAsync(cancellation);
                return authResult;
            }
            catch (MsalUiRequiredException)
            {
                return await Login(cancellation);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
