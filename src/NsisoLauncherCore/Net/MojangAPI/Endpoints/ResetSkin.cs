using System;
using System.Net;
using System.Threading.Tasks;
using NsisoLauncherCore.Net.MojangApi.Api;

namespace NsisoLauncherCore.Net.MojangApi.Endpoints
{
    /// <summary>
    ///     删除用户皮肤
    /// </summary>
    public class ResetSkin : IEndpoint<Response>
    {
        /// <summary>
        ///     使用给定的UUID创建更改外观请求
        /// </summary>
        /// <param name="accessToken">玩家的验证令牌</param>
        /// <param name="uuid">玩家UUID</param>
        public ResetSkin(string accessToken, string uuid)
        {
            Address = new Uri($"https://api.mojang.com/user/profile/{uuid}/skin");
            Arguments.Add(accessToken);
        }

        /// <summary>
        ///     执行换肤
        /// </summary>
        public override async Task<Response> PerformRequestAsync()
        {
            Response = await Requester.Delete(this);

            if (Response.Code == HttpStatusCode.NoContent || Response.IsSuccess)
                return new Response(Response) {IsSuccess = true};
            return new Response(Response);
        }
    }
}