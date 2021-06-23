using NsisoLauncherCore.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Modules
{
    /// <summary>
    /// 基本数据要素类
    /// </summary>
    public class Sha1SizeUrl : IDownloadable
    {
        /// <summary>
        /// SHA1
        /// </summary>
        public string Sha1 { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 下载URL
        /// </summary>
        public string Url { get; set; }

        public string GetDownloadSourceURL()
        {
            return Url;
        }
    }

    /// <summary>
    /// 基本数据要素类（包括路径）
    /// </summary>
    public class PathSha1SizeUrl : Sha1SizeUrl
    {
        /// <summary>
        /// 路径
        /// </summary>
        public string Path { get; set; }
    }
}
