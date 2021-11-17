using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NsisoLauncherCore.Net.Apis.Modules.Yggdrasil.Requests
{
    public class RefreshRequest : AccessClientTokenPair
    {
        [JsonProperty("requestUser")]
        public bool RequestUser { get; set; } = false;

        public RefreshRequest()
        {

        }

        public RefreshRequest(AccessClientTokenPair pair)
        {
            this.AccessToken = pair.AccessToken;
            this.ClientToken = pair.ClientToken;
            this.RequestUser = false;
        }
    }
}
