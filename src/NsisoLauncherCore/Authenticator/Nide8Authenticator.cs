using NsisoLauncherCore.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Authenticator
{
    public class Nide8Authenticator : YggdrasilAuthenticator
    {
        public string Nide8ID { get; set; }

        private Net.Nide8API.APIHandler nide8Handler;

        public Nide8Authenticator(string nide8ID, string client_token) : base(string.Format("https://auth2.nide8.com:233/{0}/authserver", nide8ID), client_token)
        {
            nide8Handler = new Net.Nide8API.APIHandler(nide8ID);
        }

        public async Task UpdateApiRoot()
        {
            var result = await nide8Handler.GetInfoAsync();
            this.YggdrasilApiAddress = result.APIRoot;
        }

        public async Task<Net.Nide8API.APIModules> GetInfo()
        {
            return await nide8Handler.GetInfoAsync();
        }

        public async Task<bool> TestIsNide8DnsOK()
        {
            return await nide8Handler.TestIsNide8DnsOK();
        }
    }
}
