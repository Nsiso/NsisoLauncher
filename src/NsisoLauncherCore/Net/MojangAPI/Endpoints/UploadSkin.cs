using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using NsisoLauncherCore.Net.MojangApi.Api;

namespace NsisoLauncherCore.Net.MojangApi.Endpoints
{
    /// <summary>
    ///     上传皮肤endpoint类
    /// </summary>
    public class UploadSkin : IEndpoint<Response>
    {
        /// <summary>
        ///     使用给定的UUID创建更改外观请求
        /// </summary>
        /// <param name="accessToken">玩家的验证令牌</param>
        /// <param name="uuid">玩家UUID</param>
        /// <param name="skin">皮肤路径</param>
        /// <param name="slim">定义是否使用slim模型</param>
        public UploadSkin(string accessToken, string uuid, FileInfo skin, bool slim = false)
        {
            Address = new Uri($"https://api.mojang.com/user/profile/{uuid}/skin");
            Arguments.Add(accessToken);
            Arguments.Add(slim.ToString());
            Skin = skin;
        }

        /// <summary>
        ///     选择皮肤本地路径
        /// </summary>
        public FileInfo Skin { get; internal set; }

        /// <summary>
        ///     执行换肤
        /// </summary>
        public override async Task<Response> PerformRequestAsync()
        {
            Response = await Requester.Put(this, Skin);

            if (Response.Code == HttpStatusCode.NoContent || Response.IsSuccess)
                return new Response(Response) {IsSuccess = true};
            return new Response(Response);
        }
    }
}