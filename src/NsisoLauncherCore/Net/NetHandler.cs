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
        /// 请求器
        /// </summary>
        public NetRequester Requester { get; set; }

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
            Requester = new NetRequester();
            Mirrors = new MirrorInventory();
#if DEBUG
            NsisoAPIHandler = new PhalAPI.APIHandler(true, Requester);
#else
            NsisoAPIHandler = new PhalAPI.APIHandler(NoTracking, Requester);
#endif
            Downloader = new MultiThreadDownloader(Requester);
            Downloader.MirrorList = Mirrors.DownloadableMirrorList;
        }

        public void Dispose()
        {
            Downloader.Dispose();
            Requester.Dispose();
        }
    }
}
