using NsisoLauncherCore.Net.Mirrors;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net
{
    /// <summary>
    /// 下载任务接口，主要用于多线程下载器，并与UI绑定
    /// 接口应与UI绑定项相符
    /// </summary>
    public interface IDownloadTask : INotifyPropertyChanged
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        string TaskName { get; set; }

        /// <summary>
        /// 下载总大小
        /// </summary>
        long TotalSize { get; set; }

        /// <summary>
        /// 已下载的大小
        /// </summary>
        long DownloadedSize { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        string State { get; set; }

        /// <summary>
        /// 显示的下载源
        /// </summary>
        string DisplayFrom { get; set; }

        /// <summary>
        /// 显示的下载源
        /// </summary>
        string DisplayTo { get; set; }

        /// <summary>
        /// 显示的下载源
        /// </summary>
        string DisplayDownloadSourceName { get; set; }

        /// <summary>
        /// 显示的原下载URI
        /// </summary>
        string DisplayOriginFrom { get; set; }

        ProgressCallback ProgressCallback { get; set; }

        Task<DownloadResult> DownloadAsync(CancellationToken cancellationToken,
            ManualResetEventSlim manualResetEvent,
            IDownloadableMirror mirror,
            DownloadSetting downloadSetting);
    }
}
