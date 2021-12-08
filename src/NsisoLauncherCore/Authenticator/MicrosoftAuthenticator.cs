using Microsoft.Identity.Client;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.MicrosoftLogin;
using NsisoLauncherCore.Net.MicrosoftLogin.Modules;
using NsisoLauncherCore.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Authenticator
{
    public class MicrosoftAuthenticator
    {
        private OAuthFlow oAuthFlower;
        private XboxliveAuth xboxliveAuther;
        private MinecraftServices mcServices;

        public MicrosoftAuthenticator()
        {
            oAuthFlower = new OAuthFlow();
            xboxliveAuther = new XboxliveAuth();
            mcServices = new MinecraftServices();
        }

        public async Task<MinecraftToken> LoginGetMinecraftToken(CancellationToken cancellation = default)
        {
            AuthenticationResult result = await oAuthFlower.Login(cancellation);
            XboxLiveToken xbox_result = await xboxliveAuther.Authenticate(result.AccessToken, cancellation);
            MinecraftToken mc_result = await mcServices.Authenticate(xbox_result, cancellation);

            return mc_result;
        }

        public async Task<MinecraftToken> RefreshGetMinecraftToken(IAccount account, CancellationToken cancellation = default)
        {
            AuthenticationResult result = await oAuthFlower.Login(account, cancellation);
            XboxLiveToken xbox_result = await xboxliveAuther.Authenticate(result.AccessToken, cancellation);
            MinecraftToken mc_result = await mcServices.Authenticate(xbox_result, cancellation);

            return mc_result;
        }
    }
}
