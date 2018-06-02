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

namespace NsisoLauncher
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static LaunchHandler handler;
        public static Config.ConfigHandler config;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            config = new Config.ConfigHandler();
            handler = new LaunchHandler(Path.GetFullPath(".minecraft"), Java.GetSuitableJava(), true);

            Core.Net.MojangApi.Api.Requester.ClientToken = config.MainConfig.User.ClientToken;

            //debug
            Windows.DebugWindow debugWindow = new Windows.DebugWindow();
            debugWindow.Show();
            handler.Log += (s, log) => debugWindow.AppendLog(s, log);
        }
    }
}
