using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace NsisoLauncherCore.Net.Server
{
    /// <summary>
    /// Contains information about a modded server install.
    /// </summary>
    public class ModInfo
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("modList")]
        public List<Mod> ModList { get; set; }
    }

    public class Mod
    {
        [JsonProperty("modid")]
        public string ModId { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }
}
