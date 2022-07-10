using Newtonsoft.Json;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Authenticator
{
    public class Nide8Authenticator : YggdrasilAuthenticator
    {
        public string Nide8ID { get; set; }

        private Net.Nide8API.APIHandler _nide8Handler;
        private string _jarVersion = "1.0";
        private Library _jarLib => new Library()
        {
            Url = "https://login.mc-user.com:233/index/jar",
            Name = new Artifact(string.Format("com.nide8:login2:{0}", _jarVersion))
        };

        [JsonConstructor]
        public Nide8Authenticator(string nide8ID, string clientToken) : base(string.Format("https://auth.mc-user.com:233/{0}/authserver", nide8ID), clientToken)
        {
            _nide8Handler = new Net.Nide8API.APIHandler(nide8ID);
        }

        public async Task UpdateApiRoot()
        {
            var result = await _nide8Handler.GetInfoAsync();
            this.YggdrasilApiAddress = result.APIRoot;
        }

        public async Task<Net.Nide8API.APIModules> GetInfo()
        {
            return await _nide8Handler.GetInfoAsync();
        }

        public async Task<bool> TestIsNide8DnsOK()
        {
            return await _nide8Handler.TestIsNide8DnsOK();
        }

        public override async Task UpdateAuthenticatorAsync(CancellationToken cancellation)
        {
            await this.UpdateApiRoot();
            string jarVer = await _nide8Handler.GetJarVersion();
            if (!string.IsNullOrWhiteSpace(jarVer))
            {
                this._jarVersion = jarVer;
            }
        }

        public override string GetExtraJvmArgument(LaunchHandler handler)
        {
            return string.Format("-javaagent:\"{0}\"=\"{1}\"", handler.GetLibraryPath(_jarLib), this.Nide8ID);
        }

        public override List<Library> Libraries => new List<Library>(1) { _jarLib };
    }
}
