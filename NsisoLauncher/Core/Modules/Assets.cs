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
        public List<AssetsInfo> Infos { get; set; }
    }

    /// <summary>
    /// 表示资源文件信息
    /// </summary>
    public class AssetsInfo
    {
        /// <summary>
        /// Hash
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// 大小
        /// </summary>
        public int Size { get; set; }
    }
}
