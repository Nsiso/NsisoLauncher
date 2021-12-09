using ControlzEx.Theming;
using NsisoLauncher.Config;
using NsisoLauncher.Core.Util;
using NsisoLauncher.ViewModels.Pages;
using NsisoLauncher.ViewModels.Windows;
using NsisoLauncher.Views.Windows;
using NsisoLauncherCore;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.Mirrors;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace NsisoLauncher
{

    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        #region 全局属性
        /// <summary>
        /// 启动主模块
        /// </summary>
        public static LaunchHandler Handler { get; private set; }

        /// <summary>
        /// 应用配置文件
        /// </summary>
        public static ConfigHandler Config { get; private set; }

        /// <summary>
        /// 日志处理器
        /// </summary>
        public static LogHandler LogHandler { get; private set; }

        /// <summary>
        /// 网络处理类
        /// </summary>
        public static NetHandler NetHandler { get; private set; }

        /// <summary>
        /// 游戏启动信号
        /// </summary>
        public static LaunchSignal LaunchSignal { get; private set; }

        #region 全局数据集合属性

        /// <summary>
        /// JAVA本机列表
        /// </summary>
        public static ObservableCollection<Java> JavaList { get; private set; }

        /// <summary>
        /// 版本
        /// </summary>
        public static ObservableCollection<VersionBase> VersionList { get; private set; }
        #endregion

        #region 全局数据属性
        /// <summary>
        /// 启动器数据
        /// </summary>
        public static ObservableLauncherData LauncherData { get; set; }
        #endregion
        #endregion

        #region 应用窗口级属性
        /// <summary>
        /// 主窗口VM
        /// </summary>
        public static MainWindowViewModel MainWindowVM { get; set; }

        public static MainPageViewModel MainPageVM { get; set; }
        #endregion

        public static event EventHandler<AggregateExceptionArgs> AggregateExceptionCatched;

        public static void CatchAggregateException(object sender, AggregateExceptionArgs arg)
        {
            AggregateExceptionCatched?.Invoke(sender, arg);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            InitializeApplication(e);
        }

        private void InitializeApplication(StartupEventArgs e)
        {
            #region DEBUG初始化
            //debug
            LogHandler = new LogHandler();
            AggregateExceptionCatched += (a, b) => LogHandler.AppendFatal(b.AggregateException);
            if (e.Args.Contains("-debug"))
            {
                DebugWindow debugWindow = new DebugWindow();
                debugWindow.Show();
                LogHandler.OnLog += (s, log) => debugWindow?.AppendLog(s, log);
            }
            #endregion

            #region 路径/配置文件初始化
            try
            {
                //启动器运行时文件夹初始化
                PathManager.InitLauncherDirectory();

                //启动器配置文件初始化
                Config = new ConfigHandler();
            }
            catch (UnauthorizedAccessException ex)
            {
                NoAccessWarning(ex);
            }
            catch (System.Security.SecurityException ex)
            {
                NoAccessWarning(ex);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "启动器路径/配置文件初始化错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            #endregion

            #region DEBUG初始化（基于配置文件）
            if (Config.MainConfig.Launcher.Debug && !e.Args.Contains("-debug"))
            {
                DebugWindow debugWindow = new DebugWindow();
                debugWindow.Show();
                LogHandler.OnLog += (s, log) => debugWindow?.AppendLog(s, log);
            }
            LogHandler.WriteToFile = Config.MainConfig.Launcher.WriteLog;
            #endregion

            #region 自定义主题初始化
            var custom = Config.MainConfig.Customize;
            if (!string.IsNullOrWhiteSpace(custom.AccentColor))
            {
                LogHandler.AppendInfo("自定义->更改主题颜色:" + custom.AccentColor);
                ThemeManager.Current.ChangeThemeColorScheme(Current, custom.AccentColor);
            }
            if (!string.IsNullOrWhiteSpace(custom.AppTheme))
            {
                LogHandler.AppendInfo("自定义->更改主题:" + custom.AppTheme);
                ThemeManager.Current.ChangeThemeBaseColor(Current, custom.AppTheme);

            }
            #endregion

            #region 数据初始化
            Config.Environment env = Config.MainConfig.Environment;

            RefreshJavaList();

            //设置启动器数据
            LauncherData = new ObservableLauncherData();

            //设置版本路径
            string gameroot = null;
            switch (env.GamePathType)
            {
                case GameDirEnum.ROOT:
                    gameroot = Path.GetFullPath(".minecraft");
                    break;
                case GameDirEnum.APPDATA:
                    gameroot = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\.minecraft";
                    break;
                case GameDirEnum.PROGRAMFILES:
                    gameroot = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles) + "\\.minecraft";
                    break;
                case GameDirEnum.CUSTOM:
                    gameroot = env.GamePath + "\\.minecraft";
                    break;
                default:
                    throw new ArgumentException("判断游戏目录类型时出现异常，请检查配置文件中GamePathType节点");
            }
            LogHandler.AppendInfo("核心初始化->游戏根目录(默认则为空):" + gameroot);

            ////设置JAVA
            //Java java = null;
            //if (env.AutoJava)
            //{
            //    java = Java.GetSuitableJava(JavaList);
            //}
            //else
            //{
            //    java = JavaList.Where(x => x.Path == env.JavaPath).FirstOrDefault();
            //    if (java == null)
            //    {
            //        java = Java.GetJavaInfo(env.JavaPath);
            //    }

            //}
            //if (java != null)
            //{
            //    env.JavaPath = java.Path;
            //    LogHandler.AppendInfo("核心初始化->Java路径:" + java.Path);
            //    LogHandler.AppendInfo("核心初始化->Java版本:" + java.Version);
            //    LogHandler.AppendInfo("核心初始化->Java位数:" + java.Arch);
            //}
            //else
            //{
            //    LogHandler.AppendWarn("核心初始化失败，当前电脑未匹配到JAVA");
            //}

            //设置版本独立
            bool verIso = Config.MainConfig.Environment.VersionIsolation;
            #endregion

            #region 启动核心初始化
            Handler = new LaunchHandler(gameroot, verIso);
            Handler.GameLog += (s, log) => LogHandler.AppendLog(s, log);
            Handler.LaunchLog += (s, log) => LogHandler.AppendLog(s, log);
            Handler.Javas = JavaList;
            #endregion

            #region 网络功能初始化
            #region 网络核心初始化
            NetHandler = new NetHandler();

            if (Config.MainConfig.Net.IsLauncherUseProxy)
            {
                WebProxy proxy = new WebProxy(Config.MainConfig.Net.ProxyHost, Config.MainConfig.Net.ProxyPort);
                if (!string.IsNullOrWhiteSpace(Config.MainConfig.Net.ProxyUsername))
                {
                    proxy.Credentials = new NetworkCredential(Config.MainConfig.Net.ProxyUsername, Config.MainConfig.Net.ProxyPassword);
                }
                NetRequester.NetProxy = proxy;
            }
            #endregion

            #region 下载核心设置
            ServicePointManager.DefaultConnectionLimit = 10;

            Net downloadCfg = Config.MainConfig.Net;
            NetHandler.Downloader.ProtocolType = Config.MainConfig.Net.DownloadProtocolType;
            NetHandler.Downloader.ProcessorSize = Config.MainConfig.Net.DownloadThreadsSize;
            NetHandler.Downloader.CheckFileHash = Config.MainConfig.Net.CheckDownloadFileHash;
            NetHandler.Downloader.DownloadLog += (s, log) => LogHandler?.AppendLog(s, log);
            #endregion

            #region 处理镜像源
            if (NetHandler.Downloader.MirrorList == null)
            {
                NetHandler.Downloader.MirrorList = new List<IDownloadableMirror>();
            }

            switch (Config.MainConfig.Net.DownloadSource)
            {
                case DownloadSource.Auto:
                    NetHandler.Mirrors.DownloadableMirrorList.Add(NetHandler.Mirrors.GetBmclApi());
                    NetHandler.Mirrors.DownloadableMirrorList.Add(NetHandler.Mirrors.GetMcbbsApi());
                    break;
                case DownloadSource.Mojang:
                    break;
                case DownloadSource.BMCLAPI:
                    NetHandler.Mirrors.DownloadableMirrorList.Add(NetHandler.Mirrors.GetBmclApi());
                    break;
                case DownloadSource.MCBBS:
                    NetHandler.Mirrors.DownloadableMirrorList.Add(NetHandler.Mirrors.GetMcbbsApi());
                    break;
                default:
                    break;
            }

            switch (Config.MainConfig.Net.VersionSource)
            {
                case VersionSourceType.MOJANG:
                    break;
                case VersionSourceType.BMCLAPI:
                    NetHandler.Mirrors.VersionListMirrorList.Add(NetHandler.Mirrors.GetBmclApi());
                    break;
                case VersionSourceType.MCBBS:
                    NetHandler.Mirrors.VersionListMirrorList.Add(NetHandler.Mirrors.GetMcbbsApi());
                    break;
                default:
                    break;
            }

            switch (Config.MainConfig.Net.FunctionSource)
            {
                case FunctionSourceType.BMCLAPI:
                    NetHandler.Mirrors.FunctionalMirrorList.Add(NetHandler.Mirrors.GetBmclApi());
                    break;
                case FunctionSourceType.MCBBS:
                    NetHandler.Mirrors.FunctionalMirrorList.Add(NetHandler.Mirrors.GetMcbbsApi());
                    break;
                default:
                    break;
            }
            #endregion
            #endregion

            LaunchSignal = new LaunchSignal();

            #region 初始化主窗体
            MainWindowViewModel window_vm = new MainWindowViewModel();
            MainWindow mainwindow = new MainWindow(window_vm);

            MainWindowVM = window_vm;
            this.MainWindow = mainwindow;

            mainwindow.Show();
            #endregion

            #region 窗口提示事件绑定
            App.NetHandler.Downloader.DownloadCompleted += Downloader_DownloadCompleted;
            #endregion
        }

        private async void Downloader_DownloadCompleted(object sender, DownloadCompletedArg e)
        {
            if (e.ErrorList == null || e.ErrorList.Count == 0)
            {
                await MainWindowVM?.ShowMessageAsync(App.GetResourceString("String.Downloadwindow.DownloadComplete"),
                    App.GetResourceString("String.Downloadwindow.DownloadComplete2"));
            }
            else
            {
                await MainWindowVM?.ShowMessageAsync(App.GetResourceString("String.Downloadwindow.DownloadCompleteWithError"),
                    string.Format(App.GetResourceString("String.Downloadwindow.DownloadCompleteWithError2"), e.ErrorList.Count, e.ErrorList.First().Value?.Message));
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogHandler.AppendFatal(e.Exception);
            e.Handled = true;
        }

        public static string GetResourceString(string key)
        {
            return (string)Current.FindResource(key);
        }

        /// <summary>
        /// 重启启动器
        /// </summary>
        /// <param name="admin">是否用管理员模式重启</param>
        public static void Reboot(bool admin)
        {
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var args = System.Environment.GetCommandLineArgs();
            foreach (var item in args)
            {
                info.Arguments += (item + ' ');
            }
            if (admin)
            {
                info.Verb = "runas";
            }
            info.Arguments += "-reboot";
            System.Diagnostics.Process.Start(info);
            Current.Shutdown();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                Config.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static void RefreshJavaList()
        {
            if (JavaList == null)
            {
                JavaList = new ObservableCollection<Java>();
            }
            JavaList.Clear();
            List<Java> javas = Java.GetJavaList();
            javas.AddRange(Java.GetRuntimeRootJavaList(PathManager.RuntimeDirectory));
            foreach (var item in javas)
            {
                JavaList.Add(item);
            }

            //if (Directory.Exists(PathManager.RuntimeDirectory))
            //{
            //    string[] dirs = Directory.GetDirectories(PathManager.RuntimeDirectory);
            //    string core = "java-runtime-alpha";
            //    foreach (var item in dirs)
            //    {
            //        string native = null;
            //        switch (SystemTools.GetOsType())
            //        {
            //            case OsType.Windows:
            //                string[]
            //                break;
            //            case OsType.Linux:
            //                native = "linux";
            //                break;
            //            case OsType.MacOS:
            //                native = "macos";
            //                break;
            //            default:
            //                break;
            //        }

            //    }
            //}
        }

        public async static Task RefreshVersionListAsync()
        {
            if (VersionList == null)
            {
                VersionList = new ObservableCollection<VersionBase>();
            }
            var list = await Handler.GetVersionsAsync();
            VersionList.Clear();
            foreach (var item in list)
            {
                VersionList.Add(item);
            }
        }

        public static void RefreshVersionList()
        {
            if (VersionList == null)
            {
                VersionList = new ObservableCollection<VersionBase>();
            }
            var list = Handler.GetVersions();
            VersionList.Clear();
            foreach (var item in list)
            {
                VersionList.Add(item);
            }
        }

        private static void NoAccessWarning(Exception e)
        {
            var result = MessageBox.Show("启动器无法正常读/写配置文件。\n" +
                    "这可能是由于您将启动器放置在系统敏感目录（如C盘，桌面等系统关键位置）\n" +
                    "或您没有足够的系统操作权限而导致系统自我保护机制，禁止启动器读写文件。\n" +
                    "是否以管理员模式运行启动器？若拒绝则请自行移动到有权限的路径运行。\n" +
                    "详细信息:\n" +
                    e.ToString(),
                    "启动器权限不足", MessageBoxButton.YesNo, MessageBoxImage.Error);
            if (result == MessageBoxResult.Yes)
            {
                App.Reboot(true);
            }
        }
    }

    //定义异常参数处理
    public class AggregateExceptionArgs : EventArgs
    {
        public AggregateException AggregateException { get; set; }
    }
}
