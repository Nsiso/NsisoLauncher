using NsisoLauncherCore.Net.MojangApi.Api;
using System;

namespace NsisoLauncherCore.Net.MojangApi.Responses
{

    /// <summary>
    /// 包含玩家在给定时间的UUID的响应
    /// </summary>
    public class UuidAtTimeResponse : Response
    {
        internal UuidAtTimeResponse(Response response) : base(response)
        {
        }

        /// <summary>
        /// Uuid
        /// </summary>
        public Uuid Uuid { get; internal set; }

        /// <summary>
        /// UUID对应的日期
        /// </summary>
        public DateTime Date { get; internal set; }
    }

}
