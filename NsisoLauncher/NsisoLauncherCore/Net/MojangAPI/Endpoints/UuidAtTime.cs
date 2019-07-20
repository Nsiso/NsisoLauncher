using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Responses;
using System;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.MojangApi.Endpoints
{

    /// <summary>
    /// UuidAtTime请求类
    /// </summary>
    public class UuidAtTime : IEndpoint<UuidAtTimeResponse>
    {

        /// <summary>
        /// 实例化允许在特定时间获得玩家的UUID的endpoint
        /// <paramref name="username">你想获得UUID的玩家的用户名</paramref>
        /// <paramref name="date">您想要获取UUID的日期</paramref>
        /// </summary>
        public UuidAtTime(string username, DateTime date)
        {
            int timespan = (Int32)date.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            this.Address = new Uri($"https://api.mojang.com/users/profiles/minecraft/{username}?at={timespan}");
            this.Arguments.Add(username);
            this.Arguments.Add(timespan.ToString());
        }

        /// <summary>
        /// 执行UuidAtTime请求
        /// </summary>
        /// <returns></returns>
        public async override Task<UuidAtTimeResponse> PerformRequestAsync()
        {
            this.Response = await Requester.Get(this);

            if (this.Response.IsSuccess)
            {
                JObject uuid = JObject.Parse(this.Response.RawMessage);


                // Fixing #6 - 13/04/2018
                return new UuidAtTimeResponse(this.Response)
                {
                    Uuid = new Uuid()
                    {
                        PlayerName = uuid["name"].ToObject<string>(),
                        Value = uuid["id"].ToObject<string>(),

                        // The accuracy of these verifications have to be verified.
                        Legacy = this.Response.RawMessage.Contains("\"legacy\""),
                        Demo = this.Response.RawMessage.Contains("\"demo\""),
                    },
                    //Date = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Convert.ToDouble(this.Arguments[1])).ToLocalTime()
                };
            }
            else
                return new UuidAtTimeResponse(Error.GetError(this.Response));
        }
    }

}
