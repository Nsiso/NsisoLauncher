using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Config;
using NsisoLauncher.Utils;
using NsisoLauncher.Views.Pages;
using NsisoLauncher.Views.Windows;
using NsisoLauncherCore.Auth;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Modules.Yggdrasil;
using NsisoLauncherCore.Modules.Yggdrasil.Requests;
using NsisoLauncherCore.Net.Tools;
using NsisoLauncherCore.Net.Yggdrasil;
using NsisoLauncherCore.Util;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NsisoLauncher.ViewModels.Pages
{
    public class WelcomePageViewModel : INotifyPropertyChanged
    {
        public DelegateCommand LoadedCommand { get; set; }

        /// <summary>
        /// 欢迎Image源
        /// </summary>
        public string WelcomeImageSource { get; set; }

        /// <summary>
        /// 欢迎页面标题
        /// </summary>
        public string WelcomeTitle { get; set; }

        /// <summary>
        /// Icon源
        /// </summary>
        public string IconImageSource { get; set; } = "/NsisoLauncher;component/Resource/icon.ico";

        public string NowState { get; set; }

        public CancellationTokenSource CancellationSource { get; set; }

        public ICommand CancelCmd { get; set; }

        public WelcomePageViewModel()
        {
            if (App.Config != null)
            {
                if (!string.IsNullOrWhiteSpace(App.Config.MainConfig?.Customize?.WelcomeTitle))
                {
                    this.WelcomeTitle = App.Config.MainConfig.Customize.WelcomeTitle;
                }
                if (!string.IsNullOrWhiteSpace(App.Config.MainConfig?.Customize?.WelcomeImageSource))
                {
                    this.WelcomeImageSource = App.Config.MainConfig.Customize.WelcomeImageSource;
                }
                if (!string.IsNullOrWhiteSpace(App.Config.MainConfig?.Customize?.WelcomeIconSource))
                {
                    this.IconImageSource = App.Config.MainConfig.Customize.WelcomeIconSource;
                }
            }

            LoadedCommand = new DelegateCommand(async (a) =>
            {
                await App.RefreshVersionListAsync();
                await CheckEnvironment();
                MainPage mainPage = new MainPage();
                App.MainWindowVM.NavigationService.Navigate(mainPage);
            });

            this.CancellationSource = new CancellationTokenSource();

            CancelCmd = new DelegateCommand((a) =>
            {
                this.CancellationSource.Cancel();
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;


        private async Task CheckEnvironment()
        {
            #region 无JAVA提示
            if ((App.JavaList == null || App.JavaList.Count == 0) && App.Handler.Java == null)
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
                    DownloadWindow downloadWindow = new DownloadWindow();
                    App.NetHandler.Downloader.AddDownloadTask(GetJavaInstaller.GetDownloadTask("8", arch, JavaImageType.JRE,
                               () =>
                               {
                                   App.Current.Dispatcher.Invoke(() =>
                                   {
                                       App.RefreshJavaList();
                                   });
                               }));
                    downloadWindow.Show();
                    await App.NetHandler.Downloader.StartDownload();
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
                var ver = await App.NetHandler.NsisoAPIHandler.GetLatestLauncherVersion(CancellationSource.Token);
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
            UserNode selectedUser = App.Config.MainConfig.User.SelectedUser;
            string clientToken = App.Config.MainConfig.User.ClientToken;
            if (selectedUser != null)
            {
                AuthenticationNode authenticationNode = App.Config.MainConfig.User.GetUserAuthenticationNode(selectedUser);
                IAuthenticator authenticator;
                switch (authenticationNode.AuthType)
                {
                    case AuthenticationType.OFFLINE:
                        return;
                    case AuthenticationType.MOJANG:
                        authenticator = new MojangAuthenticator(App.NetHandler.Requester);
                        NowState = "正在进行正版登录";
                        break;
                    case AuthenticationType.NIDE8:
                        authenticator = new Nide8Authenticator(App.NetHandler.Requester, authenticationNode.Property["Nide8ID"]);
                        NowState = "正在进行统一通行证登录";
                        break;
                    case AuthenticationType.AUTHLIB_INJECTOR:
                        authenticator = new YggdrasilAuthenticator(new Uri(authenticationNode.Property["authserver"]), App.NetHandler.Requester);
                        NowState = "正在进行Authlib_injector登录";
                        break;
                    case AuthenticationType.CUSTOM_SERVER:
                        authenticator = new YggdrasilAuthenticator(new Uri(authenticationNode.Property["authserver"]), App.NetHandler.Requester);
                        NowState = string.Format("正在进行{0}登录", authenticationNode.Name);
                        break;
                    case AuthenticationType.MICROSOFT:
                        return;
                    default:
                        authenticator = new YggdrasilAuthenticator(new Uri(authenticationNode.Property["authserver"]), App.NetHandler.Requester);
                        NowState = string.Format("正在进行{0}登录", authenticationNode.Name);
                        break;
                }

                AccessClientTokenPair tokens = new AccessClientTokenPair()
                {
                    AccessToken = selectedUser.AccessToken,
                    ClientToken = clientToken
                };

                var result = await authenticator.Validate(tokens);

                if (result.IsSuccess)
                {
                    return;
                }
                else
                {
                    var refresh_result = await authenticator.Refresh(new RefreshRequest(tokens));
                    if (refresh_result.IsSuccess)
                    {
                        selectedUser.AccessToken = refresh_result.Data.AccessToken;
                    }
                    else
                    {
                        await App.MainWindowVM.ShowMessageAsync("当前登录用户信息已过期", "请在用户页面重新登录");
                        App.Config.MainConfig.User.SelectedUserUuid = null;
                    }
                }
            }
        }
    }
}
