using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Net.Apis.Modules.Yggdrasil.Responses
{
    /// <summary>
    /// 对验证请求的响应
    /// </summary>
    public class AuthenticateResponse : Response
    {
        internal AuthenticateResponse(Response response) : base(response) { }

        internal AuthenticateResponse() { }

        public AuthenticateResponseData Data { get; internal set; }

        public YggdrasilUser YggdrasilUser { get => new YggdrasilUser(Data); }

    }

    public class AuthenticateResponseData
    {
        /// <summary>
        /// 此用户的访问令牌
        /// </summary>
        [JsonProperty("accessToken")]
        public string AccessToken { get; internal set; }

        /// <summary>
        /// 必须与Requester.ClientToken相同
        /// </summary>
        [JsonProperty("clientToken")]
        public string ClientToken { get; internal set; }

        /// <summary>
        /// 可用profile的列表
        /// </summary>
        [JsonProperty("availableProfiles")]
        public List<PlayerProfile> AvailableProfiles { get; internal set; }

        /// <summary>
        /// 用户选择的最后一个profile
        /// </summary>
        [JsonProperty("selectedProfile")]
        public PlayerProfile SelectedProfile { get; internal set; }

        /// <summary>
        /// 由requestUser发送的用户数据
        /// </summary>
        [JsonProperty("user")]
        public UserData User { get; internal set; }
    }

    /// <summary>
    /// 代表使用身份验证API时发生错误
    /// </summary>
    public class AuthenticationResponseError : Error
    {
        /// <summary>
        /// 此错误的原因（可选）
        /// </summary>
        [JsonProperty("cause")]
        public string Cause { get; private set; }
    }
}
