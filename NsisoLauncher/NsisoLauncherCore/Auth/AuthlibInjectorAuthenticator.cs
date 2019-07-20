using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Endpoints;
using NsisoLauncherCore.Net.MojangApi.Responses;

namespace NsisoLauncherCore.Auth
{
    public class AuthlibInjectorAuthenticator : YggdrasilAuthenticator
    {
        public string ServerRootAddress { get; set; }
        public AuthlibInjectorAuthenticator(string serverRootAddr, Credentials credentials) : base(credentials)
        {
            this.ServerRootAddress = serverRootAddr;
            this.ProxyAuthServerAddress = ServerRootAddress + "/authserver";
        }
    }

    public class AuthlibInjectorTokenAuthenticator : YggdrasilTokenAuthenticator
    {
        public string ServerRootAddress { get; set; }
        public AuthlibInjectorTokenAuthenticator(string serverRootAddr, string token, Uuid selectedProfileUUID, AuthenticateResponse.UserData userData) : base(token, selectedProfileUUID, userData)
        {
            this.ServerRootAddress = serverRootAddr;
            this.ProxyAuthServerAddress = ServerRootAddress + "/authserver";
        }
    }
}
