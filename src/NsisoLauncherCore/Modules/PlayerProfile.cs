using Newtonsoft.Json;

namespace NsisoLauncherCore.Modules
{
    public class PlayerProfile
    {
        /// <summary>
        /// 角色Uuid值
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; internal set; }

        /// <summary>
        /// 玩家昵称
        /// </summary>
        [JsonProperty("name")]
        public string PlayerName { get; internal set; }
    }
}
