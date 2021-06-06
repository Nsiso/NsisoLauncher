using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NsisoLauncherCore.Net.MojangAPI
{
    public class APIModules
    {
        public class SessionProfile
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("properties")]
            public List<SessionProfileProperty> Properties { get; set; }
        }

        public class SessionProfileProperty
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("value")]
            public string Value { get; set; }
        }

        public class Textures
        {
            [JsonProperty("timestamp")]
            public long Timestamp { get; set; }

            [JsonProperty("profileId")]
            public string ProfileId { get; set; }

            [JsonProperty("profileName")]
            public string ProfileName { get; set; }

            [JsonProperty("signatureRequired")]
            public bool SignatureRequired { get; set; }

            [JsonProperty("textures")]
            public JObject TexturesObjs { get; set; }
        }
    }
}
