using NsisoLauncherCore.Net.MojangApi.Api;
using System;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.MojangApi.Endpoints
{

    /// <summary>
    /// 安全位置的IP请求类
    /// </summary>
    public class SecureIP : IEndpoint<Response>
    {

        /// <summary>
        /// 实例化允许查看此IP是否安全的endpoint
        /// </summary>
        /// <param name="token">A valid user token.</param>
        public SecureIP(string accessToken)
        {
            this.Address = new Uri("https://api.mojang.com/user/security/location");
            this.Arguments.Add(accessToken);
        }

        /// <summary>
        /// 执行请求并返回响应属性
        /// </summary>
        public async override Task<Response> PerformRequestAsync()
        {
            this.Response = await Requester.Get(this, true);

            if (this.Response.IsSuccess)
                return new Response(this.Response);
            else
                return new Response(Error.GetError(this.Response));
        }
    }
}
