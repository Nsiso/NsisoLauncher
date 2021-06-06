using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Net.Apis.Modules
{
    public class JavaAll
    {
        [JsonProperty("gamecore")]
        public Dictionary<string, List<JObject>> Gamecore { get; set; }

        [JsonProperty("windows-x64")]
        public Dictionary<string, List<JavaMeta>> Windows_x64 { get; set; }

        [JsonProperty("windows-x86")]
        public Dictionary<string, List<JavaMeta>> Windows_x86 { get; set; }
    }

    public class JavaMeta
    {
        public Availability Availability { get; set; }

        public Sha1SizeUrl Manifest { get; set; }

        public JavaVersion Version { get; set; }
    }

    public class Availability
    {
        [JsonProperty("group")]
        public int Group { get; set; }

        [JsonProperty("progress")]
        public int Progress { get; set; }
    }

    public class JavaVersion
    {
        public string Name { get; set; }

        public DateTime Released { get; set; }
    }
}
