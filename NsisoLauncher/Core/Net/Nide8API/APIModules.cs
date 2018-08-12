using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace NsisoLauncher.Core.Net.Nide8API
{
    public class APIModules
    {
        [JsonProperty("meta")]
        public Meta Meta { get; set; }

        [JsonProperty("apiRoot")]
        public string APIRoot { get; set; }
    }

    public class Meta
    {
        [JsonProperty("serverName")]
        public string ServerName { get; set; }

        [JsonProperty("serverIP")]
        public string ServerIP { get; set; }

        [JsonProperty("implementationName")]
        public string ImplementaionName { get; set; }

        [JsonProperty("implementationVersion")]
        public string ImplementationVersion { get; set; }
    }
}
