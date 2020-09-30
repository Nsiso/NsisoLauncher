using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net.MojangApi.Api;
using System.Collections.Generic;

namespace NsisoLauncherCore.Net.MojangApi.Responses
{

    /// <summary>
    /// 对验证请求的响应
    /// </summary>
    public class AuthenticateResponse : Response
    {
        internal AuthenticateResponse(Response response) : base(response) { }

        internal AuthenticateResponse() { }

        /// <summary>
        /// 此用户的访问令牌
        /// </summary>
        public string AccessToken { get; internal set; }

        /// <summary>
        /// 必须与Requester.ClientToken相同
        /// </summary>
        public string ClientToken { get; internal set; }

        /// <summary>
        /// 可用profile的列表
        /// </summary>
        public List<PlayerProfile> AvailableProfiles { get; internal set; }

        /// <summary>
        /// 用户选择的最后一个profile
        /// </summary>
        public PlayerProfile SelectedProfile { get; internal set; }

        /// <summary>
        /// 由requestUser发送的用户数据
        /// </summary>
        public UserData User { get; internal set; }

    }

    /// <summary>
    /// 表示对返回令牌的有效负载的响应
    /// </summary>
    public class TokenResponse : Response
    {
        internal TokenResponse(Response response) : base(response) { }

        /// <summary>
        /// 此实例的验证令牌
        /// </summary>
        public string AccessToken { get; internal set; }

    }

    // ---

    /// <summary>
    /// 代表使用身份验证API时发生错误
    /// </summary>
    public class AuthenticationResponseError : Error
    {
        internal AuthenticationResponseError(JObject json)
        {
            this.ErrorTag = json["error"].ToObject<string>();
            this.ErrorMessage = json["errorMessage"].ToObject<string>();
            if (json.ToString().Contains("cause"))
            {
                this.Cause = json["cause"].ToObject<string>();
            }
        }

        /// <summary>
        /// 此错误的原因（可选）
        /// </summary>
        public string Cause { get; private set; }
    }

}
