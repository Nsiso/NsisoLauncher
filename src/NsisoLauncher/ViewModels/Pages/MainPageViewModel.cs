using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Config;
using NsisoLauncher.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using NsisoLauncherCore.Modules;
using Version = NsisoLauncherCore.Modules.Version;
using System.Windows;
using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Auth;
using NsisoLauncherCore.Net.MojangApi.Endpoints;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Util;
using System.IO;
using NsisoLauncher.Views.Windows;
using System.Threading;
using System.Windows.Media;
using System.Windows.Controls;
using NsisoLauncherCore;

namespace NsisoLauncher.ViewModels.Pages
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public ObservableDictionary<string, AuthenticationNode> AuthNodes { get; }
        public ObservableCollection<Version> Versions { get; }
        public ObservableDictionary<string, UserNode> Users { get; }

        #region Commands
        /// <summary>
        /// 启动命令
        /// </summary>
        public ICommand LaunchCmd { get; set; }

        /// <summary>
        /// 打开设置窗口命令
        /// </summary>
        public ICommand OpenSettingCmd { get; set; }

        /// <summary>
        /// 打开下载窗口命令
        /// </summary>
        public ICommand OpenDownloadingCmd { get; set; }
        #endregion

        #region Launch Data
        public Version LaunchVersion { get; set; }
        public KeyValuePair<string, UserNode>? LaunchUserPair { get; set; }
        public KeyValuePair<string, AuthenticationNode>? LaunchAuthNodePair { get; set; }
        public string LaunchUserNameText { get; set; }
        #endregion

        #region ElementsState

        public double Volume { get; set; } = 0.5;

        public Uri MediaSource { get; set; }

        public bool IsPlaying { get; set; }

        public string BackgroundImageSource { get; set; } = "../../Resource/bg.jpg";
        #endregion

        public ViewModels.Windows.MainWindowViewModel MainWindowVM { get; set; }

        /// <summary>
        /// 下载数
        /// </summary>
        public int DownloadTaskCount { get; set; }

        public MainPageViewModel(ViewModels.Windows.MainWindowViewModel mainWindowVM)
        {
            this.MainWindowVM = mainWindowVM;

            #region 命令初始化
            LaunchCmd = new DelegateCommand(
               //launch
               async (obj) => await LaunchFromVM(), (obj) =>
               {
                   if (App.Handler == null)
                       return false;
                   else
                       return !App.Handler.IsBusyLaunching;
               });
            OpenDownloadingCmd = new DelegateCommand((obj) =>
            {
                new DownloadWindow().ShowDialog();
            });
            OpenSettingCmd = new DelegateCommand((obj) =>
            {
                new SettingWindow().ShowDialog();
            });
            #endregion


            if (App.Handler != null)
            {
                AuthNodes = App.Config.MainConfig.User.AuthenticationDic;
                Users = App.Config.MainConfig.User.UserDatabase;
                Versions = App.VersionList;

                #region 记忆
                if (!string.IsNullOrEmpty(App.Config.MainConfig.History.SelectedUserNodeID) &&
                    App.Config.MainConfig.User.UserDatabase.ContainsKey(App.Config.MainConfig.History.SelectedUserNodeID))
                {
                    KeyValuePair<string, UserNode> userPair = new KeyValuePair<string, UserNode>(App.Config.MainConfig.History.SelectedUserNodeID,
                        App.Config.MainConfig.User.UserDatabase[App.Config.MainConfig.History.SelectedUserNodeID]);
                    LaunchUserPair = userPair;
                    //LaunchUserNameText = userPair.Value.UserName;
                    if (!string.IsNullOrEmpty(userPair.Value.AuthModule) &&
                        App.Config.MainConfig.User.AuthenticationDic.ContainsKey(userPair.Value.AuthModule))
                    {
                        LaunchAuthNodePair = new KeyValuePair<string, AuthenticationNode>(userPair.Value.AuthModule,
                            App.Config.MainConfig.User.AuthenticationDic[userPair.Value.AuthModule]);
                    }
                }
                if (!string.IsNullOrEmpty(App.Config.MainConfig.History.LastLaunchVersion))
                {
                    LaunchVersion = App.VersionList.FirstOrDefault((x) => x.ID == App.Config.MainConfig.History.LastLaunchVersion);
                }
                #endregion

                App.Downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                App.Downloader.DownloadCompleted += Downloader_DownloadCompleted;

                _ = CustomizeRefresh();
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

        public async Task LaunchFromVM()
        {
            try
            {
                #region 检查有效数据
                if (LaunchVersion == null)
                {
                    await MainWindowVM.ShowMessageAsync(App.GetResourceString("String.Message.EmptyLaunchVersion"),
                        App.GetResourceString("String.Message.EmptyLaunchVersion2"));
                    return;
                }
                if (string.IsNullOrWhiteSpace(LaunchUserNameText))
                {
                    await MainWindowVM.ShowMessageAsync(App.GetResourceString("String.Message.EmptyUsername"),
                        App.GetResourceString("String.Message.EmptyUsername2"));
                    return;
                }
                if (LaunchAuthNodePair == null)
                {
                    await MainWindowVM.ShowMessageAsync(App.GetResourceString("String.Message.EmptyAuthType"),
                        App.GetResourceString("String.Message.EmptyAuthType2"));
                    return;
                }
                if (App.Handler.Java == null)
                {
                    await MainWindowVM.ShowMessageAsync(App.GetResourceString("String.Message.NoJava"), App.GetResourceString("String.Message.NoJava2"));
                    return;
                }
                #endregion

                #region 保存启动数据
                App.Config.MainConfig.History.LastLaunchVersion = LaunchVersion.ID;
                App.Config.MainConfig.History.LastLaunchTime = DateTime.Now;
                #endregion

                #region 用户处理
                bool isNewUser = false;
                UserNode launchUser = null;
                if (LaunchUserPair != null)
                {
                    isNewUser = false;
                    launchUser = LaunchUserPair?.Value;
                }
                else
                {
                    isNewUser = true;
                    launchUser = new UserNode() { UserName = LaunchUserNameText };
                    //undo: to add new user support
                }
                #endregion

                LaunchSetting launchSetting = new LaunchSetting()
                {
                    Version = LaunchVersion
                };

                //启动中DIALOG显示
                ViewModels.Dialogs.LaunchingDialogViewModel launchingDialogVM = new Dialogs.LaunchingDialogViewModel();
                Views.Dialogs.LaunchingDialog launchingDialog = new Views.Dialogs.LaunchingDialog(launchingDialogVM);
                await MainWindowVM.ShowMetroDialogAsync(launchingDialog);

                try
                {

                    #region 验证
                    AuthenticationNode LaunchAuthNode = LaunchAuthNodePair?.Value;

                    #region 设置ClientToken
                    if (string.IsNullOrWhiteSpace(App.Config.MainConfig.User.ClientToken))
                    {
                        App.Config.MainConfig.User.ClientToken = Guid.NewGuid().ToString("N");
                    }
                    else
                    {
                        Requester.ClientToken = App.Config.MainConfig.User.ClientToken;
                    }
                    #endregion

                    #region 多语言支持变量
                    LoginDialogSettings loginDialogSettings = new LoginDialogSettings()
                    {
                        NegativeButtonText = App.GetResourceString("String.Base.Cancel"),
                        AffirmativeButtonText = App.GetResourceString("String.Base.Login"),
                        RememberCheckBoxText = App.GetResourceString("String.Base.ShouldRememberLogin"),
                        UsernameWatermark = App.GetResourceString("String.Base.Username"),
                        InitialUsername = launchUser.UserName,
                        RememberCheckBoxVisibility = Visibility.Visible,
                        EnablePasswordPreview = true,
                        PasswordWatermark = App.GetResourceString("String.Base.Password"),
                        NegativeButtonVisibility = Visibility.Visible
                    };
                    #endregion

                    //主验证器接口
                    IAuthenticator authenticator = null;
                    bool shouldRemember = false;

                    //bool isSameAuthType = (authNode.AuthenticationType == auth);
                    bool isRemember = (!string.IsNullOrWhiteSpace(launchUser.AccessToken)) && ((launchUser.SelectProfileUUID != null));
                    //bool isSameName = userName == App.config.MainConfig.User.UserName;

                    switch (LaunchAuthNode.AuthType)
                    {
                        #region 离线验证
                        case AuthenticationType.OFFLINE:
                            if (isNewUser)
                            {
                                authenticator = new OfflineAuthenticator(launchUser.UserName);
                            }
                            else
                            {
                                authenticator = new OfflineAuthenticator(launchUser.UserName, launchUser.UserData, launchUser.SelectProfileUUID);
                            }
                            break;
                        #endregion

                        #region MOJANG验证
                        case AuthenticationType.MOJANG:
                            if (isRemember)
                            {
                                var mYggTokenAuthenticator = new YggdrasilTokenAuthenticator(launchUser.AccessToken,
                                    launchUser.GetSelectProfileUUID(), launchUser.UserData);
                                mYggTokenAuthenticator.ProxyAuthServerAddress = "https://authserver.mojang.com";
                                authenticator = mYggTokenAuthenticator;
                            }
                            else
                            {
                                var mojangLoginDResult = await MainWindowVM.ShowLoginAsync(App.GetResourceString("String.Mainwindow.Auth.Mojang.Login"),
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
                                    await MainWindowVM.ShowMessageAsync("您输入的账号或密码为空", "请检查您是否正确填写登录信息");
                                    return;
                                }
                            }
                            break;
                        #endregion

                        #region NIDE8验证
                        case AuthenticationType.NIDE8:
                            string nide8ID = LaunchAuthNode.Property["nide8ID"];
                            if (string.IsNullOrWhiteSpace(nide8ID))
                            {
                                await MainWindowVM.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Nide8.NoID"),
                                    App.GetResourceString("String.Mainwindow.Auth.Nide8.NoID2"));
                                return;
                            }
                            var nide8ChooseResult = await MainWindowVM.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Nide8.Login2"),
                                App.GetResourceString("String.Base.Choose"),
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
                                    System.Diagnostics.Process.Start(string.Format("https://login2.nide8.com:233/{0}/register", nide8ID));
                                    return;
                                case MessageDialogResult.Affirmative:
                                    if (isRemember)
                                    {
                                        var nYggTokenCator = new Nide8TokenAuthenticator(nide8ID, launchUser.AccessToken,
                                            launchUser.GetSelectProfileUUID(),
                                            launchUser.UserData);
                                        authenticator = nYggTokenCator;
                                    }
                                    else
                                    {
                                        var nide8LoginDResult = await MainWindowVM.ShowLoginAsync(App.GetResourceString("String.Mainwindow.Auth.Nide8.Login"),
                                            App.GetResourceString("String.Mainwindow.Auth.Nide8.Login2"),
                                            loginDialogSettings);
                                        if (IsValidateLoginData(nide8LoginDResult))
                                        {
                                            var nYggCator = new Nide8Authenticator(
                                                nide8ID,
                                                new Credentials()
                                                {
                                                    Username = nide8LoginDResult.Username,
                                                    Password = nide8LoginDResult.Password
                                                });
                                            authenticator = nYggCator;
                                            shouldRemember = nide8LoginDResult.ShouldRemember;
                                        }
                                        else
                                        {
                                            await MainWindowVM.ShowMessageAsync("您输入的账号或密码为空", "请检查您是否正确填写登录信息");
                                            return;
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        #endregion

                        #region AUTHLIB验证
                        case AuthenticationType.AUTHLIB_INJECTOR:
                            string aiRootAddr = LaunchAuthNode.Property["authserver"];
                            if (string.IsNullOrWhiteSpace(aiRootAddr))
                            {
                                await MainWindowVM.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Custom.NoAdrress"),
                                    App.GetResourceString("String.Mainwindow.Auth.Custom.NoAdrress2"));
                                return;
                            }
                            else
                            {
                                if (isRemember)
                                {
                                    var cYggTokenCator = new AuthlibInjectorTokenAuthenticator(aiRootAddr,
                                        launchUser.AccessToken,
                                        launchUser.GetSelectProfileUUID(),
                                        launchUser.UserData);
                                    authenticator = cYggTokenCator;
                                }
                                else
                                {
                                    var customLoginDResult = await MainWindowVM.ShowLoginAsync(App.GetResourceString("String.Mainwindow.Auth.Custom.Login"),
                                   App.GetResourceString("String.Mainwindow.Auth.Custom.Login2"),
                                   loginDialogSettings);
                                    if (IsValidateLoginData(customLoginDResult))
                                    {
                                        var cYggAuthenticator = new AuthlibInjectorAuthenticator(
                                            aiRootAddr,
                                            new Credentials()
                                            {
                                                Username = customLoginDResult.Username,
                                                Password = customLoginDResult.Password
                                            });
                                        authenticator = cYggAuthenticator;
                                        shouldRemember = customLoginDResult.ShouldRemember;
                                    }
                                    else
                                    {
                                        await MainWindowVM.ShowMessageAsync("您输入的账号或密码为空", "请检查您是否正确填写登录信息");
                                        return;
                                    }
                                }
                            }
                            break;
                        #endregion

                        #region 自定义验证
                        case AuthenticationType.CUSTOM_SERVER:
                            string customAuthServer = LaunchAuthNode.Property["authserver"];
                            if (string.IsNullOrWhiteSpace(customAuthServer))
                            {
                                await MainWindowVM.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Custom.NoAdrress"),
                                    App.GetResourceString("String.Mainwindow.Auth.Custom.NoAdrress2"));
                                return;
                            }
                            else
                            {
                                if (isRemember)
                                {
                                    var cYggTokenCator = new YggdrasilTokenAuthenticator(launchUser.AccessToken,
                                    launchUser.GetSelectProfileUUID(),
                                    launchUser.UserData);
                                    cYggTokenCator.ProxyAuthServerAddress = customAuthServer;
                                }
                                else
                                {
                                    var customLoginDResult = await MainWindowVM.ShowLoginAsync(App.GetResourceString("String.Mainwindow.Auth.Custom.Login"),
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
                                        await MainWindowVM.ShowMessageAsync("您输入的账号或密码为空", "请检查您是否正确填写登录信息");
                                        return;
                                    }
                                }
                            }
                            break;
                        #endregion

                        #region 意外情况
                        default:
                            if (isNewUser)
                            {
                                authenticator = new OfflineAuthenticator(launchUser.UserName);
                            }
                            else
                            {
                                authenticator = new OfflineAuthenticator(launchUser.UserName,
                                    launchUser.UserData,
                                    launchUser.SelectProfileUUID);
                            }
                            break;
                            #endregion
                    }

                    //如果验证方式不是离线验证
                    if (LaunchAuthNode.AuthType != AuthenticationType.OFFLINE)
                    {

                        string currentLoginType = string.Format("正在进行{0}中...", LaunchAuthNode.Name);
                        string loginMsg = "这需要联网进行操作，可能需要一分钟的时间";
                        var loader = await MainWindowVM.ShowProgressAsync(currentLoginType, loginMsg, true);

                        loader.SetIndeterminate();
                        var authResult = await authenticator.DoAuthenticateAsync();
                        await loader.CloseAsync();

                        #region 错误处日志
                        if (authResult.State != AuthState.SUCCESS)
                        {
                            App.LogHandler.AppendInfo(string.Format("验证失败：{0}", authResult.State));
                        }
                        #endregion

                        switch (authResult.State)
                        {
                            case AuthState.SUCCESS:
                                launchUser.SelectProfileUUID = authResult.SelectedProfileUUID.Value;
                                launchUser.UserData = authResult.UserData;
                                if (authResult.Profiles != null)
                                {
                                    launchUser.Profiles.Clear();
                                    authResult.Profiles.ForEach(x => launchUser.Profiles.Add(x.Value, x));
                                }
                                if (shouldRemember)
                                {
                                    launchUser.AccessToken = authResult.AccessToken;
                                }
                                launchSetting.AuthenticateResult = authResult;
                                break;
                            case AuthState.REQ_LOGIN:
                                //todo 添加更好的注销服务
                                launchUser.ClearAuthCache();
                                await MainWindowVM.ShowMessageAsync("验证失败：您的登录信息已过期",
                                    string.Format("请您重新进行登录。具体信息：{0}", authResult.Error.ErrorMessage));
                                return;
                            case AuthState.ERR_INVALID_CRDL:
                                await MainWindowVM.ShowMessageAsync("验证失败：您的登录账号或密码错误",
                                    string.Format("请您确认您输入的账号密码正确。具体信息：{0}", authResult.Error.ErrorMessage));
                                return;
                            case AuthState.ERR_NOTFOUND:
                                if (LaunchAuthNode.AuthType == AuthenticationType.CUSTOM_SERVER || LaunchAuthNode.AuthType == AuthenticationType.AUTHLIB_INJECTOR)
                                {
                                    await MainWindowVM.ShowMessageAsync("验证失败：代理验证服务器地址有误或账号未找到",
                                    string.Format("请确认您的Authlib-Injector验证服务器（Authlib-Injector验证）或自定义验证服务器（自定义验证）地址正确或确认账号和游戏角色存在。具体信息：{0}",
                                    authResult.Error.ErrorMessage));
                                }
                                else
                                {
                                    await MainWindowVM.ShowMessageAsync("验证失败：您的账号未找到",
                                    string.Format("请确认您的账号和游戏角色存在。具体信息：{0}", authResult.Error.ErrorMessage));
                                }
                                return;
                            case AuthState.ERR_OTHER:
                                await MainWindowVM.ShowMessageAsync("验证失败：其他错误",
                                    string.Format("具体信息：{0}", authResult.Error.ErrorMessage));
                                return;
                            case AuthState.ERR_INSIDE:
                                if (authResult.Error.Exception != null)
                                {
                                    App.LogHandler.AppendFatal(authResult.Error.Exception);
                                }
                                else
                                {
                                    await MainWindowVM.ShowMessageAsync("验证失败：启动器内部错误(无exception对象)",
                                        string.Format("建议您联系启动器开发者进行解决。具体信息：{0}：\n\r{1}",
                                        authResult.Error.ErrorMessage, authResult.Error.Exception?.ToString()));
                                }
                                return;
                            default:
                                await MainWindowVM.ShowMessageAsync("验证失败：未知错误",
                                    "建议您联系启动器开发者进行解决。");
                                return;
                        }
                    }
                    else
                    {
                        var authResult = await authenticator.DoAuthenticateAsync();
                        launchSetting.AuthenticateResult = authResult;
                        launchUser.UserData = authResult.UserData;
                        launchUser.SelectProfileUUID = authResult.SelectedProfileUUID.Value;
                    }
                    #endregion

                    #region 验证后用户处理
                    App.Config.MainConfig.History.SelectedUserNodeID = launchUser.UserData.Uuid;
                    if (!App.Config.MainConfig.User.UserDatabase.ContainsKey(launchUser.UserData.Uuid))
                    {
                        launchUser.AuthModule = LaunchAuthNodePair?.Key;
                        App.Config.MainConfig.User.UserDatabase.Add(launchUser.UserData.Uuid, launchUser);
                        LaunchUserPair = new KeyValuePair<string, UserNode>(launchUser.UserData.Uuid, launchUser);
                    }
                    #endregion

                    #region 检查游戏完整
                    List<DownloadTask> losts = new List<DownloadTask>();

                    App.LogHandler.AppendInfo("检查丢失的依赖库文件中...");
                    var lostDepend = await FileHelper.GetLostDependDownloadTaskAsync(
                        App.Config.MainConfig.Download.DownloadSource,
                        App.Handler,
                        launchSetting.Version);

                    if (LaunchAuthNode.AuthType == AuthenticationType.NIDE8)
                    {
                        string nideJarPath = App.Handler.GetNide8JarPath();
                        if (!File.Exists(nideJarPath))
                        {
                            lostDepend.Add(new DownloadTask("统一通行证核心", "https://login2.nide8.com:233/index/jar", nideJarPath));
                        }
                    }
                    else if (LaunchAuthNode.AuthType == AuthenticationType.AUTHLIB_INJECTOR)
                    {
                        string aiJarPath = App.Handler.GetAIJarPath();
                        if (!File.Exists(aiJarPath))
                        {
                            lostDepend.Add(await NsisoLauncherCore.Net.Tools.GetDownloadUrl.GetAICoreDownloadTask(App.Config.MainConfig.Download.DownloadSource, aiJarPath));
                        }
                    }

                    if (App.Config.MainConfig.Environment.DownloadLostDepend && lostDepend.Count != 0)
                    {
                        MessageDialogResult downDependResult = await MainWindowVM.ShowMessageAsync(App.GetResourceString("String.Mainwindow.NeedDownloadDepend"),
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
                                App.Config.MainConfig.Environment.DownloadLostDepend = false;
                                break;
                            default:
                                break;
                        }

                    }

                    App.LogHandler.AppendInfo("检查丢失的资源文件中...");
                    if (App.Config.MainConfig.Environment.DownloadLostAssets && (await FileHelper.IsLostAssetsAsync(App.Config.MainConfig.Download.DownloadSource,
                        App.Handler, launchSetting.Version)))
                    {
                        MessageDialogResult downDependResult = await MainWindowVM.ShowMessageAsync(App.GetResourceString("String.Mainwindow.NeedDownloadAssets"),
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
                                    App.Config.MainConfig.Download.DownloadSource,
                                    App.Handler, launchSetting.Version);
                                losts.AddRange(lostAssets);
                                break;
                            case MessageDialogResult.FirstAuxiliary:
                                App.Config.MainConfig.Environment.DownloadLostAssets = false;
                                break;
                            default:
                                break;
                        }

                    }

                    if (losts.Count != 0)
                    {
                        if (!App.Downloader.IsBusy)
                        {
                            App.Downloader.AddDownloadTask(losts);
                            App.Downloader.StartDownload();
                            var downloadResult = await new DownloadWindow().ShowWhenDownloading();
                            if (downloadResult?.ErrorList?.Count != 0)
                            {
                                await MainWindowVM.ShowMessageAsync(string.Format("有{0}个文件下载补全失败", downloadResult.ErrorList.Count),
                                    "这可能是因为本地网络问题或下载源问题，您可以尝试检查网络环境或在设置中切换首选下载源，启动器将继续尝试启动");
                            }
                        }
                        else
                        {
                            await MainWindowVM.ShowMessageAsync("无法下载补全：当前有正在下载中的任务", "请等待其下载完毕或取消下载，启动器将尝试继续启动");
                        }
                    }

                    #endregion

                    #region 根据配置文件设置
                    launchSetting.AdvencedGameArguments += App.Config.MainConfig.Environment.AdvencedGameArguments;
                    launchSetting.AdvencedJvmArguments += App.Config.MainConfig.Environment.AdvencedJvmArguments;
                    launchSetting.GCArgument += App.Config.MainConfig.Environment.GCArgument;
                    launchSetting.GCEnabled = App.Config.MainConfig.Environment.GCEnabled;
                    launchSetting.GCType = App.Config.MainConfig.Environment.GCType;
                    launchSetting.JavaAgent += App.Config.MainConfig.Environment.JavaAgent;
                    if (LaunchAuthNode.AuthType == AuthenticationType.NIDE8)
                    {
                        launchSetting.JavaAgent += string.Format(" \"{0}\"={1}", App.Handler.GetNide8JarPath(), LaunchAuthNode.Property["nide8ID"]);
                    }
                    else if (LaunchAuthNode.AuthType == AuthenticationType.AUTHLIB_INJECTOR)
                    {
                        launchSetting.JavaAgent += string.Format(" \"{0}\"={1}", App.Handler.GetAIJarPath(), LaunchAuthNode.Property["authserver"]);
                    }

                    //直连服务器设置
                    var lockAuthNode = App.Config.MainConfig.User.GetLockAuthNode();
                    if (App.Config.MainConfig.User.Nide8ServerDependence &&
                        (lockAuthNode != null) &&
                            (lockAuthNode.AuthType == AuthenticationType.NIDE8))
                    {
                        var nide8ReturnResult = await (new NsisoLauncherCore.Net.Nide8API.APIHandler(lockAuthNode.Property["nide8ID"])).GetInfoAsync();
                        if (!string.IsNullOrWhiteSpace(nide8ReturnResult.Meta.ServerIP))
                        {
                            NsisoLauncherCore.Modules.Server server = new NsisoLauncherCore.Modules.Server();
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
                    else if (App.Config.MainConfig.Server.LaunchToServer)
                    {
                        launchSetting.LaunchToServer = new NsisoLauncherCore.Modules.Server() { Address = App.Config.MainConfig.Server.Address, Port = App.Config.MainConfig.Server.Port };
                    }

                    //自动内存设置
                    if (App.Config.MainConfig.Environment.AutoMemory)
                    {
                        var m = SystemTools.GetBestMemory(App.Handler.Java);
                        App.Config.MainConfig.Environment.MaxMemory = m;
                        launchSetting.MaxMemory = m;
                    }
                    else
                    {
                        launchSetting.MaxMemory = App.Config.MainConfig.Environment.MaxMemory;
                    }
                    launchSetting.VersionType = App.Config.MainConfig.Customize.VersionInfo;
                    launchSetting.WindowSize = App.Config.MainConfig.Environment.WindowSize;
                    #endregion

                    #region 配置文件处理
                    App.Config.Save();
                    #endregion

                    #region 启动

                    App.LogHandler.OnLog += (a, b) => { launchingDialogVM.LogLine = b.Message; };
                    var result = await App.Handler.LaunchAsync(launchSetting);
                    App.LogHandler.OnLog -= (a, b) => { launchingDialogVM.LogLine = b.Message; };

                    //程序猿是找不到女朋友的了 :) 
                    if (!result.IsSuccess)
                    {
                        await MainWindowVM.ShowMessageAsync(App.GetResourceString("String.Mainwindow.LaunchError") + result.LaunchException.Title, result.LaunchException.Message);
                        App.LogHandler.AppendError(result.LaunchException);
                    }
                    else
                    {
                        launchingDialogVM.CancelLaunchingCommand = new DelegateCommand((obj) => CancelLaunching(result));

                        #region 等待游戏响应
                        try
                        {
                            await Task.Factory.StartNew(() =>
                            {
                                result.Process.WaitForInputIdle();
                            });
                        }
                        catch (Exception ex)
                        {
                            await MainWindowVM.ShowMessageAsync("启动后等待游戏窗口响应异常",
                                "这可能是由于游戏进程发生意外（闪退）导致的。具体原因:" + ex.Message);
                            return;
                        }
                        #endregion

                        launchingDialogVM.CancelLaunchingCommand = null;

                        if (!result.Process.HasExited)
                        {
                            MainWindowVM.WindowState = WindowState.Minimized;
                        }
                       

                        #region 数据反馈
#if DEBUG
#else
                        //API使用次数计数器+1
                        await App.NsisoAPIHandler.RefreshUsingTimesCounter();
#endif
                        #endregion

                        App.Config.MainConfig.History.LastLaunchUsingMs = result.LaunchUsingMs;
                        if (App.Config.MainConfig.Environment.ExitAfterLaunch)
                        {
                            Application.Current.Shutdown();
                        }

                        //自定义处理
                        if (!string.IsNullOrWhiteSpace(App.Config.MainConfig.Customize.GameWindowTitle))
                        {
                            GameHelper.SetGameTitle(result, App.Config.MainConfig.Customize.GameWindowTitle);
                        }
                        if (App.Config.MainConfig.Customize.CustomBackGroundMusic)
                        {
                            Volume = 0.5;
                            await Task.Run(() =>
                            {
                                for (int i = 0; i < 50; i++)
                                {
                                    Volume -= 0.01;
                                    Thread.Sleep(50);
                                }
                            });
                            MediaSource = null;
                        }
                    }
                    #endregion
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    await MainWindowVM.HideMetroDialogAsync(launchingDialog);
                    await MainWindowVM.ShowMessageAsync("游戏已被取消启动", "游戏已被取消启动");
                }
            }
            catch (Exception ex)
            {
                App.LogHandler.AppendFatal(ex);
            }
        }

        private async Task CustomizeRefresh()
        {
            if (!string.IsNullOrWhiteSpace(App.Config.MainConfig.Customize.LauncherTitle))
            {
                MainWindowVM.WindowTitle = App.Config.MainConfig.Customize.LauncherTitle;
            }
            if (App.Config.MainConfig.Customize.CustomBackGroundPicture)
            {
                string[] files = Directory.GetFiles(Path.GetDirectoryName(App.Config.MainConfigPath), "bgpic_?.png");
                if (files.Count() != 0)
                {
                    Random random = new Random();
                    BackgroundImageSource = files[random.Next(files.Count())];
                    //ImageBrush brush = new ImageBrush(new BitmapImage(new Uri()))
                    //{ TileMode = TileMode.FlipXY, AlignmentX = AlignmentX.Right, Stretch = Stretch.UniformToFill };
                    //this.Background = brush;
                }
            }

            //undo app back server info control
            //if (App.Config.MainConfig.User.Nide8ServerDependence)
            //{
            //    try
            //    {
            //        var lockAuthNode = App.Config.MainConfig.User.GetLockAuthNode();
            //        if ((lockAuthNode != null) &&
            //            (lockAuthNode.AuthType == AuthenticationType.NIDE8))
            //        {
            //            Config.Server nide8Server = new Config.Server() { ShowServerInfo = true };
            //            var nide8ReturnResult = await (new NsisoLauncherCore.Net.Nide8API.APIHandler(lockAuthNode.Property["nide8ID"])).GetInfoAsync();
            //            if (!string.IsNullOrWhiteSpace(nide8ReturnResult.Meta.ServerIP))
            //            {
            //                string[] serverIp = nide8ReturnResult.Meta.ServerIP.Split(':');
            //                if (serverIp.Length == 2)
            //                {
            //                    nide8Server.Address = serverIp[0];
            //                    nide8Server.Port = ushort.Parse(serverIp[1]);
            //                }
            //                else
            //                {
            //                    nide8Server.Address = nide8ReturnResult.Meta.ServerIP;
            //                    nide8Server.Port = 25565;
            //                }
            //                nide8Server.ServerName = nide8ReturnResult.Meta.ServerName;
            //                serverInfoControl.SetServerInfo(nide8Server);
            //            }
            //        }

            //    }
            //    catch (Exception)
            //    { }
            //}
            //else if (App.Config.MainConfig.Server != null)
            //{
            //    serverInfoControl.SetServerInfo(App.Config.MainConfig.Server);
            //}

            if (App.Config.MainConfig.Customize.CustomBackGroundMusic)
            {
                string[] files = Directory.GetFiles(Path.GetDirectoryName(App.Config.MainConfigPath), "bgmusic_?.mp3");
                if (files.Count() != 0)
                {
                    Random random = new Random();
                    MediaSource = new Uri(files[random.Next(files.Count())]);
                    Volume = 0;
                    await Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            for (int i = 0; i < 50; i++)
                            {
                                Volume += 0.01;
                                Thread.Sleep(50);
                            }
                        }
                        catch (Exception) { }
                    });
                }
            }

        }

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
                            App.Downloader.AddDownloadTask(new DownloadTask("32位JAVA安装包", @"https://bmclapi.bangbang93.com/java/jre_x86.exe", "jre_x86.exe"));
                            App.Downloader.StartDownload();
                            await new DownloadWindow().ShowWhenDownloading();
                            System.Diagnostics.Process.Start("Explorer.exe", "jre_x86.exe");
                            break;
                        case ArchEnum.x64:
                            App.Downloader.AddDownloadTask(new DownloadTask("64位JAVA安装包", @"https://bmclapi.bangbang93.com/java/jre_x64.exe", "jre_x64.exe"));
                            App.Downloader.StartDownload();
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

        private void CancelLaunching(LaunchResult result)
        {
            if (!result.Process.HasExited)
            {
                result.Process.Kill();
            }
        }

        private async Task CheckUpdate()
        {
            try
            {
                var ver = await App.NsisoAPIHandler.GetLatestLauncherVersion();
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

    public class MainPageDesignViewModel
    {
        /// <summary>
        /// 是否在启动
        /// </summary>
        public bool IsLaunching { get; set; } = false;

        public string BackgroundImageSource { get; set; } = "../../Resource/bg.jpg";
    }
}
