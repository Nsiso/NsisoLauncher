namespace NsisoLauncherCore.Net.MojangApi.Responses
{

    /// <summary>
    /// 包含Mojang服务的所有状态
    /// </summary>
    public class ApiStatusResponse : Response
    {
        internal ApiStatusResponse(Response response) : base(response)
        {
        }

        /// <summary>
        /// Mojang服务的可能状态
        /// </summary>
        public enum Status
        {

            /// <summary>
            /// 无法检索状态. API更改或服务不可用
            /// </summary>
            Unknown = 0, // Setting it by default.

            /// <summary>
            /// 该服务是可用的,没有问题
            /// </summary>
            Available,

            /// <summary>
            /// 该服务可用,但有一些问题
            /// </summary>
            SomeIssues,

            /// <summary>
            /// 这些服务存在问题并且无法使用
            /// </summary>
            Unavailable
        }

        /// <summary>
        /// 分析输入以获取状态枚举
        /// </summary>
        internal static Status Parse(string input)
        {
            // input ?? "" is to avoid getting a reference exception
            switch ((input ?? "").ToLower().Trim())
            {
                case "green": return Status.Available;
                case "yellow": return Status.SomeIssues;
                case "red": return Status.Unavailable;
                default: return Status.Unknown;
            }
        }

        /// <summary>
        /// Status of Minecraft.net
        /// </summary>
        public Status Minecraft { get; internal set; }

        /// <summary>
        /// Status of mojang.com
        /// </summary>
        public Status Mojang { get; internal set; }

        /// <summary>
        /// Status of session.minecraft.net
        /// </summary>
        public Status Sessions { get; internal set; }

        /// <summary>
        /// Status of textures.minecraft.net
        /// </summary>
        public Status Textures { get; internal set; }

        /// <summary>
        /// Status of sessionserver.mojang.com
        /// </summary>
        public Status MojangSessionsServer { get; internal set; }

        /// <summary>
        /// Status of accounts.mojang.com
        /// </summary>
        public Status MojangAccounts { get; internal set; }

        /// <summary>
        /// Status of auth.mojang.com
        /// </summary>
        public Status MojangAuthenticationService { get; internal set; }

        /// <summary>
        /// Status of authserver.mojang.com
        /// </summary>
        public Status MojangAutenticationServers { get; internal set; }

        /// <summary>
        /// Status of skins.minecraft.net
        /// </summary>
        public Status Skins { get; internal set; }

        /// <summary>
        /// Status of api.mojang.com
        /// </summary>
        public Status MojangApi { get; internal set; }
    }

}
