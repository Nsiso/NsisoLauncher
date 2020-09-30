using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Config;
using NsisoLauncher.Utils;
using NsisoLauncher.Views.Pages;
using NsisoLauncher.Views.Windows;
using NsisoLauncherCore.Auth;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.MojangApi.Api;
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
                await CheckUpdate();
            }
            #endregion

            #region 处理用户
            await RefreshUser();
            #endregion
        }

        private async Task CheckUpdate()
        {
            try
            {
                NowState = "正在检查更新...";
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

        private async Task RefreshUser()
        {
            NowState = "正在登录用户";
            UserNode selectedUser = App.Config.MainConfig.User.GetSelectedUser();
            if (selectedUser != null)
            {
                PlayerProfile selectedProfile = selectedUser.SelectedProfile;
                AuthenticationNode authenticationNode = App.Config.MainConfig.User.GetUserAuthenticationNode(selectedUser);
                if (authenticationNode.AuthType == AuthenticationType.OFFLINE)
                {
                    App.LogedInUser = selectedUser;
                }
                else if (authenticationNode.AuthType == AuthenticationType.NIDE8)
                {
                    Nide8TokenAuthenticator nideTokenAuthenticator = new Nide8TokenAuthenticator(
                            authenticationNode.Property["Nide8ID"], selectedUser.AccessToken);
                    NowState = "正在进行统一通行证登录";
                    var nide8Result = await nideTokenAuthenticator.DoAuthenticateAsync();
                    if (nide8Result.State == AuthState.SUCCESS)
                    {
                        selectedUser.AccessToken = nide8Result.AccessToken;
                        App.LogedInUser = selectedUser;
                    }
                }
                else
                {
                    YggdrasilTokenAuthenticator tokenAuthenticator = new YggdrasilTokenAuthenticator(selectedUser.AccessToken);
                    NowState = "正在进行正版登录";
                    var mojangResult = await tokenAuthenticator.DoAuthenticateAsync();
                    if (mojangResult.State == AuthState.SUCCESS)
                    {
                        selectedUser.AccessToken = mojangResult.AccessToken;
                        App.LogedInUser = selectedUser;
                    }
                }
            }
        }
    }
}
