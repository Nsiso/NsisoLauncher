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

        public static event EventHandler<Log> Log;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //debug
            Windows.DebugWindow debugWindow = new Windows.DebugWindow();
            debugWindow.Show();
            TaskScheduler.UnobservedTaskException += (a, b) => debugWindow.AppendLog(a, new Log() { LogLevel = LogLevel.ERROR, Message = b.ToString() });
            DispatcherUnhandledException += (a, b) => debugWindow.AppendLog(a, new Log() { LogLevel = LogLevel.ERROR, Message = b.ToString() });
            Log += (s, log) => debugWindow?.AppendLog(s, log);

            config = new Config.ConfigHandler();
            handler = new LaunchHandler(Path.GetFullPath(".minecraft"), Java.GetSuitableJava(), true);
            handler.GameLog += (s, log) => debugWindow?.AppendGameLog(s, log);
            downloader = new MultiThreadDownloader();


            Core.Net.MojangApi.Api.Requester.ClientToken = config.MainConfig.User.ClientToken;

        }

        public static void SendLog(object sender, Log log)
        {
            Log?.Invoke(sender, log);
        }
    }
}
