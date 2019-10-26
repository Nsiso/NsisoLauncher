using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NsisoLauncher.Config
{
    public class LauncherProfilesConfig
    {
        [JsonProperty("selectedProfile")]
        public string SelectedProfile { get; set; }

        [JsonProperty("profiles")]
        public JObject Profiles { get; set; }

        [JsonProperty("clientToken")]
        public string ClientToken { get; set; }
    }
}
