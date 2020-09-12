using NsisoLauncherCore.Net;
using System.ComponentModel;

namespace NsisoLauncher.ViewModels.Pages
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 下载数
        /// </summary>
        public int DownloadTaskCount { get; set; }

        public MainPageViewModel()
        {
            if (App.Handler != null)
            {
                App.NetHandler.Downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                App.NetHandler.Downloader.DownloadCompleted += Downloader_DownloadCompleted;
            }
        }

        #region 下载事件处理
        private void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedArg e)
        {
            DownloadTaskCount = e.LeftTasksCount;
        }
        private void Downloader_DownloadCompleted(object sender, DownloadCompletedArg e)
        {
            DownloadTaskCount = 0;
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
