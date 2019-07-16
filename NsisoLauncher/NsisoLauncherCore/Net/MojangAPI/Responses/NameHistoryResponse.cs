using System;
using System.Collections.Generic;

namespace NsisoLauncherCore.Net.MojangApi.Responses
{
    /// <summary>
    /// 包含所有名称更改的响应
    /// </summary>
    public class NameHistoryResponse : Response
    {
        internal NameHistoryResponse(Response response) : base(response)
        {
            this.NameHistory = new List<NameHistoryEntry>();
        }

        /// <summary>
        /// 所有名称变更的历史
        /// </summary>
        public List<NameHistoryEntry> NameHistory { get; internal set; }

        /// <summary>
        /// 名称历史记录的记录
        /// </summary>
        public class NameHistoryEntry
        {
            /// <summary>
            /// 选择的名字
            /// </summary>
            public string Name { get; internal set; }

            /// <summary>
            /// 用户名称更改的日期
            /// </summary>
            public DateTime? ChangedToAt { get; internal set; }
        }
    }
}
