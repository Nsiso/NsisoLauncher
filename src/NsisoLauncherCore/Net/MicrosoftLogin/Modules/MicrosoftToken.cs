using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NsisoLauncherCore.Net.MicrosoftLogin.Modules
{
    public class MicrosoftToken
    {
        [JsonProperty("token_type")]
        public string Token_type { get; set; }

        [JsonProperty("expires_in")]
        public int Expires_in { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("access_token")]
        public string Access_token { get; set; }

        [JsonProperty("refresh_token")]
        public string Refresh_token { get; set; }

        [JsonProperty("user_id")]
        public string User_id { get; set; }

        [JsonProperty("foci")]
        public string Foci { get; set; }

        [JsonIgnore]
        public DateTime IssuedTime { get; set; } = DateTime.Now;

        public bool CheckValid()
        {
            TimeSpan span = DateTime.Now - IssuedTime;
            return span.Seconds < Expires_in;
        }
    }
}
