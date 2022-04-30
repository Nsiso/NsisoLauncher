using Newtonsoft.Json;
using NsisoLauncherCore.Util.Checker;
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

    public class CheckSums : IHashProvider
    {
        [JsonProperty("sha256")]
        public string Sha256 { get; set; }

        public Hash GetHash()
        {
            return new Hash(HashType.SHA256, Sha256);
        }
    }
}
