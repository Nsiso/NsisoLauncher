﻿using Newtonsoft.Json;
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

        private readonly SystemWebViewOptions systemWebViewOptions = CustomHTML.GetCustomHTML();

        /// <summary>
        /// Azure client ID
        /// </summary>
        public string ClientId { get; set; }


        public OAuthFlow(string clientId)
        {
            this.ClientId = clientId;

            publicClientApp = PublicClientApplicationBuilder.Create(ClientId)
                    .WithAuthority(AzureCloudInstance.AzurePublic, tenant)
                    .WithRedirectUri("http://localhost")
                    .Build();
        }

        /// <summary>
        /// Default using NsisoLauncher's azure client id.
        /// </summary>
        public OAuthFlow() : this("aca71205-b0f3-4c94-b8a8-9b58c2a8f555")
        {

        }

        public async Task<AuthenticationResult> Login(CancellationToken cancellation = default)
        {
            try
            {
                var authResult = await publicClientApp.AcquireTokenInteractive(scopes)
                    .WithSystemWebViewOptions(systemWebViewOptions)
                    .ExecuteAsync(cancellation);
                return authResult;
            }
            catch
            {
                throw;
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
            catch
            {
                throw;
            }
        }

        public async Task<IAccount> GetAccountAsync(string identifier)
        {
            return await publicClientApp.GetAccountAsync(identifier);
        }
    }
}
