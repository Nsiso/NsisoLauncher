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

        public Windows.MainWindowViewModel MainWindowVM { get; set; }

        /// <summary>
        /// 下载数
        /// </summary>
        public int DownloadTaskCount { get; set; }

        public MainPageViewModel()
        {
            if (App.MainWindowVM != null)
            {
                this.MainWindowVM = App.MainWindowVM;
            }

            if (App.Handler != null)
            {
                App.NetHandler.Downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                App.NetHandler.Downloader.DownloadCompleted += Downloader_DownloadCompleted;

                //检查环境
                _ = CheckEnvironment();
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

        private async Task CheckEnvironment()
        {
            #region 无JAVA提示
            if (App.Handler.Java == null)
            {
                var result = await MainWindowVM.ShowMessageAsync(App.GetResourceString("String.Message.NoJava"),
                    App.GetResourceString("String.Message.NoJava2"),
                    MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings()
                    {
                        AffirmativeButtonText = App.GetResourceString("String.Base.Yes"),
                        NegativeButtonText = App.GetResourceString("String.Base.Cancel"),
                        DefaultButtonFocus = MessageDialogResult.Affirmative
                    });
                if (result == MessageDialogResult.Affirmative)
                {
                    var arch = SystemTools.GetSystemArch();
                    switch (arch)
                    {
                        case ArchEnum.x32:
                            App.NetHandler.Downloader.AddDownloadTask(new DownloadTask("32位JAVA安装包", new StringUrl(@"https://bmclapi.bangbang93.com/java/jre_x86.exe"), "jre_x86.exe"));
                            await App.NetHandler.Downloader.StartDownload();
                            await new DownloadWindow().ShowWhenDownloading();
                            System.Diagnostics.Process.Start("Explorer.exe", "jre_x86.exe");
                            break;
                        case ArchEnum.x64:
                            App.NetHandler.Downloader.AddDownloadTask(new DownloadTask("64位JAVA安装包", new StringUrl(@"https://bmclapi.bangbang93.com/java/jre_x64.exe"), "jre_x64.exe"));
                            await App.NetHandler.Downloader.StartDownload();
                            await new DownloadWindow().ShowWhenDownloading();
                            System.Diagnostics.Process.Start("Explorer.exe", "jre_x64.exe");
                            break;
                        default:
                            break;
                    }
                }
            }
            #endregion

            #region 检查更新
            if (App.Config.MainConfig.Launcher.CheckUpdate)
            {
                await CheckUpdate();
            }
            #endregion
        }

        private async Task CheckUpdate()
        {
            try
            {
                var ver = await App.NetHandler.NsisoAPIHandler.GetLatestLauncherVersion();
                if (ver != null)
                {
                    System.Version currentVersion = Application.ResourceAssembly.GetName().Version;
                    if ((ver.Version > currentVersion) &&
                        ver.ReleaseType.Equals("release", StringComparison.OrdinalIgnoreCase))
                    {
                        new UpdateWindow(ver).Show();
                    }
                }
            }
            catch (Exception e)
            { App.LogHandler.AppendError(e); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
