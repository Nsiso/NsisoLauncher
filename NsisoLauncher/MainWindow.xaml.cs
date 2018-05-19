using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using NsisoLauncher.Core.Modules;

namespace NsisoLauncher
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void launchButton_Click(object sender, RoutedEventArgs e)
        {
            var auth = Core.Auth.OfflineAuthenticator.OfflineAuthenticate("Nsiso");
            LaunchSetting launchSetting = new LaunchSetting()
            {
                Version = App.handler.GetVersions().First(),
                MaxMemory = 1024,
                AuthenticateResponse = auth.Item2,
                AuthenticateSelectedUUID = auth.Item1
            };
            App.handler.Launch(launchSetting);
        }
    }
}
