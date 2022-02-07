using Newtonsoft.Json;
using System.Collections.Generic;

namespace NsisoLauncherCore.Modules
{
    public class PlayerProfile
    {
        /// <summary>
        /// The id of this player profile
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The name of this player profile
        /// </summary>
        [JsonProperty("name")]
        public string PlayerName { get; set; }
    }

    public class MicrosoftPlayerProfile : PlayerProfile
    {
        /// <summary>
        /// 角色Uuid值
        /// </summary>
        [JsonProperty("skins")]
        public List<MicrosoftSkin> Skins { get; set; }

        /// <summary>
        /// 角色Uuid值
        /// </summary>
        [JsonProperty("capes")]
        public List<object> Capes { get; set; }
    }
}
