using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Responses;

namespace NsisoLauncherCore.Net.MojangApi.Endpoints
{
    /// <summary>
    ///     UuidAtTime请求类
    /// </summary>
    public class UuidAtTime : IEndpoint<UuidAtTimeResponse>
    {
        /// <summary>
        ///     实例化允许在特定时间获得玩家的UUID的endpoint
        ///     <paramref name="username">你想获得UUID的玩家的用户名</paramref>
        ///     <paramref name="date">您想要获取UUID的日期</paramref>
        /// </summary>
        public UuidAtTime(string username, DateTime date)
        {
            var timespan = (int) date.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            Address = new Uri($"https://api.mojang.com/users/profiles/minecraft/{username}?at={timespan}");
            Arguments.Add(username);
            Arguments.Add(timespan.ToString());
        }

        /// <summary>
        ///     执行UuidAtTime请求
        /// </summary>
        /// <returns></returns>
        public override async Task<UuidAtTimeResponse> PerformRequestAsync()
        {
            Response = await Requester.Get(this);

            if (Response.IsSuccess)
            {
                var uuid = JObject.Parse(Response.RawMessage);


                // Fixing #6 - 13/04/2018
                return new UuidAtTimeResponse(Response)
                {
                    Uuid = new Uuid
                    {
                        PlayerName = uuid["name"].ToObject<string>(),
                        Value = uuid["id"].ToObject<string>(),

                        // The accuracy of these verifications have to be verified.
                        Legacy = Response.RawMessage.Contains("\"legacy\""),
                        Demo = Response.RawMessage.Contains("\"demo\"")
                    }
                    //Date = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Convert.ToDouble(this.Arguments[1])).ToLocalTime()
                };
            }

            return new UuidAtTimeResponse(Error.GetError(Response));
        }
    }
}