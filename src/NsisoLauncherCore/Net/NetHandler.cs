using NsisoLauncherCore.Net.Mirrors;
using System;

namespace NsisoLauncherCore.Net
{
    public class NetHandler : IDisposable
    {
        /// <summary>
        /// 启用不追踪
        /// </summary>
        public bool NoTracking { get; set; }

        /// <summary>
        /// 下载器
        /// </summary>
        public MultiThreadDownloader Downloader { get; set; }

        /// <summary>
        /// 镜像仓库
        /// </summary>
        public MirrorInventory Mirrors { get; set; }

        /// <summary>
        /// 应用API
        /// </summary>
        public PhalAPI.APIHandler NsisoAPIHandler { get; private set; }

        public NetHandler()
        {
            Mirrors = new MirrorInventory();
#if DEBUG
            NsisoAPIHandler = new PhalAPI.APIHandler(true);
#else
            NsisoAPIHandler = new PhalAPI.APIHandler(NoTracking);
#endif
            Downloader = new MultiThreadDownloader();
            Downloader.MirrorList = Mirrors.DownloadableMirrorList;
        }

        public void Dispose()
        {
            Downloader.Dispose();
        }
    }
}
