
using MahApps.Metro;
using NsisoLauncher.Config;
using NsisoLauncher.Core.Util;
using NsisoLauncher.Views.Windows;
using NsisoLauncherCore;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Version = NsisoLauncherCore.Modules.Version;

namespace NsisoLauncher
{

    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static LaunchHandler Handler { get; private set; }
        public static ConfigHandler Config { get; private set; }
        public static MultiThreadDownloader Downloader { get; private set; }
        public static LogHandler LogHandler { get; private set; }
        public static List<Java> JavaList { get; private set; }
        public static ObservableCollection<Version> VersionList { get; private set; }
        public static NsisoLauncherCore.Net.PhalAPI.APIHandler NsisoAPIHandler { get; private set; }


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
            LogHandler = new LogHandler(true);
            AggregateExceptionCatched += (a, b) => LogHandler.AppendFatal(b.AggregateException);
            if (e.Args.Contains("-debug"))
            {
                DebugWindow debugWindow = new DebugWindow();
                debugWindow.Show();
                LogHandler.OnLog += (s, log) => debugWindow?.AppendLog(s, log);
            }
            #endregion

            Config = new ConfigHandler();

            #region DEBUG初始化（基于配置文件）
            if (Config.MainConfig.Launcher.Debug && !e.Args.Contains("-debug"))
            {
                DebugWindow debugWindow = new DebugWindow();
                debugWindow.Show();
                LogHandler.OnLog += (s, log) => debugWindow?.AppendLog(s, log);
            }
            #endregion

            #region Nsiso反馈API初始化

#if DEBUG
            NsisoAPIHandler = new NsisoLauncherCore.Net.PhalAPI.APIHandler(true);
#else
            NsisoAPIHandler = new NsisoLauncherCore.Net.PhalAPI.APIHandler(Config.MainConfig.Launcher.NoTracking);
#endif

            #endregion

            #region 数据初始化
            Config.Environment env = Config.MainConfig.Environment;

            JavaList = Java.GetJavaList();

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

            //设置JAVA
            Java java = null;
            if (env.AutoJava)
            {
                java = Java.GetSuitableJava(JavaList);
            }
            else
            {
                java = JavaList.Find(x => x.Path == env.JavaPath);
                if (java == null)
                {
                    java = Java.GetJavaInfo(env.JavaPath);
                }

            }
            if (java != null)
            {
                env.JavaPath = java.Path;
                LogHandler.AppendInfo("核心初始化->Java路径:" + java.Path);
                LogHandler.AppendInfo("核心初始化->Java版本:" + java.Version);
                LogHandler.AppendInfo("核心初始化->Java位数:" + java.Arch);
            }
            else
            {
                LogHandler.AppendWarn("核心初始化失败，当前电脑未匹配到JAVA");
            }

            //设置版本独立
            bool verIso = Config.MainConfig.Environment.VersionIsolation;
            #endregion

            #region 启动核心初始化
            Handler = new LaunchHandler(gameroot, java, verIso);
            Handler.GameLog += (s, log) => LogHandler.AppendLog(s, new Log() { LogLevel = LogLevel.GAME, Message = log });
            Handler.LaunchLog += (s, log) => LogHandler.AppendLog(s, log);
            #endregion

            #region 下载核心初始化
            ServicePointManager.DefaultConnectionLimit = 10;

            Download downloadCfg = Config.MainConfig.Download;
            Downloader = new MultiThreadDownloader();
            if (!string.IsNullOrWhiteSpace(downloadCfg.DownloadProxyAddress))
            {
                WebProxy proxy = new WebProxy(downloadCfg.DownloadProxyAddress, downloadCfg.DownloadProxyPort);
                if (!string.IsNullOrWhiteSpace(downloadCfg.ProxyUserName))
                {
                    NetworkCredential credential = new NetworkCredential(downloadCfg.ProxyUserName, downloadCfg.ProxyUserPassword);
                    proxy.Credentials = credential;
                }
                Downloader.Proxy = proxy;
            }
            Downloader.ProcessorSize = App.Config.MainConfig.Download.DownloadThreadsSize;
            Downloader.CheckFileHash = App.Config.MainConfig.Download.CheckDownloadFileHash;
            Downloader.DownloadLog += (s, log) => LogHandler?.AppendLog(s, log);
            #endregion

            #region 自定义主题初始化
            var custom = Config.MainConfig.Customize;
            if (!string.IsNullOrWhiteSpace(custom.AccentColor) && !string.IsNullOrWhiteSpace(custom.AppThme))
            {
                LogHandler.AppendInfo("自定义->更改主题颜色:" + custom.AccentColor);
                LogHandler.AppendInfo("自定义->更改主题:" + custom.AppThme);
                ThemeManager.ChangeAppStyle(Current, ThemeManager.GetAccent(custom.AccentColor), ThemeManager.GetAppTheme(custom.AppThme));
            }
            #endregion    
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            App.LogHandler.AppendFatal(e.Exception);
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
            App.Current.Shutdown();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                Config.Save();
            }
            catch (Exception)
            { }
        }

        public async static Task RefreshVersionListAsync()
        {
            if (VersionList == null)
            {
                VersionList = new ObservableCollection<Version>();
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
                VersionList = new ObservableCollection<Version>();
            }
            var list = Handler.GetVersions();
            VersionList.Clear();
            foreach (var item in list)
            {
                VersionList.Add(item);
            }
        }
    }

    //定义异常参数处理
    public class AggregateExceptionArgs : EventArgs
    {
        public AggregateException AggregateException { get; set; }
    }
}
