using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Config;
using NsisoLauncher.Utils;
using NsisoLauncher.Views.Windows;
using NsisoLauncherCore;
using NsisoLauncherCore.LaunchException;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.Server;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NsisoLauncherCore.Net.Tools;
using NsisoLauncherCore.Net.Apis;
using NsisoLauncherCore.Net.Apis.Modules;

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

        /// <summary>
        /// 窗口加载完毕命令
        /// </summary>
        public ICommand LoadedCmd { get; set; }

        /// <summary>
        /// 跳转到用户界面
        /// </summary>
        public ICommand ToUserPageCmd { get; set; }
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

        public ObservableCollection<VersionBase> Versions { get; }


        #region 服务器
        public ServerInfo Server { get; set; }

        public bool ServerIsLoading { get; set; } = true;
        #endregion


        #region Launch Data
        /// <summary>
        /// 启动的版本
        /// </summary>
        public VersionBase LaunchVersion { get; set; }

        /// <summary>
        /// 选中版本id
        /// </summary>
        public string SelectedLaunchVersionId { get; set; }
        #endregion

        private bool page_cutback = false;

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
                if (App.LaunchSignal.IsLaunching)
                {
                    this.LogLine = App.LaunchSignal.LatestLog;
                    this.page_cutback = true;
                    if (App.LaunchSignal.LaunchingInstance != null)
                    {
                        App.LaunchSignal.LaunchingInstance.Log += OnLog;
                        CancelLaunchingCmd = new DelegateCommand(
                        (obj) =>
                        {
                            App.LaunchSignal.LaunchingInstance.Kill();
                        });
                    }

                }
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
            LoadedCmd = new DelegateCommand(
               //launch
               async (obj) =>
               {
                   await Loaded();
               });

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
               });

            ToUserPageCmd = new DelegateCommand(
                (obj) =>
                {
                    App.MainPageVM.NavigateToUserPage();
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

        private async Task Loaded()
        {
            #region 自定义
            if (App.Config?.MainConfig != null)
            {
                if (App.Config.MainConfig.Server?.ShowServerInfo == true)
                {
                    ServerIsLoading = true;
                    Server = new ServerInfo(App.Config.MainConfig.Server.Address, App.Config.MainConfig.Server.Port);
                    Server.ServerName = App.Config.MainConfig.Server.ServerName;
                    await Server.StartGetServerInfoAsync();
                    ServerIsLoading = false;
                }
            }
            #endregion
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
            if (e.PropertyName == "LaunchingInstance")
            {
                if (page_cutback && App.LaunchSignal.LaunchingInstance != null)
                {
                    this.LogLine = "等待游戏响应";
                    App.LogHandler.OnLog += OnLog;
                    CancelLaunchingCmd = new DelegateCommand(async (obj) => await CancelLaunching(App.LaunchSignal.LaunchingInstance));
                }

                if (page_cutback && App.LaunchSignal.LaunchingInstance == null)
                {
                    App.LogHandler.OnLog -= OnLog;
                    CancelLaunchingCmd = null;
                }
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
                UserName = userNode.User.DisplayUsername;
                //todo 添加prifile name
                UserProfileName = userNode.User.LaunchPlayerName;
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
                if (launchUser.User.LaunchUuid == null)
                {
                    await MainWindowVM.ShowMessageAsync("没有选择游戏角色",
                        "您已经登录，但您没有可以进行游戏的角色（Profile），有可能您未选择或者列表为空");
                    return;
                }
                //if (LaunchAuthNodePair == null)
                //{
                //    await MainWindowVM.ShowMessageAsync(App.GetResourceString("String.Message.EmptyAuthType"),
                //        App.GetResourceString("String.Message.EmptyAuthType2"));
                //    return;
                //}
                if ((App.JavaList == null || App.JavaList.Count == 0))
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
                        App.NetHandler.Downloader.AddDownloadTask(GetJavaInstaller.GetDownloadTask("8", arch, JavaImageType.JRE,
                            () =>
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    App.RefreshJavaList();
                                });
                            }));
                        App.MainPageVM.NavigateToDownloadPage();
                        await App.NetHandler.Downloader.StartDownload();
                    }
                }
                //else
                //{
                //    await MainWindowVM.ShowMessageAsync("没有设置启动所使用的java",
                //        "您的电脑安装了java，但您没有设置启动所使用的java，请转移至设置中的：环境设置-java设置 以设置启动使用的java");
                //}
                #endregion

                #region 保存启动数据
                App.Config.MainConfig.History.LastLaunchVersion = LaunchVersion.Id;
                App.Config.MainConfig.History.LastLaunchTime = DateTime.Now;
                #endregion

                LaunchSetting launchSetting = new LaunchSetting()
                {
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
                launchSetting.LaunchUser = launchUser.User;
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
                    App.Handler, LaunchVersion, App.NetHandler.Mirrors.VersionListMirrorList, App.NetHandler.Requester);

                #region 检查验证核心
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
                        DownloadTask aicore = await GetDownloadUri.GetAICoreDownloadTask(App.Config.MainConfig.Net.DownloadSource,
                            aiJarPath, App.NetHandler.Requester);
                        if (aicore != null)
                        {
                            lostDepend.Add(aicore);
                        }
                    }
                }
                #endregion

                #region 检查依赖库
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
                #endregion

                #region 检查资源文件
                App.LogHandler.AppendInfo("检查丢失的资源文件中...");
                if (App.Config.MainConfig.Environment.DownloadLostAssets && (await FileHelper.IsLostAssetsAsync(App.Handler, LaunchVersion)))
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
                                App.Handler, LaunchVersion);
                            losts.AddRange(lostAssets);
                            break;
                        case MessageDialogResult.FirstAuxiliary:
                            App.Config.MainConfig.Environment.DownloadLostAssets = false;
                            break;
                        default:
                            break;
                    }

                }
                #endregion


                if (losts.Count != 0)
                {
                    if (!App.NetHandler.Downloader.IsBusy)
                    {
                        App.NetHandler.Downloader.AddDownloadTask(losts);
                        App.MainPageVM.NavigateToDownloadPage();
                        var downloadResult = await App.NetHandler.Downloader.StartDownloadAndWaitDone();
                        App.MainPageVM.NavigateToLaunchPage();
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
                    var nide8ReturnResult = await new NsisoLauncherCore.Net.Nide8API.APIHandler(lockAuthNode.Property["nide8ID"]).GetInfoAsync();
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

                //设置java
                if (App.Config.MainConfig.Environment.AutoJava)
                {
                    launchSetting.UsingJava = Java.GetSuitableJava(App.JavaList, LaunchVersion);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(App.Config.MainConfig.Environment.JavaPath) && File.Exists(App.Config.MainConfig.Environment.JavaPath))
                    {
                        launchSetting.UsingJava = await Java.GetJavaInfoAsync(App.Config.MainConfig.Environment.JavaPath);
                    }
                    else
                    {
                        await App.MainWindowVM.ShowMessageAsync("您手动设置的JAVA不存在", "请检查您手动设置的java路径填写正确");
                        return;
                    }
                }
                //内存设置
                launchSetting.AutoMemory = App.Config.MainConfig.Environment.AutoMemory;
                launchSetting.MaxMemory = App.Config.MainConfig.Environment.MaxMemory;
                launchSetting.MinMemory = App.Config.MainConfig.Environment.MinMemory;

                launchSetting.VersionType = App.Config.MainConfig.Customize.VersionInfo;
                launchSetting.WindowSize = App.Config.MainConfig.Environment.WindowSize;

                #endregion

                #region 性能警告
                if (launchSetting.UsingJava.Arch == ArchEnum.x32 && SystemTools.GetSystemArch() == ArchEnum.x64)
                {
                    await MainWindowVM.ShowMessageAsync("性能优化提示",
                        "您正在使用32位的java启动minecraft，但您的系统为64位，使用64位的java能提升游戏性能");
                }
                if (launchSetting.UsingJava.Arch == ArchEnum.x32 && App.Config.MainConfig.Environment.AutoMemory == false && App.Config.MainConfig.Environment.MaxMemory > 1536)
                {
                    await MainWindowVM.ShowMessageAsync("内存分配警告",
                        "您正在使用32位的java启动minecraft，但您设置了手动分配内存且最大内存超过32位java限制。这可能导致游戏无法启动或崩溃");
                }
                #endregion

                #region 配置文件处理
                App.Config.Save();
                #endregion

                #region 启动
                try
                {
                    App.LogHandler.OnLog += OnLog;

                    //启动
                    var result = await App.Handler.LaunchAsync(LaunchVersion, launchSetting);

                    //标记启动中的实例
                    App.LaunchSignal.LaunchingInstance = result.Instance;

                    #region Debug
                    if (launchType == LaunchType.DEBUG)
                    {
                        DebugWindow debugWindow = new DebugWindow();
                        debugWindow.Show();
                        debugWindow.Title = LaunchVersion.Id;

                        debugWindow.AppendLog(this, new Log(LogLevel.DEBUG,
                            string.Format("Using java: versiom:{0} arch:{1} path:{2}",
                            result.UsingJava?.Version, result.UsingJava?.Arch, result.UsingJava?.Path)));
                        debugWindow.AppendLog(this, new Log(LogLevel.DEBUG, result.LaunchArguments));

                        if (result.Instance != null)
                        {
                            result.Instance.Log += debugWindow.AppendLog;
                        }
                    }
                    #endregion

                    //程序猿是找不到女朋友的了 :) 
                    if (!result.IsSuccess)
                    {
                        if (result.LaunchException != null)
                        {
                            if (result.LaunchException is GameValidateFailedException)
                            {
                                StringBuilder validate_str_builder = new StringBuilder();
                                foreach (var item in ((GameValidateFailedException)result.LaunchException).FailedFiles)
                                {
                                    validate_str_builder.AppendLine(Path.GetFileName(item.Key));
                                }
                                await MainWindowVM.ShowMessageAsync("游戏文件缺失或破损，无法安全启动",
                                    string.Format("游戏文件存在缺失或破损的情况，请检查游戏完整性\n{0}", validate_str_builder.ToString()));
                            }
                            else if (result.LaunchException is JavaNotMatchedException java_ex)
                            {
                                var java_choose = await MainWindowVM.ShowMessageAsync("该minecraft版本要求更高版本的JAVA",
                                   string.Format("游戏要求使用最低的java版本{0}，而启动所使用的java版本为{1}，是否下载该版本支持的java？",
                                   java_ex.RequiredVersion.MajorVersion, java_ex.CurrentJava.MajorVersion),
                                   MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings()
                                   {
                                       AffirmativeButtonText = "下载",
                                       NegativeButtonText = "取消"
                                   });
                                switch (java_choose)
                                {
                                    case MessageDialogResult.Canceled:
                                        break;
                                    case MessageDialogResult.Negative:
                                        break;
                                    case MessageDialogResult.Affirmative:
                                        NsisoLauncherCore.Modules.JavaVersion javaVersion = java_ex.RequiredVersion;
                                        if (javaVersion == null)
                                        {
                                            await MainWindowVM.ShowMessageAsync("该版本没有提供java信息", "无法进行自动下载，请自行下载最新版本java");
                                            return;
                                        }
                                        else
                                        {
                                            NativeJavaMeta meta = await LauncherMetaApi.GetNativeJavaMeta(javaVersion.Component);
                                            List<IDownloadTask> tasks = GetDownloadUri.GetJavaDownloadTasks(meta);
                                            App.NetHandler.Downloader.AddDownloadTask(tasks);
                                            App.MainPageVM.NavigateToDownloadPage();
                                            await App.NetHandler.Downloader.StartDownloadAndWaitDone();
                                            App.RefreshJavaList();
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                await MainWindowVM.ShowMessageAsync(App.GetResourceString("String.Mainwindow.LaunchError") + result.LaunchException.Title, result.LaunchException.Message);
                            }
                            App.LogHandler.AppendError(result.LaunchException);
                        }
                        else
                        {
                            await MainWindowVM.ShowMessageAsync("启动失败", "但启动核心并没有返回任何异常实例，这可能是因为内部原因导致的，请联系开发者");
                        }
                    }
                    else
                    {
                        if (launchType == LaunchType.CREATE_SHORT)
                        {
                            await MainWindowVM.ShowMessageAsync("创建快捷启动脚本成功",
                                "建立的快捷脚本启动器处于同一目录中，文件名：LaunchMinecraft.bat，将为您打开文件夹并选中该文件");
                            Process.Start("explorer", "/select,\"LaunchMinecraft.bat\"");
                            return;
                        }
                        CancelLaunchingCmd = new DelegateCommand(async (obj) => await CancelLaunching(result.Instance));

                        #region 等待游戏响应
                        try
                        {
                            await result.Instance.WaitForInputIdleAsync();
                        }
                        catch (Exception ex)
                        {
                            await MainWindowVM.ShowMessageAsync("启动后等待游戏窗口响应异常",
                                "这可能是由于游戏进程发生意外（闪退）导致的。具体原因:" + ex.Message);
                            return;
                        }
                        #endregion

                        CancelLaunchingCmd = null;

                        if (!result.Instance.HasExited)
                        {
                            MainWindowVM.WindowState = WindowState.Minimized;
                        }

                        #region 启动结束后

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
                            result.Instance.SetWindowTitle(App.Config.MainConfig.Customize.GameWindowTitle);
                        }
                        if (App.Config.MainConfig.Customize.CustomBackGroundMusic)
                        {
                            App.MainWindowVM.MediaSource = null;
                        }

                        #endregion
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    App.LogHandler.OnLog -= OnLog;
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
                App.LaunchSignal.LaunchingInstance = null;
            }
        }

        private async Task CancelLaunching(LaunchInstance instance)
        {
            if (!instance.HasExited)
            {
                instance.Kill();
            }
            await MainWindowVM.ShowMessageAsync("已取消启动", "已取消启动");
        }

        private void OnLog(object obj, Log l)
        {
            LogLine = l.Message;
        }
    }
}
