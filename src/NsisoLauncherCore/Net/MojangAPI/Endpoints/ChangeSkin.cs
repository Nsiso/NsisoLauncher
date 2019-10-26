using NsisoLauncherCore.Net.MojangApi.Api;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.MojangApi.Endpoints
{

    /// <summary>
    /// 通过URL更改玩家的皮肤
    /// </summary>
    public class ChangeSkin : IEndpoint<Response>
    {
        /// <summary>
        /// 使用给定的UUID创建更改外观请求
        /// </summary>
        /// <param name="accessToken">用户访问令牌</param>
        /// <param name="uuid">玩家的UUID</param>
        /// <param name="skinUrl">皮肤的URL</param>
        /// <param name="slim">定义是否使用slim模型</param>
        public ChangeSkin(string accessToken, string uuid, string skinUrl, bool slim = false)
        {
            this.Address = new Uri($"https://api.mojang.com/user/profile/{uuid}/skin");
            this.Arguments.Add(accessToken);
            this.Arguments.Add(skinUrl);
            this.Arguments.Add(slim.ToString());
        }

        /// <summary>
        /// 执行换肤
        /// </summary>
        public async override Task<Response> PerformRequestAsync()
        {
            this.Response = await Requester.Post(this, new Dictionary<string, string>() {
                { "model", (bool.Parse(Arguments[2]) == true ? "slim" : null) },
                { "url", Arguments[1] },
            });

            if (this.Response.Code == HttpStatusCode.NoContent || this.Response.IsSuccess)
                return new Response(this.Response) { IsSuccess = true };
            else
                return new Response(this.Response);
        }
    }
}
