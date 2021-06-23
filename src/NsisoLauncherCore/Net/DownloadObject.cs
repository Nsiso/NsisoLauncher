using NsisoLauncherCore.Util.Checker;
using System;
using System.IO;
using System.Threading;

namespace NsisoLauncherCore.Net
{
    public class DownloadObject
    {
        /// <summary>
        /// 任务下载来源
        /// </summary>
        public IDownloadable Downloadable { get; set; }

        /// <summary>
        /// 下载原本来源
        /// </summary>
        public string OriginFrom { get; set; }

        /// <summary>
        /// 下载到路径
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get => Path.GetFileName(To); }

        /// <summary>
        /// 下载完成后执行方法
        /// </summary>
        public Func<ProgressCallback, CancellationToken, Exception> Todo { get; set; }

        /// <summary>
        /// 校验器，不设置即不校验
        /// </summary>
        public IChecker Checker { get; set; }

        public DownloadObject(IDownloadable downloadable, string to)
        {
            Downloadable = downloadable;
            OriginFrom = downloadable.GetDownloadSourceURL();
            To = to;
        }
    }
}
