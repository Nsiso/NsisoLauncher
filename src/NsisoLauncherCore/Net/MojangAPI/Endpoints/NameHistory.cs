using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Responses;
using static NsisoLauncherCore.Net.MojangApi.Responses.NameHistoryResponse;

namespace NsisoLauncherCore.Net.MojangApi.Endpoints
{
    /// <summary>
    ///     UUID命名历史endpoint
    /// </summary>
    public class NameHistory : IEndpoint<NameHistoryResponse>
    {
        /// <summary>
        ///     返回用户过去使用的所有用户名
        /// </summary>
        /// <param name="uuid">User's UUID.</param>
        public NameHistory(string uuid)
        {
            Address = new Uri($"https://api.mojang.com/user/profiles/{uuid}/names");
        }

        /// <summary>
        ///     执行名称历史记录请求
        /// </summary>
        /// <returns></returns>
        public override async Task<NameHistoryResponse> PerformRequestAsync()
        {
            Response = await Requester.Get(this);

            if (Response.IsSuccess)
            {
                var entries = JArray.Parse(Response.RawMessage);

                var history = new NameHistoryResponse(Response);
                foreach (var entry in entries)
                    history.NameHistory.Add(new NameHistoryEntry
                    {
                        Name = entry["name"].ToObject<string>(),
                        ChangedToAt = entry.Last != entry.First
                            ? new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                                .AddMilliseconds(entry["changedToAt"].ToObject<double>()).ToLocalTime()
                            : new DateTime?()
                    });
                return history;
            }

            return new NameHistoryResponse(Error.GetError(Response));
        }
    }
}