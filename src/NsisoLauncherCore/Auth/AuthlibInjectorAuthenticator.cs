using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Endpoints;
using NsisoLauncherCore.Net.MojangApi.Responses;

namespace NsisoLauncherCore.Auth
{
    public class AuthlibInjectorAuthenticator : YggdrasilAuthenticator
    {
        public AuthlibInjectorAuthenticator(string serverRootAddr, Credentials credentials) : base(credentials)
        {
            ServerRootAddress = serverRootAddr;
            ProxyAuthServerAddress = ServerRootAddress + "/authserver";
        }

        public string ServerRootAddress { get; set; }
    }

    public class AuthlibInjectorTokenAuthenticator : YggdrasilTokenAuthenticator
    {
        public AuthlibInjectorTokenAuthenticator(string serverRootAddr, string token, Uuid selectedProfileUUID,
            AuthenticateResponse.UserData userData) : base(token, selectedProfileUUID, userData)
        {
            ServerRootAddress = serverRootAddr;
            ProxyAuthServerAddress = ServerRootAddress + "/authserver";
        }

        public string ServerRootAddress { get; set; }
    }
}