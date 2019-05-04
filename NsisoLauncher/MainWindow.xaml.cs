using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net.MojangApi.Endpoints;
using System.IO;
using System.Linq;
using System.Threading;
using NsisoLauncherCore.Util;
using NsisoLauncher.Windows;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore;
using NsisoLauncherCore.Auth;

namespace NsisoLauncher
{
    public class AuthTypeItem
    {
        public Config.AuthenticationType Type { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private List<AuthTypeItem> authTypes = new List<AuthTypeItem>()
        {
            new AuthTypeItem(){Type = Config.AuthenticationType.OFFLINE, Name = App.GetResourceString("String.MainWindow.Auth.Offline")},
            new AuthTypeItem(){Type = Config.AuthenticationType.MOJANG, Name = App.GetResourceString("String.MainWindow.Auth.Mojang")},
            new AuthTypeItem(){Type = Config.AuthenticationType.NIDE8, Name = App.GetResourceString("String.MainWindow.Auth.Nide8")},
            new AuthTypeItem(){Type = Config.AuthenticationType.CUSTOM_SERVER, Name = App.GetResourceString("String.MainWindow.Auth.Custom")}
        };

        public MainWindow()
        {
            InitializeComponent();
            App.logHandler.AppendDebug("启动器主窗体已载入");
            App.handler.GameExit += Handler_GameExit;
            Refresh();
            CustomizeRefresh();
        }

        private void Handler_GameExit(object sender, GameExitArg arg)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.WindowState = WindowState.Normal;
                if (!arg.IsNormalExit())
                {
                    this.ShowMessageAsync("游戏非正常退出", "这很有可能是因为游戏崩溃导致的，退出代码:" + arg.ExitCode);
                }
            }));
        }

        private async void Refresh()
        {
            this.playerNameTextBox.Text = App.config.MainConfig.User.UserName;
            authTypeCombobox.ItemsSource = this.authTypes;

            //全局统一验证设置
            bool isAllUsingNide8 = (App.nide8Handler != null) && App.config.MainConfig.User.AllUsingNide8;
            if (isAllUsingNide8)
            {
                authTypeCombobox.SelectedItem = authTypes.Find(x => x.Type == Config.AuthenticationType.NIDE8);
                authTypeCombobox.IsEnabled = false;
            }
            else
            {
                this.authTypeCombobox.SelectedItem = authTypes.Find(x => x.Type == App.config.MainConfig.User.AuthenticationType);
            }

            launchVersionCombobox.ItemsSource = await App.handler.GetVersionsAsync();
            this.launchVersionCombobox.Text = App.config.MainConfig.History.LastLaunchVersion;

            //头像自定义显示皮肤
            bool isNeedRefreshIcon = (!string.IsNullOrWhiteSpace(App.config.MainConfig.User.AuthenticationUUID?.Value)) &&
                App.config.MainConfig.User.AuthenticationType == Config.AuthenticationType.MOJANG;
            if (isNeedRefreshIcon)
            {
                await headScul.RefreshIcon(App.config.MainConfig.User.AuthenticationUUID.Value);
            }
            App.logHandler.AppendDebug("启动器主窗体数据重载完毕");
        }

        #region 自定义
        private async void CustomizeRefresh()
        {
            if (!string.IsNullOrWhiteSpace(App.config.MainConfig.Customize.LauncherTitle))
            {
                this.Title = App.config.MainConfig.Customize.LauncherTitle;
            }
            if (App.config.MainConfig.Customize.CustomBackGroundPicture)
            {
                string[] files = Directory.GetFiles(Path.GetDirectoryName(App.config.MainConfigPath), "bgpic_?.png");
                if (files.Count() != 0)
                {
                    Random random = new Random();
                    ImageBrush brush = new ImageBrush(new BitmapImage(new Uri(files[random.Next(files.Count())])))
                    { TileMode = TileMode.FlipXY, AlignmentX = AlignmentX.Right, Stretch = Stretch.UniformToFill };
                    this.Background = brush;
                }
            }

            if ((App.nide8Handler != null) && App.config.MainConfig.User.AllUsingNide8)
            {
                try
                {
                    Config.Server nide8Server = new Config.Server() { ShowServerInfo = true };
                    var nide8ReturnResult = await App.nide8Handler.GetInfoAsync();
                    if (!string.IsNullOrWhiteSpace(nide8ReturnResult.Meta.ServerIP))
                    {
                        string[] serverIp = nide8ReturnResult.Meta.ServerIP.Split(':');
                        if (serverIp.Length == 2)
                        {
                            nide8Server.Address = serverIp[0];
                            nide8Server.Port = ushort.Parse(serverIp[1]);
                        }
                        else
                        {
                            nide8Server.Address = nide8ReturnResult.Meta.ServerIP;
                            nide8Server.Port = 25565;
                        }
                        nide8Server.ServerName = nide8ReturnResult.Meta.ServerName;
                        serverInfoControl.SetServerInfo(nide8Server);
                    }
                }
                catch (Exception)
                { }
            }
            else if (App.config.MainConfig.Server != null)
            {
                serverInfoControl.SetServerInfo(App.config.MainConfig.Server);
            }

            if (App.config.MainConfig.Customize.CustomBackGroundMusic)
            {
                string[] files = Directory.GetFiles(Path.GetDirectoryName(App.config.MainConfigPath), "bgmusic_?.mp3");
                if (files.Count() != 0)
                {
                    Random random = new Random();
                    mediaElement.Source = new Uri(files[random.Next(files.Count())]);
                    this.volumeButton.Visibility = Visibility.Visible;
                    mediaElement.Play();
                    mediaElement.Volume = 0;
                    await Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            for (int i = 0; i < 50; i++)
                            {
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    this.mediaElement.Volume += 0.01;
                                }));
                                Thread.Sleep(50);
                            }
                        }
                        catch (Exception) { }
                    });
                }
            }

        }

        private void volumeButton_Click(object sender, RoutedEventArgs e)
        {
            this.mediaElement.IsMuted = !this.mediaElement.IsMuted;
        }

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElement.Stop();
            mediaElement.Play();
        }
        #endregion

        private async void launchButton_Click(object sender, RoutedEventArgs e)
        {
            await LaunchGameFromWindow();
        }

        private async Task LaunchGameFromWindow()
        {
            try
            {
                string userName = playerNameTextBox.Text;

                #region 检查有效数据
                if (authTypeCombobox.SelectedItem == null)
                {
                    await this.ShowMessageAsync(App.GetResourceString("String.Message.EmptyAuthType"),
                        App.GetResourceString("String.Message.EmptyAuthType2"));
                    return;
                }
                if (string.IsNullOrWhiteSpace(userName))
                {
                    await this.ShowMessageAsync(App.GetResourceString("String.Message.EmptyUsername"),
                        App.GetResourceString("String.Message.EmptyUsername2"));
                    return;
                }
                if (launchVersionCombobox.SelectedItem == null)
                {
                    await this.ShowMessageAsync(App.GetResourceString("String.Message.EmptyLaunchVersion"),
                        App.GetResourceString("String.Message.EmptyLaunchVersion2"));
                    return;
                }
                if (App.handler.Java == null)
                {
                    await this.ShowMessageAsync(App.GetResourceString("String.Message.NoJava"), App.GetResourceString("String.Message.NoJava2"));
                    return;
                }
                #endregion

                NsisoLauncherCore.Modules.Version launchVersion = (NsisoLauncherCore.Modules.Version)launchVersionCombobox.SelectedItem;
                AuthTypeItem auth = (AuthTypeItem)authTypeCombobox.SelectedItem;

                #region 保存启动数据
                App.config.MainConfig.History.LastLaunchVersion = launchVersion.ID;
                #endregion

                LaunchSetting launchSetting = new LaunchSetting()
                {
                    Version = (NsisoLauncherCore.Modules.Version)launchVersionCombobox.SelectedItem
                };

                this.loadingGrid.Visibility = Visibility.Visible;
                this.loadingRing.IsActive = true;

                #region 验证

                #region 设置ClientToken
                if (string.IsNullOrWhiteSpace(App.config.MainConfig.User.ClientToken))
                {
                    App.config.MainConfig.User.ClientToken = Guid.NewGuid().ToString("N");
                }
                else
                {
                    Requester.ClientToken = App.config.MainConfig.User.ClientToken;
                }
                #endregion

                #region 多语言支持变量(old)
                //string autoVerifyingMsg = null;
                //string autoVerifyingMsg2 = null;
                //string autoVerificationFailedMsg = null;
                //string autoVerificationFailedMsg2 = null;
                //string loginMsg = null;
                //string loginMsg2 = null;
                LoginDialogSettings loginDialogSettings = new LoginDialogSettings()
                {
                    NegativeButtonText = App.GetResourceString("String.Base.Cancel"),
                    AffirmativeButtonText = App.GetResourceString("String.Base.Login"),
                    RememberCheckBoxText = App.GetResourceString("String.Base.ShouldRememberLogin"),
                    UsernameWatermark = App.GetResourceString("String.Base.Username"),
                    InitialUsername = userName,
                    RememberCheckBoxVisibility = Visibility,
                    EnablePasswordPreview = true,
                    PasswordWatermark = App.GetResourceString("String.Base.Password"),
                    NegativeButtonVisibility = Visibility.Visible
                };
                //string verifyingMsg = null, verifyingMsg2 = null, verifyingFailedMsg = null, verifyingFailedMsg2 = null;
                #endregion

                //主验证器接口
                IAuthenticator authenticator = null;
                bool shouldRemember = false;

                bool isSameAuthType = App.config.MainConfig.User.AuthenticationType == auth.Type;
                bool isRemember = (!string.IsNullOrWhiteSpace(App.config.MainConfig.User.AccessToken)) && (App.config.MainConfig.User.AuthenticationUUID != null);
                bool isSameName = userName == App.config.MainConfig.User.UserName;

                switch (auth.Type)
                {
                    #region 离线验证
                    case Config.AuthenticationType.OFFLINE:
                        authenticator = new OfflineAuthenticator(userName);
                        break;
                    #endregion

                    #region Mojang验证
                    case Config.AuthenticationType.MOJANG:
                        if (isSameAuthType && isSameName && isRemember)
                        {
                            var mYggTokenAuthenticator = new YggdrasilTokenAuthenticator(App.config.MainConfig.User.AccessToken,
                                App.config.MainConfig.User.AuthenticationUUID,
                                App.config.MainConfig.User.AuthenticationUserData);
                            mYggTokenAuthenticator.ProxyAuthServerAddress = "https://authserver.mojang.com";
                            authenticator = mYggTokenAuthenticator;
                        }
                        else
                        {
                            var mojangLoginDResult = await this.ShowLoginAsync(App.GetResourceString("String.Mainwindow.Auth.Mojang.Login"),
                                App.GetResourceString("String.Mainwindow.Auth.Mojang.Login2"),
                                loginDialogSettings);
                            if (IsValidateLoginData(mojangLoginDResult))
                            {
                                var mYggAuthenticator = new YggdrasilAuthenticator(new Credentials()
                                {
                                    Username = mojangLoginDResult.Username,
                                    Password = mojangLoginDResult.Password
                                });
                                mYggAuthenticator.ProxyAuthServerAddress = "https://authserver.mojang.com";
                                authenticator = mYggAuthenticator;
                                shouldRemember = mojangLoginDResult.ShouldRemember;
                            }
                            else
                            {
                                await this.ShowMessageAsync("您输入的账号或密码为空", "请检查您是否正确填写登陆信息");
                                return;
                            }
                        }
                        break;
                    #endregion

                    #region 统一通行证验证
                    case Config.AuthenticationType.NIDE8:
                        if (App.nide8Handler == null)
                        {
                            await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Nide8.NoID"),
                                App.GetResourceString("String.Mainwindow.Auth.Nide8.NoID2"));
                            return;
                        }
                        var nide8ChooseResult = await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Nide8.Login2"), App.GetResourceString("String.Base.Choose"),
                            MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
                            new MetroDialogSettings()
                            {
                                AffirmativeButtonText = App.GetResourceString("String.Base.Login"),
                                NegativeButtonText = App.GetResourceString("String.Base.Cancel"),
                                FirstAuxiliaryButtonText = App.GetResourceString("String.Base.Register"),
                                DefaultButtonFocus = MessageDialogResult.Affirmative
                            });
                        switch (nide8ChooseResult)
                        {
                            case MessageDialogResult.Canceled:
                                return;
                            case MessageDialogResult.Negative:
                                return;
                            case MessageDialogResult.FirstAuxiliary:
                                System.Diagnostics.Process.Start(string.Format("https://login2.nide8.com:233/{0}/register", App.nide8Handler.ServerID));
                                return;
                            case MessageDialogResult.Affirmative:
                                if (isSameAuthType && isSameName && isRemember)
                                {
                                    var nYggTokenCator = new Nide8TokenAuthenticator(App.config.MainConfig.User.AccessToken,
                                        App.config.MainConfig.User.AuthenticationUUID,
                                        App.config.MainConfig.User.AuthenticationUserData);
                                    nYggTokenCator.ProxyAuthServerAddress = string.Format("{0}authserver", App.nide8Handler.BaseURL);
                                    authenticator = nYggTokenCator;
                                }
                                else
                                {
                                    var nide8LoginDResult = await this.ShowLoginAsync(App.GetResourceString("String.Mainwindow.Auth.Nide8.Login"),
                                        App.GetResourceString("String.Mainwindow.Auth.Nide8.Login2"),
                                        loginDialogSettings);
                                    if (IsValidateLoginData(nide8LoginDResult))
                                    {
                                        var nYggCator = new Nide8Authenticator(new Credentials()
                                        {
                                            Username = nide8LoginDResult.Username,
                                            Password = nide8LoginDResult.Password
                                        });
                                        nYggCator.ProxyAuthServerAddress = string.Format("{0}authserver", App.nide8Handler.BaseURL);
                                        authenticator = nYggCator;
                                        shouldRemember = nide8LoginDResult.ShouldRemember;
                                    }
                                    else
                                    {
                                        await this.ShowMessageAsync("您输入的账号或密码为空", "请检查您是否正确填写登陆信息");
                                        return;
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    #endregion

                    #region 自定义验证
                    case Config.AuthenticationType.CUSTOM_SERVER:
                        string customAuthServer = App.config.MainConfig.User.AuthServer;
                        if (string.IsNullOrWhiteSpace(customAuthServer))
                        {
                            await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Custom.NoAdrress"),
                                App.GetResourceString("String.Mainwindow.Auth.Custom.NoAdrress2"));
                            return;
                        }
                        else
                        {
                            if (shouldRemember && isSameName && isSameAuthType)
                            {
                                var cYggTokenCator = new YggdrasilTokenAuthenticator(App.config.MainConfig.User.AccessToken,
                                App.config.MainConfig.User.AuthenticationUUID,
                                App.config.MainConfig.User.AuthenticationUserData);
                                cYggTokenCator.ProxyAuthServerAddress = customAuthServer;
                            }
                            else
                            {
                                var customLoginDResult = await this.ShowLoginAsync(App.GetResourceString("String.Mainwindow.Auth.Custom.Login"),
                               App.GetResourceString("String.Mainwindow.Auth.Custom.Login2"),
                               loginDialogSettings);
                                if (IsValidateLoginData(customLoginDResult))
                                {
                                    var cYggAuthenticator = new YggdrasilAuthenticator(new Credentials()
                                    {
                                        Username = customLoginDResult.Username,
                                        Password = customLoginDResult.Password
                                    });
                                    cYggAuthenticator.ProxyAuthServerAddress = customAuthServer;
                                    authenticator = cYggAuthenticator;
                                    shouldRemember = customLoginDResult.ShouldRemember;
                                }
                                else
                                {
                                    await this.ShowMessageAsync("您输入的账号或密码为空", "请检查您是否正确填写登陆信息");
                                    return;
                                }
                            }
                        }
                        break;
                    #endregion

                    default:
                        authenticator = new OfflineAuthenticator(userName);
                        break;
                }

                #region Old
                //if (auth.Type != Config.AuthenticationType.OFFLINE)
                //{
                //    try
                //    {
                //        #region 如果记住登陆(有登陆记录)
                //        if ((!string.IsNullOrWhiteSpace(App.config.MainConfig.User.AccessToken)) && (App.config.MainConfig.User.AuthenticationUUID != null))
                //        {
                //            authenticator = new YggdrasilTokenAuthenticator(App.config.MainConfig.User.AccessToken,
                //                App.config.MainConfig.User.AuthenticationUUID,
                //                App.config.MainConfig.User.AuthenticationUserData);
                //            //var loader = await this.ShowProgressAsync(autoVerifyingMsg, autoVerifyingMsg2);
                //            //loader.SetIndeterminate();
                //            //Validate validate = new Validate(App.config.MainConfig.User.AccessToken);
                //            //var validateResult = await validate.PerformRequestAsync();
                //            //if (validateResult.IsSuccess)
                //            //{

                //            //    launchSetting.AuthenticateAccessToken = App.config.MainConfig.User.AccessToken;
                //            //    launchSetting.AuthenticateUUID = App.config.MainConfig.User.AuthenticationUUID;
                //            //    await loader.CloseAsync();
                //            //}
                //            //else
                //            //{
                //            //    Refresh refresher = new Refresh(App.config.MainConfig.User.AccessToken);
                //            //    var refreshResult = await refresher.PerformRequestAsync();
                //            //    await loader.CloseAsync();
                //            //    if (refreshResult.IsSuccess)
                //            //    {
                //            //        App.config.MainConfig.User.AccessToken = refreshResult.AccessToken;

                //            //        launchSetting.AuthenticateUUID = App.config.MainConfig.User.AuthenticationUUID;
                //            //        launchSetting.AuthenticateAccessToken = refreshResult.AccessToken;
                //            //    }
                //            //    else
                //            //    {
                //            //        App.config.MainConfig.User.AccessToken = string.Empty;
                //            //        App.config.Save();
                //            //        await this.ShowMessageAsync(autoVerificationFailedMsg, autoVerificationFailedMsg2);
                //            //        return;
                //            //    }
                //            //}
                //        }
                //        #endregion

                //        #region 从零登陆
                //        else
                //        {
                //            var loginMsgResult = await this.ShowLoginAsync(loginMsg, loginMsg2, loginDialogSettings);

                //            if (loginMsgResult == null)
                //            {
                //                return;
                //            }
                //            var loader = await this.ShowProgressAsync(verifyingMsg, verifyingMsg2);
                //            loader.SetIndeterminate();
                //            userName = loginMsgResult.Username;
                //            Authenticate authenticate = new Authenticate(new Credentials() { Username = loginMsgResult.Username, Password = loginMsgResult.Password });
                //            var aloginResult = await authenticate.PerformRequestAsync();
                //            await loader.CloseAsync();
                //            if (aloginResult.IsSuccess)
                //            {
                //                if (loginMsgResult.ShouldRemember)
                //                {
                //                    App.config.MainConfig.User.AccessToken = aloginResult.AccessToken;
                //                }
                //                App.config.MainConfig.User.AuthenticationUserData = aloginResult.User;
                //                App.config.MainConfig.User.AuthenticationUUID = aloginResult.SelectedProfile;

                //                launchSetting.AuthenticateAccessToken = aloginResult.AccessToken;
                //                launchSetting.AuthenticateUUID = aloginResult.SelectedProfile;
                //                launchSetting.AuthenticationUserData = aloginResult.User;
                //            }
                //            else
                //            {
                //                await this.ShowMessageAsync(verifyingFailedMsg, verifyingFailedMsg2 + aloginResult.Error.ErrorMessage);
                //                return;
                //            }
                //        }
                //        #endregion
                //    }
                //    catch (Exception ex)
                //    {
                //        await this.ShowMessageAsync(verifyingFailedMsg, verifyingFailedMsg2 + ex.Message);
                //        return;
                //    }
                //}
                #endregion

                //如果验证方式不是离线验证
                if (auth.Type != Config.AuthenticationType.OFFLINE)
                {
                    string currentLoginType = string.Format("正在进行{0}中...", auth.Name);
                    string loginMsg = "这需要联网进行操作，可能需要一分钟的时间";
                    var loader = await this.ShowProgressAsync(currentLoginType, loginMsg, true);
                    
                    loader.SetIndeterminate();
                    var authResult = await authenticator.DoAuthenticateAsync();
                    await loader.CloseAsync();

                    switch (authResult.State)
                    {
                        case AuthState.SUCCESS:
                            if (shouldRemember)
                            {
                                App.config.MainConfig.User.AccessToken = authResult.AccessToken;
                                App.config.MainConfig.User.AuthenticationUUID = authResult.UUID;
                                App.config.MainConfig.User.AuthenticationUserData = authResult.UserData;
                            }
                            launchSetting.AuthenticateResult = authResult;
                            break;
                        case AuthState.REQ_LOGIN:
                            await this.ShowMessageAsync("验证失败：您的登陆信息已过期",
                                string.Format("请您重新进行登陆。具体信息：{0}", authResult.Error.ErrorMessage));
                            return;
                        case AuthState.ERR_INVALID_CRDL:
                            await this.ShowMessageAsync("验证失败：您的登陆账号或密码错误",
                                string.Format("请您确认您输入的账号密码正确。具体信息：{0}", authResult.Error.ErrorMessage));
                            return;
                        case AuthState.ERR_NOTFOUND:
                            await this.ShowMessageAsync("验证失败：您的账号未找到",
                                string.Format("请确认您的账号和游戏角色存在。具体信息：{0}", authResult.Error.ErrorMessage));
                            return;
                        case AuthState.ERR_OTHER:
                            await this.ShowMessageAsync("验证失败：其他错误",
                                string.Format("具体信息：{0}", authResult.Error.ErrorMessage));
                            return;
                        case AuthState.ERR_INSIDE:
                            await this.ShowMessageAsync("验证失败：启动器内部错误",
                                string.Format("建议您联系启动器开发者进行解决。具体信息：{0}", authResult.Error.ErrorMessage));
                            return;
                        default:
                            await this.ShowMessageAsync("验证失败：未知错误",
                                "建议您联系启动器开发者进行解决。");
                            return;
                    }
                }
                else
                {
                    launchSetting.AuthenticateResult = await authenticator.DoAuthenticateAsync();
                }
                App.config.MainConfig.User.AuthenticationType = auth.Type;
                App.config.MainConfig.User.UserName = userName;
                #endregion

                #region 检查游戏完整
                List<DownloadTask> losts = new List<DownloadTask>();

                App.logHandler.AppendInfo("检查丢失的依赖库文件中...");
                var lostDepend = await FileHelper.GetLostDependDownloadTaskAsync(
                    App.config.MainConfig.Download.DownloadSource,
                    App.handler,
                    launchSetting.Version);

                if (auth.Type == Config.AuthenticationType.NIDE8)
                {
                    string nideJarPath = App.handler.GetNide8JarPath();
                    if (!File.Exists(nideJarPath))
                    {
                        lostDepend.Add(new DownloadTask("统一通行证核心", "https://login2.nide8.com:233/index/jar", nideJarPath));
                    }
                }
                if (App.config.MainConfig.Environment.DownloadLostDepend && lostDepend.Count != 0)
                {
                    MessageDialogResult downDependResult = await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.NeedDownloadDepend"),
                        App.GetResourceString("String.Mainwindow.NeedDownloadDepend2"),
                        MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, new MetroDialogSettings()
                        {
                            AffirmativeButtonText = App.GetResourceString("String.Base.Download"),
                            NegativeButtonText = App.GetResourceString("String.Base.Cancel"),
                            FirstAuxiliaryButtonText = App.GetResourceString("String.Base.Unremember"),
                            DefaultButtonFocus = MessageDialogResult.Affirmative
                        });
                    switch (downDependResult)
                    {
                        case MessageDialogResult.Affirmative:
                            losts.AddRange(lostDepend);
                            break;
                        case MessageDialogResult.FirstAuxiliary:
                            App.config.MainConfig.Environment.DownloadLostDepend = false;
                            break;
                        default:
                            break;
                    }

                }

                App.logHandler.AppendInfo("检查丢失的资源文件中...");
                if (App.config.MainConfig.Environment.DownloadLostAssets && (await FileHelper.IsLostAssetsAsync(App.config.MainConfig.Download.DownloadSource,
                    App.handler, launchSetting.Version)))
                {
                    MessageDialogResult downDependResult = await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.NeedDownloadAssets"),
                        App.GetResourceString("String.Mainwindow.NeedDownloadAssets2"),
                        MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, new MetroDialogSettings()
                        {
                            AffirmativeButtonText = App.GetResourceString("String.Base.Download"),
                            NegativeButtonText = App.GetResourceString("String.Base.Cancel"),
                            FirstAuxiliaryButtonText = App.GetResourceString("String.Base.Unremember"),
                            DefaultButtonFocus = MessageDialogResult.Affirmative
                        });
                    switch (downDependResult)
                    {
                        case MessageDialogResult.Affirmative:
                            var lostAssets = await FileHelper.GetLostAssetsDownloadTaskAsync(
                                App.config.MainConfig.Download.DownloadSource,
                                App.handler, launchSetting.Version);
                            losts.AddRange(lostAssets);
                            break;
                        case MessageDialogResult.FirstAuxiliary:
                            App.config.MainConfig.Environment.DownloadLostAssets = false;
                            break;
                        default:
                            break;
                    }

                }

                if (losts.Count != 0)
                {
                    App.downloader.SetDownloadTasks(losts);
                    App.downloader.StartDownload();
                    await new Windows.DownloadWindow().ShowWhenDownloading();
                }

                #endregion

                #region 根据配置文件设置
                launchSetting.AdvencedGameArguments += App.config.MainConfig.Environment.AdvencedGameArguments;
                launchSetting.AdvencedJvmArguments += App.config.MainConfig.Environment.AdvencedJvmArguments;
                launchSetting.GCArgument += App.config.MainConfig.Environment.GCArgument;
                launchSetting.GCEnabled = App.config.MainConfig.Environment.GCEnabled;
                launchSetting.GCType = App.config.MainConfig.Environment.GCType;
                launchSetting.JavaAgent += App.config.MainConfig.Environment.JavaAgent;
                if (auth.Type == Config.AuthenticationType.NIDE8)
                {
                    launchSetting.JavaAgent += string.Format(" \"{0}\"={1}", App.handler.GetNide8JarPath(), App.config.MainConfig.User.Nide8ServerID);
                }

                //直连服务器设置
                if ((auth.Type == Config.AuthenticationType.NIDE8) && App.config.MainConfig.User.AllUsingNide8)
                {
                    var nide8ReturnResult = await App.nide8Handler.GetInfoAsync();
                    if (App.config.MainConfig.User.AllUsingNide8 && !string.IsNullOrWhiteSpace(nide8ReturnResult.Meta.ServerIP))
                    {
                        Server server = new Server();
                        string[] serverIp = nide8ReturnResult.Meta.ServerIP.Split(':');
                        if (serverIp.Length == 2)
                        {
                            server.Address = serverIp[0];
                            server.Port = ushort.Parse(serverIp[1]);
                        }
                        else
                        {
                            server.Address = nide8ReturnResult.Meta.ServerIP;
                            server.Port = 25565;
                        }
                        launchSetting.LaunchToServer = server;
                    }
                }
                else if (App.config.MainConfig.Server.LaunchToServer)
                {
                    launchSetting.LaunchToServer = new Server() { Address = App.config.MainConfig.Server.Address, Port = App.config.MainConfig.Server.Port };
                }

                //自动内存设置
                if (App.config.MainConfig.Environment.AutoMemory)
                {
                    var m = SystemTools.GetBestMemory(App.handler.Java);
                    App.config.MainConfig.Environment.MaxMemory = m;
                    launchSetting.MaxMemory = m;
                }
                else
                {
                    launchSetting.MaxMemory = App.config.MainConfig.Environment.MaxMemory;
                }
                launchSetting.VersionType = App.config.MainConfig.Customize.VersionInfo;
                launchSetting.WindowSize = App.config.MainConfig.Environment.WindowSize;
                #endregion

                #region 配置文件处理
                App.config.Save();
                #endregion

                #region 启动

                App.logHandler.OnLog += (a, b) => { this.Invoke(() => { launchInfoBlock.Text = b.Message; }); };
                var result = await App.handler.LaunchAsync(launchSetting);
                App.logHandler.OnLog -= (a, b) => { this.Invoke(() => { launchInfoBlock.Text = b.Message; }); };

                //程序猿是找不到女朋友的了 :) 
                if (!result.IsSuccess)
                {
                    await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.LaunchError") + result.LaunchException.Title, result.LaunchException.Message);
                    App.logHandler.AppendError(result.LaunchException);
                }
                else
                {
                    try
                    {
                        await Task.Factory.StartNew(() =>
                        {
                            result.Process.WaitForInputIdle();
                        });
                    }
                    catch (Exception ex)
                    {
                        await this.ShowMessageAsync("启动后等待游戏窗口响应异常", "这可能是由于游戏进程发生意外（闪退）导致的。具体原因:" + ex.Message);
                        return;
                    }
                    if (App.config.MainConfig.Environment.ExitAfterLaunch)
                    {
                        Application.Current.Shutdown();
                    }
                    this.WindowState = WindowState.Minimized;
                    Refresh();

                    //自定义处理
                    if (!string.IsNullOrWhiteSpace(App.config.MainConfig.Customize.GameWindowTitle))
                    {
                        GameHelper.SetGameTitle(result, App.config.MainConfig.Customize.GameWindowTitle);
                    }
                    if (App.config.MainConfig.Customize.CustomBackGroundMusic)
                    {
                        mediaElement.Volume = 0.5;
                        await Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                for (int i = 0; i < 50; i++)
                                {
                                    this.Dispatcher.Invoke(new Action(() =>
                                    {
                                        this.mediaElement.Volume -= 0.01;
                                    }));
                                    Thread.Sleep(50);
                                }
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    this.mediaElement.Stop();
                                }));
                            }
                            catch (Exception) { }
                        });
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                App.logHandler.AppendFatal(ex);
            }
            finally
            {
                this.loadingGrid.Visibility = Visibility.Hidden;
                this.loadingRing.IsActive = false;
            }
        }


        private void downloadButton_Click(object sender, RoutedEventArgs e)
        {
            new DownloadWindow().ShowDialog();
            Refresh();
        }

        private void configButton_Click(object sender, RoutedEventArgs e)
        {
            new SettingWindow().ShowDialog();
            Refresh();
            CustomizeRefresh();
        }

        private async void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            #region 无JAVA提示
            if (App.handler.Java == null)
            {
                var result = await this.ShowMessageAsync(App.GetResourceString("String.Message.NoJava"),
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
                            App.downloader.SetDownloadTasks(new DownloadTask("32位JAVA安装包", @"https://bmclapi.bangbang93.com/java/jre_x86.exe", "jre_x86.exe"));
                            App.downloader.StartDownload();
                            await new DownloadWindow().ShowWhenDownloading();
                            System.Diagnostics.Process.Start("Explorer.exe", "jre_x86.exe");
                            break;
                        case ArchEnum.x64:
                            App.downloader.SetDownloadTasks(new DownloadTask("64位JAVA安装包", @"https://bmclapi.bangbang93.com/java/jre_x64.exe", "jre_x64.exe"));
                            App.downloader.StartDownload();
                            await new DownloadWindow().ShowWhenDownloading();
                            System.Diagnostics.Process.Start("Explorer.exe", "jre_x64.exe");
                            break;
                        default:
                            break;
                    }
                }
            }
            #endregion

        }

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (App.downloader.IsBusy)
            {
                var choose = this.ShowModalMessageExternal("后台正在下载中", "是否确认关闭程序？这将会取消下载"
                , MessageDialogStyle.AffirmativeAndNegative,
                new MetroDialogSettings()
                {
                    AffirmativeButtonText = App.GetResourceString("String.Base.Yes"),
                    NegativeButtonText = App.GetResourceString("String.Base.Cancel")
                });
                if (choose == MessageDialogResult.Affirmative)
                {
                    App.downloader.RequestStop();
                }
                else
                {
                    e.Cancel = true;
                }
            }

        }

        private bool IsValidateLoginData(LoginDialogData data)
        {
            if (data == null)
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(data.Username))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(data.Password))
            {
                return false;
            }
            return true;
        }
    }
}
