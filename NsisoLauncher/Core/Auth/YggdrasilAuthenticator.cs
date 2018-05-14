using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NsisoLauncher.Core.Net.MojangApi;
using NsisoLauncher.Core.Net.MojangApi.Endpoints;
using NsisoLauncher.Core.Net.MojangApi.Responses;

namespace NsisoLauncher.Core.Auth
{
    class YggdrasilAuthenticator : IAuth
    {
        public AuthType AuthType { get => AuthType.ONLINE; }
        public Credentials Credentials { get; set; }

        public YggdrasilAuthenticator(string username, string password)
        {
            this.Credentials = new Credentials() { Username = username, Password = password };
        }

        public async Task<AuthenticateResponse> DoAuthenticationAsync()
        {
            Authenticate authenticate = new Authenticate(this.Credentials);
            return await authenticate.PerformRequestAsync();
        }
    }
}