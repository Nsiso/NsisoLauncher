using NsisoLauncherCore.Net.MojangApi.Api;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.MojangApi.Endpoints
{

    /// <summary>
    /// 上传皮肤endpoint类
    /// </summary>
    public class UploadSkin : IEndpoint<Response>
    {

        /// <summary>
        /// 选择皮肤本地路径
        /// </summary>
        public FileInfo Skin { get; internal set; }

        /// <summary>
        /// 使用给定的UUID创建更改外观请求
        /// </summary>
        /// <param name="accessToken">玩家的验证令牌</param>
        /// <param name="uuid">玩家UUID</param>
        /// <param name="skin">皮肤路径</param>
        /// <param name="slim">定义是否使用slim模型</param>
        public UploadSkin(string accessToken, string uuid, FileInfo skin, bool slim = false)
        {
            this.Address = new Uri($"https://api.mojang.com/user/profile/{uuid}/skin");
            this.Arguments.Add(accessToken);
            this.Arguments.Add(slim.ToString());
            this.Skin = skin;
        }

        /// <summary>
        /// 执行换肤
        /// </summary>
        public async override Task<Response> PerformRequestAsync()
        {
            this.Response = await Requester.Put(this, this.Skin);

            if (this.Response.Code == HttpStatusCode.NoContent || this.Response.IsSuccess)
                return new Response(this.Response) { IsSuccess = true };
            else
                return new Response(this.Response);
        }
    }
}
