using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Responses;
using static NsisoLauncherCore.Net.MojangApi.Responses.ProfileResponse;

namespace NsisoLauncherCore.Net.MojangApi.Endpoints
{
    /// <summary>
    ///     Profile请求类
    /// </summary>
    public class Profile : IEndpoint<ProfileResponse>
    {
        /// <summary>
        ///     返回玩家的用户名和其他信息
        /// </summary>
        /// <param name="uuid">Player UUID</param>
        /// <param name="unsigned"></param>
        public Profile(string uuid, bool unsigned = true)
        {
            Unsigned = unsigned;

            if (Unsigned)
                Address = new Uri($"https://sessionserver.mojang.com/session/minecraft/profile/{uuid}");
            else
                Address = new Uri($"https://sessionserver.mojang.com/session/minecraft/profile/{uuid}?unsigned=false");
            Arguments.Add(uuid);
            Arguments.Add(unsigned.ToString());
        }

        /// <summary>
        ///     将未签名设置应用于请求
        /// </summary>
        public bool Unsigned { get; }

        /// <summary>
        ///     执行profile请求
        /// </summary>
        public override async Task<ProfileResponse> PerformRequestAsync()
        {
            Response = await Requester.Get(this);

            if (Response.IsSuccess)
            {
                var profile = JObject.Parse(Response.RawMessage);

                return new ProfileResponse(Response)
                {
                    Uuid = new Uuid
                    {
                        PlayerName = profile["name"].ToObject<string>(),
                        Value = profile["id"].ToObject<string>()
                    },
                    Properties =
                        new ProfileProperties(profile["properties"].ToObject<JArray>()[0]["value"].ToObject<string>())
                };
            }

            if (Response.Code == (HttpStatusCode) 429)
            {
                var error = new ProfileResponseError(JObject.Parse(Response.RawMessage));
                return new ProfileResponse(Response) {Error = error};
            }

            return new ProfileResponse(Error.GetError(Response));
        }
    }
}