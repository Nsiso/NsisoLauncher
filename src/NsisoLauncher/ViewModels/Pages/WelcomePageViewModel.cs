using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Utils;
using NsisoLauncher.Views.Pages;
using NsisoLauncher.Views.Windows;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Util;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace NsisoLauncher.ViewModels.Pages
{
    public class WelcomePageViewModel : INotifyPropertyChanged
    {
        public DelegateCommand LoadedCommand { get; set; }

        public string NowState { get; set; }

        public WelcomePageViewModel()
        {
            LoadedCommand = new DelegateCommand(async (a) =>
            {
                await App.RefreshVersionListAsync();
                await CheckEnvironment();
                MainPage mainPage = new MainPage();
                App.MainWindowVM.NavigationService.Navigate(mainPage);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;


        private async Task CheckEnvironment()
        {
            #region 无JAVA提示
            if (App.Handler.Java == null)
            {
                var result = await App.MainWindowVM.ShowMessageAsync(App.GetResourceString("String.Message.NoJava"),
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
                NowState = "正在检查更新...";
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
    }
}
