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
            //debug
            logHandler = new LogHandler(true);
            Windows.DebugWindow debugWindow = new Windows.DebugWindow();
            debugWindow.Show();
            TaskScheduler.UnobservedTaskException += (a, b) => logHandler.AppendError(b.Exception);
            logHandler.OnLog += (s, log) => debugWindow?.AppendLog(s, log);

            config = new ConfigHandler();

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
            #endregion

            handler = new LaunchHandler(gameroot, java, true);
            handler.GameLog += (s, log) => debugWindow?.AppendGameLog(s, log);

            downloader = new MultiThreadDownloader();
            downloader.ProcessorSize = App.config.MainConfig.Download.DownloadThreadsSize;
            downloader.DownloadLog += (s, log) => logHandler?.AppendLog(s, log);

            Core.Net.MojangApi.Api.Requester.ClientToken = config.MainConfig.User.ClientToken;
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
    }
}
