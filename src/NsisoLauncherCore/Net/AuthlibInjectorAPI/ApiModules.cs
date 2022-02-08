using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.AuthlibInjectorAPI
{
    public class LatestArtifact
    {
        [JsonProperty("build_number")]
        public int BuildNumber { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("download_url")]
        public string DownloadUrl { get; set; }

        [JsonProperty("checksums")]
        public CheckSums CheckSums { get; set; }
    }

    public class CheckSums
    {
        [JsonProperty("sha256")]
        public string Sha256 { get; set; }
    }
}
