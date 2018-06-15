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

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //debug
            logHandler = new LogHandler(true);
            Windows.DebugWindow debugWindow = new Windows.DebugWindow();
            debugWindow.Show();
            TaskScheduler.UnobservedTaskException += (a, b) => logHandler.AppendError(b.Exception);
            logHandler.OnLog += (s, log) => debugWindow?.AppendLog(s, log);

            config = new Config.ConfigHandler();

            handler = new LaunchHandler(Path.GetFullPath(".minecraft"), Java.GetSuitableJava(), true);
            handler.GameLog += (s, log) => debugWindow?.AppendGameLog(s, log);

            downloader = new MultiThreadDownloader();
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
