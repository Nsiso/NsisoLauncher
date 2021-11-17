using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.Apis;
using NsisoLauncherCore.Net.Apis.Modules.Yggdrasil.Requests;
using NsisoLauncherCore.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Authenticator
{
    public class YggdrasilAuthenticator
    {
        public string YggdrasilApiAddress
        {
            get { return api.ApiAddress; }
            set { api.ApiAddress = value; }
        }

        public string ClientToken { get; set; }

        YggdrasilApi api;

        public YggdrasilAuthenticator(NetRequester requester, string client_token)
        {
            api = new YggdrasilApi(requester);
            this.ClientToken = client_token;
        }

        public YggdrasilAuthenticator(string api_addr, NetRequester requester, string client_token)
        {
            api = new YggdrasilApi(api_addr, requester);
            this.ClientToken = client_token;
        }

        async Task<YggdrasilUser> AuthenticateAsync(string username, string password, CancellationToken cancellation = default)
        {
            var result = await api.Authenticate(new AuthenticateRequest(username, password, ClientToken), cancellation);
        }
    }
}
