using MahApps.Metro.Controls.Dialogs;
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

namespace NsisoLauncher.ViewModels.Pages
{
    public class LaunchPageViewModel : INotifyPropertyChanged
    {
        public Windows.MainWindowViewModel MainWindowVM { get; set; }

        public Config.User UserSetting { get; set; }

        public UserNode LoggedInUser { get; set; }

        #region Commands
        /// <summary>
        /// 启动命令
        /// </summary>
        public ICommand LaunchCmd { get; set; }

        /// <summary>	
        /// 取消启动命令	
        /// </summary>	
        public ICommand CancelLaunchingCmd { get; set; }
        #endregion

        #region ElementsProp
        public string UserName { get; set; }
        public string UserProfileName { get; set; }
        #endregion

        /// <summary>	
        /// 日志行	
        /// </summary>	
        public string LogLine { get; set; }

        /// <summary>
        /// 游戏是否在启动
        /// </summary>
        public bool IsLaunching { get; set; }

        public ObservableCollection<Version> Versions { get; }

        #region Launch Data
        /// <summary>
        /// 启动的版本
        /// </summary>
        public Version LaunchVersion { get; set; }

        /// <summary>
        /// 选中版本id
        /// </summary>
        public string SelectedLaunchVersionId { get; set; }
        #endregion

        public LaunchPageViewModel()
        {
            if (App.VersionList != null)
            {
                Versions = App.VersionList;
            }
            if (App.MainWindowVM != null)
            {
                MainWindowVM = App.MainWindowVM;
            }
            if (App.LaunchSignal != null)
            {
                App.LaunchSignal.PropertyChanged += LaunchSignal_PropertyChanged;
                this.IsLaunching = App.LaunchSignal.IsLaunching;
            }
            if (App.LauncherData != null)
            {
                this.LaunchVersion = App.LauncherData.SelectedVersion;
                this.SelectedLaunchVersionId = App.LauncherData.SelectedVersion?.Id;
                App.LauncherData.PropertyChanged += LauncherData_PropertyChanged;
            }
            UserSetting = App.Config?.MainConfig?.User;
            if (UserSetting != null)
            {

                UserSetting.PropertyChanged += User_PropertyChanged;
            }
            RefreshUserBinding();

            #region 命令初始化
            LaunchCmd = new DelegateCommand(
               //launch
               async (obj) =>
               {
                   if ((obj != null) && (obj is LaunchType))
                   {
                       LaunchType launchType = (LaunchType)obj;
                       await LaunchFromVM(launchType);
                   }
                   else
                   {
                       await LaunchFromVM(LaunchType.NORMAL);
                   }
               },
               (obj) =>
               {
                   if (App.Handler == null)
                   {
                       return false;
                   }
                   else
                   {
                       return !App.Handler.IsBusyLaunching;
                   }
               });
            #endregion

            if (string.IsNullOrWhiteSpace(SelectedLaunchVersionId) && App.Config != null)
            {
                if (!string.IsNullOrWhiteSpace(App.Config?.MainConfig?.History?.LastLaunchVersion))
                {
                    SelectedLaunchVersionId = App.Config.MainConfig.History.LastLaunchVersion;
                }
            }

            this.PropertyChanged += LaunchPageViewModel_PropertyChanged;
        }

        private void LaunchPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LaunchVersion")
            {
                App.LauncherData.SelectedVersion = this.LaunchVersion;
            }
        }

        private void LauncherData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedVersion")
            {
                this.LaunchVersion = App.LauncherData.SelectedVersion;
                this.SelectedLaunchVersionId = App.LauncherData.SelectedVersion?.Id;
            }
        }

        private void LaunchSignal_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsLaunching")
            {
                this.IsLaunching = App.LaunchSignal.IsLaunching;
            }
        }

        private void User_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedUser")
            {
                RefreshUserBinding();
            }
        }

        private void RefreshUserBinding()
        {
            UserNode userNode = UserSetting?.SelectedUser;
            if (userNode != null)
            {
                UserName = userNode.Username;
                UserProfileName = userNode.SelectedProfile?.PlayerName;
            }
            else
            {
                UserName = null;
                UserProfileName = null;
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        public async Task LaunchFromVM(LaunchType launchType)
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
                UserNode launchUser = UserSetting.SelectedUser;
                if (launchUser == null)
                {
                    await MainWindowVM.ShowMessageAsync(App.GetResourceString("String.Message.EmptyUsername"),
                        App.GetResourceString("String.Message.EmptyUsername2"));
                    return;
                }
                if ((launchUser.Profiles == null) || (launchUser.Profiles.Count == 0))
                {
                    await MainWindowVM.ShowMessageAsync("没有可用的游戏角色",
                        "您已经登录，但您没有可以进行游戏的角色（Profile），其角色列表为空");
                    return;
                }
                PlayerProfile selectedProfile = launchUser.SelectedProfile;
                if (selectedProfile == null)
                {
                    await MainWindowVM.ShowMessageAsync("没有选择游戏角色",
                        "您已经登录，但您没有选择进行游戏的角色（Profile），请在用户页面进行选择要进行游戏的角色");
                    return;
                }
                //if (LaunchAuthNodePair == null)
                //{
                //    await MainWindowVM.ShowMessageAsync(App.GetResourceString("String.Message.EmptyAuthType"),
                //        App.GetResourceString("String.Message.EmptyAuthType2"));
                //    return;
                //}
                if (App.Handler.Java == null)
                {
                    await MainWindowVM.ShowMessageAsync(App.GetResourceString("String.Message.NoJava"), App.GetResourceString("String.Message.NoJava2"));
                    return;
                }
                #endregion

                #region 保存启动数据
                App.Config.MainConfig.History.LastLaunchVersion = LaunchVersion.Id;
                App.Config.MainConfig.History.LastLaunchTime = DateTime.Now;
                #endregion

                LaunchSetting launchSetting = new LaunchSetting()
                {
                    Version = LaunchVersion,
                    LaunchType = launchType
                };

                //标记控件状态启动中
                App.LaunchSignal.IsLaunching = true;

                #region 验证
                AuthenticationNode launchAuthNode = null;
                if (UserSetting.AuthenticationDic.ContainsKey(launchUser.AuthModule))
                {
                    launchAuthNode = UserSetting.AuthenticationDic[launchUser.AuthModule];
                }
                if (launchAuthNode == null)
                {
                    throw new Exception("所使用用户没有验证器类型");
                }
                launchSetting.LaunchUser = launchUser;

                ////主验证器接口
                //IAuthenticator authenticator = null;
                //bool shouldRemember = false;

                ////bool isSameAuthType = (authNode.AuthenticationType == auth);
                //bool isRemember = (!string.IsNullOrWhiteSpace(launchUser.AccessToken)) && ((launchUser.SelectProfileUUID != null));
                ////bool isSameName = userName == App.config.MainConfig.User.UserName;

                //switch (launchAuthNode.AuthType)
                //{
                //    #region 离线验证
                //    case AuthenticationType.OFFLINE:
                //        //if (isNewUser)
                //        //{
                //        //    authenticator = new OfflineAuthenticator(launchUser.UserName);
                //        //}
                //        //else
                //        //{
                //        //    authenticator = new OfflineAuthenticator(launchUser.UserName, launchUser.UserData, launchUser.SelectProfileUUID);
                //        //}
                //        authenticator = new OfflineAuthenticator(launchUser.UserName, launchUser.UserData, launchUser.SelectProfileUUID);
                //        break;
                //    #endregion

                //    #region MOJANG验证
                //    case AuthenticationType.MOJANG:
                //        if (isRemember)
                //        {
                //            var mYggTokenAuthenticator = new YggdrasilTokenAuthenticator(launchUser.AccessToken,
                //                launchUser.GetSelectProfile(), launchUser.UserData);
                //            mYggTokenAuthenticator.ProxyAuthServerAddress = "https://authserver.mojang.com";
                //            authenticator = mYggTokenAuthenticator;
                //        }
                //        else
                //        {
                //            var mojangLoginDResult = await MainWindowVM.ShowLoginAsync(App.GetResourceString("String.Mainwindow.Auth.Mojang.Login"),
                //                App.GetResourceString("String.Mainwindow.Auth.Mojang.Login2"),
                //                loginDialogSettings);
                //            if (IsValidateLoginData(mojangLoginDResult))
                //            {
                //                var mYggAuthenticator = new YggdrasilAuthenticator(new Credentials()
                //                {
                //                    Username = mojangLoginDResult.Username,
                //                    Password = mojangLoginDResult.Password
                //                });
                //                mYggAuthenticator.ProxyAuthServerAddress = "https://authserver.mojang.com";
                //                authenticator = mYggAuthenticator;
                //                shouldRemember = mojangLoginDResult.ShouldRemember;
                //            }
                //            else
                //            {
                //                await MainWindowVM.ShowMessageAsync("您输入的账号或密码为空", "请检查您是否正确填写登录信息");
                //                return;
                //            }
                //        }
                //        break;
                //    #endregion

                //    #region NIDE8验证
                //    case AuthenticationType.NIDE8:
                //        string nide8ID = launchAuthNode.Property["nide8ID"];
                //        if (string.IsNullOrWhiteSpace(nide8ID))
                //        {
                //            await MainWindowVM.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Nide8.NoID"),
                //                App.GetResourceString("String.Mainwindow.Auth.Nide8.NoID2"));
                //            return;
                //        }
                //        var nide8ChooseResult = await MainWindowVM.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Nide8.Login2"),
                //            App.GetResourceString("String.Base.Choose"),
                //            MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
                //            new MetroDialogSettings()
                //            {
                //                AffirmativeButtonText = App.GetResourceString("String.Base.Login"),
                //                NegativeButtonText = App.GetResourceString("String.Base.Cancel"),
                //                FirstAuxiliaryButtonText = App.GetResourceString("String.Base.Register"),
                //                DefaultButtonFocus = MessageDialogResult.Affirmative
                //            });
                //        switch (nide8ChooseResult)
                //        {
                //            case MessageDialogResult.Canceled:
                //                return;
                //            case MessageDialogResult.Negative:
                //                return;
                //            case MessageDialogResult.FirstAuxiliary:
                //                System.Diagnostics.Process.Start(string.Format("https://login2.nide8.com:233/{0}/register", nide8ID));
                //                return;
                //            case MessageDialogResult.Affirmative:
                //                if (isRemember)
                //                {
                //                    var nYggTokenCator = new Nide8TokenAuthenticator(nide8ID, launchUser.AccessToken,
                //                        launchUser.GetSelectProfile(),
                //                        launchUser.UserData);
                //                    authenticator = nYggTokenCator;
                //                }
                //                else
                //                {
                //                    var nide8LoginDResult = await MainWindowVM.ShowLoginAsync(App.GetResourceString("String.Mainwindow.Auth.Nide8.Login"),
                //                        App.GetResourceString("String.Mainwindow.Auth.Nide8.Login2"),
                //                        loginDialogSettings);
                //                    if (IsValidateLoginData(nide8LoginDResult))
                //                    {
                //                        var nYggCator = new Nide8Authenticator(
                //                            nide8ID,
                //                            new Credentials()
                //                            {
                //                                Username = nide8LoginDResult.Username,
                //                                Password = nide8LoginDResult.Password
                //                            });
                //                        authenticator = nYggCator;
                //                        shouldRemember = nide8LoginDResult.ShouldRemember;
                //                    }
                //                    else
                //                    {
                //                        await MainWindowVM.ShowMessageAsync("您输入的账号或密码为空", "请检查您是否正确填写登录信息");
                //                        return;
                //                    }
                //                }
                //                break;
                //            default:
                //                break;
                //        }
                //        break;
                //    #endregion

                //    #region AUTHLIB验证
                //    case AuthenticationType.AUTHLIB_INJECTOR:
                //        string aiRootAddr = launchAuthNode.Property["authserver"];
                //        if (string.IsNullOrWhiteSpace(aiRootAddr))
                //        {
                //            await MainWindowVM.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Custom.NoAdrress"),
                //                App.GetResourceString("String.Mainwindow.Auth.Custom.NoAdrress2"));
                //            return;
                //        }
                //        else
                //        {
                //            if (isRemember)
                //            {
                //                var cYggTokenCator = new AuthlibInjectorTokenAuthenticator(aiRootAddr,
                //                    launchUser.AccessToken,
                //                    launchUser.GetSelectProfile(),
                //                    launchUser.UserData);
                //                authenticator = cYggTokenCator;
                //            }
                //            else
                //            {
                //                var customLoginDResult = await MainWindowVM.ShowLoginAsync(App.GetResourceString("String.Mainwindow.Auth.Custom.Login"),
                //               App.GetResourceString("String.Mainwindow.Auth.Custom.Login2"),
                //               loginDialogSettings);
                //                if (IsValidateLoginData(customLoginDResult))
                //                {
                //                    var cYggAuthenticator = new AuthlibInjectorAuthenticator(
                //                        aiRootAddr,
                //                        new Credentials()
                //                        {
                //                            Username = customLoginDResult.Username,
                //                            Password = customLoginDResult.Password
                //                        });
                //                    authenticator = cYggAuthenticator;
                //                    shouldRemember = customLoginDResult.ShouldRemember;
                //                }
                //                else
                //                {
                //                    await MainWindowVM.ShowMessageAsync("您输入的账号或密码为空", "请检查您是否正确填写登录信息");
                //                    return;
                //                }
                //            }
                //        }
                //        break;
                //    #endregion

                //    #region 自定义验证
                //    case AuthenticationType.CUSTOM_SERVER:
                //        string customAuthServer = launchAuthNode.Property["authserver"];
                //        if (string.IsNullOrWhiteSpace(customAuthServer))
                //        {
                //            await MainWindowVM.ShowMessageAsync(App.GetResourceString("String.Mainwindow.Auth.Custom.NoAdrress"),
                //                App.GetResourceString("String.Mainwindow.Auth.Custom.NoAdrress2"));
                //            return;
                //        }
                //        else
                //        {
                //            if (isRemember)
                //            {
                //                var cYggTokenCator = new YggdrasilTokenAuthenticator(launchUser.AccessToken,
                //                launchUser.GetSelectProfile(),
                //                launchUser.UserData);
                //                cYggTokenCator.ProxyAuthServerAddress = customAuthServer;
                //            }
                //            else
                //            {
                //                var customLoginDResult = await MainWindowVM.ShowLoginAsync(App.GetResourceString("String.Mainwindow.Auth.Custom.Login"),
                //               App.GetResourceString("String.Mainwindow.Auth.Custom.Login2"),
                //               loginDialogSettings);
                //                if (IsValidateLoginData(customLoginDResult))
                //                {
                //                    var cYggAuthenticator = new YggdrasilAuthenticator(new Credentials()
                //                    {
                //                        Username = customLoginDResult.Username,
                //                        Password = customLoginDResult.Password
                //                    });
                //                    cYggAuthenticator.ProxyAuthServerAddress = customAuthServer;
                //                    authenticator = cYggAuthenticator;
                //                    shouldRemember = customLoginDResult.ShouldRemember;
                //                }
                //                else
                //                {
                //                    await MainWindowVM.ShowMessageAsync("您输入的账号或密码为空", "请检查您是否正确填写登录信息");
                //                    return;
                //                }
                //            }
                //        }
                //        break;
                //    #endregion

                //    #region 意外情况
                //    default:
                //        authenticator = new OfflineAuthenticator(launchUser.UserName,
                //                launchUser.UserData,
                //                launchUser.SelectProfileUUID);
                //        break;
                //        #endregion
                //}

                ////如果验证方式不是离线验证
                //if (launchAuthNode.AuthType != AuthenticationType.OFFLINE)
                //{

                //    string currentLoginType = string.Format("正在进行{0}中...", launchAuthNode.Name);
                //    string loginMsg = "这需要联网进行操作，可能需要一分钟的时间";
                //    var loader = await MainWindowVM.ShowProgressAsync(currentLoginType, loginMsg, true, null);

                //    loader.SetIndeterminate();
                //    var authResult = await authenticator.DoAuthenticateAsync();
                //    await loader.CloseAsync();

                //    #region 错误处日志
                //    if (authResult.State != AuthState.SUCCESS)
                //    {
                //        App.LogHandler.AppendInfo(string.Format("验证失败：{0}", authResult.State));
                //    }
                //    #endregion

                //    switch (authResult.State)
                //    {
                //        case AuthState.SUCCESS:
                //            #region 成功登陆
                //            #region 无选中角色处理
                //            if (authResult.SelectedProfile == null)
                //            {
                //                if (authResult.Profiles == null || authResult.Profiles.Count == 0)
                //                {
                //                    await MainWindowVM.ShowMessageAsync("验证失败：您没有可用的游戏角色（Profile）",
                //                    "如果您是正版验证，则您可能还未购买游戏本体。如果您是外置登录，则您可能未设置或添加可用角色");
                //                    return;
                //                }
                //                ChooseProfileDialog chooseProfileDialog = new ChooseProfileDialog(MainWindowVM, authResult.Profiles);
                //                await MainWindowVM.ShowMetroDialogAsync(chooseProfileDialog);
                //                await chooseProfileDialog.WaitUntilUnloadedAsync();
                //                if (chooseProfileDialog.SelectedProfile == null)
                //                {
                //                    await MainWindowVM.ShowMessageAsync("验证失败：您没有选中任何游戏角色（Profile）",
                //               "请选中您要进行游戏的角色");
                //                    return;
                //                }
                //                else
                //                {
                //                    authResult.SelectedProfile = chooseProfileDialog.SelectedProfile;
                //                }
                //            }
                //            #endregion
                //            launchUser.SelectProfileUUID = authResult.SelectedProfile.Value;
                //            launchUser.UserData = authResult.UserData;
                //            if (authResult.Profiles != null)
                //            {
                //                launchUser.Profiles.Clear();
                //                authResult.Profiles.ForEach(x => launchUser.Profiles.Add(x.Value, x));
                //            }
                //            if (shouldRemember)
                //            {
                //                launchUser.AccessToken = authResult.AccessToken;
                //            }
                //            launchSetting.AuthenticateResult = authResult;
                //            #endregion
                //            break;

                //        case AuthState.REQ_LOGIN:
                //            #region 要求再次登陆
                //            //todo 添加更好的注销服务
                //            launchUser.ClearAuthCache();
                //            await MainWindowVM.ShowMessageAsync("验证失败：您的登录信息已过期",
                //                string.Format("请您重新进行登录。具体信息：{0}", authResult.Error.ErrorMessage));
                //            #endregion
                //            return;

                //        case AuthState.ERR_INVALID_CRDL:
                //            #region 错误的验证信息/禁止访问
                //            await MainWindowVM.ShowMessageAsync("验证失败：您的登录账号或密码错误",
                //                string.Format("请您确认您输入的账号密码正确。也有可能是因为验证请求频率过高，被服务器暂时禁止访问。具体信息：{0}", authResult.Error.ErrorMessage));
                //            #endregion
                //            return;

                //        case AuthState.ERR_NOTFOUND:
                //            #region 页面未找到
                //            if (launchAuthNode.AuthType == AuthenticationType.CUSTOM_SERVER || launchAuthNode.AuthType == AuthenticationType.AUTHLIB_INJECTOR)
                //            {
                //                await MainWindowVM.ShowMessageAsync("验证失败：代理验证服务器地址有误或账号未找到",
                //                string.Format("请确认您的Authlib-Injector验证服务器（Authlib-Injector验证）或自定义验证服务器（自定义验证）地址正确或确认账号和游戏角色存在。具体信息：{0}",
                //                authResult.Error.ErrorMessage));
                //            }
                //            else
                //            {
                //                await MainWindowVM.ShowMessageAsync("验证失败：您的账号未找到",
                //                string.Format("请确认您的账号和游戏角色存在。具体信息：{0}", authResult.Error.ErrorMessage));
                //            }
                //            #endregion
                //            return;

                //        case AuthState.ERR_OTHER:
                //            await MainWindowVM.ShowMessageAsync("验证失败：其他错误",
                //                string.Format("具体信息：{0}", authResult.Error.ErrorMessage));
                //            return;
                //        case AuthState.ERR_INSIDE:
                //            if (authResult.Error.Exception != null)
                //            {
                //                App.LogHandler.AppendFatal(authResult.Error.Exception);
                //            }
                //            else
                //            {
                //                await MainWindowVM.ShowMessageAsync("验证失败：启动器内部错误(无exception对象)",
                //                    string.Format("建议您联系启动器开发者进行解决。具体信息：{0}：\n\r{1}",
                //                    authResult.Error.ErrorMessage, authResult.Error.Exception?.ToString()));
                //            }
                //            return;
                //        default:
                //            await MainWindowVM.ShowMessageAsync("验证失败：未知错误",
                //                "建议您联系启动器开发者进行解决。");
                //            return;
                //    }
                //}
                //else
                //{
                //    var authResult = await authenticator.DoAuthenticateAsync();
                //    launchSetting.AuthenticateResult = authResult;
                //    launchUser.UserData = authResult.UserData;
                //    launchUser.SelectProfileUUID = authResult.SelectedProfile.Value;
                //}
                #endregion

                #region 验证后用户处理
                //todo:增加验证后用户处理
                //App.Config.MainConfig.History.SelectedUserNodeID = launchUser.UserData.ID;
                //if (!App.Config.MainConfig.User.UserDatabase.ContainsKey(launchUser.UserData.ID))
                //{
                //    launchUser.AuthModule = LaunchAuthNodePair?.Key;
                //    App.Config.MainConfig.User.UserDatabase.Add(launchUser.UserData.ID, launchUser);
                //    LaunchUserPair = new KeyValuePair<string, UserNode>(launchUser.UserData.ID, launchUser);
                //}
                #endregion

                #region 检查游戏完整
                List<IDownloadTask> losts = new List<IDownloadTask>();

                App.LogHandler.AppendInfo("检查丢失的依赖库文件中...");
                var lostDepend = await FileHelper.GetLostDependDownloadTaskAsync(
                    App.Handler,
                    launchSetting.Version, App.NetHandler.Mirrors.VersionListMirrorList, App.NetHandler.Requester);

                if (launchAuthNode.AuthType == AuthenticationType.NIDE8)
                {
                    string nideJarPath = App.Handler.GetNide8JarPath();
                    if (!File.Exists(nideJarPath))
                    {
                        lostDepend.Add(new DownloadTask("统一通行证核心", new StringUrl("https://login2.nide8.com:233/index/jar"), nideJarPath));
                    }
                }
                else if (launchAuthNode.AuthType == AuthenticationType.AUTHLIB_INJECTOR)
                {
                    string aiJarPath = App.Handler.GetAIJarPath();
                    if (!File.Exists(aiJarPath))
                    {
                        DownloadTask aicore = await NsisoLauncherCore.Net.Tools.GetDownloadUri.GetAICoreDownloadTask(App.Config.MainConfig.Net.DownloadSource,
                            aiJarPath, App.NetHandler.Requester);
                        if (aicore != null)
                        {
                            lostDepend.Add(aicore);
                        }
                    }
                }

                if (App.Config.MainConfig.Environment.DownloadLostDepend && lostDepend.Count() != 0)
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
                if (App.Config.MainConfig.Environment.DownloadLostAssets && (await FileHelper.IsLostAssetsAsync(App.Handler, launchSetting.Version)))
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
                            var lostAssets = FileHelper.GetLostAssetsDownloadTaskAsync(
                                App.Handler, launchSetting.Version);
                            losts.Add(lostAssets);
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
                    if (!App.NetHandler.Downloader.IsBusy)
                    {
                        App.NetHandler.Downloader.AddDownloadTask(losts);
                        await App.NetHandler.Downloader.StartDownload();
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
                if (App.Config.MainConfig.Net.IsGameUseProxy)
                {
                    launchSetting.GameProxy = new Proxy()
                    {
                        ProxyHost = App.Config.MainConfig.Net.ProxyHost,
                        ProxyPort = App.Config.MainConfig.Net.ProxyPort,
                        ProxyUsername = App.Config.MainConfig.Net.ProxyUsername,
                        ProxyPassword = App.Config.MainConfig.Net.ProxyPassword
                    };
                }
                if (launchAuthNode.AuthType == AuthenticationType.NIDE8)
                {
                    launchSetting.JavaAgent += string.Format(" \"{0}\"={1}", App.Handler.GetNide8JarPath(), launchAuthNode.Property["nide8ID"]);
                }
                else if (launchAuthNode.AuthType == AuthenticationType.AUTHLIB_INJECTOR)
                {
                    launchSetting.JavaAgent += string.Format(" \"{0}\"={1}", App.Handler.GetAIJarPath(), launchAuthNode.Property["authserver"]);
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

                App.LogHandler.OnLog += (a, b) => { LogLine = b.Message; };
                var result = await App.Handler.LaunchAsync(launchSetting);
                App.LogHandler.OnLog -= (a, b) => { LogLine = b.Message; };

                //程序猿是找不到女朋友的了 :) 
                if (!result.IsSuccess)
                {
                    await MainWindowVM.ShowMessageAsync(App.GetResourceString("String.Mainwindow.LaunchError") + result.LaunchException.Title, result.LaunchException.Message);
                    App.LogHandler.AppendError(result.LaunchException);
                }
                else
                {
                    CancelLaunchingCmd = new DelegateCommand(async (obj) => await CancelLaunching(result));

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

                    CancelLaunchingCmd = null;

                    if (!result.Process.HasExited)
                    {
                        MainWindowVM.WindowState = WindowState.Minimized;
                    }

                    #region 数据反馈
#if !DEBUG
                    //API使用次数计数器+1
                    await App.NetHandler.NsisoAPIHandler.RefreshUsingTimesCounter();
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
                        App.MainWindowVM.MediaSource = null;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                App.LogHandler.AppendFatal(ex);
            }
            finally
            {
                App.LaunchSignal.IsLaunching = false;
            }
        }

        private async Task CancelLaunching(LaunchResult result)
        {
            if (!result.Process.HasExited)
            {
                result.Process.Kill();
            }
            await MainWindowVM.ShowMessageAsync("已取消启动", "已取消启动");
        }
    }
}
