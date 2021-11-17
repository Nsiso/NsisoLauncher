using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Net.Apis.Modules.Yggdrasil.Responses
{
    /// <summary>
    /// 表示对返回令牌的有效负载的响应
    /// </summary>
    public class TokenResponse : Response
    {
        internal TokenResponse(Response response) : base(response) { }

        /// <summary>
        /// 此实例的验证令牌
        /// </summary>
        public AccessClientTokenPair Data { get; internal set; }

    }
}
