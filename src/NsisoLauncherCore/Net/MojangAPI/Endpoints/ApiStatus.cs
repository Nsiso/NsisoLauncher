using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.MojangApi.Endpoints
{
    /// <summary>
    /// Api状态请求类
    /// </summary>
    public class ApiStatus : IEndpoint<ApiStatusResponse>
    {

        /// <summary>
        /// 实例化获得Mojang的API状态的endpoint.
        /// </summary>
        public ApiStatus()
        {
            this.Address = new Uri("https://status.mojang.com/check");
        }

        /// <summary>
        /// 执行请求并返回响应属性.
        /// </summary>
        public async override Task<ApiStatusResponse> PerformRequestAsync()
        {
            this.Response = await Requester.Get(this);

            if (this.Response.IsSuccess)
            {
                JArray jstatuses = JArray.Parse(this.Response.RawMessage);

                // Fixes #6 - 13/04/2018
                Dictionary<string, string> statuses = new Dictionary<string, string>();
                foreach (JObject item in jstatuses)
                {
                    JToken token = item.First;
                    statuses.Add(
                        ((JProperty)token).Name,
                        token.First.ToString());
                }

                string v = null;
                return new ApiStatusResponse(this.Response)
                {
                    Minecraft = ApiStatusResponse.Parse(statuses.TryGetValue("minecraft.net", out v) ? v : null),
                    Sessions = ApiStatusResponse.Parse(statuses.TryGetValue("session.minecraft.net", out v) ? v : null),
                    MojangAccounts = ApiStatusResponse.Parse(statuses.TryGetValue("account.mojang.com", out v) ? v : null),
                    MojangAutenticationServers = ApiStatusResponse.Parse(statuses.TryGetValue("authserver.mojang.com", out v) ? v : null),
                    MojangSessionsServer = ApiStatusResponse.Parse(statuses.TryGetValue("sessionserver.mojang.com", out v) ? v : null),
                    MojangApi = ApiStatusResponse.Parse(statuses.TryGetValue("api.mojang.com", out v) ? v : null),
                    Textures = ApiStatusResponse.Parse(statuses.TryGetValue("textures.minecraft.net", out v) ? v : null),
                    Mojang = ApiStatusResponse.Parse(statuses.TryGetValue("mojang.com", out v) ? v : null),

                    // These two seems to not get taken into account anymore
                    MojangAuthenticationService = ApiStatusResponse.Parse(statuses.TryGetValue("auth.mojang.com", out v) ? v : null),
                    Skins = ApiStatusResponse.Parse(statuses.TryGetValue("skins.minecraft.net", out v) ? v : null),
                };
            }
            else
                return new ApiStatusResponse(Error.GetError(this.Response));

        }
    }


}
