using NsisoLauncherCore.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Authenticator
{
    public class AuthlibInjectorAuthenticator : YggdrasilAuthenticator
    {
        public string AuthlibInjectorUri { get; set; }
        public AuthlibInjectorAuthenticator(string ai_url, string client_token) : base(string.Format("{0}/{1}", ai_url, "authserver"), client_token)
        {
            this.AuthlibInjectorUri = ai_url;
        }
    }
}
