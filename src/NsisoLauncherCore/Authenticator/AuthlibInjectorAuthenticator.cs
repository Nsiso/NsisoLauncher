using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Authenticator
{
    public class AuthlibInjectorAuthenticator : YggdrasilAuthenticator
    {
        public string AuthlibInjectorUri { get; set; }

        private Net.AuthlibInjectorAPI.APIHandler ai_api;

        private Library _jarLib;
        public AuthlibInjectorAuthenticator(string ai_url, string client_token) : base(string.Format("{0}/{1}", ai_url, "authserver"), client_token)
        {
            this.AuthlibInjectorUri = ai_url;
            ai_api = new Net.AuthlibInjectorAPI.APIHandler();
        }

        public override string GetExtraJvmArgument(LaunchHandler handler)
        {
            return string.Format("-javaagent:\"{0}\"=\"{1}\"", handler.GetLibraryPath(_jarLib), AuthlibInjectorUri);
        }

        public override async Task UpdateAuthenticatorAsync(CancellationToken cancellation)
        {
            _jarLib = await ai_api.GetCoreLibraryAsync();
        }

        public override List<Library> Libraries => new List<Library>(1) { _jarLib };


    }
}
