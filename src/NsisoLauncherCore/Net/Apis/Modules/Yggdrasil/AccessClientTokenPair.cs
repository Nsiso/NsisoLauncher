using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NsisoLauncherCore.Net.Apis.Modules.Yggdrasil
{
    public class AccessClientTokenPair
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty("clientToken")]
        public string ClientToken { get; set; }

        public AccessClientTokenPair(string access, string client)
        {
            this.AccessToken = access;
            this.ClientToken = client;
        }

        public AccessClientTokenPair()
        {

        }
    }
}
