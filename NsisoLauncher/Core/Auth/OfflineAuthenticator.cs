using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NsisoLauncher.Core.Net.MojangApi;
using NsisoLauncher.Core.Net.MojangApi.Api;
using NsisoLauncher.Core.Net.MojangApi.Responses;

namespace NsisoLauncher.Core.Auth
{
    public static class OfflineAuthenticator
    {
        public static Tuple<Uuid,AuthenticateResponse> OfflineAuthenticate(string displayname)
        {

            Uuid uuid = new Uuid()
            {
                PlayerName = displayname,
                Value = Guid.NewGuid().ToString("N")
            };

            AuthenticateResponse response = new AuthenticateResponse()
            {
                AccessToken = Guid.NewGuid().ToString("N"),
                User = new AuthenticateResponse.UserData() { Properties = new List<AuthenticateResponse.UserData.Property>()}
            };

            return new Tuple<Uuid, AuthenticateResponse>(uuid, response);
        }
    }
}
