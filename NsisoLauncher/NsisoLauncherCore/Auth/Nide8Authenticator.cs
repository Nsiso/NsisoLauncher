using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Endpoints;
using static NsisoLauncherCore.Net.MojangApi.Responses.AuthenticateResponse;

namespace NsisoLauncherCore.Auth
{
    public class Nide8Authenticator : YggdrasilAuthenticator
    {
        public string Nide8ID { get; set; }
        public Nide8Authenticator(string nide8ID, Credentials credentials) : base(credentials)
        {
            Nide8ID = nide8ID;
            ProxyAuthServerAddress = string.Format("https://auth2.nide8.com:233/{0}/authserver", Nide8ID);
        }
    }

    public class Nide8TokenAuthenticator : YggdrasilTokenAuthenticator
    {
        public string Nide8ID { get; set; }
        public Nide8TokenAuthenticator(string nide8ID, string token, Uuid selectedProfileUUID, UserData userData) : base(token, selectedProfileUUID, userData)
        {
            Nide8ID = nide8ID;
            ProxyAuthServerAddress = string.Format("https://auth2.nide8.com:233/{0}/authserver", Nide8ID);
        }
    }
}
