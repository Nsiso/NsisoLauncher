using NsisoLauncherCore.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Authenticator
{
    public class AuthlibInjectorAuthenticator : YggdrasilAuthenticator
    {
        public string AuthlibInjectorUri { get; set; }
        public AuthlibInjectorAuthenticator(string ai_url, NetRequester requester, string client_token) : base(string.Format("{0}/{1}", ai_url, "authserver"), requester, client_token)
        {
            this.AuthlibInjectorUri = ai_url;
        }
    }
}
