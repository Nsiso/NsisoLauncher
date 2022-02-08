using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Authenticator
{
    public class AuthlibInjectorAuthenticator : YggdrasilAuthenticator
    {
        public string AuthlibInjectorUri { get; set; }

        private string _jarVersion = "1.0";
        private Library _jarLib => new Library()
        {
            Url = "https://login2.nide8.com:233/index/jar",
            Name = new Artifact(string.Format("com.nide8:login2:{0}", _jarVersion))
        };
        public AuthlibInjectorAuthenticator(string ai_url, string client_token) : base(string.Format("{0}/{1}", ai_url, "authserver"), client_token)
        {
            this.AuthlibInjectorUri = ai_url;
        }

        public override string GetExtraJvmArgument(LaunchHandler handler)
        {
            return string.Format("-javaagent:\"{0}\"=\"{1}\"", handler.GetLibraryPath(_jarLib), this.Nide8ID);
        }

        public override List<Library> Libraries => new List<Library>(1) { _jarLib };
    }
}
