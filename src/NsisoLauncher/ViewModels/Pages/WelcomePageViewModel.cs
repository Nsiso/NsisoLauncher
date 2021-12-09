using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Config;
using NsisoLauncher.Utils;
using NsisoLauncher.Views.Pages;
using NsisoLauncher.Views.Windows;
using NsisoLauncherCore;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net.Apis.Modules.Yggdrasil;
using NsisoLauncherCore.Net.Apis.Modules.Yggdrasil.Requests;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.Apis;
using NsisoLauncherCore.Net.Apis.Modules;
using NsisoLauncherCore.Net.MicrosoftLogin;
using NsisoLauncherCore.Net.Tools;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NsisoLauncherCore.Authenticator;

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
            #region check java runtime
            if (App.JavaList == null || App.JavaList.Count == 0)
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
                    DownloadWindow downloadWindow = new DownloadWindow();
                    string core = "java-runtime-alpha";
                    NativeJavaMeta java = await LauncherMetaApi.GetNativeJavaMeta(core, CancellationSource.Token);
                    List<IDownloadTask> tasks = GetDownloadUri.GetJavaDownloadTasks(java);
                    App.NetHandler.Downloader.AddDownloadTask(tasks);
                    downloadWindow.Show();
                    await App.NetHandler.Downloader.StartDownloadAndWaitDone();
                    App.RefreshJavaList();
                }
            }

            //if (App.Handler.Java != null && !App.Handler.Java.Version.StartsWith("1.16"))
            //{
            //    var result = await App.MainWindowVM.ShowMessageAsync("Java版本过时",
            //        "官方从21w19a版本开始建议使用JAVA16，请及时更新，需要自动下载JRE16（Zulu）吗？",
            //        MessageDialogStyle.AffirmativeAndNegative, null);
            //    if (result == MessageDialogResult.Affirmative)
            //    {
            //        var arch = SystemTools.GetSystemArch();
            //        DownloadWindow downloadWindow = new DownloadWindow();
            //        App.NetHandler.Downloader.AddDownloadTask(GetJavaInstaller.GetDownloadTask("16", arch, JavaImageType.JRE,
            //                   () =>
            //                   {
            //                       App.Current.Dispatcher.Invoke(() =>
            //                       {
            //                           App.RefreshJavaList();
            //                       });
            //                   }));
            //        downloadWindow.Show();
            //        await App.NetHandler.Downloader.StartDownload();
            //    }
            //}
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
                IAuthenticator authenticator = null;
                switch (authenticationNode.AuthType)
                {
                    case AuthenticationType.OFFLINE:
                        return;
                    case AuthenticationType.MOJANG:
                        authenticator = new MojangAuthenticator();
                        NowState = "正在进行正版登录";
                        break;
                    case AuthenticationType.NIDE8:
                        authenticator = new Nide8Authenticator(authenticationNode.Property["nide8ID"]);
                        NowState = "正在进行统一通行证登录";
                        break;
                    case AuthenticationType.AUTHLIB_INJECTOR:
                        authenticator = new AuthlibInjectorAuthenticator(authenticationNode.Property["authserver"]);
                        NowState = "正在进行Authlib_injector登录";
                        break;
                    case AuthenticationType.CUSTOM_SERVER:
                        authenticator = new YggdrasilAuthenticator(authenticationNode.Property["authserver"]);
                        NowState = string.Format("正在进行{0}登录", authenticationNode.Name);
                        break;
                    case AuthenticationType.MICROSOFT:
                        {
                            try
                            {
                                NowState = string.Format("正在进行微软登录", authenticationNode.Name);
                                if (selectedUser.User is MicrosoftUser ms && !string.IsNullOrWhiteSpace(ms.MinecraftToken?.AccessToken))
                                {
                                    //检查minecraft token是否过期

                                    //如果未过期
                                    if (Jwt.ValidateExp(ms.MinecraftToken.AccessToken))
                                    {
                                        break;
                                    }

                                    //如果过期
                                    //检查微软token是否过期
                                    OauthLoginWindow loginWindow = new OauthLoginWindow(App.NetHandler.Requester);
                                    loginWindow.CancelToken = CancellationSource.Token;
                                    if (DateTime.UtcNow < ms.MicrosoftToken.IssuedTime.AddSeconds(ms.MicrosoftToken.Expires_in))
                                    {
                                        ms.MinecraftToken = await loginWindow.RefreshMinecraftToken(ms.MicrosoftToken);
                                    }
                                    else
                                    {
                                        await loginWindow.Login();
                                        if (loginWindow.LoggedInUser != null)
                                        {
                                            ms.MicrosoftToken = loginWindow.LoggedInUser.MicrosoftToken;
                                            //todo 感觉微软登录刷新还没写完
                                            break;
                                        }
                                        else
                                        {
                                            await App.MainWindowVM.ShowMessageAsync("登录失败", "请检查微软登录是否成功进行");
                                        }
                                    }
                                }
                                //todo 刷新微软账户
                                return;
                            }
                            catch (Exception ex)
                            {
                                await App.MainWindowVM.ShowMessageAsync(string.Format("登录失败", ex.Message), ex.ToString());
                            }
                        }
                        return;
                    default:
                        authenticator = new YggdrasilAuthenticator(authenticationNode.Property["authserver"]);
                        NowState = string.Format("正在进行{0}登录", authenticationNode.Name);
                        break;
                }

                if (authenticator == null)
                {
                    return;
                }

                AccessClientTokenPair tokens = new AccessClientTokenPair()
                {
                    AccessToken = selectedUser.User.LaunchAccessToken,
                    ClientToken = clientToken
                };

                var result = await authenticator.Validate(tokens, CancellationSource.Token);

                if (result.IsSuccess)
                {
                    return;
                }
                else
                {
                    var refresh_result = await authenticator.Refresh(new RefreshRequest(tokens), CancellationSource.Token);
                    if (refresh_result.IsSuccess)
                    {
                        if (selectedUser.User is YggdrasilUser ygg)
                        {
                            ygg.AccessToken = refresh_result.Data.AccessToken;
                        }
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
