using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.Yggdrasil;
using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Auth
{
    public class AuthlibInjectorAuthenticator : YggdrasilAuthenticator
    {
        public string AuthlibInjectorUri { get; set; }
        public AuthlibInjectorAuthenticator(string ai_url, NetRequester requester) : base(string.Format("{0}/{1}", ai_url, "authserver"), requester)
        {
            this.AuthlibInjectorUri = ai_url;
        }
    }
}
