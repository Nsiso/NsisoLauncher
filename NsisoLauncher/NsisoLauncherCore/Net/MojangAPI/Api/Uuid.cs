using Newtonsoft.Json;

namespace NsisoLauncherCore.Net.MojangApi.Api
{

    /// <summary>
    /// 表示玩家的UUID数据
    /// </summary>
    public class Uuid
    {

        /// <summary>
        /// 实例化一个 Uuid item.
        /// </summary>
        public Uuid()
        {
            this.Legacy = false;
            this.Demo = false;
        }

        /// <summary>
        /// 玩家UUID值.
        /// </summary>
        [JsonProperty("id")]
        public string Value { get; internal set; }

        /// <summary>
        /// 玩家昵称.
        /// </summary>
        [JsonProperty("name")]
        public string PlayerName { get; internal set; }

        /// <summary>
        /// 玩家的账户是否已迁移到Mojang.
        /// 如果为true,则此账户尚未迁移.
        /// </summary>
        [JsonProperty("legacy")]
        public bool Legacy { get; internal set; }

        /// <summary>
        /// 玩家账户是否为演示账户.
        /// 如果为true,则该账户还未购买Minecraft.
        /// </summary>
        [JsonProperty("demo")]
        public bool? Demo { get; internal set; }
    }
}
