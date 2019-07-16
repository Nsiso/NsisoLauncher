using NsisoLauncherCore.Net.MojangApi.Api;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.MojangApi.Endpoints
{

    /// <summary>
    /// 删除用户皮肤
    /// </summary>
    public class ResetSkin : IEndpoint<Response>
    {

        /// <summary>
        /// 使用给定的UUID创建更改外观请求
        /// </summary>
        /// <param name="accessToken">玩家的验证令牌</param>
        /// <param name="uuid">玩家UUID</param>
        public ResetSkin(string accessToken, string uuid)
        {
            this.Address = new Uri($"https://api.mojang.com/user/profile/{uuid}/skin");
            this.Arguments.Add(accessToken);
        }

        /// <summary>
        /// 执行换肤
        /// </summary>
        public async override Task<Response> PerformRequestAsync()
        {
            this.Response = await Requester.Delete(this);

            if (this.Response.Code == HttpStatusCode.NoContent || this.Response.IsSuccess)
                return new Response(this.Response) { IsSuccess = true };
            else
                return new Response(this.Response);
        }
    }
}
