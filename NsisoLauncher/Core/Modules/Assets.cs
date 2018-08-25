using NsisoLauncher.Core.Util;
using System.Collections.Generic;

namespace NsisoLauncher.Core.Modules
{
    /// <summary>
    /// 表示资源文件
    /// </summary>
    public class Assets
    {
        /// <summary>
        /// ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 引导文件下载地址
        /// </summary>
        public string IndexURL { get; set; }

        /// <summary>
        /// 资源文件总大小
        /// </summary>
        public long TotalSize { get; set; }

        /// <summary>
        /// 每一个资源文件的信息
        /// </summary>
        public JAssets Infos { get; set; }
    }
}
