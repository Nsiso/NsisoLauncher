using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;
using NsisoLauncher.Core.Util;
using NsisoLauncher.Core;

namespace NsisoLauncher
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static LaunchHandler handler = new LaunchHandler(Path.GetFullPath(".minecraft"), Java.GetSuitableJava(), true);

        private void Application_Startup(object sender, StartupEventArgs e)
        {
        }
    }
}
