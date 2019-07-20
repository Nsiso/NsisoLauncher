using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Responses;
using System;
using System.Net;
using System.Threading.Tasks;
using static NsisoLauncherCore.Net.MojangApi.Responses.ProfileResponse;

namespace NsisoLauncherCore.Net.MojangApi.Endpoints
{

    /// <summary>
    /// Profile请求类
    /// </summary>
    public class Profile : IEndpoint<ProfileResponse>
    {

        /// <summary>
        /// 将未签名设置应用于请求
        /// </summary>
        public bool Unsigned { get; private set; }

        /// <summary>
        /// 返回玩家的用户名和其他信息
        /// </summary>
        /// <param name="uuid">Player UUID</param>
        /// <param name="unsigned"></param>
        public Profile(string uuid, bool unsigned = true)
        {
            this.Unsigned = unsigned;

            if (this.Unsigned)
                this.Address = new Uri($"https://sessionserver.mojang.com/session/minecraft/profile/{uuid}");
            else
                this.Address = new Uri($"https://sessionserver.mojang.com/session/minecraft/profile/{uuid}?unsigned=false");
            this.Arguments.Add(uuid);
            this.Arguments.Add(unsigned.ToString());
        }

        /// <summary>
        /// 执行profile请求
        /// </summary>
        public async override Task<ProfileResponse> PerformRequestAsync()
        {
            this.Response = await Requester.Get(this);

            if (this.Response.IsSuccess)
            {
                JObject profile = JObject.Parse(this.Response.RawMessage);

                return new ProfileResponse(this.Response)
                {
                    Uuid = new Uuid()
                    {
                        PlayerName = profile["name"].ToObject<string>(),
                        Value = profile["id"].ToObject<string>()
                    },
                    Properties = new ProfileProperties(profile["properties"].ToObject<JArray>()[0]["value"].ToObject<string>())
                };
            }
            else
            {
                if (this.Response.Code == (HttpStatusCode)429)
                {
                    ProfileResponseError error = new ProfileResponseError(JObject.Parse(this.Response.RawMessage));
                    return new ProfileResponse(this.Response) { Error = error };
                }
                else
                    return new ProfileResponse(Error.GetError(this.Response));
            }
        }
    }

}
