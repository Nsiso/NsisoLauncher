using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NsisoLauncher.Core.Net.MojangApi;
using NsisoLauncher.Core.Net.MojangApi.Api;
using NsisoLauncher.Core.Net.MojangApi.Responses;

namespace NsisoLauncher.Core.Auth
{
    public static class OfflineAuthenticator
    {
        public static Tuple<string, Uuid> DoAuthenticate(string displayname)
        {
            Uuid uuid = new Uuid()
            {
                PlayerName = displayname,
                Value = Guid.NewGuid().ToString("N")
            };

            string accessToken = Guid.NewGuid().ToString("N");

            return new Tuple<string, Uuid>(accessToken, uuid);
        }
    }
}
