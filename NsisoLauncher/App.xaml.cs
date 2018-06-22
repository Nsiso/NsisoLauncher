using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;
using NsisoLauncher.Core.Util;
using NsisoLauncher.Core;
using System.Threading.Tasks;
using NsisoLauncher.Core.Modules;
using NsisoLauncher.Utils;
using NsisoLauncher.Config;
using MahApps.Metro;

namespace NsisoLauncher
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static LaunchHandler handler;
        public static Config.ConfigHandler config;
        public static MultiThreadDownloader downloader;
        public static LogHandler logHandler;
        public static List<Java> javaList;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            #region DEBUG初始化
            //debug
            logHandler = new LogHandler(true);
            TaskScheduler.UnobservedTaskException += (a, b) => logHandler.AppendError(b.Exception);
            if (e.Args.Contains("-debug"))
            {
                Windows.DebugWindow debugWindow = new Windows.DebugWindow();
                debugWindow.Show();
                logHandler.OnLog += (s, log) => debugWindow?.AppendLog(s, log);
            }
            #endregion

            config = new ConfigHandler();

            #region DEBUG初始化（基于配置文件）
            if (config.MainConfig.Launcher.Debug)
            {
                Windows.DebugWindow debugWindow = new Windows.DebugWindow();
                debugWindow.Show();
                logHandler.OnLog += (s, log) => debugWindow?.AppendLog(s, log);
            }
            #endregion

            #region 数据初始化
            Config.Environment env = App.config.MainConfig.Environment;

            javaList = Java.GetJavaList();

            string gameroot = null;
            switch (env.GamePathType)
            {
                case GameDirEnum.ROOT:
                    gameroot = Path.GetFullPath(".minecraft");
                    break;
                case GameDirEnum.APPDATA:
                    gameroot = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
                    break;
                case GameDirEnum.PROGRAMFILES:
                    gameroot = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles);
                    break;
                case GameDirEnum.CUSTOM:
                    gameroot = env.GamePath;
                    break;
                default:
                    throw new ArgumentException("判断游戏目录类型时出现异常，请检查配置文件中GamePathType节点");
            }
            logHandler.AppendInfo("核心初始化->游戏根目录(默认则为空):" + gameroot);

            //设置JAVA
            Java java = null;
            if (env.AutoJava)
            {
                java = Java.GetSuitableJava(javaList);
                env.JavaPath = java.Path;
            }
            else
            {
                foreach (var item in javaList)
                {
                    if (item.Path == env.JavaPath)
                    {
                        java = item;
                        break;
                    }
                }
                if (java == null)
                {
                    java = Java.GetJavaInfo(env.JavaPath);
                }

            }
            if (java != null)
            {
                logHandler.AppendInfo("核心初始化->Java路径:" + java.Path);
                logHandler.AppendInfo("核心初始化->Java版本:" + java.Version);
                logHandler.AppendInfo("核心初始化->Java位数:" + java.Arch);
            }
            else
            {
                logHandler.AppendWarn("核心初始化失败，当前电脑未匹配到JAVA");
            }

            bool verIso = config.MainConfig.Environment.VersionIsolation;
            #endregion

            #region 启动核心初始化
            handler = new LaunchHandler(gameroot, java, verIso);
            handler.GameLog += (s, log) => logHandler.AppendLog(s, new Log() { LogLevel = LogLevel.GAME, Message = log });
            #endregion

            #region 下载核心初始化
            downloader = new MultiThreadDownloader();
            downloader.ProcessorSize = App.config.MainConfig.Download.DownloadThreadsSize;
            downloader.DownloadLog += (s, log) => logHandler?.AppendLog(s, log);
            #endregion

            #region 自定义主题初始化
            var custom = config.MainConfig.Customize;
            if (!string.IsNullOrWhiteSpace(custom.AccentColor) && !string.IsNullOrWhiteSpace(custom.AppThme))
            {
                logHandler.AppendInfo("自定义->更改主题颜色:" + custom.AccentColor);
                logHandler.AppendInfo("自定义->更改主题:" + custom.AppThme);
                ThemeManager.ChangeAppStyle(Current, ThemeManager.GetAccent(custom.AccentColor), ThemeManager.GetAppTheme(custom.AppThme));
            }
            #endregion
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            App.logHandler.AppendFatal(e.Exception);
            e.Handled = true;
        }

        public static string GetResourceString(string key)
        {
            return (string)Current.FindResource(key);
        }

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
    }
}
