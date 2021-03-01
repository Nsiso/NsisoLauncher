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

        public int SelectedIndex { get; set; }

        public int SelectedOptionsIndex { get; set; } = -1;

        public MainPageViewModel()
        {
            App.MainPageVM = this;
            if (App.NetHandler != null)
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

        public void NavigateToLaunchPage()
        {
            SelectedIndex = 0;
        }

        public void NavigateToExtendPage()
        {
            SelectedIndex = 1;
        }

        public void NavigateToDownloadPage()
        {
            SelectedOptionsIndex = 0;
        }

        public void NavigateToSettingPage()
        {
            SelectedOptionsIndex = 1;
        }

        public void NavigateToUserPage()
        {
            SelectedOptionsIndex = 2;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
