using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NsisoLauncherCore.Net.Apis.Modules.Yggdrasil
{
    public class UsernamePasswordPair
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
