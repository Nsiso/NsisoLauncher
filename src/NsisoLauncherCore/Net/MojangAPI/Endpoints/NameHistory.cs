using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Responses;
using System;
using System.Threading.Tasks;
using static NsisoLauncherCore.Net.MojangApi.Responses.NameHistoryResponse;

namespace NsisoLauncherCore.Net.MojangApi.Endpoints
{

    /// <summary>
    /// UUID命名历史endpoint
    /// </summary>
    public class NameHistory : IEndpoint<NameHistoryResponse>
    {

        /// <summary>
        /// 返回用户过去使用的所有用户名
        /// </summary>
        /// <param name="uuid">User's UUID.</param>
        public NameHistory(string uuid)
        {
            this.Address = new Uri($"https://api.mojang.com/user/profiles/{uuid}/names");
        }

        /// <summary>
        /// 执行名称历史记录请求
        /// </summary>
        /// <returns></returns>
        public async override Task<NameHistoryResponse> PerformRequestAsync()
        {
            this.Response = await Requester.Get(this);

            if (this.Response.IsSuccess)
            {
                JArray entries = JArray.Parse(this.Response.RawMessage);

                NameHistoryResponse history = new NameHistoryResponse(this.Response);
                foreach (JToken entry in entries)
                {
                    history.NameHistory.Add(new NameHistoryEntry()
                    {
                        Name = entry["name"].ToObject<string>(),
                        ChangedToAt = (entry.Last != entry.First ?
                                      new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(entry["changedToAt"].ToObject<double>()).ToLocalTime() :
                                      new DateTime?())
                    });
                }
                return history;
            }
            else
                return new NameHistoryResponse(Error.GetError(this.Response));
        }
    }


}
