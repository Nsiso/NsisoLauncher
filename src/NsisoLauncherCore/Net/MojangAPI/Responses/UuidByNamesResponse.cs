using System.Collections.Generic;
using NsisoLauncherCore.Net.MojangApi.Api;

namespace NsisoLauncherCore.Net.MojangApi.Responses
{
    /// <summary>
    ///     包含UUID列表的响应
    /// </summary>
    public class UuidByNamesResponse : Response
    {
        internal UuidByNamesResponse(Response response) : base(response)
        {
        }

        /// <summary>
        ///     与名称对应的UUID列表
        /// </summary>
        public List<Uuid> UuidList { get; internal set; }
    }
}