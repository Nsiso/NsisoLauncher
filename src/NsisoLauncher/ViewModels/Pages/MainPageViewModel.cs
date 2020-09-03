using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;
using NsisoLauncher.Config;
using NsisoLauncher.Utils;
using NsisoLauncher.Views.Dialogs;
using NsisoLauncher.Views.Windows;
using NsisoLauncherCore.Auth;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Endpoints;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Version = NsisoLauncherCore.Modules.Version;
using MahAppsMetroHamburgerMenuNavigation.ViewModels;

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
